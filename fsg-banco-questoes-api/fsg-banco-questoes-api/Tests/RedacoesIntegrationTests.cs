using Microsoft.AspNetCore.Mvc.Testing;

namespace BancoQuestoes.Tests;

public class RedacoesIntegrationTests : IntegrationTestBase
{
    public RedacoesIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    /// <summary>Monta uma prova com tema de redação, faz o aluno de teste iniciar e finalizar
    /// com um texto, e devolve o ID da tentativa — ponto de partida de quase todo teste aqui.</summary>
    private async Task<int> CriarTentativaFinalizadaComRedacaoAsync(string textoRedacao = "Texto de redação de teste.")
    {
        var questaoResponse = await _client.PostAsJsonAsync("/api/questoes", new CreateQuestaoRequest
        {
            Titulo = "Questão de teste",
            Area = AreaConhecimento.Matematica,
            Alternativas =
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        });
        var questaoCreated = await questaoResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var temaResponse = await _client.PostAsJsonAsync("/api/temas-redacao", new CreateTemaRedacaoRequest
        {
            Titulo = "Tema de teste",
            TextoMotivador = "Texto motivador de teste."
        });
        var temaCreated = await temaResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var provaResponse = await _client.PostAsJsonAsync("/api/provas", new CreateProvaRequest
        {
            Titulo = "Prova com redação",
            TemaRedacaoId = temaCreated!.Id,
            QuestoesIds = [questaoCreated!.Id]
        });
        var provaCreated = await provaResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var iniciarResponse = await _alunoClient.PostAsJsonAsync("/api/tentativas/iniciar", new IniciarTentativaRequest { ProvaId = provaCreated!.Id });
        var tentativa = await iniciarResponse.Content.ReadFromJsonAsync<TentativaIniciadaResponse>();

        await _alunoClient.PostAsJsonAsync($"/api/tentativas/{tentativa!.IdTentativa}/finalizar", new FinalizarTentativaRequest { TextoRedacao = textoRedacao });

        return tentativa.IdTentativa;
    }

    private static CorrigirRedacaoRequest NotasValidas(int c1 = 160, int c2 = 140, int c3 = 160, int c4 = 120, int c5 = 160, string? comentario = null) =>
        new()
        {
            NotaComp1 = c1,
            NotaComp2 = c2,
            NotaComp3 = c3,
            NotaComp4 = c4,
            NotaComp5 = c5,
            ComentarioRedacao = comentario
        };

    [Fact]
    public async Task GetRedacoes_ShouldListPendente_AfterAlunoFinalizaComRedacao()
    {
        // Arrange
        await CriarTentativaFinalizadaComRedacaoAsync();

        // Act
        var response = await _client.GetAsync("/api/redacoes");

        // Assert
        response.EnsureSuccessStatusCode();
        var redacoes = await response.Content.ReadFromJsonAsync<List<RedacaoPendenteResponse>>();
        Assert.Single(redacoes!);
        Assert.False(redacoes![0].Corrigida);
        Assert.Null(redacoes[0].NotaRedacao);
    }

    [Fact]
    public async Task CorrigirRedacao_ShouldComputeNotaRedacao_AsSumOfCompetencias()
    {
        // Arrange
        var tentativaId = await CriarTentativaFinalizadaComRedacaoAsync();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/redacoes/{tentativaId}", NotasValidas(160, 140, 160, 120, 160));

        // Assert
        response.EnsureSuccessStatusCode();
        var detalhe = await (await _client.GetAsync($"/api/redacoes/{tentativaId}")).Content.ReadFromJsonAsync<RedacaoDetailResponse>();
        Assert.Equal(740, detalhe!.NotaRedacao);
        Assert.Equal(160, detalhe.NotaComp1);
        Assert.Equal(120, detalhe.NotaComp4);
    }

    [Fact]
    public async Task CorrigirRedacao_ShouldReturnBadRequest_WhenCompetenciaNotMultipleOf20()
    {
        // Arrange
        var tentativaId = await CriarTentativaFinalizadaComRedacaoAsync();

        // Act — C3 = 150 não é múltiplo de 20
        var response = await _client.PutAsJsonAsync($"/api/redacoes/{tentativaId}", NotasValidas(c3: 150));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CorrigirRedacao_ShouldReturnBadRequest_WhenCompetenciaAcimaDoLimite()
    {
        // Arrange
        var tentativaId = await CriarTentativaFinalizadaComRedacaoAsync();

        // Act — C1 = 220 está fora do intervalo 0-200 (rejeitado pela validação do Request)
        var response = await _client.PutAsJsonAsync($"/api/redacoes/{tentativaId}", NotasValidas(c1: 220));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CorrigirRedacao_ShouldSaveComentario()
    {
        // Arrange
        var tentativaId = await CriarTentativaFinalizadaComRedacaoAsync();

        // Act
        await _client.PutAsJsonAsync($"/api/redacoes/{tentativaId}", NotasValidas(comentario: "Bom texto, atenção à coesão."));

        // Assert
        var detalhe = await (await _client.GetAsync($"/api/redacoes/{tentativaId}")).Content.ReadFromJsonAsync<RedacaoDetailResponse>();
        Assert.Equal("Bom texto, atenção à coesão.", detalhe!.ComentarioRedacao);
    }

    [Fact]
    public async Task GetRedacoes_ShouldShowCorrigida_AfterCorrecao()
    {
        // Arrange
        var tentativaId = await CriarTentativaFinalizadaComRedacaoAsync();
        await _client.PutAsJsonAsync($"/api/redacoes/{tentativaId}", NotasValidas());

        // Act
        var redacoes = await (await _client.GetAsync("/api/redacoes")).Content.ReadFromJsonAsync<List<RedacaoPendenteResponse>>();

        // Assert
        var redacao = redacoes!.Single();
        Assert.True(redacao.Corrigida);
        Assert.Equal(740, redacao.NotaRedacao);
    }

    [Fact]
    public async Task GetResultado_ShouldShowNotaRedacaoNull_BeforeCorrecao()
    {
        // Arrange
        var tentativaId = await CriarTentativaFinalizadaComRedacaoAsync();

        // Act — o próprio aluno consulta o resultado (papel Aluno)
        var response = await _alunoClient.GetAsync($"/api/tentativas/{tentativaId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var resultado = await response.Content.ReadFromJsonAsync<ResultadoTentativaResponse>();
        Assert.Null(resultado!.NotaRedacao);
    }

    [Fact]
    public async Task GetResultado_ShouldShowCompetencias_AfterCorrecao()
    {
        // Arrange
        var tentativaId = await CriarTentativaFinalizadaComRedacaoAsync();
        await _client.PutAsJsonAsync($"/api/redacoes/{tentativaId}", NotasValidas(160, 140, 160, 120, 160));

        // Act
        var resultado = await (await _alunoClient.GetAsync($"/api/tentativas/{tentativaId}")).Content.ReadFromJsonAsync<ResultadoTentativaResponse>();

        // Assert
        Assert.Equal(740, resultado!.NotaRedacao);
        Assert.Equal(160, resultado.NotaComp1);
        Assert.Equal(140, resultado.NotaComp2);
        Assert.Equal(160, resultado.NotaComp3);
        Assert.Equal(120, resultado.NotaComp4);
        Assert.Equal(160, resultado.NotaComp5);
    }
}
