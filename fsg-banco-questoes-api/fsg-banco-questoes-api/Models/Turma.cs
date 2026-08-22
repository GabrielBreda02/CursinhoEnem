using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Models;

/// <summary>Um grupo de alunos que o professor pode atribuir provas em conjunto.</summary>
public class Turma
{
    public int IdTurma { get; set; }

    [Required(ErrorMessage = "O nome da turma é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    public List<Usuario> Alunos { get; set; } = new();

    public List<Prova> Provas { get; set; } = new();
}
