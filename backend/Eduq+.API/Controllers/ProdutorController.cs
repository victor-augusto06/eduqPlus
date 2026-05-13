using System.Security.Claims;
using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduqPlus.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class ProdutorController : ControllerBase {

        private readonly IProdutorService _produtorService;

        public ProdutorController(IProdutorService produtorService) {
            _produtorService = produtorService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromBody] ProdutorCreateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                dto.UsuarioId = Guid.Parse(usuarioIdToken!);

                var produtor = await _produtorService.CriarProdutorAsync(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = produtor.Id }, produtor);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObterTodos() {
            var produtores = await _produtorService.ObterTodosAsync();
            return Ok(produtores);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorId(Guid id) {
            try {
                var produtor = await _produtorService.ObterPorIdAsync(id);
                return Ok(produtor);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("curso/{cursoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterProdutoresCurso(Guid cursoId) {
            try {
                var produtor = await _produtorService.ObterProdutoresCursoAsync(cursoId);
                return Ok(produtor);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] ProdutorUpdateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var produtor = await _produtorService.AlterarProdutorAsync(id, usuarioId, dto);
                return Ok(produtor);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarAdmin(Guid id, [FromBody] ProdutorUpdateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid adminId = Guid.Parse(usuarioIdToken!);

                var produtor = await _produtorService.AlterarProdutorAdminAsync(id, adminId, dto);
                return Ok(produtor);
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

                var sucesso = await _produtorService.ExcluirProdutorAsync(id, usuarioId);
                if (sucesso) return NoContent();

                return BadRequest(new { mensagem = "Não foi possível excluir o produtor." });
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}