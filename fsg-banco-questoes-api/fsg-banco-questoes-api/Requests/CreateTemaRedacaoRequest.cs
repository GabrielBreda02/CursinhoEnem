using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class CreateTemaRedacaoRequest
{
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(300, ErrorMessage = "O título deve ter no máximo 300 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O texto motivador é obrigatório")]
    public string TextoMotivador { get; set; } = string.Empty;

    public int? Ano { get; set; }

    [StringLength(150, ErrorMessage = "A fonte deve ter no máximo 150 caracteres")]
    public string? Fonte { get; set; }
}
