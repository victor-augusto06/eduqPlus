using System.Text;
using System.Text.Json;
using EduqPlus.API.DTOs;
using EduqPlus.API.Interfaces;
using EduqPlus.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EduqPlus.API.Service
{
    public class RecommendationService : IRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecommendationService> _logger;
        private readonly EduqPlusContext _context;
        private readonly IMemoryCache? _cache;

        public RecommendationService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            ILogger<RecommendationService> logger,
            EduqPlusContext context,
            IMemoryCache? cache = null)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public async Task<List<Guid>> ObterCursosRecomendadosAsync(Guid cursoAlvoId, int quantidade = 3)
        {
            var cacheKey = $"recsys_ids_{cursoAlvoId}_{quantidade}";
            if (_cache != null && _cache.TryGetValue(cacheKey, out List<Guid>? cachedIds) && cachedIds != null)
            {
                _logger.LogInformation($"[RECSYS] Recomendações recuperadas do cache em memória para o curso {cursoAlvoId}.");
                return cachedIds;
            }

            _logger.LogInformation($"[RECSYS] Iniciando busca de recomendações para o curso {cursoAlvoId}.");

            var requestPayload = new RecommendationRequestDto
            {
                TargetCourseId = cursoAlvoId.ToString(),
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

            _logger.LogInformation($"[RECSYS] Disparando requisição HTTP POST para a IA em {recUrl} ...");
            
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
                .OrderBy(r => r.Distance)
                .Take(quantidade)
                .Select(r => Guid.Parse(r.CourseId))
                .ToList();

            _logger.LogInformation($"[RECSYS] Recebidos {recommendedIds.Count} cursos mais similares com sucesso.");

            if (_cache != null && recommendedIds.Any())
            {
                _cache.Set(cacheKey, recommendedIds, TimeSpan.FromMinutes(20));
            }

            return recommendedIds;
        }
    }
}
