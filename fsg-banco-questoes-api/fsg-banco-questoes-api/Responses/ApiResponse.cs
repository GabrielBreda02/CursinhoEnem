namespace BancoQuestoes.Api.Responses;

public class ApiResponse
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
}

public class ApiResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}

public class CreatedResponse
{
    public int Id { get; set; }
    public string Message { get; set; } = "Criado com sucesso";
} 