using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class CreateTurmaRequest
{
    [Required(ErrorMessage = "O nome da turma é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;
}

public class MatricularAlunoRequest
{
    [Required(ErrorMessage = "O aluno é obrigatório")]
    public int AlunoId { get; set; }
}
