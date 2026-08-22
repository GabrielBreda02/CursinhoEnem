using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BancoQuestoes.Api.Data;
using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;

namespace BancoQuestoes.Api.Controllers;

/// <summary>
/// Turmas: grupos de alunos que o professor monta pra atribuir provas em conjunto.
/// </summary>
[ApiController]
[Route("api/turmas")]
[Produces("application/json")]
[Authorize(Roles = "Professor")]
public class TurmasController : ControllerBase
{
    private readonly BancoQuestoesContext _context;

    public TurmasController(BancoQuestoesContext context)
    {
        _context = context;
    }

    /// <summary>Lista todas as turmas, com a quantidade de alunos matriculados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TurmaListResponse>), 200)]
    public async Task<ActionResult<List<TurmaListResponse>>> GetTurmas()
    {
        var turmas = await _context.Turmas
            .Include(t => t.Alunos)
            .OrderBy(t => t.Nome)
            .ToListAsync();

        var response = turmas.Select(t => new TurmaListResponse
        {
            IdTurma = t.IdTurma,
            Nome = t.Nome,
            QuantidadeAlunos = t.Alunos.Count
        }).ToList();

        return Ok(response);
    }

    /// <summary>Lista todos os alunos cadastrados, com a turma atual de cada um (se houver) —
    /// usado pra montar o seletor de matrícula.</summary>
    [HttpGet("alunos")]
    [ProducesResponseType(typeof(List<AlunoResumoResponse>), 200)]
    public async Task<ActionResult<List<AlunoResumoResponse>>> GetAlunos()
    {
        var alunos = await _context.Usuarios
            .Include(u => u.Turma)
            .Where(u => u.Tipo == "Aluno")
            .OrderBy(u => u.Nome)
            .ToListAsync();

        var response = alunos.Select(a => new AlunoResumoResponse
        {
            IdUsuario = a.IdUsuario,
            Nome = a.Nome,
            Email = a.Email,
            TurmaId = a.TurmaId,
            TurmaNome = a.Turma?.Nome
        }).ToList();

        return Ok(response);
    }

    /// <summary>Detalhe de uma turma, com a lista de alunos matriculados.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TurmaDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<TurmaDetailResponse>> GetTurma(int id)
    {
        var turma = await _context.Turmas
            .Include(t => t.Alunos)
            .FirstOrDefaultAsync(t => t.IdTurma == id);

        if (turma == null)
        {
            return NotFound(new ApiResponse { Message = $"Turma com ID {id} não encontrada", Success = false });
        }

        return Ok(new TurmaDetailResponse
        {
            IdTurma = turma.IdTurma,
            Nome = turma.Nome,
            Alunos = turma.Alunos.Select(a => new AlunoResumoResponse
            {
                IdUsuario = a.IdUsuario,
                Nome = a.Nome,
                Email = a.Email,
                TurmaId = a.TurmaId,
                TurmaNome = turma.Nome
            }).OrderBy(a => a.Nome).ToList()
        });
    }

    /// <summary>Cria uma nova turma.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreatedResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<CreatedResponse>> CreateTurma([FromBody] CreateTurmaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse { Message = "Dados inválidos", Success = false });
        }

        var turma = new Turma { Nome = request.Nome.Trim() };

        _context.Turmas.Add(turma);
        await _context.SaveChangesAsync();

        var response = new CreatedResponse { Id = turma.IdTurma, Message = "Turma criada com sucesso" };
        return CreatedAtAction(nameof(GetTurma), new { id = turma.IdTurma }, response);
    }

    /// <summary>Renomeia uma turma.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> UpdateTurma(int id, [FromBody] CreateTurmaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse { Message = "Dados inválidos", Success = false });
        }

        var turma = await _context.Turmas.FindAsync(id);
        if (turma == null)
        {
            return NotFound(new ApiResponse { Message = $"Turma com ID {id} não encontrada", Success = false });
        }

        turma.Nome = request.Nome.Trim();
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Turma atualizada com sucesso" });
    }

    /// <summary>Remove uma turma. Alunos e provas ligados a ela ficam sem turma (não são apagados).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteTurma(int id)
    {
        var turma = await _context.Turmas.FindAsync(id);
        if (turma == null)
        {
            return NotFound(new ApiResponse { Message = $"Turma com ID {id} não encontrada", Success = false });
        }

        _context.Turmas.Remove(turma);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Turma removida com sucesso" });
    }

    /// <summary>Matricula um aluno na turma (se ele já estava em outra, é transferido).</summary>
    [HttpPost("{id:int}/alunos")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> MatricularAluno(int id, [FromBody] MatricularAlunoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse { Message = "Dados inválidos", Success = false });
        }

        var turma = await _context.Turmas.FindAsync(id);
        if (turma == null)
        {
            return NotFound(new ApiResponse { Message = $"Turma com ID {id} não encontrada", Success = false });
        }

        var aluno = await _context.Usuarios.FindAsync(request.AlunoId);
        if (aluno == null || aluno.Tipo != "Aluno")
        {
            return BadRequest(new ApiResponse { Message = "Aluno não encontrado", Success = false });
        }

        aluno.TurmaId = turma.IdTurma;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Aluno matriculado com sucesso" });
    }

    /// <summary>Remove um aluno da turma.</summary>
    [HttpDelete("{id:int}/alunos/{alunoId:int}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DesmatricularAluno(int id, int alunoId)
    {
        var aluno = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == alunoId && u.TurmaId == id);
        if (aluno == null)
        {
            return NotFound(new ApiResponse { Message = "Aluno não encontrado nessa turma", Success = false });
        }

        aluno.TurmaId = null;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Aluno removido da turma" });
    }
}
