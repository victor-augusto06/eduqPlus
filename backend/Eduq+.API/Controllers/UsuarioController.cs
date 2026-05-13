using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduqPlus.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase {

        private readonly IUsuarioService _usuarioService;
        private readonly IConfiguration _configuration;

        public UsuarioController(IUsuarioService usuarioService, IConfiguration configuration) {
            _usuarioService = usuarioService;
            _configuration = configuration;
        }

        [HttpPost("registrar")]
        [AllowAnonymous]
        public async Task<IActionResult> Registrar([FromBody] UsuarioCreateDTO dto) {
            try {
                var usuario = await _usuarioService.RegistrarAsync(dto);

                return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDTO dto) {
            try {
                var usuario = await _usuarioService.LoginAsync(dto.Email, dto.Senha);

                var token = GerarTokenJwt(usuario);

                return Ok(new {
                    usuario = usuario,
                    token = token
                });
            } catch (Exception ex) {
                return Unauthorized(new { mensagem = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObterTodos() {
            var usuarios = await _usuarioService.ObterTodosAsync();
            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> ObterPorId(Guid id) {
            try {
                var usuario = await _usuarioService.ObterPorIdAsync(id);
                return Ok(usuario);
            } catch (Exception ex) {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpPut("perfil")]
        [Authorize]
        public async Task<IActionResult> AlterarPerfil([FromBody] UsuarioUpdateDTO dto) {
            try {
                var idToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(idToken)) return Unauthorized();

                Guid usuarioId = Guid.Parse(idToken);

                var usuarioAtualizado = await _usuarioService.AlterarPerfilAsync(usuarioId, usuarioId, dto);

                return Ok(usuarioAtualizado);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AlterarRole(Guid id, [FromBody] ERoleUsuario novaRole) {
            try {
                var adminIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid adminId = Guid.Parse(adminIdToken!);

                var usuario = await _usuarioService.AlterarRoleAdminAsync(id, adminId, novaRole);
                return Ok(usuario);
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Excluir(Guid id) {
            try {
                var requisitanteIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid usuarioRequisicaoId = Guid.Parse(requisitanteIdToken!);

                var sucesso = await _usuarioService.ExcluirUsuarioAsync(id, usuarioRequisicaoId);
                if (sucesso) return NoContent();

                return BadRequest(new { mensagem = "Não foi possível excluir o usuário." });
            } catch (Exception ex) {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        private string GerarTokenJwt(UsuarioResponseDTO usuario) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings:Secret").Value!);

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Role, usuario.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}