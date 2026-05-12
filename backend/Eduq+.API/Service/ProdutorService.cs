using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Service {
    public class ProdutorService : IProdutorService {

        private readonly EduqPlusContext _context;

        public ProdutorService(EduqPlusContext context) {
            _context = context;
        }

        public async Task<ProdutorResponseDTO> AlterarProdutorAdminAsync(Guid id, Guid usuarioId, ProdutorUpdateDTO produtorDto) {
            var produtor = await _context.Produtors
                .FirstOrDefaultAsync(c => c.Id == id);

            if (produtor == null)
                throw new Exception("Produtor não encontrado.");

            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null || usuario.Role != ERoleUsuario.Admin)
                throw new Exception("Você não tem permissão para alterar este produtor.");

            produtor.Nome = produtorDto.Nome;
            produtor.NichoPrincipal = produtorDto.NichoPrincipal;
            produtor.LinksSociais = produtorDto.LinksSociais;

            await _context.SaveChangesAsync();

            return new ProdutorResponseDTO {
                Id = produtor.Id,
                UsuarioId = produtor.UsuarioId,
                Nome = produtor.Nome,
                NichoPrincipal = produtor.NichoPrincipal,
                LinksSociais = produtor.LinksSociais
            };
        }

        public async Task<ProdutorResponseDTO> AlterarProdutorAsync(Guid id, Guid usuarioId, ProdutorUpdateDTO produtorDto) {
            var produtor = await _context.Produtors
                .FirstOrDefaultAsync(c => c.Id == id);

            if (produtor == null)
                throw new Exception("Produtor não encontrado.");

            if (produtor.UsuarioId != usuarioId)
                throw new Exception("Você não tem permissão para alterar este produtor.");

            produtor.Nome = produtorDto.Nome;
            produtor.NichoPrincipal = produtorDto.NichoPrincipal;
            produtor.LinksSociais = produtorDto.LinksSociais;

            await _context.SaveChangesAsync();

            return new ProdutorResponseDTO {
                Id = produtor.Id,
                UsuarioId = produtor.UsuarioId,
                Nome = produtor.Nome,
                NichoPrincipal = produtor.NichoPrincipal,
                LinksSociais = produtor.LinksSociais
            };
        }

        public async Task<ProdutorResponseDTO> CriarProdutorAsync(ProdutorCreateDTO produtorDto) {
            var novoProdutor = new Produtor {
                Id = Guid.NewGuid(),
                UsuarioId = produtorDto.UsuarioId,
                Nome = produtorDto.Nome,
                NichoPrincipal = produtorDto.NichoPrincipal,
                LinksSociais = produtorDto.LinksSociais
            };

            _context.Produtors.Add(novoProdutor);
            await _context.SaveChangesAsync();

            return new ProdutorResponseDTO {
                Id = novoProdutor.Id,
                UsuarioId = novoProdutor.UsuarioId,
                Nome = novoProdutor.Nome,
                NichoPrincipal = novoProdutor.NichoPrincipal,
                LinksSociais = novoProdutor.LinksSociais
            };
        }

        public async Task<bool> ExcluirProdutorAsync(Guid id, Guid usuarioId) {
            try {
                var produtorExistente = await _context.Produtors
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (produtorExistente == null)
                    throw new Exception("Produtor não encontrado.");

                var usuarioRequisicao = await _context.Usuarios.FindAsync(usuarioId);

                bool isDono = produtorExistente.UsuarioId == usuarioId;
                bool isAdmin = usuarioRequisicao != null && usuarioRequisicao.Role == ERoleUsuario.Admin;

                if (!isDono && !isAdmin)
                    throw new Exception("Você não tem permissão para excluir este produtor.");

                _context.Produtors.Remove(produtorExistente);
                await _context.SaveChangesAsync();

                return true;
            } catch (Exception ex) {
                Console.WriteLine("ProdutorService.ExcluirProdutorAsync - Erro inesperado ocorreu " + ex.ToString());
                return false;
            }
        }

        public async Task<ProdutorResponseDTO> ObterPorIdAsync(Guid id) {
            var produtor = await _context.Produtors
                .AsNoTracking()
                .Select(c => new ProdutorResponseDTO {
                    Id = c.Id,
                    UsuarioId = c.UsuarioId,
                    Nome = c.Nome,
                    NichoPrincipal = c.NichoPrincipal,
                    LinksSociais = c.LinksSociais
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (produtor == null)
                throw new Exception("Produtor não foi encontrado.");

            return produtor;
        }

        public async Task<ProdutorResponseDTO> ObterProdutoresCursoAsync(Guid cursoId) {
            var produtor = await _context.Produtors
                .AsNoTracking()
                .Where(p => p.Cursos.Any(curso => curso.Id == cursoId))
                .Select(c => new ProdutorResponseDTO {
                    Id = c.Id,
                    UsuarioId = c.UsuarioId,
                    Nome = c.Nome,
                    NichoPrincipal = c.NichoPrincipal,
                    LinksSociais = c.LinksSociais
                })
                .FirstOrDefaultAsync();

            if (produtor == null)
                throw new Exception("Nenhum produtor foi encontrado para esse curso.");

            return produtor;
        }

        public async Task<IEnumerable<ProdutorResponseDTO>> ObterTodosAsync() {
            var produtores = await _context.Produtors
                .AsNoTracking()
                .Select(c => new ProdutorResponseDTO {
                    Id = c.Id,
                    UsuarioId = c.UsuarioId,
                    Nome = c.Nome,
                    NichoPrincipal = c.NichoPrincipal,
                    LinksSociais = c.LinksSociais
                })
                .ToListAsync();

            return produtores;
        }
    }
}