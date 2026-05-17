using System.Security.Claims;
using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduqPlus.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class CursoController : ControllerBase {

        private readonly ICursoService _cursoService;

        public CursoController(ICursoService cursoService) {
            _cursoService = cursoService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ObterTodos([FromQuery] int pagina = 1, [FromQuery] int tamanho = 10) {
            if (pagina <= 0) pagina = 1;
            if (tamanho <= 0) tamanho = 10;

            var resultado = await _cursoService.ObterTodosPaginadoAsync(pagina, tamanho);
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorId(Guid id) {
            try {
                var curso = await _cursoService.ObterPorIdAsync(id);
                return Ok(curso);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("categoria/{categoriaId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorCategoria(Guid categoriaId) {
            try {
                var cursos = await _cursoService.ObterCursoCategoriasAsync(categoriaId);
                return Ok(cursos);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("produtor/{produtorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorProdutor(Guid produtorId) {
            try {
                var cursos = await _cursoService.ObterCursoProdutorAsync(produtorId);
                return Ok(cursos);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpGet("status/{status}")]
        [AllowAnonymous]
        public async Task<IActionResult> ObterPorStatus(EStatusAuditoria status) {
            try {
                var cursos = await _cursoService.ObterCursoStatusAuditoriaAsync(status);
                return Ok(cursos);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Criar([FromBody] CursoCreateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                dto.UsuarioId = Guid.Parse(usuarioIdToken!);

                var curso = await _cursoService.CriarCursoAsync(dto);
                return CreatedAtAction(nameof(ObterPorId), new { id = curso.Id }, curso);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] CursoUpdateDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioId = Guid.Parse(usuarioIdToken!);

                var curso = await _cursoService.AlterarCursoAsync(id, usuarioId, dto);
                return Ok(curso);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AtualizarAdmin(Guid id, [FromBody] CursoUpdateAdminDTO dto) {
            try {
                var usuarioIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid adminId = Guid.Parse(usuarioIdToken!);

                var curso = await _cursoService.AlterarCursoAdminAsync(id, adminId, dto);
                return Ok(curso);
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

                var sucesso = await _cursoService.ExcluirCursoAsync(id, usuarioId);
                if (sucesso) return NoContent();

                return BadRequest(new { mensagem = "Não foi possível excluir o curso." });
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpGet("buscar")]
        [AllowAnonymous]
        public async Task<IActionResult> Buscar([FromQuery] string query) {
            if (string.IsNullOrWhiteSpace(query)) return BadRequest("O termo de busca é obrigatório.");

            var resultados = await _cursoService.BuscarCursosInteligenteAsync(query);
            return Ok(resultados);
        }
    }
}