namespace BancoQuestoes.Api.Responses;

/// <summary>Item da lista de redações que o professor pode corrigir.</summary>
public class RedacaoPendenteResponse
{
    public int IdTentativa { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string ProvaTitulo { get; set; } = string.Empty;
    public string TemaRedacaoTitulo { get; set; } = string.Empty;
    public DateTime FinalizadoEm { get; set; }
    public int? NotaRedacao { get; set; }
    public bool Corrigida { get; set; }
}

/// <summary>Detalhe de uma redação para o professor ler e corrigir.</summary>
public class RedacaoDetailResponse
{
    public int IdTentativa { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string ProvaTitulo { get; set; } = string.Empty;
    public string TemaRedacaoTitulo { get; set; } = string.Empty;
    public string TemaRedacaoTexto { get; set; } = string.Empty;
    public string? TextoRedacao { get; set; }
    public int? NotaComp1 { get; set; }
    public int? NotaComp2 { get; set; }
    public int? NotaComp3 { get; set; }
    public int? NotaComp4 { get; set; }
    public int? NotaComp5 { get; set; }
    public int? NotaRedacao { get; set; }
    public string? ComentarioRedacao { get; set; }
}
