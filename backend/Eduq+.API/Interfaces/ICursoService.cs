using EduqPlus.API.DTOs;
using System.Data.SqlTypes;

namespace EduqPlus.API.Interfaces {
    public interface ICursoService {
        Task<IEnumerable<CursoResponseDTO>> ObterTodosAsync();
        Task<CursoResponseDTO> CriarCursoAsync(CursoCreateDTO cursoDto);
    }
}
