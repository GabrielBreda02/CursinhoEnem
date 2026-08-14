using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Security;

namespace BancoQuestoes.Api.Data;

/// <summary>
/// Popula o banco com dados de exemplo na primeira execução, para que o sistema
/// não seja entregue/avaliado totalmente vazio. Não faz nada se já existir usuário cadastrado.
/// O acervo de questões/temas de redação reais do ENEM e a prova de exemplo são adicionados
/// separadamente (ver SeedEnem), depois que o conteúdo curado estiver pronto.
/// </summary>
public static class DbSeeder
{
    public static void Seed(BancoQuestoesContext context)
    {
        if (context.Usuarios.Any())
        {
            return;
        }

        context.Usuarios.Add(new Usuario
        {
            Nome = "Professor Teste",
            Email = "professor@teste.com",
            SenhaHash = PasswordHasher.Hash("senha123"),
            Tipo = "Professor"
        });

        context.Usuarios.Add(new Usuario
        {
            Nome = "Aluno Teste",
            Email = "aluno@teste.com",
            SenhaHash = PasswordHasher.Hash("senha123"),
            Tipo = "Aluno"
        });

        context.SaveChanges();
    }
}
