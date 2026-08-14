using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BancoQuestoes.Api.Data;
using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;

namespace BancoQuestoes.Api.Controllers;

[ApiController]
[Route("api/temas-redacao")]
[Produces("application/json")]
public class TemasRedacaoController : ControllerBase
{
    private readonly BancoQuestoesContext _context;

    public TemasRedacaoController(BancoQuestoesContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todos os temas de redação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TemaRedacaoListResponse>), 200)]
    public async Task<ActionResult<List<TemaRedacaoListResponse>>> GetTemas()
    {
        var temas = await _context.TemasRedacao.ToListAsync();

        var response = temas.Select(t => new TemaRedacaoListResponse
        {
            IdTemaRedacao = t.IdTemaRedacao,
            Titulo = t.Titulo,
            Ano = t.Ano,
            Fonte = t.Fonte
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Obtém um tema de redação específico, com o texto motivador completo
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TemaRedacaoDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<TemaRedacaoDetailResponse>> GetTema(int id)
    {
        var tema = await _context.TemasRedacao.FindAsync(id);

        if (tema == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Tema de redação com ID {id} não encontrado",
                Success = false
            });
        }

        return Ok(new TemaRedacaoDetailResponse
        {
            IdTemaRedacao = tema.IdTemaRedacao,
            Titulo = tema.Titulo,
            TextoMotivador = tema.TextoMotivador,
            Ano = tema.Ano,
            Fonte = tema.Fonte
        });
    }

    /// <summary>
    /// Cria um novo tema de redação
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Professor")]
    [ProducesResponseType(typeof(CreatedResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<CreatedResponse>> CreateTema([FromBody] CreateTemaRedacaoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse { Message = "Dados inválidos", Success = false });
        }

        var tema = new TemaRedacao
        {
            Titulo = request.Titulo,
            TextoMotivador = request.TextoMotivador,
            Ano = request.Ano,
            Fonte = request.Fonte
        };

        _context.TemasRedacao.Add(tema);
        await _context.SaveChangesAsync();

        var response = new CreatedResponse
        {
            Id = tema.IdTemaRedacao,
            Message = "Tema de redação criado com sucesso"
        };

        return CreatedAtAction(nameof(GetTema), new { id = tema.IdTemaRedacao }, response);
    }

    /// <summary>
    /// Atualiza um tema de redação existente
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Professor")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> UpdateTema(int id, [FromBody] CreateTemaRedacaoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse { Message = "Dados inválidos", Success = false });
        }

        var tema = await _context.TemasRedacao.FindAsync(id);
        if (tema == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Tema de redação com ID {id} não encontrado",
                Success = false
            });
        }

        tema.Titulo = request.Titulo;
        tema.TextoMotivador = request.TextoMotivador;
        tema.Ano = request.Ano;
        tema.Fonte = request.Fonte;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Tema de redação atualizado com sucesso" });
    }

    /// <summary>
    /// Remove um tema de redação. Provas que usam esse tema passam a não ter tema (fica nulo).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Professor")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteTema(int id)
    {
        var tema = await _context.TemasRedacao.FindAsync(id);
        if (tema == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Tema de redação com ID {id} não encontrado",
                Success = false
            });
        }

        _context.TemasRedacao.Remove(tema);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Tema de redação removido com sucesso" });
    }
}
