﻿using System.Drawing;
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
            this.label4 = new weapon_data.Label();
            this.logWindow = new weapon_data.RichTextBox();
            this.label1 = new weapon_data.Label();
            this.label2 = new weapon_data.Label();
            this.label3 = new weapon_data.Label();
            this.propertiesWindow = new weapon_data.RichTextBox();
            this.scriptSelectionLabel = new weapon_data.Label();
            this.SeperatorLine2 = new weapon_data.Label();
            this.VersionLabel = new weapon_data.Label();
            this.scriptStatusLabel = new weapon_data.Label();
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
            this.abortOrCloseBtn.Location = new System.Drawing.Point(393, 41);
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
            this.ReloadScriptBtn.Location = new System.Drawing.Point(440, 41);
            this.ReloadScriptBtn.Name = "ReloadScriptBtn";
            this.ReloadScriptBtn.Size = new System.Drawing.Size(95, 23);
            this.ReloadScriptBtn.TabIndex = 4;
            this.ReloadScriptBtn.Text = "Reload Current";
            this.ReloadScriptBtn.UseVisualStyleBackColor = false;
            this.ReloadScriptBtn.Click += new System.EventHandler(this.ReloadBinFile);
            // 
            // propertiesPanel
            // 
            this.propertiesPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.propertiesPanel.Location = new System.Drawing.Point(5, 249);
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
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Cambria", 8F);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label4.IsSeparatorLine = true;
            this.label4.Location = new System.Drawing.Point(1, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(539, 10);
            this.label4.StretchToFitForm = false;
            this.label4.TabIndex = 47;
            this.label4.Tag = "True";
            this.label4.Text = "---------------------------------------------------------------------------------" +
    "----------------------------------------------";
            // 
            // logWindow
            // 
            this.logWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.logWindow.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
            this.logWindow.ForeColor = System.Drawing.SystemColors.Window;
            this.logWindow.Location = new System.Drawing.Point(5, 101);
            this.logWindow.Name = "logWindow";
            this.logWindow.ReadOnly = true;
            this.logWindow.ShortcutsEnabled = false;
            this.logWindow.Size = new System.Drawing.Size(530, 119);
            this.logWindow.TabIndex = 46;
            this.logWindow.TabStop = false;
            this.logWindow.Text = "dead hands feel no  B r e a d";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gold;
            this.label1.IsSeparatorLine = false;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 30);
            this.label1.StretchToFitForm = false;
            this.label1.TabIndex = 12;
            this.label1.Text = "NaughtyDog DC Editor";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Cambria", 8F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label2.IsSeparatorLine = true;
            this.label2.Location = new System.Drawing.Point(7, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(644, 13);
            this.label2.StretchToFitForm = true;
            this.label2.TabIndex = 44;
            this.label2.Tag = "";
            this.label2.Text = "---------------------------------------------------------------------------------" +
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
            // propertiesWindow
            // 
            this.propertiesWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.propertiesWindow.ForeColor = System.Drawing.SystemColors.Window;
            this.propertiesWindow.Location = new System.Drawing.Point(545, 227);
            this.propertiesWindow.Name = "propertiesWindow";
            this.propertiesWindow.ReadOnly = true;
            this.propertiesWindow.ShortcutsEnabled = false;
            this.propertiesWindow.Size = new System.Drawing.Size(338, 385);
            this.propertiesWindow.TabIndex = 10;
            this.propertiesWindow.TabStop = false;
            this.propertiesWindow.Text = "recto verso";
            // 
            // scriptSelectionLabel
            // 
            this.scriptSelectionLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scriptSelectionLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.scriptSelectionLabel.ForeColor = System.Drawing.Color.Gold;
            this.scriptSelectionLabel.IsSeparatorLine = false;
            this.scriptSelectionLabel.Location = new System.Drawing.Point(7, 231);
            this.scriptSelectionLabel.Name = "scriptSelectionLabel";
            this.scriptSelectionLabel.Size = new System.Drawing.Size(526, 19);
            this.scriptSelectionLabel.StretchToFitForm = false;
            this.scriptSelectionLabel.TabIndex = 20;
            this.scriptSelectionLabel.Text = "Selected Script: [None]";
            // 
            // SeperatorLine2
            // 
            this.SeperatorLine2.Font = new System.Drawing.Font("Cambria", 8F);
            this.SeperatorLine2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine2.IsSeparatorLine = true;
            this.SeperatorLine2.Location = new System.Drawing.Point(1, 218);
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
            this.VersionLabel.Location = new System.Drawing.Point(214, 3);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(58, 13);
            this.VersionLabel.StretchToFitForm = false;
            this.VersionLabel.TabIndex = 18;
            this.VersionLabel.Text = "Ver.";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // scriptStatusLabel
            // 
            this.scriptStatusLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.scriptStatusLabel.ForeColor = System.Drawing.Color.Gold;
            this.scriptStatusLabel.IsSeparatorLine = false;
            this.scriptStatusLabel.Location = new System.Drawing.Point(4, 75);
            this.scriptStatusLabel.Name = "scriptStatusLabel";
            this.scriptStatusLabel.Size = new System.Drawing.Size(523, 23);
            this.scriptStatusLabel.StretchToFitForm = false;
            this.scriptStatusLabel.TabIndex = 0;
            this.scriptStatusLabel.Text = "Status: [Inactive]";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(888, 618);
            this.Controls.Add(this.SidBaseBrowseBtn);
            this.Controls.Add(this.abortOrCloseBtn);
            this.Controls.Add(this.ReloadScriptBtn);
            this.Controls.Add(this.BinFileBrowseBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.logWindow);
            this.Controls.Add(this.debugPanelBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dummy1);
            this.Controls.Add(this.dummy0);
            this.Controls.Add(this.optionsMenuDropdownBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.propertiesWindow);
            this.Controls.Add(this.scriptSelectionLabel);
            this.Controls.Add(this.SeperatorLine2);
            this.Controls.Add(this.propertiesPanel);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.ExitBtn);
            this.Controls.Add(this.MinimizeBtn);
            this.Controls.Add(this.scriptStatusLabel);
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
        private Button optionsMenuDropdownBtn;
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
    }
}

