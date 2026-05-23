using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WinForm_Chat_M4
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            rtbHistory = new RichTextBox();
            txtInput = new TextBox();
            btnSend = new Button();
            lblModel = new Label();
            cmbModel = new ComboBox();
            SuspendLayout();
            // 
            // rtbHistory
            // 
            rtbHistory.BackColor = Color.LightYellow;
            rtbHistory.Location = new Point(12, 44);
            rtbHistory.Name = "rtbHistory";
            rtbHistory.ReadOnly = true;
            rtbHistory.Size = new Size(776, 609);
            rtbHistory.TabIndex = 0;
            rtbHistory.Text = "";
            // 
            // txtInput
            // 
            txtInput.BackColor = Color.LightGray;
            txtInput.ForeColor = Color.Purple;
            txtInput.Location = new Point(12, 659);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(680, 27);
            txtInput.TabIndex = 1;
            txtInput.KeyDown += txtInput_KeyDown;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(698, 661);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(90, 27);
            btnSend.TabIndex = 2;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // lblModel
            // 
            lblModel.AutoSize = true;
            lblModel.Location = new Point(12, 14);
            lblModel.Name = "lblModel";
            lblModel.Size = new Size(55, 20);
            lblModel.TabIndex = 3;
            lblModel.Text = "Current Model:";
            // 
            // cmbModel
            // 
            cmbModel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbModel.FormattingEnabled = true;
            cmbModel.Items.AddRange(new object[] { "gpt-5-mini", "gpt-5-nano", "gemini-2.5-pro", "gemini-2.5-flash", "gemini-3.1-flash-lite-preview", "gemini-3.1-pro-preview", "imagen-4.0-generate-001", "gpt-image-1" });
            cmbModel.Location = new Point(120, 10);
            cmbModel.Name = "cmbModel";
            cmbModel.Size = new Size(200, 28);
            cmbModel.TabIndex = 4;
            cmbModel.SelectedIndexChanged += cmbModel_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 700);
            Controls.Add(btnSend);
            Controls.Add(txtInput);
            Controls.Add(rtbHistory);
            Controls.Add(cmbModel);
            Controls.Add(lblModel);
            Name = "Form1";
            Text = "AI Chat with history and tools include image tool ver 2.5";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbHistory;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label lblModel;
        private System.Windows.Forms.ComboBox cmbModel;
    }
}
