using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class CreateQuestaoRequest
{
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(500, ErrorMessage = "O título deve ter no máximo 500 caracteres")]
    public string Titulo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A disciplina é obrigatória")]
    [StringLength(100, ErrorMessage = "A disciplina deve ter no máximo 100 caracteres")]
    public string Disciplina { get; set; } = string.Empty;
    
    public List<string> Assuntos { get; set; } = new();
    
    [Required(ErrorMessage = "É necessário pelo menos uma alternativa")]
    [MinLength(2, ErrorMessage = "Deve haver pelo menos 2 alternativas")]
    public List<CreateAlternativaRequest> Alternativas { get; set; } = new();
}

public class CreateAlternativaRequest
{
    [Required(ErrorMessage = "A descrição da alternativa é obrigatória")]
    [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
    public string Descricao { get; set; } = string.Empty;
    
    public bool Correta { get; set; }
} 