using EduqPlus.API.Enums;
using EduqPlus.API.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EduqPlus.API.Services;

public class IaService : IIaService {
    private readonly Kernel _kernel;
    private readonly ILogger<IaService> _logger;

    public IaService(Kernel kernel, ILogger<IaService> logger) {
        _kernel = kernel;
        _logger = logger;
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

    public async Task<EStatusComprovante> ValidarComprovanteAsync(string textoOcr, string nomeCurso) {
        _logger.LogInformation($"[IA SERVICE] Iniciando validação de comprovante para o curso: '{nomeCurso}'");

        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage(
            "Você é um auditor rigoroso de um sistema de avaliações de cursos online. " +
            "Sua tarefa é classificar o comprovante abaixo em relação ao curso indicado. " +
            "Responda APENAS 'APROVADO' se o documento for autêntico e fizer menção clara ao curso ou à plataforma de vendas. " +
            "Responda APENAS 'REJEITADO' se o documento estiver legível mas NÃO contiver nenhuma menção ao curso avaliado, ou se for claramente de outra instituição, boleto aleatório ou fraude. " +
            "Responda APENAS 'PENDENTE' EXCLUSIVAMENTE se o texto extraído estiver muito borrado, incompleto ou ilegível a ponto de impedir a leitura das informações principais. " +
            "Não adicione nenhuma justificativa, formatação ou ponto final."
        );

        chatHistory.AddUserMessage($"Curso: {nomeCurso}\n\nTexto do Comprovante:\n{textoOcr}");

        _logger.LogInformation("[IA SERVICE] Prompt montado. Enviando para o modelo Llama3 aguardando resposta...");
        var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        var textResponse = result.Content?.Trim().ToUpper() ?? "";
        _logger.LogInformation($"[IA SERVICE] Resposta BRUTA recebida do modelo Llama3: '{textResponse}'");

        EStatusComprovante statusFinal = EStatusComprovante.Pendente;

        if (textResponse.Contains("APROVADO")) {
            statusFinal = EStatusComprovante.Aprovado;
        } else if (textResponse.Contains("REJEITADO") || textResponse.Contains("REPROVADO")) {
            statusFinal = EStatusComprovante.Rejeitado;
        }

        _logger.LogInformation($"[IA SERVICE] Avaliação final formatada para Enum: {statusFinal}");

        return statusFinal;
    }
}