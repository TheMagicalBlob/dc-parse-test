using System.Windows.Forms;

namespace weapon_data
{
    partial class OptionsPage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Title = new System.Windows.Forms.Label();
            this.CloseBtn = new System.Windows.Forms.Button();
            this.SidbasePathBrowseBtn = new System.Windows.Forms.Button();
            this.UnusedBrowseBtn = new System.Windows.Forms.Button();
            this.UnusedCheckBox = new System.Windows.Forms.CheckBox();
            this.dummy = new System.Windows.Forms.Button();
            this.SeperatorLine1 = new System.Windows.Forms.Label();
            this.DownloadSourceBtn = new System.Windows.Forms.Button();
            this.Title2 = new System.Windows.Forms.Label();
            this.CreditsLabel = new System.Windows.Forms.Label();
            this.sidbasePathTextBox = new weapon_data.TextBox();
            this.UnusedPathTextBox = new weapon_data.TextBox();
            this.SeperatorLine0 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Title
            // 
            this.Title.AutoSize = true;
            this.Title.Font = new System.Drawing.Font("Gadugi", 9.25F, System.Drawing.FontStyle.Bold);
            this.Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.Title.Location = new System.Drawing.Point(172, 3);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(57, 17);
            this.Title.TabIndex = 0;
            this.Title.Text = "Options";
            // 
            // CloseBtn
            // 
            this.CloseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.CloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.CloseBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.CloseBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.CloseBtn.Location = new System.Drawing.Point(391, 2);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(22, 22);
            this.CloseBtn.TabIndex = 7;
            this.CloseBtn.Text = "X";
            this.CloseBtn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CloseBtn.UseVisualStyleBackColor = false;
            // 
            // SidbasePathBrowseBtn
            // 
            this.SidbasePathBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.SidbasePathBrowseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SidbasePathBrowseBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.SidbasePathBrowseBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.SidbasePathBrowseBtn.Location = new System.Drawing.Point(347, 71);
            this.SidbasePathBrowseBtn.Name = "SidbasePathBrowseBtn";
            this.SidbasePathBrowseBtn.Size = new System.Drawing.Size(62, 20);
            this.SidbasePathBrowseBtn.TabIndex = 8;
            this.SidbasePathBrowseBtn.Text = "Browse...";
            this.SidbasePathBrowseBtn.UseVisualStyleBackColor = false;
            this.SidbasePathBrowseBtn.Click += new System.EventHandler(this.UNUSEDBrowseBtn_Click);
            // 
            // UnusedBrowseBtn
            // 
            this.UnusedBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.UnusedBrowseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.UnusedBrowseBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.UnusedBrowseBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.UnusedBrowseBtn.Location = new System.Drawing.Point(347, 95);
            this.UnusedBrowseBtn.Name = "UnusedBrowseBtn";
            this.UnusedBrowseBtn.Size = new System.Drawing.Size(62, 20);
            this.UnusedBrowseBtn.TabIndex = 9;
            this.UnusedBrowseBtn.Text = "Browse...";
            this.UnusedBrowseBtn.UseVisualStyleBackColor = false;
            this.UnusedBrowseBtn.Click += new System.EventHandler(this.BasePackagePathBrowseBtn_Click);
            // 
            // UnusedCheckBox
            // 
            this.UnusedCheckBox.AutoSize = true;
            this.UnusedCheckBox.Font = new System.Drawing.Font("Gadugi", 9.25F);
            this.UnusedCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.UnusedCheckBox.Location = new System.Drawing.Point(10, 30);
            this.UnusedCheckBox.Name = "UnusedCheckBox";
            this.UnusedCheckBox.Size = new System.Drawing.Size(95, 20);
            this.UnusedCheckBox.TabIndex = 12;
            this.UnusedCheckBox.Text = "placeholder";
            this.UnusedCheckBox.UseVisualStyleBackColor = true;
            // 
            // dummy
            // 
            this.dummy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.dummy.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.dummy.Font = new System.Drawing.Font("Microsoft Sans Serif", 0.1F);
            this.dummy.ForeColor = System.Drawing.SystemColors.WindowText;
            this.dummy.Location = new System.Drawing.Point(0, 0);
            this.dummy.Name = "dummy";
            this.dummy.Size = new System.Drawing.Size(0, 0);
            this.dummy.TabIndex = 0;
            this.dummy.UseVisualStyleBackColor = false;
            // 
            // SeperatorLine1
            // 
            this.SeperatorLine1.Font = new System.Drawing.Font("Gadugi", 9.25F, System.Drawing.FontStyle.Bold);
            this.SeperatorLine1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine1.Location = new System.Drawing.Point(3, 119);
            this.SeperatorLine1.Name = "SeperatorLine1";
            this.SeperatorLine1.Size = new System.Drawing.Size(411, 17);
            this.SeperatorLine1.TabIndex = 13;
            this.SeperatorLine1.Text = "---------------------------------------------------------------------------------" +
    "";
            // 
            // DownloadSourceBtn
            // 
            this.DownloadSourceBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.DownloadSourceBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DownloadSourceBtn.Font = new System.Drawing.Font("Gadugi", 7F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.DownloadSourceBtn.ForeColor = System.Drawing.SystemColors.Window;
            this.DownloadSourceBtn.Location = new System.Drawing.Point(291, 179);
            this.DownloadSourceBtn.Name = "DownloadSourceBtn";
            this.DownloadSourceBtn.Size = new System.Drawing.Size(120, 22);
            this.DownloadSourceBtn.TabIndex = 15;
            this.DownloadSourceBtn.Text = "download source code";
            this.DownloadSourceBtn.UseVisualStyleBackColor = false;
            this.DownloadSourceBtn.Click += new System.EventHandler(this.DownloadSourceBtn_Click);
            // 
            // Title2
            // 
            this.Title2.AutoSize = true;
            this.Title2.Font = new System.Drawing.Font("Gadugi", 8F, System.Drawing.FontStyle.Bold);
            this.Title2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.Title2.Location = new System.Drawing.Point(183, 137);
            this.Title2.Name = "Title2";
            this.Title2.Size = new System.Drawing.Size(43, 14);
            this.Title2.TabIndex = 16;
            this.Title2.Text = "Credits";
            this.Title2.Visible = false;
            // 
            // CreditsLabel
            // 
            this.CreditsLabel.Font = new System.Drawing.Font("Gadugi", 7.5F, System.Drawing.FontStyle.Bold);
            this.CreditsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.CreditsLabel.Location = new System.Drawing.Point(8, 158);
            this.CreditsLabel.Name = "CreditsLabel";
            this.CreditsLabel.Size = new System.Drawing.Size(277, 75);
            this.CreditsLabel.TabIndex = 17;
            this.CreditsLabel.Text = "Credits:\r\n  libgp4: TheMagicalBlob, Icemesh\r\n";
            // 
            // sidbasePathTextBox
            // 
            this.sidbasePathTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.sidbasePathTextBox.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.sidbasePathTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.sidbasePathTextBox.Location = new System.Drawing.Point(6, 71);
            this.sidbasePathTextBox.Name = "sidbasePathTextBox";
            this.sidbasePathTextBox.Size = new System.Drawing.Size(335, 24);
            this.sidbasePathTextBox.TabIndex = 18;
            this.sidbasePathTextBox.Text = "No valid sidbase.bin found or provided";
            this.sidbasePathTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // UnusedPathTextBox
            // 
            this.UnusedPathTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.UnusedPathTextBox.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.UnusedPathTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.UnusedPathTextBox.Location = new System.Drawing.Point(6, 95);
            this.UnusedPathTextBox.Name = "UnusedPathTextBox";
            this.UnusedPathTextBox.Size = new System.Drawing.Size(335, 24);
            this.UnusedPathTextBox.TabIndex = 19;
            this.UnusedPathTextBox.Text = "pLaCeHoLdEr";
            this.UnusedPathTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // SeperatorLine0
            // 
            this.SeperatorLine0.Font = new System.Drawing.Font("Gadugi", 9.25F, System.Drawing.FontStyle.Bold);
            this.SeperatorLine0.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine0.Location = new System.Drawing.Point(2, 52);
            this.SeperatorLine0.Name = "SeperatorLine0";
            this.SeperatorLine0.Size = new System.Drawing.Size(411, 17);
            this.SeperatorLine0.TabIndex = 20;
            this.SeperatorLine0.Text = "---------------------------------------------------------------------------------" +
    "";
            // 
            // OptionsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.ClientSize = new System.Drawing.Size(415, 237);
            this.Controls.Add(this.SeperatorLine0);
            this.Controls.Add(this.UnusedPathTextBox);
            this.Controls.Add(this.sidbasePathTextBox);
            this.Controls.Add(this.CreditsLabel);
            this.Controls.Add(this.Title2);
            this.Controls.Add(this.DownloadSourceBtn);
            this.Controls.Add(this.SeperatorLine1);
            this.Controls.Add(this.dummy);
            this.Controls.Add(this.UnusedCheckBox);
            this.Controls.Add(this.UnusedBrowseBtn);
            this.Controls.Add(this.SidbasePathBrowseBtn);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.Title);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "OptionsPage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        
        //================================\\
        //--|   Control Declarations   |--\\
        //================================\\
        #region [Control Declarations]
        private Label Title;
        private Button CloseBtn;
        private Button SidbasePathBrowseBtn;
        private Button UnusedBrowseBtn;
        private CheckBox UnusedCheckBox;
        private Label SeperatorLine1;
        private Button DownloadSourceBtn;
        private Label Title2;
        private Label CreditsLabel;
        private TextBox sidbasePathTextBox;
        private TextBox UnusedPathTextBox;

        private Button dummy;
        #endregion

        private Label SeperatorLine0;
    }
}