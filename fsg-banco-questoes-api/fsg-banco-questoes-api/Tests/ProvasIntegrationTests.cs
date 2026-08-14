using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;
using Xunit;

namespace BancoQuestoes.Tests;

public class ProvasIntegrationTests : IntegrationTestBase
{
    public ProvasIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProvas_ShouldReturnEmptyList_WhenNoProvas()
    {
        // Act
        var response = await _client.GetAsync("/api/provas");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var provas = await response.Content.ReadFromJsonAsync<List<ProvaListResponse>>();
        Assert.NotNull(provas);
        Assert.Empty(provas);
    }

    [Fact]
    public async Task CreateProva_ShouldReturnCreated_WithValidData()
    {
        // Arrange - Criar uma questão primeiro
        var questaoRequest = new CreateQuestaoRequest
        {
            Titulo = "Questão de teste",
            Disciplina = "Matemática",
            Assuntos = ["Álgebra"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var questaoResponse = await _client.PostAsJsonAsync("/api/questoes", questaoRequest);
        var questaoCreated = await questaoResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var provaRequest = new CreateProvaRequest
        {
            Titulo = "Prova de Matemática",
            Disciplina = "Matemática",
            QuestoesIds = [questaoCreated!.Id]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/provas", provaRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdResponse = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        Assert.NotNull(createdResponse);
        Assert.True(createdResponse.Id > 0);
        Assert.Equal("Prova criada com sucesso", createdResponse.Message);
    }

    [Fact]
    public async Task CreateProva_ShouldReturnBadRequest_WithInexistentQuestao()
    {
        // Arrange
        var provaRequest = new CreateProvaRequest
        {
            Titulo = "Prova com questão inexistente",
            Disciplina = "Teste",
            QuestoesIds = [999] // ID que não existe
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/provas", provaRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(errorResponse);
        Assert.False(errorResponse.Success);
        Assert.Contains("não foram encontradas", errorResponse.Message);
    }

    [Fact]
    public async Task GetProva_ShouldReturnProva_WhenExists()
    {
        // Arrange - Criar uma questão e prova primeiro
        var questaoRequest = new CreateQuestaoRequest
        {
            Titulo = "Questão de teste",
            Disciplina = "História",
            Assuntos = ["Brasil"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var questaoResponse = await _client.PostAsJsonAsync("/api/questoes", questaoRequest);
        var questaoCreated = await questaoResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var provaRequest = new CreateProvaRequest
        {
            Titulo = "Prova de História",
            Disciplina = "História",
            QuestoesIds = [questaoCreated!.Id]
        };

        var provaResponse = await _client.PostAsJsonAsync("/api/provas", provaRequest);
        var provaCreated = await provaResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        // Act
        var response = await _client.GetAsync($"/api/provas/{provaCreated!.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var prova = await response.Content.ReadFromJsonAsync<ProvaDetailResponse>();
        Assert.NotNull(prova);
        Assert.Equal(provaRequest.Titulo, prova.Titulo);
        Assert.Equal(provaRequest.Disciplina, prova.Disciplina);
        Assert.Single(prova.Questoes);
    }

    [Fact]
    public async Task GetProva_ShouldReturnNotFound_WhenNotExists()
    {
        // Act
        var response = await _client.GetAsync("/api/provas/999");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(errorResponse);
        Assert.False(errorResponse.Success);
        Assert.Contains("não encontrada", errorResponse.Message);
    }

    [Fact]
    public async Task UpdateProva_ShouldReturnOk_WithValidData()
    {
        // Arrange - Criar questões e prova primeiro
        var questao1Request = new CreateQuestaoRequest
        {
            Titulo = "Questão 1",
            Disciplina = "Física",
            Assuntos = ["Mecânica"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var questao2Request = new CreateQuestaoRequest
        {
            Titulo = "Questão 2",
            Disciplina = "Física",
            Assuntos = ["Eletricidade"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var questao1Response = await _client.PostAsJsonAsync("/api/questoes", questao1Request);
        var questao1Created = await questao1Response.Content.ReadFromJsonAsync<CreatedResponse>();

        var questao2Response = await _client.PostAsJsonAsync("/api/questoes", questao2Request);
        var questao2Created = await questao2Response.Content.ReadFromJsonAsync<CreatedResponse>();

        var provaRequest = new CreateProvaRequest
        {
            Titulo = "Prova Original",
            Disciplina = "Física",
            QuestoesIds = [questao1Created!.Id]
        };

        var provaResponse = await _client.PostAsJsonAsync("/api/provas", provaRequest);
        var provaCreated = await provaResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var updateRequest = new CreateProvaRequest
        {
            Titulo = "Prova Atualizada",
            Disciplina = "Física",
            QuestoesIds = [questao1Created.Id, questao2Created!.Id]
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/provas/{provaCreated!.Id}", updateRequest);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var updateResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(updateResponse);
        Assert.True(updateResponse.Success);
        Assert.Equal("Prova atualizada com sucesso", updateResponse.Message);
    }

    [Fact]
    public async Task DeleteProva_ShouldReturnOk_WhenExists()
    {
        // Arrange - Criar uma questão e prova primeiro
        var questaoRequest = new CreateQuestaoRequest
        {
            Titulo = "Questão para prova que será deletada",
            Disciplina = "Química",
            Assuntos = ["Átomos"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var questaoResponse = await _client.PostAsJsonAsync("/api/questoes", questaoRequest);
        var questaoCreated = await questaoResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        var provaRequest = new CreateProvaRequest
        {
            Titulo = "Prova para deletar",
            Disciplina = "Química",
            QuestoesIds = [questaoCreated!.Id]
        };

        var provaResponse = await _client.PostAsJsonAsync("/api/provas", provaRequest);
        var provaCreated = await provaResponse.Content.ReadFromJsonAsync<CreatedResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/provas/{provaCreated!.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var deleteResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(deleteResponse);
        Assert.True(deleteResponse.Success);
        Assert.Equal("Prova removida com sucesso", deleteResponse.Message);
    }

    [Fact]
    public async Task GetProvas_ShouldReturnListWithQuantidades()
    {
        // Arrange - Criar questões e prova
        var questao1Request = new CreateQuestaoRequest
        {
            Titulo = "Questão 1",
            Disciplina = "Geografia",
            Assuntos = ["Continentes"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var questao2Request = new CreateQuestaoRequest
        {
            Titulo = "Questão 2",
            Disciplina = "Geografia",
            Assuntos = ["Países"],
            Alternativas = 
            [
                new CreateAlternativaRequest { Descricao = "A", Correta = true },
                new CreateAlternativaRequest { Descricao = "B", Correta = false }
            ]
        };

        var questao1Response = await _client.PostAsJsonAsync("/api/questoes", questao1Request);
        var questao1Created = await questao1Response.Content.ReadFromJsonAsync<CreatedResponse>();

        var questao2Response = await _client.PostAsJsonAsync("/api/questoes", questao2Request);
        var questao2Created = await questao2Response.Content.ReadFromJsonAsync<CreatedResponse>();

        var provaRequest = new CreateProvaRequest
        {
            Titulo = "Prova de Geografia",
            Disciplina = "Geografia",
            QuestoesIds = [questao1Created!.Id, questao2Created!.Id]
        };

        await _client.PostAsJsonAsync("/api/provas", provaRequest);

        // Act
        var response = await _client.GetAsync("/api/provas");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var provas = await response.Content.ReadFromJsonAsync<List<ProvaListResponse>>();
        Assert.NotNull(provas);
        Assert.Single(provas);
        Assert.Equal(2, provas[0].QuantidadeQuestoes);
        Assert.Equal("Geografia", provas[0].Disciplina);
    }
}