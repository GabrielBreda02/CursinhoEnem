using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BancoQuestoes.Api.Models;

public class Questao
{
    public int IdQuestao { get; set; }
    
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(500, ErrorMessage = "O título deve ter no máximo 500 caracteres")]
    public string Titulo { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A disciplina é obrigatória")]
    [StringLength(100, ErrorMessage = "A disciplina deve ter no máximo 100 caracteres")]
    public string Disciplina { get; set; } = string.Empty;
    
    public string AssuntosJson { get; set; } = "[]";

    public List<string> Assuntos
    {
        get => string.IsNullOrEmpty(AssuntosJson) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(AssuntosJson) ?? new List<string>();
        set => AssuntosJson = JsonSerializer.Serialize(value);
    }

    /// <summary>Uma das 4 áreas do ENEM — ver <see cref="AreaConhecimento"/>.</summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>URL relativa da imagem do enunciado (ex.: /uploads/questoes/xxx.png), se houver.</summary>
    public string? ImagemUrl { get; set; }

    /// <summary>Ano do ENEM de origem, se a questão vier de uma prova real. Nulo se for autoral.</summary>
    public int? Ano { get; set; }

    /// <summary>Ex.: "ENEM 2019" ou "Autoral".</summary>
    [StringLength(150)]
    public string? Fonte { get; set; }

    public List<Alternativa> Alternativas { get; set; } = new();

    public List<Prova> Provas { get; set; } = new();
}