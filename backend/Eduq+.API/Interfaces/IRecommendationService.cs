namespace EduqPlus.API.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<Guid>> ObterCursosRecomendadosAsync(Guid cursoAlvoId, int quantidade = 5);
    }
}
