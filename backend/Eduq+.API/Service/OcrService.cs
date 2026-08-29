
using System.Text.Json;

using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;

namespace EduqPlus.API.Service {
    public class OcrService : IOcrService {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration; 

        public OcrService(HttpClient httpClient, IConfiguration configuration) {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<OcrResponseDTO> ExtrairTextosAsync(List<IFormFile> arquivos) {
            using var formContent = new MultipartFormDataContent();

            var apiKey = _configuration["OCR_API_KEY"]
                         ?? throw new InvalidOperationException("A chave OCR_API_KEY não foi configurada.");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            foreach (var arquivo in arquivos) {
                if (arquivo.Length > 0) {
                    var streamContent = new StreamContent(arquivo.OpenReadStream());
                    formContent.Add(streamContent, "files", arquivo.FileName);
                }
            }

            var response = await _httpClient.PostAsync("http://ocr_api:8000/extract-text/", formContent);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var ocrResponse = JsonSerializer.Deserialize<OcrResponseDTO>(jsonString);

            return ocrResponse;
        }
    }
}