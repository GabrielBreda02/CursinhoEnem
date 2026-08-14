using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class CreateProvaRequest
{
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A disciplina é obrigatória")]
    [StringLength(100, ErrorMessage = "A disciplina deve ter no máximo 100 caracteres")]
    public string Disciplina { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "É necessário pelo menos uma questão")]
    [MinLength(1, ErrorMessage = "Deve haver pelo menos 1 questão")]
    public List<int> QuestoesIds { get; set; } = new();
} 