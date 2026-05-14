namespace EduqPlus.API.Interfaces;

public interface IIaService {
    Task<string> GerarResumoReputacaoAsync(IEnumerable<string> comentarios);
}