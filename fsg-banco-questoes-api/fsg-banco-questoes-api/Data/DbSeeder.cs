using System.Text.Json;
using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Security;

namespace BancoQuestoes.Api.Data;

/// <summary>
/// Popula o banco na primeira execução: usuários de teste e o acervo curado de questões
/// reais do ENEM (lido de Data/Seed/questoes_enem.json). Cada etapa só roda se ainda não
/// existir o respectivo dado, então é seguro chamar em toda inicialização.
/// </summary>
public static class DbSeeder
{
    public static void Seed(BancoQuestoesContext context)
    {
        SeedUsuarios(context);
        SeedEnem(context);
        SeedProvaExemplo(context);
    }

    private static void SeedUsuarios(BancoQuestoesContext context)
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

    private static void SeedEnem(BancoQuestoesContext context)
    {
        if (context.Questoes.Any() || context.TemasRedacao.Any())
        {
            return;
        }

        var caminho = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "questoes_enem.json");
        if (!File.Exists(caminho))
        {
            return;
        }

        var json = File.ReadAllText(caminho);
        var dados = JsonSerializer.Deserialize<SeedData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (dados == null)
        {
            return;
        }

        foreach (var tema in dados.TemasRedacao)
        {
            context.TemasRedacao.Add(new TemaRedacao
            {
                Titulo = tema.Titulo,
                TextoMotivador = tema.TextoMotivador,
                Ano = tema.Ano,
                Fonte = tema.Fonte
            });
        }

        foreach (var questao in dados.Questoes)
        {
            context.Questoes.Add(new Questao
            {
                Titulo = questao.Titulo,
                Disciplina = questao.Disciplina,
                Area = questao.Area,
                Assuntos = questao.Assuntos,
                Ano = questao.Ano,
                Fonte = questao.Fonte,
                ImagemUrl = questao.ImagemUrl,
                Alternativas = questao.Alternativas.Select(a => new Alternativa
                {
                    Descricao = a.Descricao,
                    Correta = a.Correta
                }).ToList()
            });
        }

        context.SaveChanges();
    }

    private static void SeedProvaExemplo(BancoQuestoesContext context)
    {
        if (context.Provas.Any())
        {
            return;
        }

        // Uma questão de cada área, não o acervo inteiro — o acervo cresce (ver
        // Data/Seed/questoes_enem.json), mas essa prova de exemplo continua pequena de propósito.
        var questoes = context.Questoes
            .ToList()
            .GroupBy(q => q.Area)
            .Select(g => g.First())
            .ToList();

        if (questoes.Count == 0)
        {
            return;
        }

        var tema = context.TemasRedacao.FirstOrDefault();

        context.Provas.Add(new Prova
        {
            Titulo = "Simulado de exemplo — uma questão de cada área",
            Disciplina = "Geral",
            TempoLimiteMinutos = 60,
            TemaRedacaoId = tema?.IdTemaRedacao,
            Questoes = questoes
        });

        context.SaveChanges();
    }

    private class SeedData
    {
        public List<SeedTemaRedacao> TemasRedacao { get; set; } = new();
        public List<SeedQuestao> Questoes { get; set; } = new();
    }

    private class SeedTemaRedacao
    {
        public string Titulo { get; set; } = string.Empty;
        public string TextoMotivador { get; set; } = string.Empty;
        public int? Ano { get; set; }
        public string? Fonte { get; set; }
    }

    private class SeedQuestao
    {
        public string Titulo { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Disciplina { get; set; } = string.Empty;
        public List<string> Assuntos { get; set; } = new();
        public int? Ano { get; set; }
        public string? Fonte { get; set; }
        public string? ImagemUrl { get; set; }
        public List<SeedAlternativa> Alternativas { get; set; } = new();
    }

    private class SeedAlternativa
    {
        public string Descricao { get; set; } = string.Empty;
        public bool Correta { get; set; }
    }
}
