using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class CorrigirRedacaoRequest
{
    [Required(ErrorMessage = "A nota da competência 1 é obrigatória")]
    [Range(0, 200, ErrorMessage = "Cada competência vale de 0 a 200")]
    public int NotaComp1 { get; set; }

    [Required(ErrorMessage = "A nota da competência 2 é obrigatória")]
    [Range(0, 200, ErrorMessage = "Cada competência vale de 0 a 200")]
    public int NotaComp2 { get; set; }

    [Required(ErrorMessage = "A nota da competência 3 é obrigatória")]
    [Range(0, 200, ErrorMessage = "Cada competência vale de 0 a 200")]
    public int NotaComp3 { get; set; }

    [Required(ErrorMessage = "A nota da competência 4 é obrigatória")]
    [Range(0, 200, ErrorMessage = "Cada competência vale de 0 a 200")]
    public int NotaComp4 { get; set; }

    [Required(ErrorMessage = "A nota da competência 5 é obrigatória")]
    [Range(0, 200, ErrorMessage = "Cada competência vale de 0 a 200")]
    public int NotaComp5 { get; set; }

    public string? ComentarioRedacao { get; set; }
}
