namespace BancoQuestoes.Api.Responses;

public class QuestaoListResponse
{
    public int IdQuestao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public List<string> Assuntos { get; set; } = new();
    public string Area { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public int? Ano { get; set; }
    public string? Fonte { get; set; }
}
