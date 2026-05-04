#pragma warning disable OPENAI001
using Google.GenAI.Types;
using OpenAI.Responses;
using System.Text.Json;

namespace WinForm_Chat_M3
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

        public Form1()
        {
            InitializeComponent();
            // set initial model from combo
            currentModel = cmbModel.SelectedItem?.ToString() ?? currentModel;

            dtTools = new DateTimeTools();
            tavily = new TavilySearch();
            sqlTools = new SQLTools();

            gemini = new Gemini_Tools(model: currentModel);
            openAI = new OpenAI_Tools(model: currentModel);

            AppendToHistory($"Model set to: {currentModel}\n");
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
                    var parts = response.Candidates[0].Content.Parts;

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
                    }

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

                        var textPart = response.Candidates[0].Content.Parts.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Text));
                        AppendToHistory(textPart?.Text ?? string.Empty + "\n");
                        finalResponsePrinted = true;
                        break;
                    }
                    response = await gemini.Call(new List<Content> { toolOutputMessage });

                    if (!finalResponsePrinted)
                    {
                        Console.WriteLine("Max iterations reached.\n");
                    }
                }
                AppendToHistory(string.Empty + "\n");
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
                            var call = (FunctionCallResponseItem)item;
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
                        AppendToHistory(response.GetOutputText() ?? string.Empty + "\n");
                        finalResponsePrinted = true;
                        break;
                    }

                    response = await openAI.Call(toolOutputs);

                    if (!finalResponsePrinted)
                    {
                        Console.WriteLine("Max iterations reached.");
                    }
                }
                AppendToHistory(string.Empty + "\n");
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
        }
    }
}
