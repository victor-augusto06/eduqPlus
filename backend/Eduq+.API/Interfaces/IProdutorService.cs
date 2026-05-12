using EduqPlus.API.DTOs;

namespace EduqPlus.API.Interfaces {
    public interface IProdutorService {
        Task<IEnumerable<ProdutorResponseDTO>> ObterTodosAsync();
        Task<ProdutorResponseDTO> ObterPorIdAsync(Guid id);
        Task<ProdutorResponseDTO> ObterProdutoresCursoAsync(Guid cursoId);
        Task<ProdutorResponseDTO> CriarProdutorAsync(ProdutorCreateDTO produtorDto);
        Task<ProdutorResponseDTO> AlterarProdutorAsync(Guid id, Guid usuarioId, ProdutorUpdateDTO produtorDto);
        Task<ProdutorResponseDTO> AlterarProdutorAdminAsync(Guid id, Guid usuarioId, ProdutorUpdateDTO produtorDto);
        Task<bool> ExcluirProdutorAsync(Guid id, Guid usuarioId);   
    }
}