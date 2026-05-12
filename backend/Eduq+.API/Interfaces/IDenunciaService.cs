using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;

namespace EduqPlus.API.Interfaces {
    public interface IDenunciaService {
        Task<DenunciaResponseDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<DenunciaResponseDTO>> ObterTodosAsync(Guid cursoId);
        Task<IEnumerable<DenunciaResponseDTO>> ObterDenunciaUsuarioAsync(Guid usuarioId);
        Task<IEnumerable<DenunciaResponseDTO>> ObterDenunciaCategoriaAsync(Guid cursoId, ETipoDenuncia categoria);
        Task<IEnumerable<DenunciaResponseDTO>> ObterStatusDenunciaAsync(EStatusDenuncia status);
        Task<DenunciaResponseDTO> CriarDenunciaAsync(DenunciaCreateDTO denunciaDTO);
        Task<DenunciaResponseDTO> AlterarDenunciaAsync(Guid id, Guid usuarioId, DenunciaUpdateDTO denunciaDTO);
        Task<DenunciaResponseDTO> AlterarDenunciaAdminAsync(Guid id, Guid usuarioId, DenunciaUpdateAdminDTO denunciaDTO);
        Task<bool> ExcluirDenunciaAsync(Guid id, Guid usuarioId);
    }
}
