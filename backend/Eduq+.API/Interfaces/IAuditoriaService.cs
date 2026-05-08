using EduqPlus.API.DTOs;
using EduqPlus.API.Models;

namespace EduqPlus.API.Interfaces {
    public interface IAuditoriaService {
        Task<IEnumerable<AuditoriaResponseDTO>> ObterAuditoriasCursoAsync(Guid cursoId);
        Task<IEnumerable<AuditoriaResponseDTO>> ObterAuditoriasPendentesAsync();
        Task<IEnumerable<AuditoriaResponseDTO>> ObterAuditoriasConcluidasAsync(Guid? auditorId = null);
        Task<AuditoriaResponseDTO> AtualizarAuditoriaAsync(Guid id, Guid auditorId, AuditoriaUpdateDTO auditoriaDto);
        Task<AuditoriaResponseDTO> CriarAuditoriaAsync(AuditoriaCreateDTO auditoriaDto);
    }
}
