using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BancoQuestoes.Api.Data;
using BancoQuestoes.Api.Models;
using BancoQuestoes.Api.Requests;
using BancoQuestoes.Api.Responses;

namespace BancoQuestoes.Api.Controllers;

/// <summary>
/// Fluxo do aluno fazendo uma prova: iniciar (com prazo definido pelo servidor), responder
/// questão a questão, finalizar com a redação e consultar o resultado depois.
/// </summary>
[ApiController]
[Route("api/tentativas")]
[Produces("application/json")]
[Authorize(Roles = "Aluno")]
public class TentativasController : ControllerBase
{
    private readonly BancoQuestoesContext _context;

    public TentativasController(BancoQuestoesContext context)
    {
        _context = context;
    }

    private int? GetAlunoId()
    {
        var valor = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : null;
    }

    private static int CalcularNotaObjetivas(TentativaProva tentativa) =>
        tentativa.Respostas.Count(r => r.AlternativaSelecionada?.Correta == true);

    /// <summary>
    /// Inicia uma nova tentativa de prova. O prazo (ExpiraEm) é calculado aqui, no servidor.
    /// </summary>
    [HttpPost("iniciar")]
    [ProducesResponseType(typeof(TentativaIniciadaResponse), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<TentativaIniciadaResponse>> Iniciar([FromBody] IniciarTentativaRequest request)
    {
        var alunoId = GetAlunoId();
        if (alunoId == null)
        {
            return Unauthorized(new ApiResponse { Message = "Usuário não identificado", Success = false });
        }

        var prova = await _context.Provas
            .Include(p => p.Questoes)
                .ThenInclude(q => q.Alternativas)
            .Include(p => p.TemaRedacao)
            .FirstOrDefaultAsync(p => p.IdProva == request.ProvaId);

        if (prova == null)
        {
            return NotFound(new ApiResponse { Message = "Prova não encontrada", Success = false });
        }

        if (prova.Questoes.Count == 0)
        {
            return BadRequest(new ApiResponse { Message = "Essa prova não tem questões", Success = false });
        }

        // Prova atribuída a uma turma só pode ser iniciada por aluno matriculado nela — o front
        // já esconde a prova da lista (ver ProvasController.GetProvas), isso é a checagem real.
        if (prova.TurmaId != null)
        {
            var turmaDoAluno = await _context.Usuarios
                .Where(u => u.IdUsuario == alunoId.Value)
                .Select(u => u.TurmaId)
                .FirstOrDefaultAsync();

            if (turmaDoAluno != prova.TurmaId)
            {
                return Forbid();
            }
        }

        // Se já existe uma tentativa não finalizada dessa mesma prova, retoma ela em vez de
        // criar outra — assim atualizar a página no meio da prova não reinicia o cronômetro
        // nem abandona as respostas já dadas.
        var tentativaAberta = await _context.Tentativas
            .Include(t => t.Respostas)
                .ThenInclude(r => r.AlternativaSelecionada)
            .Where(t => t.ProvaId == prova.IdProva && t.AlunoId == alunoId.Value && t.FinalizadoEm == null)
            .OrderByDescending(t => t.IniciadoEm)
            .FirstOrDefaultAsync();

        TentativaProva tentativa;

        if (tentativaAberta != null && DateTime.UtcNow <= tentativaAberta.ExpiraEm)
        {
            tentativa = tentativaAberta;
        }
        else
        {
            if (tentativaAberta != null)
            {
                // O prazo passou e o aluno nunca finalizou (ex.: fechou a aba) — fecha essa
                // tentativa órfã com a nota que já tinha antes de abrir uma nova.
                tentativaAberta.FinalizadoEm = tentativaAberta.ExpiraEm;
                tentativaAberta.NotaObjetivas = CalcularNotaObjetivas(tentativaAberta);
            }

            var iniciadoEm = DateTime.UtcNow;
            tentativa = new TentativaProva
            {
                ProvaId = prova.IdProva,
                AlunoId = alunoId.Value,
                IniciadoEm = iniciadoEm,
                ExpiraEm = iniciadoEm.AddMinutes(prova.TempoLimiteMinutos)
            };
            _context.Tentativas.Add(tentativa);
        }

        await _context.SaveChangesAsync();

        var response = new TentativaIniciadaResponse
        {
            IdTentativa = tentativa.IdTentativa,
            IniciadoEm = tentativa.IniciadoEm,
            ExpiraEm = tentativa.ExpiraEm,
            TempoLimiteMinutos = prova.TempoLimiteMinutos,
            ProvaTitulo = prova.Titulo,
            TemaRedacaoTitulo = prova.TemaRedacao?.Titulo,
            TemaRedacaoTexto = prova.TemaRedacao?.TextoMotivador,
            Questoes = prova.Questoes.Select(q => new QuestaoProvaResponse
            {
                IdQuestao = q.IdQuestao,
                Titulo = q.Titulo,
                Area = q.Area,
                ImagemUrl = q.ImagemUrl,
                Alternativas = q.Alternativas.Select(a => new AlternativaProvaResponse
                {
                    IdAlternativa = a.IdAlternativa,
                    Descricao = a.Descricao
                }).ToList()
            }).ToList(),
            RespostasSalvas = tentativa.Respostas
                .Where(r => r.AlternativaSelecionadaId != null)
                .Select(r => new RespostaSalvaResponse { QuestaoId = r.QuestaoId, AlternativaId = r.AlternativaSelecionadaId!.Value })
                .ToList()
        };

        return StatusCode(201, response);
    }

    /// <summary>
    /// Salva (ou atualiza) a resposta do aluno para uma questão da tentativa.
    /// </summary>
    [HttpPut("{id:int}/respostas")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ApiResponse>> Responder(int id, [FromBody] ResponderTentativaRequest request)
    {
        var alunoId = GetAlunoId();

        var tentativa = await _context.Tentativas
            .Include(t => t.Prova)
                .ThenInclude(p => p.Questoes)
                    .ThenInclude(q => q.Alternativas)
            .FirstOrDefaultAsync(t => t.IdTentativa == id);

        if (tentativa == null)
        {
            return NotFound(new ApiResponse { Message = "Tentativa não encontrada", Success = false });
        }

        if (tentativa.AlunoId != alunoId)
        {
            return Forbid();
        }

        if (tentativa.FinalizadoEm != null)
        {
            return BadRequest(new ApiResponse { Message = "Essa prova já foi finalizada", Success = false });
        }

        if (DateTime.UtcNow > tentativa.ExpiraEm)
        {
            return BadRequest(new ApiResponse { Message = "O tempo dessa prova já se esgotou", Success = false });
        }

        var questao = tentativa.Prova.Questoes.FirstOrDefault(q => q.IdQuestao == request.QuestaoId);
        if (questao == null)
        {
            return BadRequest(new ApiResponse { Message = "Essa questão não faz parte da prova", Success = false });
        }

        var alternativaValida = questao.Alternativas.Any(a => a.IdAlternativa == request.AlternativaId);
        if (!alternativaValida)
        {
            return BadRequest(new ApiResponse { Message = "Essa alternativa não pertence à questão informada", Success = false });
        }

        var resposta = await _context.RespostasAluno
            .FirstOrDefaultAsync(r => r.TentativaId == id && r.QuestaoId == request.QuestaoId);

        if (resposta == null)
        {
            _context.RespostasAluno.Add(new RespostaAluno
            {
                TentativaId = id,
                QuestaoId = request.QuestaoId,
                AlternativaSelecionadaId = request.AlternativaId
            });
        }
        else
        {
            resposta.AlternativaSelecionadaId = request.AlternativaId;
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Resposta salva" });
    }

    /// <summary>
    /// Finaliza a tentativa: salva a redação e calcula a nota das questões objetivas.
    /// </summary>
    [HttpPost("{id:int}/finalizar")]
    [ProducesResponseType(typeof(FinalizarTentativaResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<FinalizarTentativaResponse>> Finalizar(int id, [FromBody] FinalizarTentativaRequest request)
    {
        var alunoId = GetAlunoId();

        var tentativa = await _context.Tentativas
            .Include(t => t.Prova)
                .ThenInclude(p => p.Questoes)
            .Include(t => t.Respostas)
                .ThenInclude(r => r.AlternativaSelecionada)
            .FirstOrDefaultAsync(t => t.IdTentativa == id);

        if (tentativa == null)
        {
            return NotFound(new ApiResponse { Message = "Tentativa não encontrada", Success = false });
        }

        if (tentativa.AlunoId != alunoId)
        {
            return Forbid();
        }

        if (tentativa.FinalizadoEm != null)
        {
            return BadRequest(new ApiResponse { Message = "Essa prova já foi finalizada", Success = false });
        }

        tentativa.TextoRedacao = request.TextoRedacao;
        tentativa.FinalizadoEm = DateTime.UtcNow;
        tentativa.NotaObjetivas = CalcularNotaObjetivas(tentativa);

        await _context.SaveChangesAsync();

        return Ok(new FinalizarTentativaResponse
        {
            IdTentativa = tentativa.IdTentativa,
            NotaObjetivas = tentativa.NotaObjetivas ?? 0,
            TotalQuestoes = tentativa.Prova.Questoes.Count
        });
    }

    /// <summary>
    /// Resultado detalhado de uma tentativa já finalizada (acerto/erro por questão + redação).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ResultadoTentativaResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<ActionResult<ResultadoTentativaResponse>> GetResultado(int id)
    {
        var alunoId = GetAlunoId();

        var tentativa = await _context.Tentativas
            .Include(t => t.Prova)
                .ThenInclude(p => p.Questoes)
                    .ThenInclude(q => q.Alternativas)
            .Include(t => t.Prova.TemaRedacao)
            .Include(t => t.Respostas)
            .FirstOrDefaultAsync(t => t.IdTentativa == id);

        if (tentativa == null)
        {
            return NotFound(new ApiResponse { Message = "Tentativa não encontrada", Success = false });
        }

        if (tentativa.AlunoId != alunoId)
        {
            return Forbid();
        }

        if (tentativa.FinalizadoEm == null)
        {
            return BadRequest(new ApiResponse { Message = "Finalize a prova antes de ver o resultado", Success = false });
        }

        var respostasPorQuestao = tentativa.Respostas.ToDictionary(r => r.QuestaoId, r => r.AlternativaSelecionadaId);

        var response = new ResultadoTentativaResponse
        {
            IdTentativa = tentativa.IdTentativa,
            ProvaTitulo = tentativa.Prova.Titulo,
            IniciadoEm = tentativa.IniciadoEm,
            FinalizadoEm = tentativa.FinalizadoEm,
            NotaObjetivas = tentativa.NotaObjetivas ?? 0,
            TotalQuestoes = tentativa.Prova.Questoes.Count,
            TextoRedacao = tentativa.TextoRedacao,
            TemaRedacaoTitulo = tentativa.Prova.TemaRedacao?.Titulo,
            NotaComp1 = tentativa.NotaComp1,
            NotaComp2 = tentativa.NotaComp2,
            NotaComp3 = tentativa.NotaComp3,
            NotaComp4 = tentativa.NotaComp4,
            NotaComp5 = tentativa.NotaComp5,
            NotaRedacao = tentativa.NotaRedacao,
            ComentarioRedacao = tentativa.ComentarioRedacao,
            Questoes = tentativa.Prova.Questoes.Select(q =>
            {
                respostasPorQuestao.TryGetValue(q.IdQuestao, out var alternativaSelecionadaId);
                var alternativaCorreta = q.Alternativas.FirstOrDefault(a => a.Correta);

                return new QuestaoResultadoResponse
                {
                    IdQuestao = q.IdQuestao,
                    Titulo = q.Titulo,
                    ImagemUrl = q.ImagemUrl,
                    AlternativaSelecionadaId = alternativaSelecionadaId,
                    RespondidaCorretamente = alternativaSelecionadaId != null && alternativaSelecionadaId == alternativaCorreta?.IdAlternativa,
                    Alternativas = q.Alternativas.Select(a => new AlternativaResultadoResponse
                    {
                        IdAlternativa = a.IdAlternativa,
                        Descricao = a.Descricao,
                        Correta = a.Correta
                    }).ToList()
                };
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Histórico de tentativas do aluno logado.
    /// </summary>
    [HttpGet("minhas")]
    [ProducesResponseType(typeof(List<TentativaResumoResponse>), 200)]
    public async Task<ActionResult<List<TentativaResumoResponse>>> GetMinhas()
    {
        var alunoId = GetAlunoId();

        var tentativas = await _context.Tentativas
            .Include(t => t.Prova)
                .ThenInclude(p => p.Questoes)
            .Where(t => t.AlunoId == alunoId)
            .OrderByDescending(t => t.IniciadoEm)
            .ToListAsync();

        var response = tentativas.Select(t => new TentativaResumoResponse
        {
            IdTentativa = t.IdTentativa,
            ProvaId = t.ProvaId,
            ProvaTitulo = t.Prova.Titulo,
            IniciadoEm = t.IniciadoEm,
            FinalizadoEm = t.FinalizadoEm,
            NotaObjetivas = t.NotaObjetivas,
            TotalQuestoes = t.Prova.Questoes.Count
        }).ToList();

        return Ok(response);
    }
}
