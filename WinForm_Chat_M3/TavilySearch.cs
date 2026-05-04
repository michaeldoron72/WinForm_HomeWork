using System.Net.Http;
using System.Text;
using System.Text.Json;

public class TavilySearch
{
    private readonly HttpClient client = new();
    private readonly string? apiKey;

    public TavilySearch(string? apiKey = null)
    {
        this.apiKey = Environment.GetEnvironmentVariable("TavilyAPIKey");
    }

    public async Task<string> Search(string query, int maxResults = 5)
    {
        var body = new
        {
            api_key = apiKey,
            query = query,
            max_results = maxResults
        };

        var json = JsonSerializer.Serialize(body);

        var response = await client.PostAsync(
            "https://api.tavily.com/search",
            new StringContent(json, Encoding.UTF8, "application/json"));

        return await response.Content.ReadAsStringAsync();
    }
}

