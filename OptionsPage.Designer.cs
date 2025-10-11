using System.Windows.Forms;

namespace NaughtyDogDCReader
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
            this.Title = new NaughtyDogDCReader.Label();
            this.CloseBtn = new System.Windows.Forms.Button();
            this.SidbaseBrowseBtn = new System.Windows.Forms.Button();
            this.DCFileBrowseBtn = new System.Windows.Forms.Button();
            this.ShowUnresolvedSIDsCheckBox = new System.Windows.Forms.CheckBox();
            this.dummy = new System.Windows.Forms.Button();
            this.SeperatorLine1 = new NaughtyDogDCReader.Label();
            this.DownloadSourceBtn = new System.Windows.Forms.Button();
            this.Title2 = new NaughtyDogDCReader.Label();
            this.CreditsLabel = new NaughtyDogDCReader.Label();
            this.SidbasePathTextBox = new NaughtyDogDCReader.TextBox();
            this.DCFilePathTextBox = new NaughtyDogDCReader.TextBox();
            this.SeperatorLine0 = new NaughtyDogDCReader.Label();
            this.SuspendLayout();
            // 
            // Title
            // 
            this.Title.AutoSize = true;
            this.Title.Font = new System.Drawing.Font("Gadugi", 9.25F, System.Drawing.FontStyle.Bold);
            this.Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.Title.IsSeparatorLine = false;
            this.Title.Location = new System.Drawing.Point(172, 3);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(57, 17);
            this.Title.StretchToFitForm = false;
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
            // SidbaseBrowseBtn
            // 
            this.SidbaseBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.SidbaseBrowseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SidbaseBrowseBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.SidbaseBrowseBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.SidbaseBrowseBtn.Location = new System.Drawing.Point(347, 71);
            this.SidbaseBrowseBtn.Name = "SidbaseBrowseBtn";
            this.SidbaseBrowseBtn.Size = new System.Drawing.Size(62, 20);
            this.SidbaseBrowseBtn.TabIndex = 8;
            this.SidbaseBrowseBtn.Text = "Browse...";
            this.SidbaseBrowseBtn.UseVisualStyleBackColor = false;
            this.SidbaseBrowseBtn.Click += new System.EventHandler(this.BrowseForSIDBase);
            // 
            // DCFileBrowseBtn
            // 
            this.DCFileBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.DCFileBrowseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.DCFileBrowseBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.DCFileBrowseBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.DCFileBrowseBtn.Location = new System.Drawing.Point(347, 95);
            this.DCFileBrowseBtn.Name = "DCFileBrowseBtn";
            this.DCFileBrowseBtn.Size = new System.Drawing.Size(62, 20);
            this.DCFileBrowseBtn.TabIndex = 9;
            this.DCFileBrowseBtn.Text = "Browse...";
            this.DCFileBrowseBtn.UseVisualStyleBackColor = false;
            this.DCFileBrowseBtn.Click += new System.EventHandler(this.BrowseForDCFile);
            // 
            // ShowUnresolvedSIDsCheckBox
            // 
            this.ShowUnresolvedSIDsCheckBox.AutoSize = true;
            this.ShowUnresolvedSIDsCheckBox.Font = new System.Drawing.Font("Gadugi", 9.25F);
            this.ShowUnresolvedSIDsCheckBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.ShowUnresolvedSIDsCheckBox.Location = new System.Drawing.Point(10, 30);
            this.ShowUnresolvedSIDsCheckBox.Name = "ShowUnresolvedSIDsCheckBox";
            this.ShowUnresolvedSIDsCheckBox.Size = new System.Drawing.Size(156, 20);
            this.ShowUnresolvedSIDsCheckBox.TabIndex = 12;
            this.ShowUnresolvedSIDsCheckBox.Text = "Show Unresolved SIDs";
            this.ShowUnresolvedSIDsCheckBox.UseVisualStyleBackColor = true;
            this.ShowUnresolvedSIDsCheckBox.CheckedChanged += new System.EventHandler(this.ShowUnresolvedSIDsCheckBox_CheckedChanged);
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
            this.SeperatorLine1.Font = new System.Drawing.Font("Cambria", 8F);
            this.SeperatorLine1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine1.IsSeparatorLine = false;
            this.SeperatorLine1.Location = new System.Drawing.Point(3, 120);
            this.SeperatorLine1.Name = "SeperatorLine1";
            this.SeperatorLine1.Size = new System.Drawing.Size(411, 17);
            this.SeperatorLine1.StretchToFitForm = false;
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
            this.Title2.IsSeparatorLine = false;
            this.Title2.Location = new System.Drawing.Point(183, 137);
            this.Title2.Name = "Title2";
            this.Title2.Size = new System.Drawing.Size(43, 14);
            this.Title2.StretchToFitForm = false;
            this.Title2.TabIndex = 16;
            this.Title2.Text = "Credits";
            this.Title2.Visible = false;
            // 
            // CreditsLabel
            // 
            this.CreditsLabel.Font = new System.Drawing.Font("Gadugi", 7.5F, System.Drawing.FontStyle.Bold);
            this.CreditsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.CreditsLabel.IsSeparatorLine = false;
            this.CreditsLabel.Location = new System.Drawing.Point(8, 158);
            this.CreditsLabel.Name = "CreditsLabel";
            this.CreditsLabel.Size = new System.Drawing.Size(277, 75);
            this.CreditsLabel.StretchToFitForm = false;
            this.CreditsLabel.TabIndex = 17;
            this.CreditsLabel.Text = "Credits:\r\n  libgp4: TheMagicalBlob, Icemesh\r\n";
            // 
            // SidbasePathTextBox
            // 
            this.SidbasePathTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.SidbasePathTextBox.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.SidbasePathTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.SidbasePathTextBox.Location = new System.Drawing.Point(6, 71);
            this.SidbasePathTextBox.Name = "SidbasePathTextBox";
            this.SidbasePathTextBox.Size = new System.Drawing.Size(335, 24);
            this.SidbasePathTextBox.TabIndex = 18;
            this.SidbasePathTextBox.Text = "No valid sidbase.bin found or provided";
            this.SidbasePathTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // DCFilePathTextBox
            // 
            this.DCFilePathTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.DCFilePathTextBox.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.DCFilePathTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.DCFilePathTextBox.Location = new System.Drawing.Point(6, 95);
            this.DCFilePathTextBox.Name = "DCFilePathTextBox";
            this.DCFilePathTextBox.Size = new System.Drawing.Size(335, 24);
            this.DCFilePathTextBox.TabIndex = 19;
            this.DCFilePathTextBox.Text = "No valid DC file loaded";
            this.DCFilePathTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // SeperatorLine0
            // 
            this.SeperatorLine0.Font = new System.Drawing.Font("Cambria", 8F);
            this.SeperatorLine0.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine0.IsSeparatorLine = false;
            this.SeperatorLine0.Location = new System.Drawing.Point(2, 52);
            this.SeperatorLine0.Name = "SeperatorLine0";
            this.SeperatorLine0.Size = new System.Drawing.Size(411, 17);
            this.SeperatorLine0.StretchToFitForm = false;
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
            this.Controls.Add(this.DCFilePathTextBox);
            this.Controls.Add(this.SidbasePathTextBox);
            this.Controls.Add(this.CreditsLabel);
            this.Controls.Add(this.Title2);
            this.Controls.Add(this.DownloadSourceBtn);
            this.Controls.Add(this.SeperatorLine1);
            this.Controls.Add(this.dummy);
            this.Controls.Add(this.ShowUnresolvedSIDsCheckBox);
            this.Controls.Add(this.DCFileBrowseBtn);
            this.Controls.Add(this.SidbaseBrowseBtn);
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
        private Button SidbaseBrowseBtn;
        private Button DCFileBrowseBtn;
        private CheckBox ShowUnresolvedSIDsCheckBox;
        private Label SeperatorLine1;
        private Button DownloadSourceBtn;
        private Label Title2;
        private Label CreditsLabel;
        private TextBox SidbasePathTextBox;
        private TextBox DCFilePathTextBox;

        private Button dummy;
        #endregion

        private Label SeperatorLine0;
    }
}