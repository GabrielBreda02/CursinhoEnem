namespace BancoQuestoes.Api.Responses;

public class ProvaListResponse
{
    public int IdProva { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int? TurmaId { get; set; }
    public string? TurmaNome { get; set; }
    public int QuantidadeQuestoes { get; set; }
    public int TempoLimiteMinutos { get; set; }
    public int? TemaRedacaoId { get; set; }
    public string? TemaRedacaoTitulo { get; set; }
}
