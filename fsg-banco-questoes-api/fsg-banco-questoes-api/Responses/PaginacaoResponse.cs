namespace BancoQuestoes.Api.Responses;

/// <summary>Envelope genérico de paginação, usado pelas listagens que suportam página/tamanhoPagina.</summary>
public class PaginacaoResponse<T>
{
    public List<T> Itens { get; set; } = new();
    public int PaginaAtual { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalItens { get; set; }
    public int TotalPaginas { get; set; }
}
