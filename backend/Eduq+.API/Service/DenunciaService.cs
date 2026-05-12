using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace EduqPlus.API.Service {
    public class DenunciaService : IDenunciaService {

        private readonly EduqPlusContext _context;

        public DenunciaService(EduqPlusContext context) {
            _context = context;
        }
        public async Task<DenunciaResponseDTO> AlterarDenunciaAdminAsync(Guid id, Guid usuarioId, DenunciaUpdateAdminDTO denunciaDTO) {
            var denuncia = await _context.Denuncia
                .FirstOrDefaultAsync(c => c.Id == id);

            if (denuncia == null)
                throw new Exception("Denúncia não encontrada.");

            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null || usuario.Role != ERoleUsuario.Admin)
                throw new Exception("Você não tem permissão para alterar esta denúncia.");

            denuncia.Categoria = denunciaDTO.Categoria;
            denuncia.RelatoDetalhado = denunciaDTO.RelatoDetalhado;
            denuncia.Status = denunciaDTO.Status;

            await _context.SaveChangesAsync();

            return new DenunciaResponseDTO {
                Id = denuncia.Id,
                CursoId = denuncia.CursoId,
                UsuarioId = denuncia.UsuarioId,
                Data = denuncia.Data,
                Categoria = denuncia.Categoria,
                RelatoDetalhado = denuncia.RelatoDetalhado,
                Status = denuncia.Status
            };
        }

        public async Task<DenunciaResponseDTO> AlterarDenunciaAsync(Guid id, Guid usuarioId, DenunciaUpdateDTO denunciaDTO) {
            var denuncia = await _context.Denuncia
                .FirstOrDefaultAsync(c => c.Id == id);

            if (denuncia == null)
                throw new Exception("Denúncia não encontrada.");

            if (denuncia.UsuarioId != usuarioId)
                throw new Exception("Você não tem permissão para alterar esta denúncia.");

            denuncia.Categoria = denunciaDTO.Categoria;
            denuncia.RelatoDetalhado = denunciaDTO.RelatoDetalhado;

            await _context.SaveChangesAsync();

            return new DenunciaResponseDTO {
                Id = denuncia.Id,
                CursoId = denuncia.CursoId,
                UsuarioId = denuncia.UsuarioId,
                Data = denuncia.Data,
                Categoria = denuncia.Categoria,
                RelatoDetalhado = denuncia.RelatoDetalhado,
                Status = denuncia.Status
            };
        }

        public async Task<DenunciaResponseDTO> CriarDenunciaAsync(DenunciaCreateDTO denunciaDTO) {
            var novaDenuncia = new Denuncia {
                Id = Guid.NewGuid(),
                UsuarioId = denunciaDTO.UsuarioId,
                CursoId = denunciaDTO.CursoId,
                Data = DateTime.Now,
                Categoria = denunciaDTO.Categoria,
                RelatoDetalhado = denunciaDTO.RelatoDetalhado,
                Status = EStatusDenuncia.EmAnalise,

            };

            _context.Denuncia.Add(novaDenuncia);
            await _context.SaveChangesAsync();

            return new DenunciaResponseDTO {
                Id = novaDenuncia.Id,
                CursoId = novaDenuncia.CursoId,
                UsuarioId = novaDenuncia.UsuarioId,
                Data = novaDenuncia.Data,
                Categoria = novaDenuncia.Categoria,
                RelatoDetalhado = novaDenuncia.RelatoDetalhado,
                Status = novaDenuncia.Status
            };
        }

        public async Task<bool> ExcluirDenunciaAsync(Guid id, Guid usuarioId) {
            try {
                var denunciaExistente = await _context.Denuncia
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (denunciaExistente == null)
                    throw new Exception("Denúncia não encontrada.");

                var usuarioRequisicao = await _context.Usuarios.FindAsync(usuarioId);

                bool isDono = denunciaExistente.UsuarioId == usuarioId;
                bool isAdmin = usuarioRequisicao != null && usuarioRequisicao.Role == ERoleUsuario.Admin;

                if (!isDono && !isAdmin)
                    throw new Exception("Você não tem permissão para excluir esta denúncia.");

                _context.Denuncia.Remove(denunciaExistente);
                await _context.SaveChangesAsync();

                return true;
            } catch (Exception ex) {
                Console.WriteLine("DenunciaService.ExcluirDenunciaAsync - Erro inesperado ocorreu " + ex.ToString());
                return false;
            }
        }

        public async Task<IEnumerable<DenunciaResponseDTO>> ObterDenunciaCategoriaAsync(Guid cursoId, ETipoDenuncia categoria) {
            var denuncias = await _context.Denuncia
                .AsNoTracking()
                .Where(a => a.CursoId == cursoId && a.Categoria == categoria)
                .Select(c => new DenunciaResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    Data = c.Data,
                    Categoria = c.Categoria,
                    RelatoDetalhado = c.RelatoDetalhado,
                    Status = c.Status
                })
                .ToListAsync();

            return denuncias;
        }

        public async Task<IEnumerable<DenunciaResponseDTO>> ObterDenunciaUsuarioAsync(Guid usuarioId) {
            var denuncias = await _context.Denuncia
                .AsNoTracking()
                .Where(a => a.UsuarioId == usuarioId)
                .Select(c => new DenunciaResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    Data = c.Data,
                    Categoria = c.Categoria,
                    RelatoDetalhado = c.RelatoDetalhado,
                    Status = c.Status
                })
                .ToListAsync();

            return denuncias;
        }

        public async Task<DenunciaResponseDTO> ObterPorIdAsync(Guid id) {
            var denuncia = await _context.Denuncia
                .AsNoTracking()
                .Select(c => new DenunciaResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    Data = c.Data,
                    Categoria = c.Categoria,
                    RelatoDetalhado = c.RelatoDetalhado,
                    Status = c.Status
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (denuncia == null)
                throw new Exception("Denúncia não foi encontrada!");

            return denuncia;
        }

        public async Task<IEnumerable<DenunciaResponseDTO>> ObterStatusDenunciaAsync(EStatusDenuncia status) {
            var denuncias = await _context.Denuncia
                .AsNoTracking()
                .Where(a => a.Status == status)
                .Select(c => new DenunciaResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    Data = c.Data,
                    Categoria = c.Categoria,
                    RelatoDetalhado = c.RelatoDetalhado,
                    Status = c.Status
                })
                .ToListAsync();

            return denuncias;
        }

        public async Task<IEnumerable<DenunciaResponseDTO>> ObterTodosAsync(Guid cursoId) {
            var denuncias = await _context.Denuncia
                .AsNoTracking()
                .Where(a => a.CursoId == cursoId)
                .Select(c => new DenunciaResponseDTO {
                    Id = c.Id,
                    CursoId = c.CursoId,
                    UsuarioId = c.UsuarioId,
                    Data = c.Data,
                    Categoria = c.Categoria,
                    RelatoDetalhado = c.RelatoDetalhado,
                    Status = c.Status
                })
                .ToListAsync();

            return denuncias;
        }
    }
}