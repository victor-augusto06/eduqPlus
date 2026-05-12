using EduqPlus.API.DTOs;

namespace EduqPlus.API.Interfaces {
    public interface IPromessaCursoService {
        Task<PromessaCursoResponseDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<PromessaCursoResponseDTO>> ObterPorCursoAsync(Guid cursoId);
        Task<PromessaCursoResponseDTO> CriarPromessaAsync(PromessaCursoCreateDTO promessaDto, Guid usuarioId);
        Task<PromessaCursoResponseDTO> AlterarPromessaAsync(Guid id, Guid usuarioId, PromessaCursoUpdateDTO promessaDto);
        Task<PromessaCursoResponseDTO> AlterarPromessaAdminAsync(Guid id, Guid usuarioId, PromessaCursoUpdateAdminDTO promessaDto);
        Task<bool> ExcluirPromessaAsync(Guid id, Guid usuarioId);
    }
}