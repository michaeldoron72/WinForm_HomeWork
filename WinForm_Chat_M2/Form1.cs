namespace WinForm_Chat_M2
{
    public partial class Form1 : Form
    {
        // No persistent SDK instance; create per-call to allow changing model easily
        private bool isBusy = false;
        private string currentModel = "gpt-5-mini";
        // (Select button removed) currentModel is updated directly from combo
        Gemini_SDK gemini;
        OpenAI_SDK openai;

        public Form1()
        {
            InitializeComponent();
            // set initial model from combo
            currentModel = cmbModel.SelectedItem?.ToString() ?? currentModel;
            gemini = new Gemini_SDK(currentModel);
            openai = new OpenAI_SDK(currentModel);

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
            if (isBusy) return;

            var text = txtInput.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                isBusy = true;
                AppendToHistory("You: " + text + "\n");
                txtInput.Clear();
                var selectedModel = currentModel;
                //string reply;
                AppendToHistory("AI: ");
                if (!string.IsNullOrEmpty(selectedModel) && selectedModel.IndexOf("gemini", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Stream response chunks from the chat model
                    await foreach (var chunk in gemini.CallStream(text))
                    {
                        AppendToHistory(chunk + "\n");
                    }
                }
                else
                {
                    // Stream response chunks from the chat model
                    await foreach (var chunk in openai.CallStream(text))
                    {
                        AppendToHistory(chunk + "\n");
                    }
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                AppendToHistory("Error: " + ex.Message + "\n");
            }
            finally
            {
                isBusy = false;
            }
        }

        private void AppendToHistory(string message)
        {
            if (InvokeRequired)
            {
                Invoke(() => AppendToHistory(message));
                return;
            }

            rtbHistory.AppendText(message);
            rtbHistory.ScrollToCaret();
        }
        // btnSelectModel was removed from the UI; no handler needed

        private void cmbModel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selected = cmbModel.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;

            if (selected == currentModel) return; // no change

            currentModel = selected;
            gemini = new Gemini_SDK(currentModel);
            openai = new OpenAI_SDK(currentModel);
            AppendToHistory($"Model set to: {currentModel}\n");
        }
    }
}
