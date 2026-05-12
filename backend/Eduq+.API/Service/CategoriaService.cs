using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Service {
    public class CategoriaService : ICategoriaService {
        private readonly EduqPlusContext _context;

        public CategoriaService(EduqPlusContext context) {
            _context = context;
        }

        public async Task<bool> ExcluirCategoriaAsync(Guid id, Guid usuarioId) {
            var usuarioAdmin = await _context.Usuarios.FindAsync(usuarioId);

            if (usuarioAdmin == null || usuarioAdmin.Role != ERoleUsuario.Admin)
                throw new Exception("Apenas administradores podem excluir categorias.");

            var categoria = await _context.Categorias
                .Include(c => c.Cursos) 
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
                throw new Exception("Categoria não encontrada.");

            if (categoria.Cursos.Any())
                throw new Exception("Não é possível excluir esta categoria pois existem cursos vinculados a ela.");

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CategoriaResponseDTO> AtualizarCategoriasAsync(Guid id, Guid usuarioId, CategoriaUpdateDTO categoriaDTO) {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
                throw new Exception("Categoria não encontrada.");

            var usuarioAdmin = await _context.Usuarios.FindAsync(usuarioId);

            if (usuarioAdmin == null || usuarioAdmin.Role != ERoleUsuario.Admin)
                throw new Exception("Apenas administradores podem alterar categorias.");

            var categoriaExistente = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome.ToLower() == categoriaDTO.Nome.ToLower() && c.Id != id);

            if (categoriaExistente != null)
                throw new Exception("Já existe uma categoria com este nome.");

            categoria.Nome = categoriaDTO.Nome;
            await _context.SaveChangesAsync();

            return new CategoriaResponseDTO {
                Id = categoria.Id,
                Nome = categoria.Nome
            };
        }

        public async Task<CategoriaResponseDTO> CriarCategoriaAsync(CategoriaCreateDTO categoriaDTO) {
            var categoriaExistente = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome.ToLower() == categoriaDTO.Nome.ToLower());

            if (categoriaExistente != null)
                throw new Exception("Já existe uma categoria com este nome.");

            var novaCategoria = new Categoria {
                Id = Guid.NewGuid(),
                Nome = categoriaDTO.Nome
            };

            _context.Categorias.Add(novaCategoria);
            await _context.SaveChangesAsync();

            return new CategoriaResponseDTO {
                Id = novaCategoria.Id,
                Nome = novaCategoria.Nome
            };
        }

        public async Task<CategoriaResponseDTO> ObterPorIdAsync(Guid id) {
            var categoria = await _context.Categorias
                .AsNoTracking()
                .Select(c => new CategoriaResponseDTO {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (categoria == null)
                throw new Exception("Nenhuma categoria foi encontrada");

            return categoria;
        }

        public async Task<IEnumerable<CategoriaResponseDTO>> ObterTodosAsync() {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .Select(c => new CategoriaResponseDTO {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .OrderBy(c => c.Nome)
                .ToListAsync();
            return categorias;
        }
    }
}
