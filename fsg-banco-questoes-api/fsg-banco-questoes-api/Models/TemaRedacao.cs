using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Models;

public class TemaRedacao
{
    public int IdTemaRedacao { get; set; }

    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(300, ErrorMessage = "O título deve ter no máximo 300 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O texto motivador é obrigatório")]
    public string TextoMotivador { get; set; } = string.Empty;

    /// <summary>Ano do ENEM de origem, se o tema vier de uma prova real. Nulo se for autoral.</summary>
    public int? Ano { get; set; }

    /// <summary>Ex.: "ENEM 2019" ou "Autoral". Nulo/curto o suficiente pra citar a fonte.</summary>
    [StringLength(150)]
    public string? Fonte { get; set; }
}
