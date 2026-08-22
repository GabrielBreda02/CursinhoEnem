using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Models;

public class Prova
{
    public int IdProva { get; set; }
    
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Número/identificador da turma à qual a prova se destina (opcional).</summary>
    [StringLength(50, ErrorMessage = "A turma deve ter no máximo 50 caracteres")]
    public string? Turma { get; set; }

    /// <summary>Duração da prova em minutos — define o prazo de cada tentativa.</summary>
    public int TempoLimiteMinutos { get; set; } = 180;

    /// <summary>Tema de redação associado (opcional).</summary>
    public int? TemaRedacaoId { get; set; }
    public TemaRedacao? TemaRedacao { get; set; }

    public List<Questao> Questoes { get; set; } = new();
}