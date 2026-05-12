using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Service {
    public class PromessaCursoService : IPromessaCursoService {
        private readonly EduqPlusContext _context;

        public PromessaCursoService(EduqPlusContext context) {
            _context = context;
        }

        public async Task<PromessaCursoResponseDTO> ObterPorIdAsync(Guid id) {
            var promessa = await _context.PromessaCursos
                .AsNoTracking()
                .Select(p => new PromessaCursoResponseDTO {
                    Id = p.Id,
                    CursoId = p.CursoId,
                    Descricao = p.Descricao,
                    CumpridaNaAuditoria = p.CumpridaNaAuditoria
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (promessa == null)
                throw new Exception("Promessa não encontrada.");

            return promessa;
        }

        public async Task<IEnumerable<PromessaCursoResponseDTO>> ObterPorCursoAsync(Guid cursoId) {
            var promessas = await _context.PromessaCursos
                .AsNoTracking()
                .Where(p => p.CursoId == cursoId)
                .Select(p => new PromessaCursoResponseDTO {
                    Id = p.Id,
                    CursoId = p.CursoId,
                    Descricao = p.Descricao,
                    CumpridaNaAuditoria = p.CumpridaNaAuditoria
                })
                .ToListAsync();

            return promessas;
        }

        public async Task<PromessaCursoResponseDTO> CriarPromessaAsync(PromessaCursoCreateDTO promessaDto, Guid usuarioId) {
            var curso = await _context.Cursos.FindAsync(promessaDto.CursoId);

            if (curso == null)
                throw new Exception("Curso não encontrado.");

            if (curso.UsuarioId != usuarioId)
                throw new Exception("Você não tem permissão para adicionar promessas a este curso.");

            var novaPromessa = new PromessaCurso {
                Id = Guid.NewGuid(),
                CursoId = promessaDto.CursoId,
                Descricao = promessaDto.Descricao,
                CumpridaNaAuditoria = null 
            };

            _context.PromessaCursos.Add(novaPromessa);
            await _context.SaveChangesAsync();

            return new PromessaCursoResponseDTO {
                Id = novaPromessa.Id,
                CursoId = novaPromessa.CursoId,
                Descricao = novaPromessa.Descricao,
                CumpridaNaAuditoria = novaPromessa.CumpridaNaAuditoria
            };
        }

        public async Task<PromessaCursoResponseDTO> AlterarPromessaAsync(Guid id, Guid usuarioId, PromessaCursoUpdateDTO promessaDto) {
            var promessa = await _context.PromessaCursos
                .Include(p => p.Curso)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promessa == null)
                throw new Exception("Promessa não encontrada.");

            if (promessa.Curso.UsuarioId != usuarioId)
                throw new Exception("Você não tem permissão para alterar esta promessa.");

            promessa.Descricao = promessaDto.Descricao;

            await _context.SaveChangesAsync();

            return new PromessaCursoResponseDTO {
                Id = promessa.Id,
                CursoId = promessa.CursoId,
                Descricao = promessa.Descricao,
                CumpridaNaAuditoria = promessa.CumpridaNaAuditoria
            };
        }

        public async Task<PromessaCursoResponseDTO> AlterarPromessaAdminAsync(Guid id, Guid usuarioId, PromessaCursoUpdateAdminDTO promessaDto) {
            var promessa = await _context.PromessaCursos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promessa == null)
                throw new Exception("Promessa não encontrada.");

            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null || usuario.Role != ERoleUsuario.Admin)
                throw new Exception("Você não tem permissão de administrador para alterar esta promessa.");

            promessa.Descricao = promessaDto.Descricao;
            promessa.CumpridaNaAuditoria = promessaDto.CumpridaNaAuditoria;

            await _context.SaveChangesAsync();

            return new PromessaCursoResponseDTO {
                Id = promessa.Id,
                CursoId = promessa.CursoId,
                Descricao = promessa.Descricao,
                CumpridaNaAuditoria = promessa.CumpridaNaAuditoria
            };
        }

        public async Task<bool> ExcluirPromessaAsync(Guid id, Guid usuarioId) {
            try {
                var promessaExistente = await _context.PromessaCursos
                    .Include(p => p.Curso)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (promessaExistente == null)
                    throw new Exception("Promessa não encontrada.");

                var usuarioRequisicao = await _context.Usuarios.FindAsync(usuarioId);

                bool isDono = promessaExistente.Curso.UsuarioId == usuarioId;
                bool isAdmin = usuarioRequisicao != null && usuarioRequisicao.Role == ERoleUsuario.Admin;

                if (!isDono && !isAdmin)
                    throw new Exception("Você não tem permissão para excluir esta promessa.");

                _context.PromessaCursos.Remove(promessaExistente);
                await _context.SaveChangesAsync();

                return true;
            } catch (Exception ex) {
                Console.WriteLine("PromessaCursoService.ExcluirPromessaAsync - Erro inesperado ocorreu " + ex.ToString());
                return false;
            }
        }
    }
}