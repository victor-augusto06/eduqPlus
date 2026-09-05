using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduqPlus.API.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AvaliacaoController : ControllerBase {
        private readonly IAvaliacaoService _avaliacaoService;
        private readonly IOcrService _ocrService;
        private readonly IIaService _iaService;
        private readonly ICursoService _cursoService;
        private readonly ILogger<AvaliacaoController> _logger;

        public AvaliacaoController(
            IAvaliacaoService avaliacaoService,
            IOcrService ocrService,
            IIaService iaService,
            ICursoService cursoService,
            ILogger<AvaliacaoController> logger) {
            _avaliacaoService = avaliacaoService;
            _ocrService = ocrService;
            _iaService = iaService;
            _cursoService = cursoService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorId(Guid id) {
            try {
                var avaliacao = await _avaliacaoService.ObterPorIdAsync(id);
                return Ok(avaliacao);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("curso/{cursoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorCurso(Guid cursoId) {
            var avaliacoes = await _avaliacaoService.ObterTodosAsync(cursoId);
            return Ok(avaliacoes);
        }

        [HttpGet("curso/{cursoId}/validadas")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterValidadasPorCurso(Guid cursoId) {
            var avaliacoes = await _avaliacaoService.ObterAvaliacoesValidadasAsync(cursoId);
            return Ok(avaliacoes);
        }

        [HttpGet("minhas-avaliacoes")]
        [Authorize]
        public async Task<IActionResult> ObterMinhasAvaliacoes() {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var avaliacoes = await _avaliacaoService.ObterAvaliacoesUsuarioAsync(usuarioId);
                return Ok(avaliacoes);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromForm] AvaliacaoCreateDTO dto, [FromForm] List<IFormFile>? comprovantes) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                dto.UsuarioId = Guid.Parse(usuarioIdToken!);

                dto.StatusComprovante = EStatusComprovante.Pendente;
                _logger.LogInformation($"[PIPELINE INICIADA] Usuário {dto.UsuarioId} submeteu uma avaliação para o curso {dto.CursoId}.");

                if (comprovantes != null && comprovantes.Count > 0) {
                    _logger.LogInformation($"[VALIDAÇÃO] {comprovantes.Count} arquivo(s) recebido(s). Iniciando validação de segurança.");

                    var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                    var mimeTypesPermitidos = new[] { "image/jpeg", "image/png", "application/pdf" };
                    long tamanhoMaximo = 5 * 1024 * 1024;

                    foreach (var arquivo in comprovantes) {
                        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();

                        if (!extensoesPermitidas.Contains(extensao) || !mimeTypesPermitidos.Contains(arquivo.ContentType)) {
                            _logger.LogWarning($"[FALHA] Arquivo {arquivo.FileName} rejeitado por formato inválido.");
                            return BadRequest(new { mensagem = $"O arquivo '{arquivo.FileName}' possui um formato inválido. Envie apenas imagens (JPG, PNG) ou PDF." });
                        }

                        if (arquivo.Length > tamanhoMaximo) {
                            _logger.LogWarning($"[FALHA] Arquivo {arquivo.FileName} rejeitado por exceder tamanho.");
                            return BadRequest(new { mensagem = $"O arquivo '{arquivo.FileName}' excede o tamanho máximo de 5MB." });
                        }
                    }

                    _logger.LogInformation("[OCR] Arquivos validados. Enviando para o microsserviço Python...");
                    var ocrResponse = await _ocrService.ExtrairTextosAsync(comprovantes);

                    if (ocrResponse != null && ocrResponse.Success && ocrResponse.Resultados.Count > 0) {
                        _logger.LogInformation("[OCR SUCCESS] Textos extraídos com sucesso pelas rotinas Python.");
                        var textoExtraidoCompleto = string.Join("\n\n", ocrResponse.Resultados.Select(r => r.Texto));

                        var curso = await _cursoService.ObterPorIdAsync(dto.CursoId);

                        if (curso != null) {
                            _logger.LogInformation($"[IA] Enviando textos para o Llama3 validar contra o curso: '{curso.Titulo}'.");

                            EStatusComprovante statusValidacao = await _iaService.ValidarComprovanteAsync(textoExtraidoCompleto, curso.Titulo);

                            _logger.LogInformation($"[IA RESULTADO] A Inteligência Artificial definiu o status final como: {statusValidacao}.");

                            dto.StatusComprovante = statusValidacao;
                        } else {
                            _logger.LogWarning($"[ALERTA] Curso {dto.CursoId} não encontrado. Validação de IA ignorada.");
                        }
                    } else {
                        _logger.LogError("[OCR ERRO] O microsserviço Python falhou ao extrair o texto ou retornou vazio.");
                    }
                } else {
                    _logger.LogInformation("[INFO] Avaliação submetida sem arquivos de comprovante.");
                }

                var avaliacao = await _avaliacaoService.CriarAvaliacaoAsync(dto);
                _logger.LogInformation($"[PIPELINE CONCLUÍDA] Avaliação {avaliacao.Id} salva com sucesso no banco de dados.");

                return CreatedAtAction(nameof(ObterPorId), new { id = avaliacao.Id }, avaliacao);
            } catch (Exception ex) {
                _logger.LogCritical($"[CRITICAL EXCEPTION] Falha grave na pipeline de criação: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AvaliacaoUpdateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var avaliacao = await _avaliacaoService.AtualizarAvaliacaoAsync(id, usuarioId, dto);
                return Ok(avaliacao);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Excluir(Guid id) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var sucesso = await _avaliacaoService.ExcluirAvaliacaoAsync(id, usuarioId);
                if (sucesso) return NoContent();

                return BadRequest(new { mensagem = "Não foi possível excluir a avaliação." });
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("admin/status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObterPorStatusAdmin(EStatusComprovante status) {
            var avaliacoes = await _avaliacaoService.ObterAvaliacoesAdminPorStatusAsync(status);
            return Ok(avaliacoes);
        }

        [HttpPut("{id}/validar-comprovante")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ValidarComprovante(Guid id, [FromBody] EStatusComprovante status) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid adminId = Guid.Parse(usuarioIdToken!);

                var avaliacao = await _avaliacaoService.ValidarComprovanteAsync(id, adminId, status);
                return Ok(avaliacao);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}