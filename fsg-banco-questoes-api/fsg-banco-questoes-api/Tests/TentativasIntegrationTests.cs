using Microsoft.AspNetCore.Mvc.Testing;

namespace BancoQuestoes.Tests;

/// <summary>Cobre especificamente a restrição de prova por turma em Iniciar — o resto do fluxo
/// de tentativa (responder, finalizar, resultado) fica coberto indiretamente pelos testes de
/// RedacoesIntegrationTests, que precisam completar uma prova até o fim.</summary>
public class TentativasIntegrationTests : IntegrationTestBase
{
    public TentativasIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private async Task<int> CriarQuestaoAsync()
    {
        var request = new CreateQuestaoRequest
        {
            Titulo = "Questão de teste",
            Area = AreaConhecimento.Matematica,
            Alternativas =
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/questoes", request);
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    private async Task<int> CriarProvaAsync(int questaoId, int? turmaId)
    {
        var request = new CreateProvaRequest
        {
            Titulo = "Prova de teste",
            TurmaId = turmaId,
            QuestoesIds = [questaoId]
        };

        var response = await _client.PostAsJsonAsync("/api/provas", request);
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    private async Task<int> CriarTurmaAsync(string nome)
    {
        var response = await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = nome });
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    [Fact]
    public async Task Iniciar_ShouldReturnForbidden_WhenProvaBelongsToDifferentTurma()
    {
        // Arrange — prova é da Turma A, aluno de teste não está matriculado em turma nenhuma
        var questaoId = await CriarQuestaoAsync();
        var turmaId = await CriarTurmaAsync("Turma A");
        var provaId = await CriarProvaAsync(questaoId, turmaId);

        // Act
        var response = await _alunoClient.PostAsJsonAsync("/api/tentativas/iniciar", new IniciarTentativaRequest { ProvaId = provaId });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Iniciar_ShouldSucceed_WhenProvaBelongsToSameTurma()
    {
        // Arrange — aluno matriculado na mesma turma da prova
        var questaoId = await CriarQuestaoAsync();
        var turmaId = await CriarTurmaAsync("Turma A");
        await _client.PostAsJsonAsync($"/api/turmas/{turmaId}/alunos", new MatricularAlunoRequest { AlunoId = AlunoId });
        var provaId = await CriarProvaAsync(questaoId, turmaId);

        // Act
        var response = await _alunoClient.PostAsJsonAsync("/api/tentativas/iniciar", new IniciarTentativaRequest { ProvaId = provaId });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Iniciar_ShouldSucceed_WhenProvaHasNoTurma()
    {
        // Arrange — prova aberta (sem turma), aluno sem turma nenhuma
        var questaoId = await CriarQuestaoAsync();
        var provaId = await CriarProvaAsync(questaoId, turmaId: null);

        // Act
        var response = await _alunoClient.PostAsJsonAsync("/api/tentativas/iniciar", new IniciarTentativaRequest { ProvaId = provaId });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetProvas_AsAluno_ShouldOnlyShowOpenAndOwnTurmaProvas()
    {
        // Arrange — 3 provas: aberta, da turma do aluno, e de outra turma
        var questaoId = await CriarQuestaoAsync();
        var turmaDoAluno = await CriarTurmaAsync("Turma do aluno");
        var outraTurma = await CriarTurmaAsync("Outra turma");
        await _client.PostAsJsonAsync($"/api/turmas/{turmaDoAluno}/alunos", new MatricularAlunoRequest { AlunoId = AlunoId });

        await CriarProvaAsync(questaoId, turmaId: null);
        await CriarProvaAsync(questaoId, turmaDoAluno);
        await CriarProvaAsync(questaoId, outraTurma);

        // Act
        var response = await _alunoClient.GetAsync("/api/provas");

        // Assert
        response.EnsureSuccessStatusCode();
        var provas = await response.Content.ReadFromJsonAsync<List<ProvaListResponse>>();
        Assert.Equal(2, provas!.Count);
        Assert.DoesNotContain(provas, p => p.TurmaId == outraTurma);
    }

    [Fact]
    public async Task GetProvas_AsProfessor_ShouldShowAllProvasRegardlessOfTurma()
    {
        // Arrange
        var questaoId = await CriarQuestaoAsync();
        var turmaId = await CriarTurmaAsync("Turma qualquer");
        await CriarProvaAsync(questaoId, turmaId: null);
        await CriarProvaAsync(questaoId, turmaId);

        // Act
        var response = await _client.GetAsync("/api/provas");

        // Assert
        response.EnsureSuccessStatusCode();
        var provas = await response.Content.ReadFromJsonAsync<List<ProvaListResponse>>();
        Assert.Equal(2, provas!.Count);
    }
}
