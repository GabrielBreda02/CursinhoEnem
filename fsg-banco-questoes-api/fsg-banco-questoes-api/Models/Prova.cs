using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Models;

public class Prova
{
    public int IdProva { get; set; }
    
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Turma à qual a prova foi atribuída — nula significa aberta para qualquer aluno.</summary>
    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }

    /// <summary>Duração da prova em minutos — define o prazo de cada tentativa.</summary>
    public int TempoLimiteMinutos { get; set; } = 180;

    /// <summary>Tema de redação associado (opcional).</summary>
    public int? TemaRedacaoId { get; set; }
    public TemaRedacao? TemaRedacao { get; set; }

    public List<Questao> Questoes { get; set; } = new();
}