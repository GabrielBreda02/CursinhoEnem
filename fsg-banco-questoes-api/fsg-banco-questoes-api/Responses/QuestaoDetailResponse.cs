namespace BancoQuestoes.Api.Responses;

public class QuestaoDetailResponse
{
    public int IdQuestao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public List<string> Assuntos { get; set; } = new();
    public string Area { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public int? Ano { get; set; }
    public string? Fonte { get; set; }
    public List<AlternativaResponse> Alternativas { get; set; } = new();
}

public class AlternativaResponse
{
    public int IdAlternativa { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Correta { get; set; }
} 