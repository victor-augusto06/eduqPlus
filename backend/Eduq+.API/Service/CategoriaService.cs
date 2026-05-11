using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Service
{
    public class CategoriaService : ICategoriaService
    {
        private readonly EduqPlusContext _context;

        public CategoriaService(EduqPlusContext context)
        {
            _context = context;
        }
        public async Task<CategoriaResponseDTO> AtualizarCategoriasAsync(Guid id, Guid usuarioId, CategoriaUpdateDTO categoriaDTO)
        {
            throw new NotImplementedException();
        }

        public async Task<CategoriaResponseDTO> CriarCategoriaAsync(CategoriaCreateDTO categoriaDTO)
        {
            var novaCategoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Nome = categoriaDTO.Nome
            };

            _context.Categorias.Add(novaCategoria);
            await _context.SaveChangesAsync();

            return new CategoriaResponseDTO
            {
                Id = novaCategoria.Id,
                Nome = novaCategoria.Nome
            };
        }

        public async Task<CategoriaResponseDTO> ObterPorIdAsync(Guid id)
        { 
            var categoria = await _context.Categorias
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(c => new CategoriaResponseDTO
                {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
                throw new Exception("Nenhuma categoria foi encontrada");

            return categoria;
        }

        public async Task<IEnumerable<CategoriaResponseDTO>> ObterTodosAsync()
        {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .Select(c => new CategoriaResponseDTO
                {
                    Id = c.Id,
                    Nome = c.Nome
                })
                .ToListAsync();
            return categorias;
        }
    }
}
