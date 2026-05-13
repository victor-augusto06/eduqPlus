using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduqPlus.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : ControllerBase {

        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService) {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        [AllowAnonymous] 
        public async Task<IActionResult> ObterTodas() {
            var categorias = await _categoriaService.ObterTodosAsync();
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorId(Guid id) {
            try {
                var categoria = await _categoriaService.ObterPorIdAsync(id);
                return Ok(categoria);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Criar([FromBody] CategoriaCreateDTO dto) {
            try {
                var categoria = await _categoriaService.CriarCategoriaAsync(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] CategoriaUpdateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var categoria = await _categoriaService.AtualizarCategoriasAsync(id, usuarioId, dto);
                return Ok(categoria);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Excluir(Guid id) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var sucesso = await _categoriaService.ExcluirCategoriaAsync(id, usuarioId);
                if (sucesso) return NoContent();

                return BadRequest(new { mensagem = "Não foi possível excluir a categoria." });
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}