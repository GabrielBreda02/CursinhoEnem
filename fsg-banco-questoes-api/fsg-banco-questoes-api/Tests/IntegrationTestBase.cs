using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BancoQuestoes.Api.Data;

namespace BancoQuestoes.Tests;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime, IDisposable
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly string _dbName;
    
    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        _dbName = Guid.NewGuid().ToString();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove o contexto existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<BancoQuestoesContext>));
                
                if (descriptor != null)
                    services.Remove(descriptor);
                
                // Adiciona contexto em memória para testes
                services.AddDbContext<BancoQuestoesContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        });
        
        _client = _factory.CreateClient();
        
        // Inicializa o banco de dados
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BancoQuestoesContext>();
        context.Database.EnsureCreated();
    }

    // Questoes/Provas/TemasRedacao exigem [Authorize(Roles = "Professor")] nos endpoints de
    // escrita — sem logar antes, todo POST/PUT/DELETE nos testes cai em 401. xUnit não
    // suporta construtor assíncrono, por isso o login entra aqui via IAsyncLifetime.
    public async Task InitializeAsync()
    {
        await _client.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest
        {
            Nome = "Professor Teste",
            Email = "professor.tests@teste.com",
            Senha = "senha123",
            Tipo = "Professor"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "professor.tests@teste.com",
            Senha = "senha123"
        });
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BancoQuestoesContext>();
        context.Database.EnsureDeleted();
        _client?.Dispose();
    }
} 