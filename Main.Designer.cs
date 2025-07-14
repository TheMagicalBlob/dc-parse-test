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
            this.ExitBtn = new System.Windows.Forms.Button();
            this.MinimizeBtn = new System.Windows.Forms.Button();
            this.AbortOrCloseBtn = new System.Windows.Forms.Button();
            this.ReloadScriptBtn = new System.Windows.Forms.Button();
            this.debugMiscBtn = new System.Windows.Forms.Button();
            this.PropertiesPanel = new System.Windows.Forms.GroupBox();
            this.debugDisableLinesBtn = new System.Windows.Forms.CheckBox();
            this.dummy0 = new System.Windows.Forms.Button();
            this.dummy1 = new System.Windows.Forms.Button();
            this.debugTabCheckBtn = new System.Windows.Forms.Button();
            this.label1 = new weapon_data.Label();
            this.label2 = new weapon_data.Label();
            this.label5 = new weapon_data.Label();
            this.label3 = new weapon_data.Label();
            this.PropertiesWindowRichTextBox = new weapon_data.RichTextBox();
            this.ScriptSelectionLabel = new weapon_data.Label();
            this.SeperatorLine2 = new weapon_data.Label();
            this.VersionLabel = new weapon_data.Label();
            this.ScriptStatusLabel = new weapon_data.Label();
            this.SuspendLayout();
            // 
            // BinPathBrowseBtn
            // 
            this.BinPathBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.BinPathBrowseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BinPathBrowseBtn.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BinPathBrowseBtn.Location = new System.Drawing.Point(6, 40);
            this.BinPathBrowseBtn.Name = "BinPathBrowseBtn";
            this.BinPathBrowseBtn.Size = new System.Drawing.Size(126, 23);
            this.BinPathBrowseBtn.TabIndex = 2;
            this.BinPathBrowseBtn.Text = "Browse for .bin File...";
            this.BinPathBrowseBtn.UseVisualStyleBackColor = false;
            this.BinPathBrowseBtn.Click += new System.EventHandler(this.BinPathBrowseBtn_Click);
            // 
            // optionsMenuDropdownBtn
            // 
            this.optionsMenuDropdownBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.optionsMenuDropdownBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.optionsMenuDropdownBtn.Location = new System.Drawing.Point(760, 44);
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
            this.ExitBtn.Location = new System.Drawing.Point(808, 5);
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
            this.MinimizeBtn.Location = new System.Drawing.Point(783, 5);
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
            this.AbortOrCloseBtn.Enabled = false;
            this.AbortOrCloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AbortOrCloseBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AbortOrCloseBtn.Location = new System.Drawing.Point(6, 66);
            this.AbortOrCloseBtn.Name = "AbortOrCloseBtn";
            this.AbortOrCloseBtn.Size = new System.Drawing.Size(44, 23);
            this.AbortOrCloseBtn.TabIndex = 3;
            this.AbortOrCloseBtn.Text = "Abort";
            this.AbortOrCloseBtn.UseVisualStyleBackColor = false;
            this.AbortOrCloseBtn.Click += new System.EventHandler(this.AbortOrCloseBtn_Click);
            // 
            // ReloadScriptBtn
            // 
            this.ReloadScriptBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.ReloadScriptBtn.Enabled = false;
            this.ReloadScriptBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReloadScriptBtn.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadScriptBtn.Location = new System.Drawing.Point(53, 66);
            this.ReloadScriptBtn.Name = "ReloadScriptBtn";
            this.ReloadScriptBtn.Size = new System.Drawing.Size(95, 23);
            this.ReloadScriptBtn.TabIndex = 4;
            this.ReloadScriptBtn.Text = "Reload Current";
            this.ReloadScriptBtn.UseVisualStyleBackColor = false;
            this.ReloadScriptBtn.Click += new System.EventHandler(this.ReloadBinFile);
            // 
            // debugMiscBtn
            // 
            this.debugMiscBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.debugMiscBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.debugMiscBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.debugMiscBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.debugMiscBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.debugMiscBtn.Location = new System.Drawing.Point(545, 7);
            this.debugMiscBtn.Name = "debugMiscBtn";
            this.debugMiscBtn.Size = new System.Drawing.Size(43, 21);
            this.debugMiscBtn.TabIndex = 17;
            this.debugMiscBtn.Text = "Misc.";
            this.debugMiscBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.debugMiscBtn.UseVisualStyleBackColor = false;
            this.debugMiscBtn.Click += new System.EventHandler(this.debugMiscBtn_Click);
            // 
            // PropertiesPanel
            // 
            this.PropertiesPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PropertiesPanel.Location = new System.Drawing.Point(5, 206);
            this.PropertiesPanel.Name = "PropertiesPanel";
            this.PropertiesPanel.Size = new System.Drawing.Size(531, 363);
            this.PropertiesPanel.TabIndex = 19;
            this.PropertiesPanel.TabStop = false;
            // 
            // debugDisableLinesBtn
            // 
            this.debugDisableLinesBtn.AutoSize = true;
            this.debugDisableLinesBtn.Location = new System.Drawing.Point(643, 10);
            this.debugDisableLinesBtn.Name = "debugDisableLinesBtn";
            this.debugDisableLinesBtn.Size = new System.Drawing.Size(54, 17);
            this.debugDisableLinesBtn.TabIndex = 29;
            this.debugDisableLinesBtn.Text = "noline";
            this.debugDisableLinesBtn.UseVisualStyleBackColor = true;
            this.debugDisableLinesBtn.CheckedChanged += new System.EventHandler(this.debugDisableLinesBtn_CheckedChanged);
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
            // debugTabCheckBtn
            // 
            this.debugTabCheckBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.debugTabCheckBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.debugTabCheckBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 6.5F);
            this.debugTabCheckBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.debugTabCheckBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.debugTabCheckBtn.Location = new System.Drawing.Point(590, 8);
            this.debugTabCheckBtn.Name = "debugTabCheckBtn";
            this.debugTabCheckBtn.Size = new System.Drawing.Size(49, 19);
            this.debugTabCheckBtn.TabIndex = 43;
            this.debugTabCheckBtn.Text = "tabcheck";
            this.debugTabCheckBtn.UseVisualStyleBackColor = false;
            this.debugTabCheckBtn.Click += new System.EventHandler(this.debugTabCheckBtn_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gold;
            this.label1.IsSeparatorLine = false;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(190, 30);
            this.label1.StretchToFitForm = false;
            this.label1.TabIndex = 12;
            this.label1.Text = "NaughtyDog DC Test";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Cambria", 8F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label2.IsSeparatorLine = true;
            this.label2.Location = new System.Drawing.Point(82, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(644, 13);
            this.label2.StretchToFitForm = true;
            this.label2.TabIndex = 44;
            this.label2.Tag = "";
            this.label2.Text = "---------------------------------------------------------------------------------" +
    "-----------------------------------------------------------------------------";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Cambria", 8F);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label5.IsSeparatorLine = true;
            this.label5.Location = new System.Drawing.Point(29, 90);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(644, 13);
            this.label5.StretchToFitForm = true;
            this.label5.TabIndex = 27;
            this.label5.Tag = "";
            this.label5.Text = "---------------------------------------------------------------------------------" +
    "-----------------------------------------------------------------------------";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Cambria", 8F);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label3.IsSeparatorLine = true;
            this.label3.Location = new System.Drawing.Point(537, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(8, 316);
            this.label3.StretchToFitForm = true;
            this.label3.TabIndex = 22;
            this.label3.Tag = "";
            this.label3.Text = "---------------------------------------------------------------------------------" +
    "-----------------------------------------------------------------------------";
            // 
            // PropertiesWindowRichTextBox
            // 
            this.PropertiesWindowRichTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.PropertiesWindowRichTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.PropertiesWindowRichTextBox.Location = new System.Drawing.Point(545, 184);
            this.PropertiesWindowRichTextBox.Name = "PropertiesWindowRichTextBox";
            this.PropertiesWindowRichTextBox.ReadOnly = true;
            this.PropertiesWindowRichTextBox.ShortcutsEnabled = false;
            this.PropertiesWindowRichTextBox.Size = new System.Drawing.Size(284, 385);
            this.PropertiesWindowRichTextBox.TabIndex = 10;
            this.PropertiesWindowRichTextBox.TabStop = false;
            this.PropertiesWindowRichTextBox.Text = "";
            // 
            // ScriptSelectionLabel
            // 
            this.ScriptSelectionLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ScriptSelectionLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.ScriptSelectionLabel.ForeColor = System.Drawing.Color.Gold;
            this.ScriptSelectionLabel.IsSeparatorLine = false;
            this.ScriptSelectionLabel.Location = new System.Drawing.Point(9, 190);
            this.ScriptSelectionLabel.Name = "ScriptSelectionLabel";
            this.ScriptSelectionLabel.Size = new System.Drawing.Size(526, 19);
            this.ScriptSelectionLabel.StretchToFitForm = false;
            this.ScriptSelectionLabel.TabIndex = 20;
            this.ScriptSelectionLabel.Text = "Selected Script: [None]";
            // 
            // SeperatorLine2
            // 
            this.SeperatorLine2.Font = new System.Drawing.Font("Cambria", 8F);
            this.SeperatorLine2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine2.IsSeparatorLine = true;
            this.SeperatorLine2.Location = new System.Drawing.Point(1, 177);
            this.SeperatorLine2.Name = "SeperatorLine2";
            this.SeperatorLine2.Size = new System.Drawing.Size(539, 10);
            this.SeperatorLine2.StretchToFitForm = false;
            this.SeperatorLine2.TabIndex = 21;
            this.SeperatorLine2.Tag = "True";
            this.SeperatorLine2.Text = "---------------------------------------------------------------------------------" +
    "----------------------------------------------";
            // 
            // VersionLabel
            // 
            this.VersionLabel.Font = new System.Drawing.Font("Segoe UI", 6.5F);
            this.VersionLabel.ForeColor = System.Drawing.Color.Gold;
            this.VersionLabel.IsSeparatorLine = false;
            this.VersionLabel.Location = new System.Drawing.Point(201, 3);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(58, 13);
            this.VersionLabel.StretchToFitForm = false;
            this.VersionLabel.TabIndex = 18;
            this.VersionLabel.Text = "==version==";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ScriptStatusLabel
            // 
            this.ScriptStatusLabel.Font = new System.Drawing.Font("Segoe UI", 10.25F, System.Drawing.FontStyle.Bold);
            this.ScriptStatusLabel.ForeColor = System.Drawing.Color.Gold;
            this.ScriptStatusLabel.IsSeparatorLine = false;
            this.ScriptStatusLabel.Location = new System.Drawing.Point(7, 106);
            this.ScriptStatusLabel.Name = "ScriptStatusLabel";
            this.ScriptStatusLabel.Size = new System.Drawing.Size(523, 23);
            this.ScriptStatusLabel.StretchToFitForm = false;
            this.ScriptStatusLabel.TabIndex = 0;
            this.ScriptStatusLabel.Text = "Status: Inactive";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(834, 572);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.debugTabCheckBtn);
            this.Controls.Add(this.dummy1);
            this.Controls.Add(this.dummy0);
            this.Controls.Add(this.debugDisableLinesBtn);
            this.Controls.Add(this.optionsMenuDropdownBtn);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.AbortOrCloseBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.PropertiesWindowRichTextBox);
            this.Controls.Add(this.ScriptSelectionLabel);
            this.Controls.Add(this.SeperatorLine2);
            this.Controls.Add(this.PropertiesPanel);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.debugMiscBtn);
            this.Controls.Add(this.ReloadScriptBtn);
            this.Controls.Add(this.ExitBtn);
            this.Controls.Add(this.MinimizeBtn);
            this.Controls.Add(this.ScriptStatusLabel);
            this.Controls.Add(this.BinPathBrowseBtn);
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
        private Button BinPathBrowseBtn;
        private RichTextBox PropertiesWindowRichTextBox;
        private Button optionsMenuDropdownBtn;
        private Label ScriptStatusLabel;
        private Button ExitBtn;
        private Button MinimizeBtn;
        private Button AbortOrCloseBtn;
        private Button ReloadScriptBtn;
        private Label label1;
        #endregion

        private Button debugMiscBtn;
        private Label VersionLabel;
        private GroupBox PropertiesPanel;
        private Label ScriptSelectionLabel;
        private Label SeperatorLine2;
        private Label label3;
        private Label label5;
        private CheckBox debugDisableLinesBtn;
        private Button dummy0;
        private Button dummy1;
        private Button debugTabCheckBtn;
        private Label label2;
    }
}

