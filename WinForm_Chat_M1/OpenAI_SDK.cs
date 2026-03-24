using OpenAI.Chat;

public class OpenAI_SDK
{
    private readonly ChatClient GPTModel;
    private readonly List<ChatMessage> history = new();

    public OpenAI_SDK(string model)
    {
        var OpenAIKey = Environment.GetEnvironmentVariable("OpenAIKey");
        GPTModel = new ChatClient(model, OpenAIKey);
    }

    public async Task<string> Call(string userMessage)
    {
        history.Add(new UserChatMessage(userMessage));
               
        var completion = await GPTModel.CompleteChatAsync(history);
        var text = completion.Value.Content[0].Text;

        history.Add(new AssistantChatMessage(text));
        return text;
    }
}
