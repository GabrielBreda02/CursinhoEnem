using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Models;

public class Prova
{
    public int IdProva { get; set; }
    
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A disciplina é obrigatória")]
    [StringLength(100, ErrorMessage = "A disciplina deve ter no máximo 100 caracteres")]
    public string Disciplina { get; set; } = string.Empty;
    
    public List<Questao> Questoes { get; set; } = new();
} 