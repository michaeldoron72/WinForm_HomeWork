using Google.GenAI;
using Google.GenAI.Types;
using System.Text.Json;

public class Gemini_Tools
{
    private readonly Client GeminiModel;
    private string model;
    private readonly List<Content> history = new();
    private readonly string? systemPrompt;
    private readonly List<Tool>? tools;
    private Tool tool;
    public Gemini_Tools(string model)
    {
        Initailize();
        GeminiModel = new Client();
        this.model = model;
        this.systemPrompt = GetsystemPrompt();
        this.tools = new List<Tool> { tool };
    }

    private void Initailize()
    {
        var noParamsSchema = new Schema { Type = Google.GenAI.Types.Type.Object };

        var tavilySchema = new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Properties = new Dictionary<string, Schema>
            {
                ["query"] = new Schema { Type = Google.GenAI.Types.Type.String }
            },
            Required = new List<string> { "query" }
        };

        var sqlSchema = new Schema
        {
            Type = Google.GenAI.Types.Type.Object,
            Properties = new Dictionary<string, Schema>
            {
                ["sql"] = new Schema { Type = Google.GenAI.Types.Type.String }
            },
            Required = new List<string> { "sql" }
        };

        var getDate = new FunctionDeclaration
        {
            Name = "GetDate",
            Description = "Get today's date",
            Parameters = noParamsSchema
        };

        var getTime = new FunctionDeclaration
        {
            Name = "GetTime",
            Description = "Get the current time",
            Parameters = noParamsSchema
        };

        var tavilySearch = new FunctionDeclaration
        {
            Name = "TavilySearch",
            Description = "Search the web and return results for a query",
            Parameters = tavilySchema
        };

        var getSchema = new FunctionDeclaration
        {
            Name = "GetSchema",
            Description = "Get the structure of the SQL database",
            Parameters = noParamsSchema
        };

        var retrieveTable = new FunctionDeclaration
        {
            Name = "RetrieveTable",
            Description = "Run a SELECT query on the SQL database and return the result as JSON",
            Parameters = sqlSchema
        };

        var executeNonQuery = new FunctionDeclaration
        {
            Name = "ExecuteNonQuery",
            Description = "Run INSERT, UPDATE, or DELETE on the SQL database " +
            "and return the number of affected rows",
            Parameters = sqlSchema
        };

        tool = new Tool
        {
            FunctionDeclarations = [getDate, getTime, tavilySearch, getSchema, retrieveTable, executeNonQuery]
        };

    }

    public Task<GenerateContentResponse> Call(string userMessage)
    {
        var newItems = new List<Content>
        {
            new Content { Role = "user", Parts = [ new Part { Text = userMessage } ] }
        };

        return Call(newItems);
    }

    public async Task<GenerateContentResponse> Call(List<Content> newItems)
    {
        foreach (var item in newItems)
        {
            history.Add(item);
        }

        var config = CreateConfig();

        var response = await GeminiModel.Models.GenerateContentAsync(model: model, contents: history, config: config);

        // Save assistant text (tool flow will be handled by caller)
        if (response.Candidates.Count > 0 && response.Candidates[0].Content is not null)
        {
            history.Add(response.Candidates[0].Content);
        }

        return response;
    }

    private GenerateContentConfig CreateConfig()
    {
        var config = new GenerateContentConfig();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            config.SystemInstruction = new Content { Parts = [ new Part { Text = systemPrompt } ] };
        }

        if (tools is not null)
        {
            config.Tools = tools;
        }

        return config;
    }

    public void UpdateModel(string currentModel)
    {
        this.model = currentModel;
    }

    public string GetsystemPrompt()
    {
        string systemPrompt = """
                You may call tools when needed.
                Use GetDate to get today's date.
                Use GetTime to get the current time.
                Use TavilySearch when you need to search the web.
                Use GetSchema to understand the SQL database structure.
                Use RetrieveTable to run SELECT queries on the SQL database.
                Use ExecuteNonQuery only when the user explicitly asks to change data in the SQL database.
                """;
        return systemPrompt;
    }
}
