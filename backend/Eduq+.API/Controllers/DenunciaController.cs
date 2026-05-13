using System.Security.Claims;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduqPlus.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DenunciaController : ControllerBase {

        private readonly IDenunciaService _denunciaService;

        public DenunciaController(IDenunciaService denunciaService) {
            _denunciaService = denunciaService;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] DenunciaCreateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var denuncia = await _denunciaService.CriarDenunciaAsync(dto, usuarioId);
                return CreatedAtAction(nameof(ObterPorId), new { id = denuncia.Id }, denuncia);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(Guid id) {
            try {
                var denuncia = await _denunciaService.ObterPorIdAsync(id);
                return Ok(denuncia);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("meus-relatos")]
        public async Task<IActionResult> ObterDenunciasUsuario() {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var denuncias = await _denunciaService.ObterDenunciaUsuarioAsync(usuarioId);
                return Ok(denuncias);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("curso/{cursoId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObterPorCurso(Guid cursoId) {
            try {
                var denuncias = await _denunciaService.ObterTodosAsync(cursoId);
                return Ok(denuncias);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("curso/{cursoId}/categoria/{categoria}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObterPorCategoria(Guid cursoId, ETipoDenuncia categoria) {
            try {
                var denuncias = await _denunciaService.ObterDenunciaCategoriaAsync(cursoId, categoria);
                return Ok(denuncias);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObterPorStatus(EStatusDenuncia status) {
            try {
                var denuncias = await _denunciaService.ObterStatusDenunciaAsync(status);
                return Ok(denuncias);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] DenunciaUpdateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var denuncia = await _denunciaService.AlterarDenunciaAsync(id, usuarioId, dto);
                return Ok(denuncia);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarAdmin(Guid id, [FromBody] DenunciaUpdateAdminDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid adminId = Guid.Parse(usuarioIdToken!);

                var denuncia = await _denunciaService.AlterarDenunciaAdminAsync(id, adminId, dto);
                return Ok(denuncia);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(Guid id) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var sucesso = await _denunciaService.ExcluirDenunciaAsync(id, usuarioId);
                if (sucesso) return NoContent();

                return BadRequest(new { mensagem = "Não foi possível excluir a denúncia." });
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}