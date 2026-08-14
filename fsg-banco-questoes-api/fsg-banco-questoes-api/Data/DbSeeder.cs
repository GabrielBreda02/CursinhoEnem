using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Security;

namespace BancoQuestoes.Api.Data;

/// <summary>
/// Popula o banco com dados de exemplo na primeira execução, para que o sistema
/// não seja entregue/avaliado totalmente vazio. Não faz nada se já existir usuário cadastrado.
/// </summary>
public static class DbSeeder
{
    public static void Seed(BancoQuestoesContext context)
    {
        if (context.Usuarios.Any())
        {
            return;
        }

        var usuario = new Usuario
        {
            Nome = "Professor Teste",
            Email = "professor@teste.com",
            SenhaHash = PasswordHasher.Hash("senha123")
        };
        context.Usuarios.Add(usuario);

        var questao1 = new Questao
        {
            Titulo = "Qual tag HTML é usada para criar um hyperlink?",
            Disciplina = "Programação Web",
            Assuntos = new List<string> { "HTML" },
            Alternativas = new List<Alternativa>
            {
                new() { Descricao = "<a>", Correta = true },
                new() { Descricao = "<link>", Correta = false },
                new() { Descricao = "<href>", Correta = false },
                new() { Descricao = "<nav>", Correta = false }
            }
        };

        var questao2 = new Questao
        {
            Titulo = "Qual propriedade CSS controla o espaçamento interno de um elemento?",
            Disciplina = "Programação Web",
            Assuntos = new List<string> { "CSS" },
            Alternativas = new List<Alternativa>
            {
                new() { Descricao = "margin", Correta = false },
                new() { Descricao = "padding", Correta = true },
                new() { Descricao = "border", Correta = false },
                new() { Descricao = "gap", Correta = false }
            }
        };

        var questao3 = new Questao
        {
            Titulo = "Em JavaScript, qual palavra-chave declara uma variável que não pode ser reatribuída?",
            Disciplina = "Programação Web",
            Assuntos = new List<string> { "JavaScript" },
            Alternativas = new List<Alternativa>
            {
                new() { Descricao = "var", Correta = false },
                new() { Descricao = "let", Correta = false },
                new() { Descricao = "const", Correta = true },
                new() { Descricao = "static", Correta = false }
            }
        };

        context.Questoes.AddRange(questao1, questao2, questao3);

        var prova = new Prova
        {
            Titulo = "Avaliação de Programação Web - 1º Bimestre",
            Disciplina = "Programação Web",
            Questoes = new List<Questao> { questao1, questao2, questao3 }
        };
        context.Provas.Add(prova);

        context.SaveChanges();
    }
}
