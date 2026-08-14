namespace BancoQuestoes.Api.Responses;

public class ProvaListResponse
{
    public int IdProva { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
    public int QuantidadeQuestoes { get; set; }
    public int TempoLimiteMinutos { get; set; }
    public int? TemaRedacaoId { get; set; }
    public string? TemaRedacaoTitulo { get; set; }
}
