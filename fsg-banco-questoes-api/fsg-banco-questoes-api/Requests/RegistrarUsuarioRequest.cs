using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Requests;

public class RegistrarUsuarioRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    [StringLength(200, ErrorMessage = "O e-mail deve ter no máximo 200 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O tipo de usuário é obrigatório")]
    [RegularExpression("^(Professor|Aluno)$", ErrorMessage = "Tipo deve ser 'Professor' ou 'Aluno'")]
    public string Tipo { get; set; } = string.Empty;
}
