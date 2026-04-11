#pragma warning disable OPENAI001
using OpenAI.Responses;
using System.Text;

public class OpenAI_SDK
{
    private readonly ResponsesClient GPTModel;
    private readonly List<ResponseItem> history = new();
    private readonly CreateResponseOptions config;

    public OpenAI_SDK(string model, string? systemPrompt = null)
    {
        var OpenAIKey = Environment.GetEnvironmentVariable("OpenAIKey");
        GPTModel = new ResponsesClient(OpenAIKey);

        config = new CreateResponseOptions
        {
            Model = model, // Model name to run (for example: gpt-5.2 / gpt-5-mini)
            //MaxOutputTokenCount = 512, // Upper bound for generated tokens in each response
            TruncationMode = ResponseTruncationMode.Auto, // Automatically trims old context if request becomes too large
            EndUserId = "user-1234", // Optional ID representing the end user, for OpenAI's monitoring and abuse detection systems

            ReasoningOptions = new ResponseReasoningOptions
            {
                ReasoningEffortLevel = ResponseReasoningEffortLevel.Medium, // Controls depth/cost of reasoning (low -> high)
                ReasoningSummaryVerbosity = ResponseReasoningSummaryVerbosity.Auto // How detailed the reasoning summary should be
            },
        };

        // in gpt 5.x models you can set these params only if ReasoningOptions is not set
        if (config.ReasoningOptions == null)
        {
            config.Temperature = 0.7f; // Sampling randomness (higher = more diverse/creative output)
            config.TopP = 0.95f; // sample from smallest token set with cumulative probability >= Top
        }
        // System Prompt
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            config.Instructions = systemPrompt;
        }
    }

    public async Task<string> Call(string userMessage)
    {
        history.Add(ResponseItem.CreateUserMessageItem(userMessage));

        config.InputItems.Clear();
        foreach (var item in history)
        {
            config.InputItems.Add(item);
        }

        ResponseResult response = await GPTModel.CreateResponseAsync(config);

        foreach (var item in response.OutputItems)
        {
            history.Add(item);
        }

        return response.GetOutputText();
    }

    public async IAsyncEnumerable<string> CallStream(string userMessage)
    {
        history.Add(ResponseItem.CreateUserMessageItem(userMessage));

        config.InputItems.Clear();
        foreach (var item in history)
        {
            config.InputItems.Add(item);
        }

        var sb = new StringBuilder();
        config.StreamingEnabled = true;
        await foreach (var update in GPTModel.CreateResponseStreamingAsync(config))
        {
            if (update is StreamingResponseOutputTextDeltaUpdate textDelta)
            {
                sb.Append(textDelta.Delta);
                yield return textDelta.Delta;
            }
        }
        config.StreamingEnabled = false;

        history.Add(ResponseItem.CreateAssistantMessageItem(sb.ToString()));
    }
}