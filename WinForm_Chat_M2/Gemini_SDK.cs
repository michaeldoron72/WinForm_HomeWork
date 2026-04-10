using Google.GenAI;
using Google.GenAI.Types;

public class Gemini_SDK
{
    private readonly Client GeminiModel;
    private readonly string Model;
    private readonly List<Content> history = new();

    public Gemini_SDK(string model)
    {
        GeminiModel = new Client();
        Model = model;
    }

    public async Task<string> Call(string userMessage)
    {
        history.Add(new Content { Role = "user", Parts = [new Part { Text = userMessage }] });

        var response = await this.GeminiModel.Models.GenerateContentAsync(
            model: this.Model, contents: history);

        var text = response.Candidates[0].Content.Parts[0].Text;

        history.Add(new Content { Role = "model", Parts = [new Part { Text = text }] });
        return text;
    }
}
