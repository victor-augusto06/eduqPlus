using System.Security.Claims;
using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduqPlus.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class AvaliacaoController : ControllerBase {

        private readonly IAvaliacaoService _avaliacaoService;

        public AvaliacaoController(IAvaliacaoService avaliacaoService) {
            _avaliacaoService = avaliacaoService;
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
        public async Task<IActionResult> Criar([FromBody] AvaliacaoCreateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                dto.UsuarioId = Guid.Parse(usuarioIdToken!);

                var avaliacao = await _avaliacaoService.CriarAvaliacaoAsync(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = avaliacao.Id }, avaliacao);
            } catch (Exception ex) {
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