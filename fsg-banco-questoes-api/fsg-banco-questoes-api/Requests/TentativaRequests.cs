using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class IniciarTentativaRequest
{
    [Required(ErrorMessage = "O ID da prova é obrigatório")]
    public int ProvaId { get; set; }
}

public class ResponderTentativaRequest
{
    [Required(ErrorMessage = "O ID da questão é obrigatório")]
    public int QuestaoId { get; set; }

    [Required(ErrorMessage = "O ID da alternativa é obrigatório")]
    public int AlternativaId { get; set; }
}

public class FinalizarTentativaRequest
{
    public string? TextoRedacao { get; set; }
}
