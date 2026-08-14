using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Models;

public class Alternativa
{
    public int IdAlternativa { get; set; }
    
    [Required(ErrorMessage = "A descrição da alternativa é obrigatória")]
    [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
    public string Descricao { get; set; } = string.Empty;
    
    public bool Correta { get; set; }
    
    public int QuestaoId { get; set; }
    public Questao Questao { get; set; } = null!;
} 