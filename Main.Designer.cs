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
            this.BinPathBrowseBtn = new System.Windows.Forms.Button();
            this.optionsMenuDropdownBtn = new System.Windows.Forms.Button();
            this.ActiveScriptLabel = new weapon_data.Label();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.MinimizeBtn = new System.Windows.Forms.Button();
            this.AbortOrCloseBtn = new System.Windows.Forms.Button();
            this.ReloadScriptBtn = new System.Windows.Forms.Button();
            this.label1 = new weapon_data.Label();
            this.SeperatorLine1 = new weapon_data.Label();
            this.debugShowAllBtn = new System.Windows.Forms.Button();
            this.bleghBtn = new System.Windows.Forms.Button();
            this.VersionLabel = new weapon_data.Label();
            this.PropertiesPanel = new System.Windows.Forms.GroupBox();
            this.label2 = new weapon_data.Label();
            this.SeperatorLine2 = new weapon_data.Label();
            this.PropertiesWindowRichTextBox = new weapon_data.RichTextBox();
            this.binPathTextBox = new weapon_data.TextBox();
            this.label3 = new weapon_data.Label();
            this.SuspendLayout();
            // 
            // BinPathBrowseBtn
            // 
            this.BinPathBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.BinPathBrowseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BinPathBrowseBtn.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BinPathBrowseBtn.Location = new System.Drawing.Point(469, 73);
            this.BinPathBrowseBtn.Name = "BinPathBrowseBtn";
            this.BinPathBrowseBtn.Size = new System.Drawing.Size(65, 23);
            this.BinPathBrowseBtn.TabIndex = 2;
            this.BinPathBrowseBtn.Text = "Browse...";
            this.BinPathBrowseBtn.UseVisualStyleBackColor = false;
            this.BinPathBrowseBtn.Click += new System.EventHandler(this.BinPathBrowseBtn_Click);
            // 
            // optionsMenuDropdownBtn
            // 
            this.optionsMenuDropdownBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.optionsMenuDropdownBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.optionsMenuDropdownBtn.Location = new System.Drawing.Point(4, 72);
            this.optionsMenuDropdownBtn.Name = "optionsMenuDropdownBtn";
            this.optionsMenuDropdownBtn.Size = new System.Drawing.Size(71, 23);
            this.optionsMenuDropdownBtn.TabIndex = 3;
            this.optionsMenuDropdownBtn.Text = "Options...";
            this.optionsMenuDropdownBtn.UseVisualStyleBackColor = false;
            this.optionsMenuDropdownBtn.Click += new System.EventHandler(this.OptionsMenuDropdownBtn_Click);
            // 
            // ActiveScriptLabel
            // 
            this.ActiveScriptLabel.Font = new System.Drawing.Font("Segoe UI", 10.25F, System.Drawing.FontStyle.Bold);
            this.ActiveScriptLabel.ForeColor = System.Drawing.Color.Gold;
            this.ActiveScriptLabel.Location = new System.Drawing.Point(8, 171);
            this.ActiveScriptLabel.Name = "ActiveScriptLabel";
            this.ActiveScriptLabel.Size = new System.Drawing.Size(531, 23);
            this.ActiveScriptLabel.TabIndex = 0;
            this.ActiveScriptLabel.Text = "Selected Script: none selected";
            this.ActiveScriptLabel.TopMost = false;
            // 
            // ExitBtn
            // 
            this.ExitBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.ExitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ExitBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.ExitBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ExitBtn.Location = new System.Drawing.Point(810, 1);
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
            this.MinimizeBtn.Location = new System.Drawing.Point(788, 1);
            this.MinimizeBtn.Name = "MinimizeBtn";
            this.MinimizeBtn.Size = new System.Drawing.Size(22, 22);
            this.MinimizeBtn.TabIndex = 7;
            this.MinimizeBtn.Text = "-";
            this.MinimizeBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.MinimizeBtn.UseVisualStyleBackColor = false;
            // 
            // AbortOrCloseBtn
            // 
            this.AbortOrCloseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.AbortOrCloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AbortOrCloseBtn.Location = new System.Drawing.Point(321, 73);
            this.AbortOrCloseBtn.Name = "AbortOrCloseBtn";
            this.AbortOrCloseBtn.Size = new System.Drawing.Size(44, 23);
            this.AbortOrCloseBtn.TabIndex = 0;
            this.AbortOrCloseBtn.Text = "Abort";
            this.AbortOrCloseBtn.UseVisualStyleBackColor = false;
            this.AbortOrCloseBtn.Visible = false;
            this.AbortOrCloseBtn.Click += new System.EventHandler(this.AbortOrCloseBtn_Click);
            // 
            // ReloadScriptBtn
            // 
            this.ReloadScriptBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.ReloadScriptBtn.Enabled = false;
            this.ReloadScriptBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReloadScriptBtn.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadScriptBtn.Location = new System.Drawing.Point(368, 73);
            this.ReloadScriptBtn.Name = "ReloadScriptBtn";
            this.ReloadScriptBtn.Size = new System.Drawing.Size(95, 23);
            this.ReloadScriptBtn.TabIndex = 10;
            this.ReloadScriptBtn.Text = "Reload Current";
            this.ReloadScriptBtn.UseVisualStyleBackColor = false;
            this.ReloadScriptBtn.Click += new System.EventHandler(this.ReloadBinFile);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gold;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(190, 30);
            this.label1.TabIndex = 12;
            this.label1.Text = "NaughtyDog DC Test";
            this.label1.TopMost = false;
            // 
            // SeperatorLine1
            // 
            this.SeperatorLine1.Font = new System.Drawing.Font("Gadugi", 9.25F, System.Drawing.FontStyle.Bold);
            this.SeperatorLine1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine1.Location = new System.Drawing.Point(1, 154);
            this.SeperatorLine1.Name = "SeperatorLine1";
            this.SeperatorLine1.Size = new System.Drawing.Size(539, 17);
            this.SeperatorLine1.TabIndex = 15;
            this.SeperatorLine1.Tag = "!";
            this.SeperatorLine1.Text = "---------------------------------------------------------------------------------" +
    "-----------------------------------------------------------------------------";
            this.SeperatorLine1.TopMost = false;
            // 
            // debugShowAllBtn
            // 
            this.debugShowAllBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.debugShowAllBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.debugShowAllBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.debugShowAllBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.debugShowAllBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.debugShowAllBtn.Location = new System.Drawing.Point(200, 18);
            this.debugShowAllBtn.Name = "debugShowAllBtn";
            this.debugShowAllBtn.Size = new System.Drawing.Size(56, 20);
            this.debugShowAllBtn.TabIndex = 16;
            this.debugShowAllBtn.Text = "showAll";
            this.debugShowAllBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.debugShowAllBtn.UseVisualStyleBackColor = false;
            this.debugShowAllBtn.Click += new System.EventHandler(this.debugShowAllBtn_Click);
            // 
            // bleghBtn
            // 
            this.bleghBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.bleghBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.bleghBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.bleghBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.bleghBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.bleghBtn.Location = new System.Drawing.Point(262, 17);
            this.bleghBtn.Name = "bleghBtn";
            this.bleghBtn.Size = new System.Drawing.Size(43, 21);
            this.bleghBtn.TabIndex = 17;
            this.bleghBtn.Text = "Misc.";
            this.bleghBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.bleghBtn.UseVisualStyleBackColor = false;
            this.bleghBtn.Click += new System.EventHandler(this.bleghBtn_Click);
            // 
            // VersionLabel
            // 
            this.VersionLabel.Font = new System.Drawing.Font("Segoe UI", 6.5F);
            this.VersionLabel.ForeColor = System.Drawing.Color.Gold;
            this.VersionLabel.Location = new System.Drawing.Point(200, 3);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(58, 13);
            this.VersionLabel.TabIndex = 18;
            this.VersionLabel.Text = "==version==";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.VersionLabel.TopMost = false;
            // 
            // PropertiesPanel
            // 
            this.PropertiesPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PropertiesPanel.Location = new System.Drawing.Point(5, 217);
            this.PropertiesPanel.Name = "PropertiesPanel";
            this.PropertiesPanel.Size = new System.Drawing.Size(531, 351);
            this.PropertiesPanel.TabIndex = 19;
            this.PropertiesPanel.TabStop = false;
            // 
            // label2
            // 
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Gold;
            this.label2.Location = new System.Drawing.Point(10, 202);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(526, 19);
            this.label2.TabIndex = 20;
            this.label2.Text = "abcdefghijklmnopqrstuvwxyz!?";
            this.label2.TopMost = false;
            // 
            // SeperatorLine2
            // 
            this.SeperatorLine2.Font = new System.Drawing.Font("Gadugi", 9.25F, System.Drawing.FontStyle.Bold);
            this.SeperatorLine2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine2.Location = new System.Drawing.Point(1, 189);
            this.SeperatorLine2.Name = "SeperatorLine2";
            this.SeperatorLine2.Size = new System.Drawing.Size(539, 17);
            this.SeperatorLine2.TabIndex = 21;
            this.SeperatorLine2.Tag = "True";
            this.SeperatorLine2.Text = "---------------------------------------------------------------------------------" +
    "----------------------------------------------";
            this.SeperatorLine2.TopMost = false;
            // 
            // PropertiesWindowRichTextBox
            // 
            this.PropertiesWindowRichTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.PropertiesWindowRichTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.PropertiesWindowRichTextBox.Location = new System.Drawing.Point(545, 198);
            this.PropertiesWindowRichTextBox.Name = "PropertiesWindowRichTextBox";
            this.PropertiesWindowRichTextBox.ReadOnly = true;
            this.PropertiesWindowRichTextBox.ShortcutsEnabled = false;
            this.PropertiesWindowRichTextBox.Size = new System.Drawing.Size(284, 369);
            this.PropertiesWindowRichTextBox.TabIndex = 2;
            this.PropertiesWindowRichTextBox.Text = "";
            // 
            // binPathTextBox
            // 
            this.binPathTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.binPathTextBox.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.binPathTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.binPathTextBox.Location = new System.Drawing.Point(4, 44);
            this.binPathTextBox.Name = "binPathTextBox";
            this.binPathTextBox.Size = new System.Drawing.Size(530, 24);
            this.binPathTextBox.TabIndex = 3;
            this.binPathTextBox.Text = "Browse for/Paste in a dc file path";
            this.binPathTextBox.TextChanged += new System.EventHandler(this.CheckbinPathBoxText);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Gadugi", 9.25F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label3.Location = new System.Drawing.Point(541, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(292, 17);
            this.label3.TabIndex = 22;
            this.label3.Tag = "!";
            this.label3.Text = "---------------------------------------------------------------------------------" +
    "-----------------------------------------------------------------------------";
            this.label3.TopMost = false;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(834, 572);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.PropertiesWindowRichTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.SeperatorLine2);
            this.Controls.Add(this.PropertiesPanel);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.bleghBtn);
            this.Controls.Add(this.debugShowAllBtn);
            this.Controls.Add(this.SeperatorLine1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ReloadScriptBtn);
            this.Controls.Add(this.AbortOrCloseBtn);
            this.Controls.Add(this.ExitBtn);
            this.Controls.Add(this.MinimizeBtn);
            this.Controls.Add(this.ActiveScriptLabel);
            this.Controls.Add(this.optionsMenuDropdownBtn);
            this.Controls.Add(this.BinPathBrowseBtn);
            this.Controls.Add(this.binPathTextBox);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Main";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
        

        
        //================================\\
        //--|   Control Declarations   |--\\
        //================================\\
        #region [Control Declarations]

        public Button[] DropdownMenu = new Button[2];
        private TextBox binPathTextBox;
        private Button BinPathBrowseBtn;
        private RichTextBox PropertiesWindowRichTextBox;
        private Button optionsMenuDropdownBtn;
        private Label ActiveScriptLabel;
        private Button ExitBtn;
        private Button MinimizeBtn;
        private Button AbortOrCloseBtn;
        private Button ReloadScriptBtn;
        private Label label1;
        private Label SeperatorLine1;
        private Button debugShowAllBtn;
        #endregion

        private Button bleghBtn;
        private Label VersionLabel;
        private GroupBox PropertiesPanel;
        private Label label2;
        private Label SeperatorLine2;
        private Label label3;
    }
}

