using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class CreateProvaRequest
{
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    public int? TurmaId { get; set; }

    [Required(ErrorMessage = "É necessário pelo menos uma questão")]
    [MinLength(1, ErrorMessage = "Deve haver pelo menos 1 questão")]
    public List<int> QuestoesIds { get; set; } = new();

    [Range(1, 600, ErrorMessage = "O tempo limite deve ser entre 1 e 600 minutos")]
    public int TempoLimiteMinutos { get; set; } = 180;

    /// <summary>Tema de redação da prova (opcional).</summary>
    public int? TemaRedacaoId { get; set; }
}