using System.Security.Cryptography;

namespace BancoQuestoes.Api.Security;

/// <summary>
/// Hash de senha com PBKDF2 (salt aleatório por usuário + muitas iterações),
/// evitando armazenar senha em texto puro ou usar hash reversível/rápido (ex.: MD5/SHA1 sozinho).
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string hashArmazenado)
    {
        var partes = hashArmazenado.Split('.', 3);
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(partes[1]);
        var hashEsperado = Convert.FromBase64String(partes[2]);
        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iterations, HashAlgorithmName.SHA256, hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}
