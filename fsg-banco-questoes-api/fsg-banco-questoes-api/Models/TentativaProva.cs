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

    /// <summary>As 5 competências do ENEM, cada uma de 0 a 200 em múltiplos de 20. Nulas até a
    /// redação ser corrigida pelo professor.</summary>
    public int? NotaComp1 { get; set; }
    public int? NotaComp2 { get; set; }
    public int? NotaComp3 { get; set; }
    public int? NotaComp4 { get; set; }
    public int? NotaComp5 { get; set; }

    /// <summary>Nota final da redação (soma das 5 competências, 0-1000, escala ENEM). Calculada,
    /// não gravada — só existe quando as 5 competências estão preenchidas.</summary>
    public int? NotaRedacao =>
        NotaComp1.HasValue && NotaComp2.HasValue && NotaComp3.HasValue && NotaComp4.HasValue && NotaComp5.HasValue
            ? NotaComp1 + NotaComp2 + NotaComp3 + NotaComp4 + NotaComp5
            : null;

    /// <summary>Comentário/feedback do professor sobre a redação. Opcional.</summary>
    public string? ComentarioRedacao { get; set; }

    public List<RespostaAluno> Respostas { get; set; } = new();
}
