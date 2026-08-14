using System.ComponentModel.DataAnnotations;

namespace BancoQuestoes.Api.Models;

public class Usuario
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [StringLength(200, ErrorMessage = "O e-mail deve ter no máximo 200 caracteres")]
    public string Email { get; set; } = string.Empty;

    public string SenhaHash { get; set; } = string.Empty;
}
