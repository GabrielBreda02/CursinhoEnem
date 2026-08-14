namespace BancoQuestoes.Api.Models;

/// <summary>
/// Uma tentativa de um aluno fazendo uma prova: início, prazo, respostas dadas e,
/// ao final, a redação e a nota das questões objetivas.
/// </summary>
public class TentativaProva
{
    public int IdTentativa { get; set; }

    public int ProvaId { get; set; }
    public Prova Prova { get; set; } = null!;

    public int AlunoId { get; set; }
    public Usuario Aluno { get; set; } = null!;

    public DateTime IniciadoEm { get; set; }

    /// <summary>Prazo oficial (IniciadoEm + Prova.TempoLimiteMinutos). Quem manda é o servidor —
    /// o timer do front-end só exibe essa contagem, não decide quando o tempo acaba.</summary>
    public DateTime ExpiraEm { get; set; }

    public DateTime? FinalizadoEm { get; set; }

    public string? TextoRedacao { get; set; }

    /// <summary>Quantidade de questões objetivas corretas, calculada ao finalizar.</summary>
    public int? NotaObjetivas { get; set; }

    /// <summary>Nota da redação (0-1000, escala ENEM), atribuída pelo professor. Nula até ser corrigida.</summary>
    public int? NotaRedacao { get; set; }

    /// <summary>Comentário/feedback do professor sobre a redação. Opcional.</summary>
    public string? ComentarioRedacao { get; set; }

    public List<RespostaAluno> Respostas { get; set; } = new();
}
