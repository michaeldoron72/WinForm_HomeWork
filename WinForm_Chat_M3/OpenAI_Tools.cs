#pragma warning disable OPENAI001
using OpenAI.Responses;
using System.Text.Json;

public class OpenAI_Tools
{
    private readonly ResponsesClient GPTModel;
    private readonly List<ResponseItem> history = new();
    private string model;
    private readonly string? systemPrompt;
    private List<ResponseTool>? tools;
    private readonly string? schema;

    public OpenAI_Tools(string model, string? schema = null)
    {
        Initailize();
        var OpenAIKey = Environment.GetEnvironmentVariable("OpenAIKey");
        GPTModel = new ResponsesClient(OpenAIKey);
        this.model = model;
        this.systemPrompt = GetsystemPrompt();
        this.tools = tools;
        this.schema = schema;
    }

    private void Initailize()
    {
        var noParamsSchema = BinaryData.FromString(
            """
            {
                "type":"object", 
                "properties":{}, 
                "required":[], 
                "additionalProperties":false 
            }
            """);

        var tavilySchema = BinaryData.FromString(
            """
            { 
                "type":"object", 
                "properties":{"query":{"type":"string"}}, 
                "required":["query"], 
                "additionalProperties":false 
            }
            """);

        var sqlSchema = BinaryData.FromString(
            """
            {
                "type":"object",
                "properties":{"sql":{"type":"string"}},
                "required":["sql"],
                "additionalProperties":false
            }
            """);

        var getDateTool = ResponseTool.CreateFunctionTool(
            functionName: "GetDate",
            functionParameters: noParamsSchema,
            strictModeEnabled: true,
            functionDescription: "Get today's date"
            );

        var getTimeTool = ResponseTool.CreateFunctionTool(
            functionName: "GetTime",
            functionParameters: noParamsSchema,
            strictModeEnabled: true,
            functionDescription: "Get the current time");

        var tavilyTool = ResponseTool.CreateFunctionTool(
            functionName: "TavilySearch",
            functionParameters: tavilySchema,
            strictModeEnabled: true,
            functionDescription: "Search the web and return results for a query");

        var getSchemaTool = ResponseTool.CreateFunctionTool(
            functionName: "GetSchema",
            functionParameters: noParamsSchema,
            strictModeEnabled: true,
            functionDescription: "Get the structure of the SQL database");

        var retrieveTableTool = ResponseTool.CreateFunctionTool(
            functionName: "RetrieveTable",
            functionParameters: sqlSchema,
            strictModeEnabled: true,
            functionDescription: "Run a SELECT query on the SQL database and return the result as JSON");

        var executeNonQueryTool = ResponseTool.CreateFunctionTool(
            functionName: "ExecuteNonQuery",
            functionParameters: sqlSchema,
            strictModeEnabled: true,
            functionDescription: "Run INSERT, UPDATE, or DELETE on the SQL database " +
            "and return the number of affected rows");

        tools = new List<ResponseTool>
        {
                getDateTool,
                getTimeTool,
                tavilyTool,
                getSchemaTool,
                retrieveTableTool,
                executeNonQueryTool
        };
    }

    public Task<ResponseResult> Call(string userMessage)
    {
        var newItems = new List<ResponseItem> { ResponseItem.CreateUserMessageItem(userMessage) };
        return Call(newItems);
    }

    public async Task<ResponseResult> Call(List<ResponseItem> newItems)
    {
        foreach (var item in newItems)
        {
            history.Add(item);
        }

        var config = CreateConfig();

        ResponseResult response = await GPTModel.CreateResponseAsync(config);

        foreach (var item in response.OutputItems)
        {
            history.Add(item);
        }

        return response;
    }

    private CreateResponseOptions CreateConfig()
    {
        var config = new CreateResponseOptions
        {
            Model = model,
            Instructions = systemPrompt
        };

        foreach (var item in history)
        {
            config.InputItems.Add(item);
        }

        if (tools is not null)
        {
            foreach (var tool in tools)
            {
                config.Tools.Add(tool);
            }
        }

        if (schema is not null)
        {
            config.TextOptions = new ResponseTextOptions
            {
                TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "response_schema",
                    jsonSchema: BinaryData.FromString(schema),
                    jsonSchemaIsStrict: true)
            };
        }

        return config;
    }

    public void ClearHistory()
    {
        history.Clear();
    }

    public void UpdateModel(string currentModel)
    {
        this.model = currentModel;
    }

    public string GetsystemPrompt()
    {
        var systemPrompt = """
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
