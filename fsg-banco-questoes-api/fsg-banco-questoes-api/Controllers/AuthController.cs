using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BancoQuestoes.Api.Data;
using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;
using BancoQuestoes.Api.Security;

namespace BancoQuestoes.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly BancoQuestoesContext _context;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(BancoQuestoesContext context, JwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    /// <param name="request">Dados do usuário a ser criado</param>
    /// <returns>ID do usuário criado</returns>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(CreatedResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<CreatedResponse>> Registrar([FromBody] RegistrarUsuarioRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Message = "Dados inválidos",
                Success = false
            });
        }

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        var emailJaCadastrado = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == emailNormalizado);

        if (emailJaCadastrado)
        {
            return BadRequest(new ApiResponse
            {
                Message = "Já existe um usuário cadastrado com esse e-mail",
                Success = false
            });
        }

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = emailNormalizado,
            SenhaHash = PasswordHasher.Hash(request.Senha),
            Tipo = request.Tipo
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var response = new CreatedResponse
        {
            Id = usuario.IdUsuario,
            Message = "Usuário registrado com sucesso"
        };

        return CreatedAtAction(nameof(Registrar), new { id = usuario.IdUsuario }, response);
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT
    /// </summary>
    /// <param name="request">Credenciais do usuário</param>
    /// <returns>Token JWT e dados básicos do usuário</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Message = "Dados inválidos",
                Success = false
            });
        }

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);

        if (usuario == null || !PasswordHasher.Verificar(request.Senha, usuario.SenhaHash))
        {
            return Unauthorized(new ApiResponse
            {
                Message = "E-mail ou senha inválidos",
                Success = false
            });
        }

        var (token, expiraEm) = _jwtTokenService.GerarToken(usuario);

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiraEm = expiraEm,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Tipo = usuario.Tipo
        });
    }
}
