using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class CorrigirRedacaoRequest
{
    [Required(ErrorMessage = "A nota da redação é obrigatória")]
    [Range(0, 1000, ErrorMessage = "A nota deve estar entre 0 e 1000")]
    public int NotaRedacao { get; set; }

    public string? ComentarioRedacao { get; set; }
}
