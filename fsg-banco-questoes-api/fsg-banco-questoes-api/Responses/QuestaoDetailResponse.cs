namespace BancoQuestoes.Api.Responses;

public class QuestaoDetailResponse
{
    public int IdQuestao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
    public List<string> Assuntos { get; set; } = new();
    public List<AlternativaResponse> Alternativas { get; set; } = new();
}

public class AlternativaResponse
{
    public int IdAlternativa { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Correta { get; set; }
} 