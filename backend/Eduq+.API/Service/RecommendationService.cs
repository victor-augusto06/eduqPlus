using System.Text;
using System.Text.Json;
using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EduqPlus.API.Service
{
    public class RecommendationService : IRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecommendationService> _logger;
        private readonly EduqPlusContext _context;

        public RecommendationService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<RecommendationService> logger,
            EduqPlusContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task<List<Guid>> ObterCursosRecomendadosAsync(Guid cursoAlvoId, int quantidade = 5)
        {
            _logger.LogInformation($"[RECSYS] Iniciando extração e busca de recomendações para o curso {cursoAlvoId}.");

            // Extraindo as informações do MySQL sem utilizar o campo VetorSemantico, conforme regra
            var cursos = await _context.Cursos
                .Select(c => new CourseFeatureDto
                {
                    Id = c.Id.ToString(),
                    TextContent = $"{c.Titulo} {c.DescricaoOriginal ?? string.Empty}".Trim(),
                    TrustScore = c.TrustScore ?? 0.0
                })
                .ToListAsync();

            if (!cursos.Any(c => c.Id == cursoAlvoId.ToString()))
            {
                _logger.LogWarning($"[RECSYS] Curso alvo {cursoAlvoId} não encontrado.");
                return new List<Guid>();
            }

            var requestPayload = new RecommendationRequestDto
            {
                TargetCourseId = cursoAlvoId.ToString(),
                Courses = cursos,
                K = quantidade
            };

            var apiKey = _configuration["REC_API_KEY"] ?? "default-secret-key";
            var recUrl = _configuration["REC_API_URL"] ?? "http://recsys_api:8001/recommend/";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestPayload), 
                Encoding.UTF8, 
                "application/json");

            _logger.LogInformation($"[RECSYS] Disparando requisição HTTP POST ({cursos.Count} cursos empacotados) para a IA em {recUrl} ...");
            
            var response = await _httpClient.PostAsync(recUrl, jsonContent);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                _logger.LogError($"[RECSYS] Falha na API Python de recomendação: {response.StatusCode} - {errorMsg}");
                return new List<Guid>();
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RecommendationResponseDto>(jsonString);

            if (result == null || !result.Success)
            {
                _logger.LogError($"[RECSYS] Erro de processamento na API Python: {result?.Error}");
                return new List<Guid>();
            }

            var recommendedIds = result.Recommendations
                .OrderBy(r => r.Distance) // Quanto menor a distância, mais similar
                .Select(r => Guid.Parse(r.CourseId))
                .ToList();

            _logger.LogInformation($"[RECSYS] Recebidos {recommendedIds.Count} cursos mais similares com sucesso.");

            return recommendedIds;
        }
    }
}
