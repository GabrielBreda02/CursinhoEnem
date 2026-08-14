using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BancoQuestoes.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace BancoQuestoes.Api.Security;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var chave = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Configuração Jwt:Key não encontrada");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var minutos = int.TryParse(_configuration["Jwt:ExpiraMinutos"], out var m) ? m : 120;

        var expiraEm = DateTime.UtcNow.AddMinutes(minutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
