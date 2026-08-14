namespace BancoQuestoes.Api.Responses;

public class QuestaoListResponse
{
    public int IdQuestao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
    public List<string> Assuntos { get; set; } = new();
} 