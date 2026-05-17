using EduqPlus.API.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EduqPlus.API.Services;

public class IaService : IIaService {
    private readonly Kernel _kernel;

    public IaService(Kernel kernel) {
        _kernel = kernel;
    }

    public async Task<string> GerarResumoReputacaoAsync(IEnumerable<string> comentarios) {
        var chatHistory = new ChatHistory();

        var textoComentarios = string.Join(" | ", comentarios);

        chatHistory.AddSystemMessage("Você é um analista de qualidade de cursos online da plataforma Eduq+. " +
            "Sua tarefa é ler vários comentários de alunos e gerar um resumo único, imparcial e profissional. " +
            "Destaque os pontos fortes e os pontos de atenção. Seja direto e use no máximo 4 parágrafos." +
            "Sempre que for topificar os pontos fortes e fracos se refira dessa forma: 'Pontos Fortes:' 'Pontos de atenção:'.");

        chatHistory.AddUserMessage($"Analise os seguintes comentários de alunos: {textoComentarios}");

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        return result.Content ?? "Não foi possível gerar um resumo no momento.";
    }

    public async Task<bool> VerificarIntencaoQualidadeAsync(string query) {
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage(
            "Você é um classificador binário rígido. Responda APENAS 'SIM' ou 'NAO'. " +
            "Analise se o usuário quer filtragem por QUALIDADE (notas, melhores, reputação). " +
            "IMPORTANTE: Se houver um termo de qualidade (bom, melhor, nota, top), a resposta deve ser SIM, " +
            "mesmo que a frase comece com 'me traga' ou 'quais'. " +
            "Exemplos SIM: 'me traga os melhores de ...', 'cursos com boas notas', 'quais são os mais confiáveis'. " +
            "Exemplos NAO: 'me traga cursos de ...', 'quais cursos de C# existem', 'lista de ...'.");

        chatHistory.AddUserMessage($"O usuário busca explicitamente por qualidade ou confiança nesta busca: \"{query}\"?");

        var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        var textResponse = result.Content?.Trim().ToUpper() ?? "";

        return textResponse.StartsWith("SIM") || textResponse.Contains("SIM");
    }

    public async Task<float[]> GerarEmbeddingAsync(string texto) {
        var embeddingGenerator = _kernel.GetRequiredService<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>();
        var embeddings = await embeddingGenerator.GenerateAsync(new[] { texto });
        return embeddings[0].Vector.ToArray();
    }
}