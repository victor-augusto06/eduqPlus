using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;

namespace EduqPlus.API.Interfaces {
    public interface ICursoService {
        Task<CursoResponseDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<CursoResponseDTO>> ObterTodosAsync();
        Task<IEnumerable<CursoResponseDTO>> ObterCursoCategoriasAsync(Guid categoriaId);
        Task<IEnumerable<CursoResponseDTO>> ObterCursoProdutorAsync(Guid produtorId);
        Task<IEnumerable<CursoResponseDTO>> ObterCursoStatusAuditoriaAsync(EStatusAuditoria status);
        Task<CursoResponseDTO> CriarCursoAsync(CursoCreateDTO cursoDto);
        Task<CursoResponseDTO> AlterarCursoAsync(Guid id, Guid produtorId, CursoUpdateDTO cursoDto);
        Task<CursoResponseDTO> AlterarCursoAdminAsync(Guid id, Guid usuarioId, CursoUpdateAdminDTO cursoDto);
        Task<bool> ExcluirCursoAsync(Guid id, Guid usuarioId);
    }
}