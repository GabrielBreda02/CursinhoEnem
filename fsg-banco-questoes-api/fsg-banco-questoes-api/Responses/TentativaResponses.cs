namespace BancoQuestoes.Api.Responses;

/// <summary>Alternativa exibida durante a prova — sem revelar qual é a correta.</summary>
public class AlternativaProvaResponse
{
    public int IdAlternativa { get; set; }
    public string Descricao { get; set; } = string.Empty;
}

/// <summary>Questão exibida durante a prova — sem revelar qual alternativa é a correta.</summary>
public class QuestaoProvaResponse
{
    public int IdQuestao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public List<AlternativaProvaResponse> Alternativas { get; set; } = new();
}

public class TentativaIniciadaResponse
{
    public int IdTentativa { get; set; }
    public DateTime IniciadoEm { get; set; }
    public DateTime ExpiraEm { get; set; }
    public int TempoLimiteMinutos { get; set; }
    public string ProvaTitulo { get; set; } = string.Empty;
    public string? TemaRedacaoTitulo { get; set; }
    public string? TemaRedacaoTexto { get; set; }
    public List<QuestaoProvaResponse> Questoes { get; set; } = new();

    /// <summary>Respostas já salvas, quando essa tentativa retoma uma prova em andamento.</summary>
    public List<RespostaSalvaResponse> RespostasSalvas { get; set; } = new();
}

public class RespostaSalvaResponse
{
    public int QuestaoId { get; set; }
    public int AlternativaId { get; set; }
}

public class FinalizarTentativaResponse
{
    public int IdTentativa { get; set; }
    public int NotaObjetivas { get; set; }
    public int TotalQuestoes { get; set; }
}

/// <summary>Uma alternativa no resultado final — aqui sim mostra qual é a correta.</summary>
public class AlternativaResultadoResponse
{
    public int IdAlternativa { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Correta { get; set; }
}

public class QuestaoResultadoResponse
{
    public int IdQuestao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? ImagemUrl { get; set; }
    public int? AlternativaSelecionadaId { get; set; }
    public bool RespondidaCorretamente { get; set; }
    public List<AlternativaResultadoResponse> Alternativas { get; set; } = new();
}

public class ResultadoTentativaResponse
{
    public int IdTentativa { get; set; }
    public string ProvaTitulo { get; set; } = string.Empty;
    public DateTime IniciadoEm { get; set; }
    public DateTime? FinalizadoEm { get; set; }
    public int NotaObjetivas { get; set; }
    public int TotalQuestoes { get; set; }
    public string? TextoRedacao { get; set; }
    public string? TemaRedacaoTitulo { get; set; }
    public int? NotaComp1 { get; set; }
    public int? NotaComp2 { get; set; }
    public int? NotaComp3 { get; set; }
    public int? NotaComp4 { get; set; }
    public int? NotaComp5 { get; set; }
    public int? NotaRedacao { get; set; }
    public string? ComentarioRedacao { get; set; }
    public List<QuestaoResultadoResponse> Questoes { get; set; } = new();
}

public class TentativaResumoResponse
{
    public int IdTentativa { get; set; }
    public int ProvaId { get; set; }
    public string ProvaTitulo { get; set; } = string.Empty;
    public DateTime IniciadoEm { get; set; }
    public DateTime? FinalizadoEm { get; set; }
    public int? NotaObjetivas { get; set; }
    public int TotalQuestoes { get; set; }
}
