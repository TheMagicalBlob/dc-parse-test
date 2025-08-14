using System.Drawing;
using System;
using System.Windows.Forms;
using System.Linq;

namespace weapon_data
{
    partial class Main
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
            this.BinFileBrowseBtn = new System.Windows.Forms.Button();
            this.optionsMenuDropdownBtn = new System.Windows.Forms.Button();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.MinimizeBtn = new System.Windows.Forms.Button();
            this.abortOrCloseBtn = new System.Windows.Forms.Button();
            this.ReloadScriptBtn = new System.Windows.Forms.Button();
            this.propertiesPanel = new System.Windows.Forms.GroupBox();
            this.dummy0 = new System.Windows.Forms.Button();
            this.dummy1 = new System.Windows.Forms.Button();
            this.debugPanelBtn = new System.Windows.Forms.Button();
            this.SidBaseBrowseBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.BinFileBrowseBtn = new System.Windows.Forms.Button();
            this.propertiesWindow = new weapon_data.RichTextBox();
            this.scriptStatusLabel = new weapon_data.Label();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.MinimizeBtn = new System.Windows.Forms.Button();
            this.abortOrCloseBtn = new System.Windows.Forms.Button();
            this.ReloadScriptBtn = new System.Windows.Forms.Button();
            this.label1 = new weapon_data.Label();
            this.VersionLabel = new weapon_data.Label();
            this.propertiesPanel = new System.Windows.Forms.GroupBox();
            this.scriptSelectionLabel = new weapon_data.Label();
            this.SeperatorLine2 = new weapon_data.Label();
            this.label3 = new weapon_data.Label();
            this.dummy0 = new System.Windows.Forms.Button();
            this.dummy1 = new System.Windows.Forms.Button();
            this.label2 = new weapon_data.Label();
            this.debugPanelBtn = new System.Windows.Forms.Button();
            this.logWindow = new weapon_data.RichTextBox();
            this.label4 = new weapon_data.Label();
            this.SidBaseBrowseBtn = new System.Windows.Forms.Button();
            this.label6 = new weapon_data.Label();
            this.label5 = new weapon_data.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BinFileBrowseBtn
            // 
            this.BinFileBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.BinFileBrowseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BinFileBrowseBtn.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BinFileBrowseBtn.Location = new System.Drawing.Point(6, 41);
            this.BinFileBrowseBtn.Name = "BinFileBrowseBtn";
            this.BinFileBrowseBtn.Size = new System.Drawing.Size(132, 23);
            this.BinFileBrowseBtn.TabIndex = 2;
            this.BinFileBrowseBtn.Text = "Browse for DC Script...";
            this.BinFileBrowseBtn.UseVisualStyleBackColor = false;
            this.BinFileBrowseBtn.Click += new System.EventHandler(this.BinPathBrowseBtn_Click);
            // 
            // optionsMenuDropdownBtn
            // 
            this.optionsMenuDropdownBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.optionsMenuDropdownBtn.Enabled = false;
            this.optionsMenuDropdownBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.optionsMenuDropdownBtn.Location = new System.Drawing.Point(760, 6);
            this.optionsMenuDropdownBtn.Name = "optionsMenuDropdownBtn";
            this.optionsMenuDropdownBtn.Size = new System.Drawing.Size(71, 23);
            this.optionsMenuDropdownBtn.TabIndex = 6;
            this.optionsMenuDropdownBtn.Text = "Options...";
            this.optionsMenuDropdownBtn.UseVisualStyleBackColor = false;
            this.optionsMenuDropdownBtn.Click += new System.EventHandler(this.OptionsMenuDropdownBtn_Click);
            // 
            // ExitBtn
            // 
            this.ExitBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.ExitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ExitBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.ExitBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ExitBtn.Location = new System.Drawing.Point(861, 6);
            this.ExitBtn.Name = "ExitBtn";
            this.ExitBtn.Size = new System.Drawing.Size(22, 22);
            this.ExitBtn.TabIndex = 8;
            this.ExitBtn.Text = "X";
            this.ExitBtn.UseVisualStyleBackColor = false;
            // 
            // MinimizeBtn
            // 
            this.MinimizeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.MinimizeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.MinimizeBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.MinimizeBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.MinimizeBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.MinimizeBtn.Location = new System.Drawing.Point(836, 6);
            this.MinimizeBtn.Name = "MinimizeBtn";
            this.MinimizeBtn.Size = new System.Drawing.Size(22, 22);
            this.MinimizeBtn.TabIndex = 7;
            this.MinimizeBtn.Text = "-";
            this.MinimizeBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.MinimizeBtn.UseVisualStyleBackColor = false;
            // 
            // abortOrCloseBtn
            // 
            this.abortOrCloseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.abortOrCloseBtn.Enabled = false;
            this.abortOrCloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.abortOrCloseBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.abortOrCloseBtn.Location = new System.Drawing.Point(709, 41);
            this.abortOrCloseBtn.Name = "abortOrCloseBtn";
            this.abortOrCloseBtn.Size = new System.Drawing.Size(44, 23);
            this.abortOrCloseBtn.TabIndex = 3;
            this.abortOrCloseBtn.Text = "Abort";
            this.abortOrCloseBtn.UseVisualStyleBackColor = false;
            this.abortOrCloseBtn.Click += new System.EventHandler(this.AbortOrCloseBtn_Click);
            // 
            // ReloadScriptBtn
            // 
            this.ReloadScriptBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.ReloadScriptBtn.Enabled = false;
            this.ReloadScriptBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReloadScriptBtn.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadScriptBtn.Location = new System.Drawing.Point(756, 41);
            this.ReloadScriptBtn.Name = "ReloadScriptBtn";
            this.ReloadScriptBtn.Size = new System.Drawing.Size(127, 23);
            this.ReloadScriptBtn.TabIndex = 4;
            this.ReloadScriptBtn.Text = "Reload Current Script";
            this.ReloadScriptBtn.UseVisualStyleBackColor = false;
            this.ReloadScriptBtn.Click += new System.EventHandler(this.ReloadBinFile);
            // 
            // propertiesPanel
            // 
            this.propertiesPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.propertiesPanel.Location = new System.Drawing.Point(5, 259);
            this.propertiesPanel.Name = "propertiesPanel";
            this.propertiesPanel.Size = new System.Drawing.Size(531, 363);
            this.propertiesPanel.TabIndex = 19;
            this.propertiesPanel.TabStop = false;
            // 
            // dummy0
            // 
            this.dummy0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.dummy0.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dummy0.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dummy0.Location = new System.Drawing.Point(0, 0);
            this.dummy0.Name = "dummy0";
            this.dummy0.Size = new System.Drawing.Size(0, 0);
            this.dummy0.TabIndex = 0;
            this.dummy0.UseVisualStyleBackColor = false;
            // 
            // dummy1
            // 
            this.dummy1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.dummy1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dummy1.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dummy1.Location = new System.Drawing.Point(1, 0);
            this.dummy1.Name = "dummy1";
            this.dummy1.Size = new System.Drawing.Size(0, 0);
            this.dummy1.TabIndex = 1;
            this.dummy1.UseVisualStyleBackColor = false;
            // 
            // debugPanelBtn
            // 
            this.debugPanelBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.debugPanelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.debugPanelBtn.Location = new System.Drawing.Point(695, 6);
            this.debugPanelBtn.Name = "debugPanelBtn";
            this.debugPanelBtn.Size = new System.Drawing.Size(59, 23);
            this.debugPanelBtn.TabIndex = 45;
            this.debugPanelBtn.Text = "DEBUG";
            this.debugPanelBtn.UseVisualStyleBackColor = false;
            this.debugPanelBtn.Click += new System.EventHandler(this.debugPanelBtn_Click);
            // 
            // SidBaseBrowseBtn
            // 
            this.SidBaseBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.SidBaseBrowseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SidBaseBrowseBtn.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SidBaseBrowseBtn.Location = new System.Drawing.Point(141, 41);
            this.SidBaseBrowseBtn.Name = "SidBaseBrowseBtn";
            this.SidBaseBrowseBtn.Size = new System.Drawing.Size(124, 23);
            this.SidBaseBrowseBtn.TabIndex = 48;
            this.SidBaseBrowseBtn.Text = "Browse for sidbase...";
            this.SidBaseBrowseBtn.UseVisualStyleBackColor = false;
            this.SidBaseBrowseBtn.Click += new System.EventHandler(this.SidBaseBrowseBtn_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(444, 314);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(0, 0);
            this.button1.TabIndex = 52;
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(452, 322);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(0, 0);
            this.button2.TabIndex = 53;
            this.button2.UseVisualStyleBackColor = false;
            // 
            // Main
            // 
            this.Controls.Add(BinFileBrowseBtn);
            this.Controls.Add(propertiesWindow);
            this.Controls.Add(scriptStatusLabel);
            this.Controls.Add(ExitBtn);
            this.Controls.Add(MinimizeBtn);
            this.Controls.Add(abortOrCloseBtn);
            this.Controls.Add(ReloadScriptBtn);
            this.Controls.Add(label1);
            this.Controls.Add(VersionLabel);
            this.Controls.Add(propertiesPanel);
            this.Controls.Add(scriptSelectionLabel);
            this.Controls.Add(SeperatorLine2);
            this.Controls.Add(label3);
            this.Controls.Add(dummy0);
            this.Controls.Add(dummy1);
            this.Controls.Add(label2);
            this.Controls.Add(debugPanelBtn);
            this.Controls.Add(logWindow);
            this.Controls.Add(label4);
            this.Controls.Add(SidBaseBrowseBtn);
            this.Controls.Add(label6);
            this.Controls.Add(label5);
            this.Controls.Add(button1);
            this.Controls.Add(button2);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(888, 626);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Main";
            this.ResumeLayout(false);

        }
        #endregion
        

        
        //================================\\
        //--|   Control Declarations   |--\\
        //================================\\
        #region [Control Declarations]

        public Button[] DropdownMenu = new Button[2];
        private Button BinFileBrowseBtn;
        private RichTextBox propertiesWindow;
        public Button optionsMenuDropdownBtn;
        private Label scriptStatusLabel;
        private Button ExitBtn;
        private Button MinimizeBtn;
        private Button abortOrCloseBtn;
        private Button ReloadScriptBtn;
        private Label label1;
        private Label VersionLabel;
        private GroupBox propertiesPanel;
        private Label scriptSelectionLabel;
        private Label SeperatorLine2;
        private Label label3;
        private Button dummy0;
        private Button dummy1;
        private Label label2;
        private Button debugPanelBtn;
        private RichTextBox logWindow;
        private Label label4;
        private Button SidBaseBrowseBtn;
        #endregion
        private Label label6;
        private Label label5;
        private Button button1;
        private Button button2;
    }
}

