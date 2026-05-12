using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Service {
    public class UsuarioService : IUsuarioService {
        
        private readonly EduqPlusContext _context;

        public UsuarioService(EduqPlusContext context) {
            _context = context;
        }

        public async Task<UsuarioResponseDTO> AlterarPerfilAsync(Guid id, Guid usuarioRequisicaoId, UsuarioUpdateDTO usuarioDto) {
            if (id != usuarioRequisicaoId)
                throw new Exception("Você não tem permissão para alterar o perfil de outro usuário.");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                throw new Exception("Usuário não encontrado.");

            usuario.Nome = usuarioDto.Nome;

            await _context.SaveChangesAsync();

            return new UsuarioResponseDTO {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Role = usuario.Role
            };
        }

        public async Task<UsuarioResponseDTO> AlterarRoleAdminAsync(Guid id, Guid adminId, ERoleUsuario novaRole) {
            var admin = await _context.Usuarios.FindAsync(adminId);

            if (admin == null || admin.Role != ERoleUsuario.Admin)
                throw new Exception("Apenas administradores podem alterar permissões.");

            var usuarioTarget = await _context.Usuarios.FindAsync(id);
            if (usuarioTarget == null)
                throw new Exception("Usuário não encontrado.");

            usuarioTarget.Role = novaRole;
            await _context.SaveChangesAsync();

            return new UsuarioResponseDTO {
                Id = usuarioTarget.Id,
                Nome = usuarioTarget.Nome,
                Email = usuarioTarget.Email,
                Role = usuarioTarget.Role
            };
        }

        public async Task<bool> ExcluirUsuarioAsync(Guid id, Guid usuarioRequisicaoId) {
            try {
                var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

                if (usuarioExistente == null)
                    throw new Exception("Usuário não encontrado.");

                var usuarioRequisicao = await _context.Usuarios.FindAsync(usuarioRequisicaoId);

                bool isDono = usuarioExistente.Id == usuarioRequisicaoId;
                bool isAdmin = usuarioRequisicao != null && usuarioRequisicao.Role == ERoleUsuario.Admin;

                if (!isDono && !isAdmin)
                    throw new Exception("Você não tem permissão para excluir este usuário.");

                _context.Usuarios.Remove(usuarioExistente);
                await _context.SaveChangesAsync();

                return true;
            } catch (Exception ex) {
                Console.WriteLine("UsuarioService.ExcluirUsuarioAsync - Erro inesperado ocorreu " + ex.ToString());
                return false;
            }
        }

        public async Task<UsuarioResponseDTO> LoginAsync(string email, string senhaLimpa) {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(senhaLimpa, usuario.SenhaHash))
                throw new Exception("E-mail ou senha inválidos.");

            return new UsuarioResponseDTO {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Role = usuario.Role
            };
        }

        public async Task<UsuarioResponseDTO> ObterPorIdAsync(Guid id) {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Select(u => new UsuarioResponseDTO {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Role = u.Role
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                throw new Exception("Usuário não encontrado.");

            return usuario;
        }

        public async Task<IEnumerable<UsuarioResponseDTO>> ObterTodosAsync() {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Select(u => new UsuarioResponseDTO {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return usuarios;
        }

        public async Task<UsuarioResponseDTO> RegistrarAsync(UsuarioCreateDTO usuarioDto) {
            bool emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == usuarioDto.Email);
            if (emailExiste)
                throw new Exception("Este e-mail já está em uso.");

            string senhaHash = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Senha);

            var novoUsuario = new Usuario {
                Id = Guid.NewGuid(),
                Nome = usuarioDto.Nome,
                Email = usuarioDto.Email,
                SenhaHash = senhaHash,
                Role = ERoleUsuario.Comum
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return new UsuarioResponseDTO {
                Id = novoUsuario.Id,
                Nome = novoUsuario.Nome,
                Email = novoUsuario.Email,
                Role = novoUsuario.Role
            };
        }
    }
}
