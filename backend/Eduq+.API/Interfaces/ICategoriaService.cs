using EduqPlus.API.DTOs;

namespace EduqPlus.API.Interfaces
{
    public interface ICategoriaService {
        Task<IEnumerable<CategoriaResponseDTO>> ObterTodosAsync();
        Task<CategoriaResponseDTO> ObterPorIdAsync(Guid id);
        Task<CategoriaResponseDTO> CriarCategoriaAsync(CategoriaCreateDTO categoriaDTO);
        Task<CategoriaResponseDTO> AtualizarCategoriasAsync(Guid id, Guid usuarioId, CategoriaUpdateDTO categoriaDTO);
    }
}
