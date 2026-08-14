namespace BancoQuestoes.Api.Models;

/// <summary>
/// As 4 áreas de conhecimento do ENEM. Usada para validar Questao.Area no back-end.
/// </summary>
public static class AreaConhecimento
{
    public const string Linguagens = "Linguagens, Códigos e suas Tecnologias";
    public const string CienciasHumanas = "Ciências Humanas e suas Tecnologias";
    public const string CienciasNatureza = "Ciências da Natureza e suas Tecnologias";
    public const string Matematica = "Matemática e suas Tecnologias";

    public static readonly string[] Todas =
    {
        Linguagens, CienciasHumanas, CienciasNatureza, Matematica
    };

    public static bool EhValida(string? area) => !string.IsNullOrEmpty(area) && Todas.Contains(area);
}
