using System.Drawing;
using System;
using System.Windows.Forms;
using System.Linq;

namespace NaughtyDogDCReader
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
            this.OptionsMenuDropdownBtn = new System.Windows.Forms.Button();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.MinimizeBtn = new System.Windows.Forms.Button();
            this.CloseBtn = new System.Windows.Forms.Button();
            this.ReloadScriptBtn = new System.Windows.Forms.Button();
            this.dummy0 = new System.Windows.Forms.Button();
            this.dummy1 = new System.Windows.Forms.Button();
            this.DebugOptionsPageBtn = new System.Windows.Forms.Button();
            this.SidBaseBrowseBtn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.propertyListContainer = new System.Windows.Forms.Panel();
            this.propertyListPanel = new NaughtyDogDCReader.GroupBox();
            this.PropertyEditorContainer = new System.Windows.Forms.Panel();
            this.propertyEditorPanel = new NaughtyDogDCReader.GroupBox();
            this.propertyForwardBtn = new System.Windows.Forms.Button();
            this.propertyBackBtn = new System.Windows.Forms.Button();
            this.BackAndLoadButtonsLabel = new NaughtyDogDCReader.Label();
            this.label4 = new NaughtyDogDCReader.Label();
            this.logWindow = new NaughtyDogDCReader.RichTextBox();
            this.label1 = new NaughtyDogDCReader.Label();
            this.label2 = new NaughtyDogDCReader.Label();
            this.label3 = new NaughtyDogDCReader.Label();
            this.scriptSelectionLabel = new NaughtyDogDCReader.Label();
            this.SeperatorLine2 = new NaughtyDogDCReader.Label();
            this.VersionLabel = new NaughtyDogDCReader.Label();
            this.activeScriptLabel = new NaughtyDogDCReader.Label();
            this.propertyListContainer.SuspendLayout();
            this.PropertyEditorContainer.SuspendLayout();
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
            this.BinFileBrowseBtn.Click += new System.EventHandler(this.BrowseForDCScript);
            // 
            // OptionsMenuDropdownBtn
            // 
            this.OptionsMenuDropdownBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.OptionsMenuDropdownBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OptionsMenuDropdownBtn.Location = new System.Drawing.Point(763, 5);
            this.OptionsMenuDropdownBtn.Name = "OptionsMenuDropdownBtn";
            this.OptionsMenuDropdownBtn.Size = new System.Drawing.Size(71, 23);
            this.OptionsMenuDropdownBtn.TabIndex = 6;
            this.OptionsMenuDropdownBtn.Text = "Options...";
            this.OptionsMenuDropdownBtn.UseVisualStyleBackColor = false;
            this.OptionsMenuDropdownBtn.Click += new System.EventHandler(this.ToggleOptionsMenu);
            // 
            // ExitBtn
            // 
            this.ExitBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.ExitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ExitBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.ExitBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ExitBtn.Location = new System.Drawing.Point(864, 4);
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
            this.MinimizeBtn.Location = new System.Drawing.Point(839, 4);
            this.MinimizeBtn.Name = "MinimizeBtn";
            this.MinimizeBtn.Size = new System.Drawing.Size(22, 22);
            this.MinimizeBtn.TabIndex = 7;
            this.MinimizeBtn.Text = "-";
            this.MinimizeBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.MinimizeBtn.UseVisualStyleBackColor = false;
            // 
            // CloseBtn
            // 
            this.CloseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.CloseBtn.Enabled = false;
            this.CloseBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloseBtn.Location = new System.Drawing.Point(296, 41);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new System.Drawing.Size(110, 23);
            this.CloseBtn.TabIndex = 3;
            this.CloseBtn.Text = "Close Current Script";
            this.CloseBtn.UseVisualStyleBackColor = false;
            this.CloseBtn.Click += new System.EventHandler(this.CloseBinFileButtonPressed);
            // 
            // ReloadScriptBtn
            // 
            this.ReloadScriptBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.ReloadScriptBtn.Enabled = false;
            this.ReloadScriptBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReloadScriptBtn.Font = new System.Drawing.Font("Microsoft Tai Le", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadScriptBtn.Location = new System.Drawing.Point(409, 41);
            this.ReloadScriptBtn.Name = "ReloadScriptBtn";
            this.ReloadScriptBtn.Size = new System.Drawing.Size(127, 23);
            this.ReloadScriptBtn.TabIndex = 4;
            this.ReloadScriptBtn.Text = "Reload Current Script";
            this.ReloadScriptBtn.UseVisualStyleBackColor = false;
            this.ReloadScriptBtn.Click += new System.EventHandler(this.ReloadBinFile);
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
            // DebugOptionsPageBtn
            // 
            this.DebugOptionsPageBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.DebugOptionsPageBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DebugOptionsPageBtn.Location = new System.Drawing.Point(698, 5);
            this.DebugOptionsPageBtn.Name = "DebugOptionsPageBtn";
            this.DebugOptionsPageBtn.Size = new System.Drawing.Size(59, 23);
            this.DebugOptionsPageBtn.TabIndex = 45;
            this.DebugOptionsPageBtn.Text = "DEBUG";
            this.DebugOptionsPageBtn.UseVisualStyleBackColor = false;
            this.DebugOptionsPageBtn.Click += new System.EventHandler(this.ToggleDebugOptionsPage);
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
            this.SidBaseBrowseBtn.Click += new System.EventHandler(this.BrowseForSidbase);
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
            // propertyListContainer
            // 
            this.propertyListContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.propertyListContainer.Controls.Add(this.propertyListPanel);
            this.propertyListContainer.Location = new System.Drawing.Point(6, 280);
            this.propertyListContainer.Name = "propertyListContainer";
            this.propertyListContainer.Size = new System.Drawing.Size(530, 364);
            this.propertyListContainer.TabIndex = 0;
            // 
            // propertyListPanel
            // 
            this.propertyListPanel.BackColor = System.Drawing.Color.Black;
            this.propertyListPanel.CausesValidation = false;
            this.propertyListPanel.Cursor = System.Windows.Forms.Cursors.Default;
            this.propertyListPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.propertyListPanel.Location = new System.Drawing.Point(0, -1);
            this.propertyListPanel.Name = "propertyListPanel";
            this.propertyListPanel.Size = new System.Drawing.Size(530, 364);
            this.propertyListPanel.TabIndex = 19;
            this.propertyListPanel.TabStop = false;
            // 
            // PropertyEditorContainer
            // 
            this.PropertyEditorContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PropertyEditorContainer.Controls.Add(this.propertyEditorPanel);
            this.PropertyEditorContainer.Location = new System.Drawing.Point(546, 238);
            this.PropertyEditorContainer.Name = "PropertyEditorContainer";
            this.PropertyEditorContainer.Size = new System.Drawing.Size(338, 406);
            this.PropertyEditorContainer.TabIndex = 0;
            // 
            // propertyEditorPanel
            // 
            this.propertyEditorPanel.BackColor = System.Drawing.Color.Black;
            this.propertyEditorPanel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.propertyEditorPanel.Location = new System.Drawing.Point(0, 0);
            this.propertyEditorPanel.Name = "propertyEditorPanel";
            this.propertyEditorPanel.Size = new System.Drawing.Size(338, 406);
            this.propertyEditorPanel.TabIndex = 20;
            this.propertyEditorPanel.TabStop = false;
            // 
            // propertyForwardBtn
            // 
            this.propertyForwardBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.propertyForwardBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.propertyForwardBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.propertyForwardBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.propertyForwardBtn.Location = new System.Drawing.Point(32, 254);
            this.propertyForwardBtn.Name = "propertyForwardBtn";
            this.propertyForwardBtn.Size = new System.Drawing.Size(22, 22);
            this.propertyForwardBtn.TabIndex = 55;
            this.propertyForwardBtn.Text = ">";
            this.propertyForwardBtn.UseVisualStyleBackColor = false;
            this.propertyForwardBtn.Click += new System.EventHandler(this.DeleteMe2);
            // 
            // propertyBackBtn
            // 
            this.propertyBackBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.propertyBackBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.propertyBackBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.propertyBackBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.propertyBackBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.propertyBackBtn.Location = new System.Drawing.Point(6, 254);
            this.propertyBackBtn.Name = "propertyBackBtn";
            this.propertyBackBtn.Size = new System.Drawing.Size(22, 22);
            this.propertyBackBtn.TabIndex = 54;
            this.propertyBackBtn.Text = "<";
            this.propertyBackBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.propertyBackBtn.UseVisualStyleBackColor = false;
            this.propertyBackBtn.Click += new System.EventHandler(this.DeleteMe1);
            // 
            // BackAndLoadButtonsLabel
            // 
            this.BackAndLoadButtonsLabel.Font = new System.Drawing.Font("Segoe UI", 6.5F);
            this.BackAndLoadButtonsLabel.ForeColor = System.Drawing.Color.Gold;
            this.BackAndLoadButtonsLabel.IsSeparatorLine = false;
            this.BackAndLoadButtonsLabel.Location = new System.Drawing.Point(5, 239);
            this.BackAndLoadButtonsLabel.Name = "BackAndLoadButtonsLabel";
            this.BackAndLoadButtonsLabel.Size = new System.Drawing.Size(50, 13);
            this.BackAndLoadButtonsLabel.StretchToFitForm = false;
            this.BackAndLoadButtonsLabel.TabIndex = 56;
            this.BackAndLoadButtonsLabel.Text = "Back    Load";
            this.BackAndLoadButtonsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Cambria", 8F);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label4.IsSeparatorLine = true;
            this.label4.Location = new System.Drawing.Point(1, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(539, 10);
            this.label4.StretchToFitForm = true;
            this.label4.TabIndex = 47;
            this.label4.Text = "---------------------------------------------------------------------------------" +
    "----------------------------------------------";
            // 
            // logWindow
            // 
            this.logWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.logWindow.Font = new System.Drawing.Font("Segoe UI Semibold", 7.5F);
            this.logWindow.ForeColor = System.Drawing.SystemColors.Window;
            this.logWindow.Location = new System.Drawing.Point(5, 75);
            this.logWindow.Name = "logWindow";
            this.logWindow.ReadOnly = true;
            this.logWindow.ShortcutsEnabled = false;
            this.logWindow.Size = new System.Drawing.Size(531, 153);
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
            this.label3.Location = new System.Drawing.Point(538, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(8, 612);
            this.label3.StretchToFitForm = false;
            this.label3.TabIndex = 22;
            this.label3.Tag = "";
            this.label3.Text = "---------------------------------------------------------------------------------" +
    "-----------------------------------------------------------------------------";
            // 
            // scriptSelectionLabel
            // 
            this.scriptSelectionLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scriptSelectionLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.scriptSelectionLabel.ForeColor = System.Drawing.Color.Gold;
            this.scriptSelectionLabel.IsSeparatorLine = false;
            this.scriptSelectionLabel.Location = new System.Drawing.Point(547, 80);
            this.scriptSelectionLabel.Name = "scriptSelectionLabel";
            this.scriptSelectionLabel.Size = new System.Drawing.Size(334, 147);
            this.scriptSelectionLabel.StretchToFitForm = false;
            this.scriptSelectionLabel.TabIndex = 20;
            this.scriptSelectionLabel.Text = "Angine\r\nDe\r\nPoitrine";
            // 
            // SeperatorLine2
            // 
            this.SeperatorLine2.Font = new System.Drawing.Font("Cambria", 8F);
            this.SeperatorLine2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.SeperatorLine2.IsSeparatorLine = true;
            this.SeperatorLine2.Location = new System.Drawing.Point(1, 225);
            this.SeperatorLine2.Name = "SeperatorLine2";
            this.SeperatorLine2.Size = new System.Drawing.Size(539, 10);
            this.SeperatorLine2.StretchToFitForm = true;
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
            this.VersionLabel.Size = new System.Drawing.Size(64, 13);
            this.VersionLabel.StretchToFitForm = false;
            this.VersionLabel.TabIndex = 18;
            this.VersionLabel.Text = "Ver.";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // activeScriptLabel
            // 
            this.activeScriptLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(21)))), ((int)(((byte)(21)))), ((int)(((byte)(21)))));
            this.activeScriptLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.activeScriptLabel.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.activeScriptLabel.ForeColor = System.Drawing.Color.Gold;
            this.activeScriptLabel.IsSeparatorLine = false;
            this.activeScriptLabel.Location = new System.Drawing.Point(620, 41);
            this.activeScriptLabel.Name = "activeScriptLabel";
            this.activeScriptLabel.Size = new System.Drawing.Size(204, 20);
            this.activeScriptLabel.StretchToFitForm = false;
            this.activeScriptLabel.TabIndex = 0;
            this.activeScriptLabel.Text = "no selection";
            this.activeScriptLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(890, 650);
            this.Controls.Add(this.BackAndLoadButtonsLabel);
            this.Controls.Add(this.propertyForwardBtn);
            this.Controls.Add(this.propertyBackBtn);
            this.Controls.Add(this.propertyListContainer);
            this.Controls.Add(this.PropertyEditorContainer);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.SidBaseBrowseBtn);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.ReloadScriptBtn);
            this.Controls.Add(this.BinFileBrowseBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.logWindow);
            this.Controls.Add(this.DebugOptionsPageBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dummy1);
            this.Controls.Add(this.dummy0);
            this.Controls.Add(this.OptionsMenuDropdownBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.scriptSelectionLabel);
            this.Controls.Add(this.SeperatorLine2);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.ExitBtn);
            this.Controls.Add(this.MinimizeBtn);
            this.Controls.Add(this.activeScriptLabel);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Main";
            this.propertyListContainer.ResumeLayout(false);
            this.PropertyEditorContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
        

        
        //================================\\
        //--|   Control Declarations   |--\\
        //================================\\
        #region [Control Declarations]

        public Button[] DropdownMenu = new Button[2];
        private Button BinFileBrowseBtn;
        private Button CloseBtn;
        private Button ReloadScriptBtn;
        private Label label1;
        private Label VersionLabel;
        private GroupBox propertyListPanel;
        private Label SeperatorLine2;
        private Label label3;
        private Button dummy0;
        private Button dummy1;
        private Label label2;
        private Button DebugOptionsPageBtn;
        private RichTextBox logWindow;
        private Label label4;
        private Button SidBaseBrowseBtn;
        #endregion
        private Button button1;
        private Button button2;
        public Button OptionsMenuDropdownBtn;
        public Button ExitBtn;
        public Button MinimizeBtn;
        public Label activeScriptLabel;
        public Label scriptSelectionLabel;
        private Panel propertyListContainer;
        private GroupBox propertyEditorPanel;
        public Panel PropertyEditorContainer;
        public Button propertyForwardBtn;
        public Button propertyBackBtn;
        private Label BackAndLoadButtonsLabel;
    }
}

