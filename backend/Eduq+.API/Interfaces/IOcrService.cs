using EduqPlus.API.DTOs;

namespace EduqPlus.API.Interfaces {
    public interface IOcrService {
        Task<OcrResponseDTO> ExtrairTextosAsync(List<IFormFile> files);
    }
}
