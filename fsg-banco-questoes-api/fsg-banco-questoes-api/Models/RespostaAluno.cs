namespace BancoQuestoes.Api.Models;

/// <summary>Resposta de um aluno a uma questão dentro de uma tentativa de prova.</summary>
public class RespostaAluno
{
    public int IdResposta { get; set; }

    public int TentativaId { get; set; }
    public TentativaProva Tentativa { get; set; } = null!;

    public int QuestaoId { get; set; }
    public Questao Questao { get; set; } = null!;

    /// <summary>Nula se o aluno deixou a questão em branco.</summary>
    public int? AlternativaSelecionadaId { get; set; }
    public Alternativa? AlternativaSelecionada { get; set; }
}
