using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WinForm_Chat_M3
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
            components = new System.ComponentModel.Container();
            this.rtbHistory = new System.Windows.Forms.RichTextBox();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // rtbHistory
            // 
            this.rtbHistory.Location = new System.Drawing.Point(12, 44);
            this.rtbHistory.Name = "rtbHistory";
            this.rtbHistory.ReadOnly = true;
            this.rtbHistory.BackColor = System.Drawing.Color.LightYellow;
            this.rtbHistory.Size = new System.Drawing.Size(776, 609);
            this.rtbHistory.TabIndex = 0;
            this.rtbHistory.Text = string.Empty;
            // 
            // txtInput
            // 
            this.txtInput.Location = new System.Drawing.Point(12, 659);
            this.txtInput.Name = "txtInput";
            this.txtInput.BackColor = System.Drawing.Color.LightGray;
            this.txtInput.ForeColor = System.Drawing.Color.Purple;
            this.txtInput.Size = new System.Drawing.Size(680, 23);
            this.txtInput.TabIndex = 1;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(698, 661);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(90, 27);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // lblModel
            // 
            this.lblModel = new System.Windows.Forms.Label();
            this.lblModel.AutoSize = true;
            this.lblModel.Location = new System.Drawing.Point(12, 14);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(42, 15);
            this.lblModel.TabIndex = 3;
            this.lblModel.Text = "Model:";
            // 
            // cmbModel
            // 
            this.cmbModel = new System.Windows.Forms.ComboBox();
            this.cmbModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbModel.FormattingEnabled = true;
            this.cmbModel.Items.AddRange(new object[] {
            "gpt-5-mini",
            "gpt-5-nano",
            "gemini-2.5-pro",
            "gemini-2.5-flash",
            "gemini-3.1-flash-lite-preview",
            "gemini-3.1-pro-preview"});
            this.cmbModel.Location = new System.Drawing.Point(70, 10);
            this.cmbModel.Name = "cmbModel";
            this.cmbModel.Size = new System.Drawing.Size(200, 23);
            this.cmbModel.TabIndex = 4;
            this.cmbModel.SelectedIndex = 0;
            this.cmbModel.SelectedIndexChanged += new System.EventHandler(this.cmbModel_SelectedIndexChanged);
            // 
            // btnSelectModel
            // (Select button removed)
            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.Font;
            //ClientSize = new System.Drawing.Size(800, 415);
            ClientSize = new System.Drawing.Size(800, 700);
            Controls.Add(this.btnSend);
            Controls.Add(this.txtInput);
            Controls.Add(this.rtbHistory);
            Controls.Add(this.cmbModel);
            Controls.Add(this.lblModel);
            Name = "Form1";
            Text = "AI Chat with history and six tools";
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
