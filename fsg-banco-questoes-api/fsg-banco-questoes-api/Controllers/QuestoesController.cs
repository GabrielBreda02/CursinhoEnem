using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BancoQuestoes.Api.Data;
using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;

namespace BancoQuestoes.Api.Controllers;

[ApiController]
[Route("api/questoes")]
[Produces("application/json")]
public class QuestoesController : ControllerBase
{
    private readonly BancoQuestoesContext _context;

    public QuestoesController(BancoQuestoesContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todas as questões
    /// </summary>
    /// <param name="disciplina">Filtro por disciplina (opcional)</param>
    /// <param name="assunto">Filtro por assunto (opcional)</param>
    /// <param name="area">Filtro por área de conhecimento do ENEM (opcional)</param>
    /// <returns>Lista de questões</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuestaoListResponse>), 200)]
    public async Task<ActionResult<List<QuestaoListResponse>>> GetQuestoes(
        [FromQuery] string? disciplina = null,
        [FromQuery] string? assunto = null,
        [FromQuery] string? area = null)
    {
        var query = _context.Questoes.AsQueryable();

        if (!string.IsNullOrEmpty(disciplina))
        {
            query = query.Where(q => q.Disciplina.ToLower().Contains(disciplina.ToLower()));
        }

        if (!string.IsNullOrEmpty(assunto))
        {
            query = query.Where(q => q.AssuntosJson.Contains(assunto));
        }

        if (!string.IsNullOrEmpty(area))
        {
            query = query.Where(q => q.Area == area);
        }

        var questoes = await query.ToListAsync();

        var response = questoes.Select(q => new QuestaoListResponse
        {
            IdQuestao = q.IdQuestao,
            Titulo = q.Titulo,
            Disciplina = q.Disciplina,
            Assuntos = q.Assuntos,
            Area = q.Area,
            ImagemUrl = q.ImagemUrl,
            Ano = q.Ano,
            Fonte = q.Fonte
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Obtém uma questão específica por ID
    /// </summary>
    /// <param name="id">ID da questão</param>
    /// <returns>Detalhes da questão com alternativas</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(QuestaoDetailResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<QuestaoDetailResponse>> GetQuestao(int id)
    {
        var questao = await _context.Questoes
            .Include(q => q.Alternativas)
            .FirstOrDefaultAsync(q => q.IdQuestao == id);
        
        if (questao == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Questão com ID {id} não encontrada",
                Success = false
            });
        }
        
        var response = new QuestaoDetailResponse
        {
            IdQuestao = questao.IdQuestao,
            Titulo = questao.Titulo,
            Disciplina = questao.Disciplina,
            Assuntos = questao.Assuntos,
            Area = questao.Area,
            ImagemUrl = questao.ImagemUrl,
            Ano = questao.Ano,
            Fonte = questao.Fonte,
            Alternativas = questao.Alternativas.Select(a => new AlternativaResponse
            {
                IdAlternativa = a.IdAlternativa,
                Descricao = a.Descricao,
                Correta = a.Correta
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Cria uma nova questão
    /// </summary>
    /// <param name="request">Dados da questão a ser criada</param>
    /// <returns>ID da questão criada</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreatedResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<CreatedResponse>> CreateQuestao([FromBody] CreateQuestaoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Message = "Dados inválidos",
                Success = false
            });
        }
        
        // Verificar se há pelo menos uma alternativa correta
        if (!request.Alternativas.Any(a => a.Correta))
        {
            return BadRequest(new ApiResponse
            {
                Message = "Deve haver pelo menos uma alternativa correta",
                Success = false
            });
        }

        if (!AreaConhecimento.EhValida(request.Area))
        {
            return BadRequest(new ApiResponse
            {
                Message = "Área de conhecimento inválida",
                Success = false
            });
        }

        var questao = new Questao
        {
            Titulo = request.Titulo,
            Disciplina = request.Disciplina,
            Assuntos = request.Assuntos,
            Area = request.Area,
            ImagemUrl = request.ImagemUrl,
            Ano = request.Ano,
            Fonte = request.Fonte,
            Alternativas = request.Alternativas.Select(a => new Alternativa
            {
                Descricao = a.Descricao,
                Correta = a.Correta
            }).ToList()
        };

        _context.Questoes.Add(questao);
        await _context.SaveChangesAsync();
        
        var response = new CreatedResponse
        {
            Id = questao.IdQuestao,
            Message = "Questão criada com sucesso"
        };
        
        return CreatedAtAction(nameof(GetQuestao), new { id = questao.IdQuestao }, response);
    }

    /// <summary>
    /// Atualiza uma questão existente
    /// </summary>
    /// <param name="id">ID da questão a ser atualizada</param>
    /// <param name="request">Novos dados da questão</param>
    /// <returns>Resultado da operação</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> UpdateQuestao(int id, [FromBody] CreateQuestaoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Message = "Dados inválidos",
                Success = false
            });
        }
        
        var questao = await _context.Questoes
            .Include(q => q.Alternativas)
            .FirstOrDefaultAsync(q => q.IdQuestao == id);
        
        if (questao == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Questão com ID {id} não encontrada",
                Success = false
            });
        }
        
        // Verificar se há pelo menos uma alternativa correta
        if (!request.Alternativas.Any(a => a.Correta))
        {
            return BadRequest(new ApiResponse
            {
                Message = "Deve haver pelo menos uma alternativa correta",
                Success = false
            });
        }

        if (!AreaConhecimento.EhValida(request.Area))
        {
            return BadRequest(new ApiResponse
            {
                Message = "Área de conhecimento inválida",
                Success = false
            });
        }

        questao.Titulo = request.Titulo;
        questao.Disciplina = request.Disciplina;
        questao.Assuntos = request.Assuntos;
        questao.Area = request.Area;
        questao.ImagemUrl = request.ImagemUrl;
        questao.Ano = request.Ano;
        questao.Fonte = request.Fonte;

        // Remover alternativas existentes
        _context.Alternativas.RemoveRange(questao.Alternativas);
        
        // Adicionar novas alternativas
        questao.Alternativas = request.Alternativas.Select(a => new Alternativa
        {
            Descricao = a.Descricao,
            Correta = a.Correta,
            QuestaoId = id
        }).ToList();
        
        await _context.SaveChangesAsync();
        
        return Ok(new ApiResponse
        {
            Message = "Questão atualizada com sucesso"
        });
    }

    /// <summary>
    /// Remove uma questão
    /// </summary>
    /// <param name="id">ID da questão a ser removida</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> DeleteQuestao(int id)
    {
        var questao = await _context.Questoes.FindAsync(id);
        
        if (questao == null)
        {
            return NotFound(new ApiResponse
            {
                Message = $"Questão com ID {id} não encontrada",
                Success = false
            });
        }
        
        _context.Questoes.Remove(questao);
        await _context.SaveChangesAsync();
        
        return Ok(new ApiResponse
        {
            Message = "Questão removida com sucesso"
        });
    }
} 