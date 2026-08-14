using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BancoQuestoes.Api.Data;

namespace BancoQuestoes.Tests;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly string _dbName;
    
    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        _dbName = Guid.NewGuid().ToString();
        _factory = factory.WithWebHostBuilder(builder =>
        {
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
    
    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BancoQuestoesContext>();
        context.Database.EnsureDeleted();
        _client?.Dispose();
    }
} 