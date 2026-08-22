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
    private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    private readonly BancoQuestoesContext _context;
    private readonly IWebHostEnvironment _ambiente;

    public QuestoesController(BancoQuestoesContext context, IWebHostEnvironment ambiente)
    {
        _context = context;
        _ambiente = ambiente;
    }

    /// <summary>
    /// Faz upload de uma imagem para usar no enunciado de uma questão (gráficos, tirinhas, mapas etc.)
    /// </summary>
    /// <param name="arquivo">Arquivo de imagem (jpg, jpeg, png ou webp; até 5 MB)</param>
    /// <returns>URL relativa da imagem salva, para usar em ImagemUrl</returns>
    [HttpPost("upload-imagem")]
    [Authorize(Roles = "Professor")]
    [ProducesResponseType(typeof(UploadImagemResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<ActionResult<UploadImagemResponse>> UploadImagem(IFormFile? arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
        {
            return BadRequest(new ApiResponse { Message = "Nenhum arquivo enviado", Success = false });
        }

        if (arquivo.Length > TamanhoMaximoBytes)
        {
            return BadRequest(new ApiResponse { Message = "A imagem deve ter no máximo 5 MB", Success = false });
        }

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (!ExtensoesPermitidas.Contains(extensao))
        {
            return BadRequest(new ApiResponse
            {
                Message = "Formato inválido. Envie um arquivo .jpg, .jpeg, .png ou .webp",
                Success = false
            });
        }

        var pastaUploads = Path.Combine(_ambiente.WebRootPath, "uploads", "questoes");
        Directory.CreateDirectory(pastaUploads);

        var nomeArquivo = $"{Guid.NewGuid()}{extensao}";
        var caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

        using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
        {
            await arquivo.CopyToAsync(stream);
        }

        return Created(string.Empty, new UploadImagemResponse
        {
            ImagemUrl = $"/uploads/questoes/{nomeArquivo}"
        });
    }

    /// <summary>
    /// Lista as questões, com busca por palavra no enunciado e paginação
    /// </summary>
    /// <param name="busca">Filtro por palavra(s) contida(s) no enunciado da questão (opcional)</param>
    /// <param name="area">Filtro por área de conhecimento do ENEM (opcional)</param>
    /// <param name="pagina">Número da página, a partir de 1 (padrão: 1)</param>
    /// <param name="tamanhoPagina">Quantidade de questões por página (padrão: 20)</param>
    /// <returns>Página de questões</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<QuestaoListResponse>), 200)]
    public async Task<ActionResult<PaginacaoResponse<QuestaoListResponse>>> GetQuestoes(
        [FromQuery] string? busca = null,
        [FromQuery] string? area = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20)
    {
        pagina = Math.Max(pagina, 1);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var query = _context.Questoes.AsQueryable();

        if (!string.IsNullOrEmpty(busca))
        {
            query = query.Where(q => q.Titulo.ToLower().Contains(busca.ToLower()));
        }

        if (!string.IsNullOrEmpty(area))
        {
            query = query.Where(q => q.Area == area);
        }

        var totalItens = await query.CountAsync();

        var questoes = await query
            .OrderBy(q => q.IdQuestao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        var response = new PaginacaoResponse<QuestaoListResponse>
        {
            Itens = questoes.Select(q => new QuestaoListResponse
            {
                IdQuestao = q.IdQuestao,
                Titulo = q.Titulo,
                Assuntos = q.Assuntos,
                Area = q.Area,
                ImagemUrl = q.ImagemUrl,
                Ano = q.Ano,
                Fonte = q.Fonte
            }).ToList(),
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalItens = totalItens,
            TotalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina)
        };

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
    [Authorize(Roles = "Professor")]
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
    [Authorize(Roles = "Professor")]
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
    [Authorize(Roles = "Professor")]
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