using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;
using EduqPlus.API.Models;

namespace EduqPlus.API.Interfaces {
    public interface IAvaliacaoService {
        Task<AvaliacaoResponseDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<AvaliacaoResponseDTO>> ObterTodosAsync(Guid cursoId);
        Task<IEnumerable<AvaliacaoResponseDTO>> ObterAvaliacoesUsuarioAsync(Guid usuarioId);
        Task<IEnumerable<AvaliacaoResponseDTO>> ObterAvaliacoesValidadasAsync(Guid cursoId);
        Task<IEnumerable<AvaliacaoResponseAdminDTO>> ObterAvaliacoesAdminPorStatusAsync(EStatusComprovante status);
        Task<AvaliacaoResponseDTO> AtualizarAvaliacaoAsync(Guid id, Guid usuarioId, AvaliacaoUpdateDTO avaliacaoDTO);
        Task<bool> ExcluirAvaliacaoAsync(Guid id, Guid usuarioId);
        Task<AvaliacaoResponseDTO> CriarAvaliacaoAsync(AvaliacaoCreateDTO avaliacaoDTO);
        Task<AvaliacaoResponseAdminDTO> ValidarComprovanteAsync(Guid id, Guid usuarioId, EStatusComprovante statusComprovante);
    }
}
