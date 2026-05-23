#pragma warning disable OPENAI001
using Google.GenAI.Types;
using OpenAI.Responses;
using System.ComponentModel;
using System.Text.Json;

namespace WinForm_Chat_M4
{
    public partial class Form1 : Form
    {
        // No persistent SDK instance; create per-call to allow changing model easily
        private bool isBusy = false;
        private string currentModel = "gpt-5-mini";
        // (Select button removed) currentModel is updated directly from combo
        private Gemini_Tools gemini;
        private OpenAI_Tools openAI;
        private DateTimeTools dtTools;
        private TavilySearch tavily;
        private SQLTools sqlTools;
        private string? ContainerId { get; set; }
        private int image_counter = 0;

        public Form1()
        {
            InitializeComponent();
            // set initial model from combo
            currentModel = cmbModel.SelectedItem?.ToString() ?? currentModel;
            cmbModel.Text = currentModel;

            dtTools = new DateTimeTools();
            tavily = new TavilySearch();
            sqlTools = new SQLTools();

            gemini = new Gemini_Tools(model: currentModel);
            openAI = new OpenAI_Tools(model: currentModel);

            AppendToHistory($"Model set to: {currentModel}\n");

            if (currentModel.Contains("imagen"))
            {
                AppendToHistory("\nWelcome to Google Image Creator. Write your prompt and press Enter or click Send.\n");
            }
            else if (currentModel.Contains("gpt-image"))
            {
                AppendToHistory("\nWelcome to OpenAI Image Creator. Write your prompt and press Enter or click Send.\n");
            }
            else
            {
                AppendToHistory("\nWelcome to the chat! Write your message and press Enter or click Send.\n");
            }
        }

        private async void btnSend_Click(object? sender, EventArgs e)
        {
            await SendMessageAsync();
        }

        private async void txtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            var message = txtInput.Text?.Trim();
            txtInput.Clear();

            if (isBusy || string.IsNullOrEmpty(message)) return;

            isBusy = true;
            AppendToHistory("You: " + message + "\n");
            var selectedModel = currentModel;
            //string reply;
            AppendToHistory("AI: ");
            if (string.IsNullOrWhiteSpace(message)) return;

            const int maxSteps = 5;

            if (currentModel.Contains("gemini"))
            {
                var response = await gemini.Call(message);
                bool finalResponsePrinted = false;

                for (int step = 0; step < maxSteps; step++)
                {
                    var content = response.Candidates[0].Content;
                    var parts = content.Parts;

                    int count = 0;
                    var toolOutputMessage = new Content { Role = "user", Parts = new List<Part>() };

                    foreach (var part in parts)
                    {
                        if (part.FunctionCall is not null)
                        {
                            count++;
                            var call = part.FunctionCall;
                            string toolResult;

                            try
                            {
                                if (call.Name == "GetDate")
                                {
                                    toolResult = dtTools.GetDate();
                                }
                                else if (call.Name == "GetTime")
                                {
                                    toolResult = dtTools.GetTime();
                                }
                                else if (call.Name == "TavilySearch")
                                {
                                    var query = call.Args["query"].ToString();
                                    toolResult = await tavily.Search(query);
                                }
                                else if (call.Name == "GetSchema")
                                {
                                    toolResult = sqlTools.GetSchema();
                                }
                                else if (call.Name == "RetrieveTable")
                                {
                                    var sql = call.Args["sql"].ToString();
                                    toolResult = sqlTools.RetrieveTable(sql);
                                }
                                else if (call.Name == "ExecuteNonQuery")
                                {
                                    var sql = call.Args["sql"].ToString();
                                    toolResult = sqlTools.ExecuteNonQuery(sql).ToString();
                                }
                                else
                                {
                                    toolResult = "Unknown tool: " + call.Name;
                                }
                            }
                            catch (Exception ex)
                            {
                                toolResult = "Tool error: " + ex.Message;
                            }
                            finally
                            {
                                isBusy = false;
                            }

                            toolOutputMessage.Parts.Add(new Part
                            {
                                FunctionResponse = new FunctionResponse
                                {
                                    Name = call.Name,
                                    Response = new Dictionary<string, object> { { "result", toolResult } }
                                }
                            });
                        }
                        else if (part.ExecutableCode is not null || part.CodeExecutionResult is not null)
                        {
                            count++;
                        }
                    }

                    SaveGeminiFiles(content);

                    if (count == 0)
                    {
                        isBusy = false;
                        var textPart = response.Candidates[0].Content.Parts.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Text));
                        AppendToHistory(textPart?.Text ?? string.Empty + "\n");
                        finalResponsePrinted = true;
                        break;
                    }

                    if (step == maxSteps - 1)
                    {
                        isBusy = false;
                        toolOutputMessage.Parts.Add(new Part
                        {
                            Text = "Max tool steps reached. No more tool calls are allowed. Reply normally with your best final answer using the information you already have.\n"
                        });
                        response = await gemini.Call(new List<Content> { toolOutputMessage });
                        SaveGeminiFiles(response.Candidates[0].Content);

                        var textPart = response.Candidates[0].Content.Parts.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Text));
                        AppendToHistory(textPart?.Text ?? string.Empty + "\n");
                        finalResponsePrinted = true;
                        break;
                    }
                    response = toolOutputMessage.Parts.Count > 0
                        ? await gemini.Call(new List<Content> { toolOutputMessage })
                        : await gemini.Call(new List<Content>());

                }
                AppendToHistory(string.Empty + "\n");
            }
            else if (currentModel.Contains("gpt-image"))
            {
                var imgGen = new OpenAI_Img();
                string generatedImageName = $"generatedImage_{image_counter}.png";
                while (imgGen.IsExist(generatedImageName))
                {
                    image_counter++;
                    generatedImageName = $"generatedImage_{image_counter}.png";
                }
                await imgGen.GenerateImageAsync(message, generatedImageName);
                AppendToHistory("Image " + generatedImageName + " is created successfully!\n");
                isBusy = false;
                image_counter++;
            }
            else if (currentModel.Contains("gpt"))
            {
                var response = await openAI.Call(message);
                bool finalResponsePrinted = false;

                for (int step = 0; step < maxSteps; step++)
                {
                    int count = 0;
                    var toolOutputs = new List<ResponseItem>();

                    foreach (var item in response.OutputItems)
                    {
                        if (item is FunctionCallResponseItem)
                        {
                            count++;
                        }
                        if (item is FunctionCallResponseItem call)
                        {
                            count++;
                            string toolResult;

                            try
                            {
                                if (call.FunctionName == "GetDate")
                                {
                                    toolResult = dtTools.GetDate();
                                }
                                else if (call.FunctionName == "GetTime")
                                {
                                    toolResult = dtTools.GetTime();
                                }
                                else if (call.FunctionName == "TavilySearch")
                                {
                                    var argsStr = call.FunctionArguments.ToString();
                                    var args = JsonSerializer.Deserialize<Dictionary<string, string>>(argsStr);
                                    var query = args["query"];
                                    toolResult = await tavily.Search(query);
                                }
                                else if (call.FunctionName == "GetSchema")
                                {
                                    toolResult = sqlTools.GetSchema();
                                }
                                else if (call.FunctionName == "RetrieveTable")
                                {
                                    var argsStr = call.FunctionArguments.ToString();
                                    var args = JsonSerializer.Deserialize<Dictionary<string, string>>(argsStr);
                                    var sql = args["sql"];
                                    toolResult = sqlTools.RetrieveTable(sql);
                                }
                                else if (call.FunctionName == "ExecuteNonQuery")
                                {
                                    var argsStr = call.FunctionArguments.ToString();
                                    var args = JsonSerializer.Deserialize<Dictionary<string, string>>(argsStr);
                                    var sql = args["sql"];
                                    toolResult = sqlTools.ExecuteNonQuery(sql).ToString();
                                }
                                else
                                {
                                    toolResult = "Unknown tool: " + call.FunctionName;
                                }
                            }
                            catch (Exception ex)
                            {
                                toolResult = "Tool error: " + ex.Message;
                            }

                            toolOutputs.Add(ResponseItem.CreateFunctionCallOutputItem(call.CallId, toolResult));
                        }
                    }

                    if (count == 0)
                    {
                        isBusy = false;
                        AppendToHistory(response.GetOutputText() ?? string.Empty + "\n");
                        finalResponsePrinted = true;
                        break;
                    }

                    if (step == maxSteps - 1)
                    {
                        isBusy = false;
                        toolOutputs.Add(ResponseItem.CreateUserMessageItem(
                            "Max tool steps reached. No more tool calls are allowed. Reply normally with your best final answer using the information you already have."));

                        response = await openAI.Call(toolOutputs);
                        SaveOpenAIFiles(response);
                        AppendToHistory(response.GetOutputText() ?? string.Empty + "\n");
                        finalResponsePrinted = true;
                        break;
                    }

                    response = await openAI.Call(toolOutputs);
                }
                AppendToHistory(string.Empty + "\n");
            }
            else if (currentModel.Contains("imagen"))
            {
                var imgGen = new GoogleImg();
                string generatedImageName = $"generatedImage_{image_counter}.png";
                while (imgGen.IsExist(generatedImageName))
                {
                    image_counter++;
                    generatedImageName = $"generatedImage_{image_counter}.png";
                }
                await imgGen.GenerateImageAsync(message, generatedImageName);
                AppendToHistory("Image " + generatedImageName + " is created successfully!\n");
                isBusy = false;
                image_counter++;
            }
        }

        private void AppendToHistory(string text)
        {
            if (InvokeRequired)
            {
                Invoke(() => AppendToHistory(text));
                return;
            }

            rtbHistory.AppendText(text);
            rtbHistory.ScrollToCaret();
        }
        // btnSelectModel was removed from the UI; no handler needed

        private void cmbModel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selected = cmbModel.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;

            if (selected == currentModel) return; // no change

            currentModel = selected;
            gemini.UpdateModel(currentModel);

            AppendToHistory($"Model set to: {currentModel}\n");

            if (currentModel.Contains("imagen"))
            {
                AppendToHistory("\nWelcome to Google Image Creator. Write your prompt and press Enter or click Send.\n");
            }
            else if (currentModel.Contains("gpt-image"))
            {
                AppendToHistory("\nWelcome to OpenAI Image Creator. Write your prompt and press Enter or click Send.\n");
            }
            else
            {
                AppendToHistory("\nWelcome to the chat! Write your message and press Enter or click Send.\n");
            }
        }

        private static void SaveGeminiFiles(Content content)
        {
            if (content.Parts is null) return;

            foreach (var part in content.Parts)
            {
                if (part.InlineData is not null
                    && part.InlineData.MimeType is string mime
                    && mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                    && part.InlineData.Data is byte[] bytes
                    && bytes.Length > 0)
                {
                    SaveGeminiImage(bytes, mime);
                }
            }
        }

        private void SaveOpenAIFiles(ResponseResult response)
        {
            foreach (var item in response.OutputItems)
            {
                if (item is CodeInterpreterCallResponseItem ci)
                {
                    if (!string.IsNullOrWhiteSpace(ci.ContainerId))
                    {
                        ContainerId = ci.ContainerId;
                    }

                    foreach (var output in ci.Outputs)
                    {
                        if (output is CodeInterpreterCallImageOutput image)
                        {
                            SaveOpenAIImage(image.ImageUri, ci.ContainerId);
                        }
                    }
                }
                else if (item is MessageResponseItem msg)
                {
                    foreach (var part in msg.Content)
                    {
                        foreach (var annotation in part.OutputTextAnnotations)
                        {
                            if (annotation is not ContainerFileCitationMessageAnnotation file)
                            {
                                continue;
                            }

                            if (!string.IsNullOrWhiteSpace(file.ContainerId))
                            {
                                ContainerId = file.ContainerId;
                            }

                            SaveContainerFile(file.ContainerId, file.FileId, file.Filename);
                        }

                        var imageUriProperty = part.GetType().GetProperty("ImageUri");
                        if (imageUriProperty?.GetValue(part) is Uri imageUri)
                        {
                            SaveOpenAIImage(imageUri, ContainerId);
                        }
                    }
                }
            }
        }

        private static void SaveGeminiImage(byte[] bytes, string mime)
        {
            var ext = mime.Split('/').Last();
            var fileName = $"plot_{DateTime.Now:yyyyMMdd_HHmmss_fff}.{ext}";
            var folder = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Plots"));
            Directory.CreateDirectory(folder);
            var fullPath = Path.Combine(folder, fileName);
            System.IO.File.WriteAllBytes(fullPath, bytes);
            Console.WriteLine($"[Image saved: {fullPath}]");
        }

        private static void SaveOpenAIImage(Uri imageUri, string? containerId)
        {
            try
            {
                var path = imageUri.AbsolutePath;
                var ext = Path.GetExtension(path);
                if (string.IsNullOrEmpty(ext)) ext = ".png";

                using var http = new HttpClient();
                var apiKey = System.Environment.GetEnvironmentVariable("OpenAIKey");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    http.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                }
                if (!string.IsNullOrEmpty(containerId))
                {
                    http.DefaultRequestHeaders.Add("OpenAI-Container", containerId);
                }

                var bytes = http.GetByteArrayAsync(imageUri).GetAwaiter().GetResult();

                var fileName = $"plot_{DateTime.Now:yyyyMMdd_HHmmss_fff}{ext}";
                var folder = Path.GetFullPath(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Plots"));
                Directory.CreateDirectory(folder);
                var fullPath = Path.Combine(folder, fileName);
                System.IO.File.WriteAllBytes(fullPath, bytes);
                Console.WriteLine($"[Image saved: {fullPath}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Image save failed: {ex.Message}]");
            }
        }

        private static void SaveContainerFile(string? containerId, string? fileId, string? filename)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(containerId) || string.IsNullOrWhiteSpace(fileId))
                {
                    Console.WriteLine("[File save failed: missing container id or file id]");
                    return;
                }

                using var http = new HttpClient();
                var apiKey = System.Environment.GetEnvironmentVariable("OpenAIKey");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    http.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                }

                http.DefaultRequestHeaders.Add("OpenAI-Container", containerId);

                var uri = new Uri($"https://api.openai.com/v1/containers/{containerId}/files/{fileId}/content");
                var bytes = http.GetByteArrayAsync(uri).GetAwaiter().GetResult();

                var safeName = string.IsNullOrWhiteSpace(filename)
                    ? $"file_{DateTime.Now:yyyyMMdd_HHmmss_fff}.bin"
                    : Path.GetFileName(filename);

                var folder = Path.GetFullPath(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Plots"));
                Directory.CreateDirectory(folder);

                var fullPath = Path.Combine(folder, $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_{safeName}");
                System.IO.File.WriteAllBytes(fullPath, bytes);
                Console.WriteLine($"[File saved: {fullPath}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[File save failed: {ex.Message}]");
            }
        }
    }
}
