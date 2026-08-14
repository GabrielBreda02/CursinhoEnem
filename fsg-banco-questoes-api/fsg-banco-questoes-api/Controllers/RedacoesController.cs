using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BancoQuestoes.Api.Data;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;

namespace BancoQuestoes.Api.Controllers;

/// <summary>
/// Correção de redações pelo professor: listar tentativas finalizadas com redação pendente
/// ou já corrigida, ver o detalhe de uma e atribuir nota/comentário.
/// </summary>
[ApiController]
[Route("api/redacoes")]
[Produces("application/json")]
[Authorize(Roles = "Professor")]
public class RedacoesController : ControllerBase
{
    private readonly BancoQuestoesContext _context;

    public RedacoesController(BancoQuestoesContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista as tentativas finalizadas cuja prova tem redação associada.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RedacaoPendenteResponse>), 200)]
    public async Task<ActionResult<List<RedacaoPendenteResponse>>> GetRedacoes()
    {
        var tentativas = await _context.Tentativas
            .Include(t => t.Aluno)
            .Include(t => t.Prova)
                .ThenInclude(p => p.TemaRedacao)
            .Where(t => t.FinalizadoEm != null && t.Prova.TemaRedacaoId != null)
            .OrderByDescending(t => t.FinalizadoEm)
            .ToListAsync();

        var response = tentativas.Select(t => new RedacaoPendenteResponse
        {
            IdTentativa = t.IdTentativa,
            AlunoNome = t.Aluno.Nome,
            ProvaTitulo = t.Prova.Titulo,
            TemaRedacaoTitulo = t.Prova.TemaRedacao!.Titulo,
            FinalizadoEm = t.FinalizadoEm!.Value,
            NotaRedacao = t.NotaRedacao,
            Corrigida = t.NotaRedacao != null
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Detalhe de uma redação (texto motivador + texto do aluno) para o professor corrigir.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(RedacaoDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<RedacaoDetailResponse>> GetRedacao(int id)
    {
        var tentativa = await _context.Tentativas
            .Include(t => t.Aluno)
            .Include(t => t.Prova)
                .ThenInclude(p => p.TemaRedacao)
            .FirstOrDefaultAsync(t => t.IdTentativa == id);

        if (tentativa == null)
        {
            return NotFound(new ApiResponse { Message = "Tentativa não encontrada", Success = false });
        }

        if (tentativa.FinalizadoEm == null)
        {
            return BadRequest(new ApiResponse { Message = "Essa prova ainda não foi finalizada", Success = false });
        }

        if (tentativa.Prova.TemaRedacao == null)
        {
            return BadRequest(new ApiResponse { Message = "Essa prova não tem redação associada", Success = false });
        }

        return Ok(new RedacaoDetailResponse
        {
            IdTentativa = tentativa.IdTentativa,
            AlunoNome = tentativa.Aluno.Nome,
            ProvaTitulo = tentativa.Prova.Titulo,
            TemaRedacaoTitulo = tentativa.Prova.TemaRedacao.Titulo,
            TemaRedacaoTexto = tentativa.Prova.TemaRedacao.TextoMotivador,
            TextoRedacao = tentativa.TextoRedacao,
            NotaRedacao = tentativa.NotaRedacao,
            ComentarioRedacao = tentativa.ComentarioRedacao
        });
    }

    /// <summary>
    /// Salva a nota e o comentário da redação de uma tentativa.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> CorrigirRedacao(int id, [FromBody] CorrigirRedacaoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse { Message = "Dados inválidos", Success = false });
        }

        var tentativa = await _context.Tentativas
            .Include(t => t.Prova)
            .FirstOrDefaultAsync(t => t.IdTentativa == id);

        if (tentativa == null)
        {
            return NotFound(new ApiResponse { Message = "Tentativa não encontrada", Success = false });
        }

        if (tentativa.FinalizadoEm == null)
        {
            return BadRequest(new ApiResponse { Message = "Essa prova ainda não foi finalizada", Success = false });
        }

        if (tentativa.Prova.TemaRedacaoId == null)
        {
            return BadRequest(new ApiResponse { Message = "Essa prova não tem redação associada", Success = false });
        }

        tentativa.NotaRedacao = request.NotaRedacao;
        tentativa.ComentarioRedacao = request.ComentarioRedacao;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Redação corrigida com sucesso" });
    }
}
