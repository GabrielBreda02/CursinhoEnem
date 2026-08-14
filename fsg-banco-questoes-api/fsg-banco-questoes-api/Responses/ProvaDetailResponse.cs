namespace BancoQuestoes.Api.Responses;

public class ProvaDetailResponse
{
    public int IdProva { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
    public int TempoLimiteMinutos { get; set; }
    public int? TemaRedacaoId { get; set; }
    public string? TemaRedacaoTitulo { get; set; }
    public List<QuestaoDetailResponse> Questoes { get; set; } = new();
} 