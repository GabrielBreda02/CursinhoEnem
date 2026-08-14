namespace BancoQuestoes.Api.Responses;

public class TemaRedacaoListResponse
{
    public int IdTemaRedacao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int? Ano { get; set; }
    public string? Fonte { get; set; }
}

public class TemaRedacaoDetailResponse
{
    public int IdTemaRedacao { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string TextoMotivador { get; set; } = string.Empty;
    public int? Ano { get; set; }
    public string? Fonte { get; set; }
}
