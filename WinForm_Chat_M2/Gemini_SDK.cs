using Google.GenAI;
using Google.GenAI.Types;
using System.Text;

public class Gemini_SDK
{
    private readonly Client GeminiModel;
    private readonly string Model;
    private readonly List<Content> history = new();
    private readonly string? SystemPrompt;
    private readonly GenerateContentConfig config;

    public Gemini_SDK(string model, string? systemPrompt = null)
    {
        GeminiModel = new Client();
        Model = model;
        SystemPrompt = systemPrompt;

        config = new GenerateContentConfig
        {
            Temperature = 0.7f, // Creativity (higher = more random). The 'f' suffix makes this a float literal (single-precision)
            TopP = 0.95f, // pick next tokens only from the smallest set whose cumulative probability ≥ 0.95 (filters unlikely tokens)
            TopK = 40, // Top-K sampling
            //MaxOutputTokens = 256, // Max tokens in the response
            CandidateCount = 1, // Number of candidates to generate
            StopSequences = new List<string> { "END" }, // Stop tokens
            PresencePenalty = 0.0f, // Penalize new topic repetition
            FrequencyPenalty = 0.0f, // Penalize frequent tokens
            Seed = 0, // Deterministic sampling seed
            ResponseMimeType = "text/plain" // Response format
        };

        if (!string.IsNullOrEmpty(SystemPrompt))
        {
            config.SystemInstruction = new Content { Parts = [new Part { Text = SystemPrompt }] };
        }
    }

    public async Task<string> Call(string userMessage)
    {
        history.Add(new Content { Role = "user", Parts = [new Part { Text = userMessage }] });

        var response = await GeminiModel.Models.GenerateContentAsync(
            model: Model, contents: history, config: config
        );
        var text = response.Candidates[0].Content.Parts[0].Text;
        history.Add(new Content { Role = "model", Parts = [new Part { Text = text }] });
        return text;
    }

    public async IAsyncEnumerable<string> CallStream(string userMessage)
    {
        history.Add(new Content { Role = "user", Parts = [new Part { Text = userMessage }] });

        var sb = new StringBuilder();

        var stream = GeminiModel.Models.GenerateContentStreamAsync(
            model: Model,
            contents: history,
            config: config);

        await foreach (var chunk in stream)
        {
            var text = chunk.Candidates?[0].Content?.Parts?[0].Text;
            if (!string.IsNullOrEmpty(text))
            {
                sb.Append(text);
                yield return text;
            }
        }

        //await foreach (var chunk in GeminiModel.Models.GenerateContentStreamAsync(
        //    model: Model, contents: history, config: config))
        //{
        //    var text = chunk.Candidates?[0].Content?.Parts?[0].Text;
        //    if (!string.IsNullOrEmpty(text))
        //    {
        //        sb.Append(text);
        //        yield return text;
        //    }
        //}

        history.Add(new Content { Role = "model", Parts = [new Part { Text = sb.ToString() }] });
    }
}