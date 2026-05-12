using EduqPlus.API.DTOs;
using EduqPlus.API.Enums;

namespace EduqPlus.API.Interfaces {
    public interface IUsuarioService {
        Task<UsuarioResponseDTO> ObterPorIdAsync(Guid id);
        Task<IEnumerable<UsuarioResponseDTO>> ObterTodosAsync();
        Task<UsuarioResponseDTO> RegistrarAsync(UsuarioCreateDTO usuarioDto);
        Task<UsuarioResponseDTO> LoginAsync(string email, string senhaLimpa);
        Task<UsuarioResponseDTO> AlterarPerfilAsync(Guid id, Guid usuarioRequisicaoId , UsuarioUpdateDTO usuarioDto);
        Task<UsuarioResponseDTO> AlterarRoleAdminAsync(Guid id, Guid adminId, ERoleUsuario novaRole);
        Task<bool> ExcluirUsuarioAsync(Guid id, Guid usuarioRequisicaoId);
    }
}