using Microsoft.AspNetCore.Mvc.Testing;

namespace BancoQuestoes.Tests;

public class TurmasIntegrationTests : IntegrationTestBase
{
    public TurmasIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTurmas_ShouldReturnEmptyList_WhenNoTurmas()
    {
        // Act
        var response = await _client.GetAsync("/api/turmas");

        // Assert
        response.EnsureSuccessStatusCode();
        var turmas = await response.Content.ReadFromJsonAsync<List<TurmaListResponse>>();
        Assert.NotNull(turmas);
        Assert.Empty(turmas);
    }

    [Fact]
    public async Task CreateTurma_ShouldReturnCreated_WithValidData()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Turma 101" });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task CreateTurma_ShouldReturnBadRequest_WithoutNome()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTurma_ShouldReturnNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.GetAsync("/api/turmas/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTurma_ShouldRenameTurma()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Nome Original" });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/turmas/{created!.Id}", new CreateTurmaRequest { Nome = "Nome Atualizado" });

        // Assert
        updateResponse.EnsureSuccessStatusCode();
        var turma = await (await _client.GetAsync($"/api/turmas/{created.Id}")).Content.ReadFromJsonAsync<TurmaDetailResponse>();
        Assert.Equal("Nome Atualizado", turma!.Nome);
    }

    [Fact]
    public async Task DeleteTurma_ShouldReturnOk_WhenExists()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Turma para deletar" });
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/turmas/{created!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/turmas/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task MatricularAluno_ShouldAddAlunoToTurma()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Turma 101" });
        var turmaCreated = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/turmas/{turmaCreated!.Id}/alunos", new MatricularAlunoRequest { AlunoId = AlunoId });

        // Assert
        response.EnsureSuccessStatusCode();
        var turma = await (await _client.GetAsync($"/api/turmas/{turmaCreated.Id}")).Content.ReadFromJsonAsync<TurmaDetailResponse>();
        Assert.Single(turma!.Alunos);
        Assert.Equal(AlunoId, turma.Alunos[0].IdUsuario);
    }

    [Fact]
    public async Task MatricularAluno_ShouldMoveAluno_WhenAlreadyInAnotherTurma()
    {
        // Arrange — duas turmas, aluno matriculado na primeira
        var turma1 = await (await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Turma A" }))
            .Content.ReadFromJsonAsync<CreatedResponse>();
        var turma2 = await (await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Turma B" }))
            .Content.ReadFromJsonAsync<CreatedResponse>();

        await _client.PostAsJsonAsync($"/api/turmas/{turma1!.Id}/alunos", new MatricularAlunoRequest { AlunoId = AlunoId });

        // Act — matricula na segunda turma
        await _client.PostAsJsonAsync($"/api/turmas/{turma2!.Id}/alunos", new MatricularAlunoRequest { AlunoId = AlunoId });

        // Assert — some da primeira, aparece na segunda
        var turma1Depois = await (await _client.GetAsync($"/api/turmas/{turma1.Id}")).Content.ReadFromJsonAsync<TurmaDetailResponse>();
        var turma2Depois = await (await _client.GetAsync($"/api/turmas/{turma2.Id}")).Content.ReadFromJsonAsync<TurmaDetailResponse>();
        Assert.Empty(turma1Depois!.Alunos);
        Assert.Single(turma2Depois!.Alunos);
    }

    [Fact]
    public async Task DesmatricularAluno_ShouldRemoveAlunoFromTurma()
    {
        // Arrange
        var turma = await (await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Turma 101" }))
            .Content.ReadFromJsonAsync<CreatedResponse>();
        await _client.PostAsJsonAsync($"/api/turmas/{turma!.Id}/alunos", new MatricularAlunoRequest { AlunoId = AlunoId });

        // Act
        var response = await _client.DeleteAsync($"/api/turmas/{turma.Id}/alunos/{AlunoId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var turmaDepois = await (await _client.GetAsync($"/api/turmas/{turma.Id}")).Content.ReadFromJsonAsync<TurmaDetailResponse>();
        Assert.Empty(turmaDepois!.Alunos);
    }

    [Fact]
    public async Task GetAlunos_ShouldListAlunoComTurmaAtual()
    {
        // Arrange
        var turma = await (await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Turma 101" }))
            .Content.ReadFromJsonAsync<CreatedResponse>();
        await _client.PostAsJsonAsync($"/api/turmas/{turma!.Id}/alunos", new MatricularAlunoRequest { AlunoId = AlunoId });

        // Act
        var response = await _client.GetAsync("/api/turmas/alunos");

        // Assert
        response.EnsureSuccessStatusCode();
        var alunos = await response.Content.ReadFromJsonAsync<List<AlunoResumoResponse>>();
        var alunoTeste = alunos!.Single(a => a.IdUsuario == AlunoId);
        Assert.Equal("Turma 101", alunoTeste.TurmaNome);
    }

    [Fact]
    public async Task DeleteTurma_ShouldSetAlunoTurmaToNull_NotDeleteAluno()
    {
        // Arrange
        var turma = await (await _client.PostAsJsonAsync("/api/turmas", new CreateTurmaRequest { Nome = "Turma temporária" }))
            .Content.ReadFromJsonAsync<CreatedResponse>();
        await _client.PostAsJsonAsync($"/api/turmas/{turma!.Id}/alunos", new MatricularAlunoRequest { AlunoId = AlunoId });

        // Act
        await _client.DeleteAsync($"/api/turmas/{turma.Id}");

        // Assert — o aluno continua existindo, só sem turma
        var alunos = await (await _client.GetAsync("/api/turmas/alunos")).Content.ReadFromJsonAsync<List<AlunoResumoResponse>>();
        var alunoTeste = alunos!.Single(a => a.IdUsuario == AlunoId);
        Assert.Null(alunoTeste.TurmaId);
    }
}
