using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;

namespace BancoQuestoes.Tests;

public class QuestoesIntegrationTests : IntegrationTestBase
{
    public QuestoesIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetQuestoes_ShouldReturnEmptyList_WhenNoQuestoes()
    {
        // Act
        var response = await _client.GetAsync("/api/questoes");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var questoes = await response.Content.ReadFromJsonAsync<List<QuestaoListResponse>>();
        Assert.NotNull(questoes);
        Assert.Empty(questoes);
    }

    [Fact]
    public async Task CreateQuestao_ShouldReturnCreated_WithValidData()
    {
        // Arrange
        var createRequest = new CreateQuestaoRequest
        {
            Titulo = "Qual é a capital do Brasil?",
            Disciplina = "Geografia",
            Assuntos = ["Capitais", "Brasil"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "São Paulo", Correta = false },
                new CreateAlternativaRequest { Descricao = "Rio de Janeiro", Correta = false },
                new CreateAlternativaRequest { Descricao = "Brasília", Correta = true },
                new CreateAlternativaRequest { Descricao = "Salvador", Correta = false }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/questoes", createRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdResponse = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        Assert.NotNull(createdResponse);
        Assert.True(createdResponse.Id > 0);
        Assert.Equal("Questão criada com sucesso", createdResponse.Message);
    }

    [Fact]
    public async Task CreateQuestao_ShouldReturnBadRequest_WithoutCorrectAlternative()
    {
        // Arrange
        var createRequest = new CreateQuestaoRequest
        {
            Titulo = "Questão sem alternativa correta",
            Disciplina = "Teste",
            Assuntos = ["Teste"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "Alternativa A", Correta = false },
                new CreateAlternativaRequest { Descricao = "Alternativa B", Correta = false }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/questoes", createRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(errorResponse);
        Assert.False(errorResponse.Success);
        Assert.Equal("Deve haver pelo menos uma alternativa correta", errorResponse.Message);
    }

    [Fact]
    public async Task GetQuestao_ShouldReturnQuestao_WhenExists()
    {
        // Arrange - Criar uma questão primeiro
        var createRequest = new CreateQuestaoRequest
        {
            Titulo = "Teste questão",
            Disciplina = "Teste",
            Assuntos = ["Teste"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var createResponse = await _client.PostAsJsonAsync("/api/questoes", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        // Act
        var response = await _client.GetAsync($"/api/questoes/{created!.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var questao = await response.Content.ReadFromJsonAsync<QuestaoDetailResponse>();
        Assert.NotNull(questao);
        Assert.Equal(createRequest.Titulo, questao.Titulo);
        Assert.Equal(createRequest.Disciplina, questao.Disciplina);
        Assert.Equal(2, questao.Alternativas.Count);
    }

    [Fact]
    public async Task GetQuestao_ShouldReturnNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.GetAsync("/api/questoes/999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(errorResponse);
        Assert.False(errorResponse.Success);
        Assert.Contains("não encontrada", errorResponse.Message);
    }

    [Fact]
    public async Task UpdateQuestao_ShouldReturnOk_WithValidData()
    {
        // Arrange - Criar uma questão primeiro
        var createRequest = new CreateQuestaoRequest
        {
            Titulo = "Questão original",
            Disciplina = "Original",
            Assuntos = ["Original"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var createResponse = await _client.PostAsJsonAsync("/api/questoes", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var updateRequest = new CreateQuestaoRequest
        {
            Titulo = "Questão atualizada",
            Disciplina = "Atualizada",
            Assuntos = ["Atualizada"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A atualizada", Correta = true },
                new CreateAlternativaRequest { Descricao = "B atualizada", Correta = false }
            ]
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/questoes/{created!.Id}", updateRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var updateResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(updateResponse);
        Assert.True(updateResponse.Success);
        Assert.Equal("Questão atualizada com sucesso", updateResponse.Message);
    }

    [Fact]
    public async Task DeleteQuestao_ShouldReturnOk_WhenExists()
    {
        // Arrange - Criar uma questão primeiro
        var createRequest = new CreateQuestaoRequest
        {
            Titulo = "Questão para deletar",
            Disciplina = "Teste",
            Assuntos = ["Teste"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var createResponse = await _client.PostAsJsonAsync("/api/questoes", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/questoes/{created!.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var deleteResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(deleteResponse);
        Assert.True(deleteResponse.Success);
        Assert.Equal("Questão removida com sucesso", deleteResponse.Message);
    }

    [Fact]
    public async Task GetQuestoes_ShouldFilterByDisciplina()
    {
        // Arrange - Criar questões de diferentes disciplinas
        var matematica = new CreateQuestaoRequest
        {
            Titulo = "Questão de Matemática",
            Disciplina = "Matemática",
            Assuntos = ["Álgebra"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var historia = new CreateQuestaoRequest
        {
            Titulo = "Questão de História",
            Disciplina = "História",
            Assuntos = ["Brasil Colonial"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        await _client.PostAsJsonAsync("/api/questoes", matematica);
        await _client.PostAsJsonAsync("/api/questoes", historia);

        // Act
        var response = await _client.GetAsync("/api/questoes?disciplina=Matemática");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var questoes = await response.Content.ReadFromJsonAsync<List<QuestaoListResponse>>();
        Assert.NotNull(questoes);
        Assert.Single(questoes);
        Assert.Equal("Matemática", questoes[0].Disciplina);
    }
}