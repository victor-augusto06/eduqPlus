using System.Security.Claims;
using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduqPlus.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditoriaController : ControllerBase {

        private readonly IAuditoriaService _auditoriaService;

        public AuditoriaController(IAuditoriaService auditoriaService) {
            _auditoriaService = auditoriaService;
        }

        [HttpGet("curso/{cursoId}")]
        public async Task<IActionResult> ObterPorCurso(Guid cursoId) {
            try {
                var auditorias = await _auditoriaService.ObterAuditoriasCursoAsync(cursoId);
                return Ok(auditorias);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("pendentes")]
        public async Task<IActionResult> ObterPendentes() {
            try {
                var auditorias = await _auditoriaService.ObterAuditoriasPendentesAsync();
                return Ok(auditorias);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("concluidas")]
        public async Task<IActionResult> ObterConcluidas([FromQuery] Guid? auditorId) {
            try {
                var auditorias = await _auditoriaService.ObterAuditoriasConcluidasAsync(auditorId);
                return Ok(auditorias);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] AuditoriaCreateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                dto.AuditorId = Guid.Parse(usuarioIdToken!);

                var auditoria = await _auditoriaService.CriarAuditoriaAsync(dto);
                return Ok(auditoria);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AuditoriaUpdateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid auditorId = Guid.Parse(usuarioIdToken!);

                var auditoria = await _auditoriaService.AtualizarAuditoriaAsync(id, auditorId, dto);
                return Ok(auditoria);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}