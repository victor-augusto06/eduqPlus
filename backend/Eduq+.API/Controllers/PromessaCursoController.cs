using System.Security.Claims;
using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduqPlus.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class PromessaCursoController : ControllerBase {

        private readonly IPromessaCursoService _promessaCursoService;

        public PromessaCursoController(IPromessaCursoService promessaCursoService) {
            _promessaCursoService = promessaCursoService;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorId(Guid id) {
            try {
                var promessa = await _promessaCursoService.ObterPorIdAsync(id);
                return Ok(promessa);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("curso/{cursoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorCurso(Guid cursoId) {
            try {
                var promessas = await _promessaCursoService.ObterPorCursoAsync(cursoId);
                return Ok(promessas);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromBody] PromessaCursoCreateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var promessa = await _promessaCursoService.CriarPromessaAsync(dto, usuarioId);
                return CreatedAtAction(nameof(ObterPorId), new { id = promessa.Id }, promessa);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] PromessaCursoUpdateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var promessa = await _promessaCursoService.AlterarPromessaAsync(id, usuarioId, dto);
                return Ok(promessa);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarAdmin(Guid id, [FromBody] PromessaCursoUpdateAdminDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var promessa = await _promessaCursoService.AlterarPromessaAdminAsync(id, usuarioId, dto);
                return Ok(promessa);
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

                var sucesso = await _promessaCursoService.ExcluirPromessaAsync(id, usuarioId);
                if (sucesso) return NoContent();

                return BadRequest(new { mensagem = "Não foi possível excluir a promessa." });
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}