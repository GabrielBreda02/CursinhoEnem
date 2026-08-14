using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BancoQuestoes.Api.Data;
using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;

namespace BancoQuestoes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProvasController : ControllerBase
{
    private readonly BancoQuestoesContext _context;

    public ProvasController(BancoQuestoesContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todas as provas
    /// </summary>
    /// <returns>Lista de provas</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProvaListResponse>), 200)]
    public async Task<ActionResult<List<ProvaListResponse>>> GetProvas()
    {
        var provas = await _context.Provas
            .Include(p => p.Questoes)
            .ToListAsync();
        
        var response = provas.Select(p => new ProvaListResponse
        {
            IdProva = p.IdProva,
            Titulo = p.Titulo,
            Disciplina = p.Disciplina,
            QuantidadeQuestoes = p.Questoes.Count
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Obtém uma prova específica por ID
    /// </summary>
    /// <param name="id">ID da prova</param>
    /// <returns>Detalhes da prova com questões</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProvaDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ProvaDetailResponse>> GetProva(int id)
    {
        var prova = await _context.Provas
            .Include(p => p.Questoes)
            .ThenInclude(q => q.Alternativas)
            .FirstOrDefaultAsync(p => p.IdProva == id);
        
        if (prova == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Prova com ID {id} não encontrada",
                Success = false
            });
        }
        
        var response = new ProvaDetailResponse
        {
            IdProva = prova.IdProva,
            Titulo = prova.Titulo,
            Disciplina = prova.Disciplina,
            Questoes = prova.Questoes.Select(q => new QuestaoDetailResponse
            {
                IdQuestao = q.IdQuestao,
                Titulo = q.Titulo,
                Disciplina = q.Disciplina,
                Assuntos = q.Assuntos,
                Alternativas = q.Alternativas.Select(a => new AlternativaResponse
                {
                    IdAlternativa = a.IdAlternativa,
                    Descricao = a.Descricao,
                    Correta = a.Correta
                }).ToList()
            }).ToList()
        };
        
        return Ok(response);
    }

    /// <summary>
    /// Cria uma nova prova
    /// </summary>
    /// <param name="request">Dados da prova a ser criada</param>
    /// <returns>ID da prova criada</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreatedResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<CreatedResponse>> CreateProva([FromBody] CreateProvaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Message = "Dados inválidos",
                Success = false
            });
        }
        
        // Verificar se todas as questões existem
        var questoesExistentes = await _context.Questoes
            .Where(q => request.QuestoesIds.Contains(q.IdQuestao))
            .Select(q => q.IdQuestao)
            .ToListAsync();
        
        var questoesInexistentes = request.QuestoesIds.Except(questoesExistentes).ToList();
        if (questoesInexistentes.Any())
        {
            return BadRequest(new ApiResponse
            {
                Message = $"Questões com IDs {string.Join(", ", questoesInexistentes)} não foram encontradas",
                Success = false
            });
        }
        
        var questoes = await _context.Questoes
            .Where(q => request.QuestoesIds.Contains(q.IdQuestao))
            .ToListAsync();
        
        var prova = new Prova
        {
            Titulo = request.Titulo,
            Disciplina = request.Disciplina,
            Questoes = questoes
        };
        
        _context.Provas.Add(prova);
        await _context.SaveChangesAsync();
        
        var response = new CreatedResponse
        {
            Id = prova.IdProva,
            Message = "Prova criada com sucesso"
        };
        
        return CreatedAtAction(nameof(GetProva), new { id = prova.IdProva }, response);
    }

    /// <summary>
    /// Atualiza uma prova existente
    /// </summary>
    /// <param name="id">ID da prova a ser atualizada</param>
    /// <param name="request">Novos dados da prova</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> UpdateProva(int id, [FromBody] CreateProvaRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Message = "Dados inválidos",
                Success = false
            });
        }
        
        var prova = await _context.Provas
            .Include(p => p.Questoes)
            .FirstOrDefaultAsync(p => p.IdProva == id);
        
        if (prova == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Prova com ID {id} não encontrada",
                Success = false
            });
        }
        
        // Verificar se todas as questões existem
        var questoesExistentes = await _context.Questoes
            .Where(q => request.QuestoesIds.Contains(q.IdQuestao))
            .Select(q => q.IdQuestao)
            .ToListAsync();
        
        var questoesInexistentes = request.QuestoesIds.Except(questoesExistentes).ToList();
        if (questoesInexistentes.Any())
        {
            return BadRequest(new ApiResponse
            {
                Message = $"Questões com IDs {string.Join(", ", questoesInexistentes)} não foram encontradas",
                Success = false
            });
        }
        
        var questoes = await _context.Questoes
            .Where(q => request.QuestoesIds.Contains(q.IdQuestao))
            .ToListAsync();
        
        prova.Titulo = request.Titulo;
        prova.Disciplina = request.Disciplina;
        prova.Questoes = questoes;
        
        await _context.SaveChangesAsync();
        
        return Ok(new ApiResponse
        {
            Message = "Prova atualizada com sucesso"
        });
    }

    /// <summary>
    /// Remove uma prova
    /// </summary>
    /// <param name="id">ID da prova a ser removida</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteProva(int id)
    {
        var prova = await _context.Provas.FindAsync(id);
        
        if (prova == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Prova com ID {id} não encontrada",
                Success = false
            });
        }
        
        _context.Provas.Remove(prova);
        await _context.SaveChangesAsync();
        
        return Ok(new ApiResponse
        {
            Message = "Prova removida com sucesso"
        });
    }
} 