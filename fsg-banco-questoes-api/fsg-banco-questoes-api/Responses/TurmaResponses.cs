namespace BancoQuestoes.Api.Responses;

public class TurmaListResponse
{
    public int IdTurma { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeAlunos { get; set; }
}

public class TurmaDetailResponse
{
    public int IdTurma { get; set; }
    public string Nome { get; set; } = string.Empty;
    public List<AlunoResumoResponse> Alunos { get; set; } = new();
}

/// <summary>Um aluno cadastrado, com a turma atual (se houver) — usado pra matricular.</summary>
public class AlunoResumoResponse
{
    public int IdUsuario { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? TurmaId { get; set; }
    public string? TurmaNome { get; set; }
}
