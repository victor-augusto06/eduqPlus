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
            "Destaque os pontos fortes e os pontos de atenção. Seja direto e use no máximo 4 parágrafos.");

        chatHistory.AddUserMessage($"Analise os seguintes comentários de alunos: {textoComentarios}");

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

        var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        return result.Content ?? "Não foi possível gerar um resumo no momento.";
    }
}