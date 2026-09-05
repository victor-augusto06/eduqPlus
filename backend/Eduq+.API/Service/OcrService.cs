using System.Text.Json;
using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;

namespace EduqPlus.API.Service {
    public class OcrService : IOcrService {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OcrService> _logger;

        public OcrService(HttpClient httpClient, IConfiguration configuration, ILogger<OcrService> logger) {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OcrResponseDTO> ExtrairTextosAsync(List<IFormFile> arquivos) {
            _logger.LogInformation($"[OCR SERVICE] Iniciando extração de texto para {arquivos.Count} arquivo(s).");
            using var formContent = new MultipartFormDataContent();

            var apiKey = _configuration["OCR_API_KEY"]
                         ?? throw new InvalidOperationException("A chave OCR_API_KEY não foi configurada.");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            foreach (var arquivo in arquivos) {
                if (arquivo.Length > 0) {
                    var streamContent = new StreamContent(arquivo.OpenReadStream());
                    formContent.Add(streamContent, "files", arquivo.FileName);
                    _logger.LogInformation($"[OCR SERVICE] Anexando arquivo no FormData: {arquivo.FileName}");
                }
            }

            _logger.LogInformation("[OCR SERVICE] Disparando requisição HTTP para http://ocr_api:8000/extract-text/ ...");
            var response = await _httpClient.PostAsync("http://ocr_api:8000/extract-text/", formContent);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"[OCR SERVICE] Resposta bruta da API Python recebida: {jsonString}");

            var ocrResponse = JsonSerializer.Deserialize<OcrResponseDTO>(jsonString);

            _logger.LogInformation($"[OCR SERVICE] Deserialização concluída. Atributo Success = {ocrResponse?.Success}");

            return ocrResponse;
        }
    }
}