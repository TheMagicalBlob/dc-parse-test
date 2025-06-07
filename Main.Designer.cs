using System.Drawing;
using System;
using System.Windows.Forms;

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
            this.ChoosePropertyBtn = new System.Windows.Forms.Button();
            this.ActiveScriptLabel = new System.Windows.Forms.Label();
            this.ExitBtn = new System.Windows.Forms.Button();
            this.MinimizeBtn = new System.Windows.Forms.Button();
            this.AbortBtn = new System.Windows.Forms.Button();
            this.ReloadScriptBtn = new System.Windows.Forms.Button();
            this.DisplayModeToggleCheckBox = new System.Windows.Forms.CheckBox();
            this.ClearBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.redirectCheckBox = new System.Windows.Forms.CheckBox();
            this.OutputWindowRichTextBox = new weapon_data.RichTextBox();
            this.binPathTextBox = new weapon_data.TextBox();
            this.SuspendLayout();
            // 
            // BinPathBrowseBtn
            // 
            this.BinPathBrowseBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.BinPathBrowseBtn.Location = new System.Drawing.Point(566, 65);
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
            this.optionsMenuDropdownBtn.Location = new System.Drawing.Point(7, 65);
            this.optionsMenuDropdownBtn.Name = "optionsMenuDropdownBtn";
            this.optionsMenuDropdownBtn.Size = new System.Drawing.Size(75, 23);
            this.optionsMenuDropdownBtn.TabIndex = 3;
            this.optionsMenuDropdownBtn.Text = "Options...";
            this.optionsMenuDropdownBtn.UseVisualStyleBackColor = false;
            this.optionsMenuDropdownBtn.Click += new System.EventHandler(this.OptionsMenuDropdownBtn_Click);
            // 
            // ChoosePropertyBtn
            // 
            this.ChoosePropertyBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.ChoosePropertyBtn.Location = new System.Drawing.Point(88, 66);
            this.ChoosePropertyBtn.Name = "ChoosePropertyBtn";
            this.ChoosePropertyBtn.Size = new System.Drawing.Size(103, 23);
            this.ChoosePropertyBtn.TabIndex = 4;
            this.ChoosePropertyBtn.Text = "Choose Property";
            this.ChoosePropertyBtn.UseVisualStyleBackColor = false;
            this.ChoosePropertyBtn.Visible = false;
            this.ChoosePropertyBtn.Click += new System.EventHandler(this.ChoosePropertyBtn_Click);
            // 
            // ActiveScriptLabel
            // 
            this.ActiveScriptLabel.Font = new System.Drawing.Font("Segoe UI", 10.25F, System.Drawing.FontStyle.Bold);
            this.ActiveScriptLabel.ForeColor = System.Drawing.Color.Gold;
            this.ActiveScriptLabel.Location = new System.Drawing.Point(8, 129);
            this.ActiveScriptLabel.Name = "ActiveScriptLabel";
            this.ActiveScriptLabel.Size = new System.Drawing.Size(573, 23);
            this.ActiveScriptLabel.TabIndex = 0;
            this.ActiveScriptLabel.Text = "Selected Script: none selected";
            // 
            // ExitBtn
            // 
            this.ExitBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.ExitBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ExitBtn.Font = new System.Drawing.Font("Gadugi", 8.25F, System.Drawing.FontStyle.Bold);
            this.ExitBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.ExitBtn.Location = new System.Drawing.Point(615, 3);
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
            this.MinimizeBtn.Location = new System.Drawing.Point(593, 3);
            this.MinimizeBtn.Name = "MinimizeBtn";
            this.MinimizeBtn.Size = new System.Drawing.Size(22, 22);
            this.MinimizeBtn.TabIndex = 7;
            this.MinimizeBtn.Text = "-";
            this.MinimizeBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.MinimizeBtn.UseVisualStyleBackColor = false;
            // 
            // AbortBtn
            // 
            this.AbortBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.AbortBtn.Location = new System.Drawing.Point(451, 65);
            this.AbortBtn.Name = "AbortBtn";
            this.AbortBtn.Size = new System.Drawing.Size(44, 23);
            this.AbortBtn.TabIndex = 0;
            this.AbortBtn.Text = "Abort";
            this.AbortBtn.UseVisualStyleBackColor = false;
            this.AbortBtn.Visible = false;
            this.AbortBtn.Click += new System.EventHandler(this.AbortBinFileParse);
            // 
            // ReloadScriptBtn
            // 
            this.ReloadScriptBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.ReloadScriptBtn.Enabled = false;
            this.ReloadScriptBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadScriptBtn.Location = new System.Drawing.Point(498, 65);
            this.ReloadScriptBtn.Name = "ReloadScriptBtn";
            this.ReloadScriptBtn.Size = new System.Drawing.Size(65, 23);
            this.ReloadScriptBtn.TabIndex = 10;
            this.ReloadScriptBtn.Text = "Reload";
            this.ReloadScriptBtn.UseVisualStyleBackColor = false;
            this.ReloadScriptBtn.Click += new System.EventHandler(this.ReloadBinFile);
            // 
            // DisplayModeToggleCheckBox
            // 
            this.DisplayModeToggleCheckBox.AutoSize = true;
            this.DisplayModeToggleCheckBox.Location = new System.Drawing.Point(8, 92);
            this.DisplayModeToggleCheckBox.Name = "DisplayModeToggleCheckBox";
            this.DisplayModeToggleCheckBox.Size = new System.Drawing.Size(106, 17);
            this.DisplayModeToggleCheckBox.TabIndex = 11;
            this.DisplayModeToggleCheckBox.Text = "Interactive Mode";
            this.DisplayModeToggleCheckBox.UseVisualStyleBackColor = true;
            this.DisplayModeToggleCheckBox.Visible = false;
            // 
            // ClearBtn
            // 
            this.ClearBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.ClearBtn.Location = new System.Drawing.Point(587, 131);
            this.ClearBtn.Name = "ClearBtn";
            this.ClearBtn.Size = new System.Drawing.Size(44, 23);
            this.ClearBtn.TabIndex = 4;
            this.ClearBtn.Text = "Clear";
            this.ClearBtn.UseVisualStyleBackColor = false;
            this.ClearBtn.Click += new System.EventHandler(this.ClearBtn_Click);
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
            // 
            // redirectCheckBox
            // 
            this.redirectCheckBox.AutoSize = true;
            this.redirectCheckBox.Location = new System.Drawing.Point(8, 109);
            this.redirectCheckBox.Name = "redirectCheckBox";
            this.redirectCheckBox.Size = new System.Drawing.Size(216, 17);
            this.redirectCheckBox.TabIndex = 13;
            this.redirectCheckBox.Text = "Redirect Debug Prints to OutputWindow";
            this.redirectCheckBox.UseVisualStyleBackColor = true;
            this.redirectCheckBox.CheckedChanged += new System.EventHandler(this.redirectCheckBox_CheckedChanged);
            // 
            // OutputWindowRichTextBox
            // 
            this.OutputWindowRichTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.OutputWindowRichTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.OutputWindowRichTextBox.Location = new System.Drawing.Point(8, 156);
            this.OutputWindowRichTextBox.Name = "OutputWindowRichTextBox";
            this.OutputWindowRichTextBox.ReadOnly = true;
            this.OutputWindowRichTextBox.ShortcutsEnabled = false;
            this.OutputWindowRichTextBox.Size = new System.Drawing.Size(623, 369);
            this.OutputWindowRichTextBox.TabIndex = 2;
            this.OutputWindowRichTextBox.Text = "";
            // 
            // binPathTextBox
            // 
            this.binPathTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(42)))), ((int)(((byte)(42)))), ((int)(((byte)(42)))));
            this.binPathTextBox.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic);
            this.binPathTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.binPathTextBox.Location = new System.Drawing.Point(7, 38);
            this.binPathTextBox.Name = "binPathTextBox";
            this.binPathTextBox.Size = new System.Drawing.Size(624, 24);
            this.binPathTextBox.TabIndex = 3;
            this.binPathTextBox.Text = "Select Browse Button or Paste .bin Path Here";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(639, 533);
            this.Controls.Add(this.redirectCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ClearBtn);
            this.Controls.Add(this.DisplayModeToggleCheckBox);
            this.Controls.Add(this.ReloadScriptBtn);
            this.Controls.Add(this.AbortBtn);
            this.Controls.Add(this.ExitBtn);
            this.Controls.Add(this.MinimizeBtn);
            this.Controls.Add(this.ChoosePropertyBtn);
            this.Controls.Add(this.ActiveScriptLabel);
            this.Controls.Add(this.optionsMenuDropdownBtn);
            this.Controls.Add(this.OutputWindowRichTextBox);
            this.Controls.Add(this.BinPathBrowseBtn);
            this.Controls.Add(this.binPathTextBox);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Main";
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        /// <summary>
        /// Post-InitializeComponent Configuration. <br/><br/>
        /// Create Assign Anonomous Event Handlers to Parent and Children.
        /// </summary>
        public void InitializeAdditionalEventHandlers()
        {
            MinimizeBtn.Click      += new EventHandler((sender, e) => ActiveForm.WindowState      = FormWindowState.Minimized     );
            MinimizeBtn.MouseEnter += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(90, 100, 255  ));
            MinimizeBtn.MouseLeave += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(0 , 0  , 0    ));
            ExitBtn.Click          += new EventHandler((sender, e) => Environment.Exit(                            0             ));
            ExitBtn.MouseEnter     += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(230, 100, 100 ));
            ExitBtn.MouseLeave     += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(0  , 0  , 0   ));


            // Set Event Handlers for Form Dragging
            MouseDown += new MouseEventHandler((sender, e) =>
            {
                Main.MouseDif = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);

                Main.MouseIsDown = true;
                DropdownMenu[1].Visible = DropdownMenu[0].Visible = false;
            });

            MouseUp   += new MouseEventHandler((sender, e) =>
            {
                Main.MouseIsDown = false;

                if (Main.OptionsPageIsOpen)
                    Main.Azem?.BringToFront();
            });

            MouseMove += new MouseEventHandler((sender, e) => Main.MoveForm());


            // Set appropriate event handlers for the controls on the form as well
            foreach (Control Item in Controls)
            {
                if (Item.Name == "SwapBrowseModeBtn") // lazy fix to avoid the mouse down event confliciting with the button
                    continue;
                
                Item.MouseDown += new MouseEventHandler((sender, e) =>
                {
                    Main.MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                    Main.MouseIsDown = true;
                    DropdownMenu[1].Visible = DropdownMenu[0].Visible = false;
                });
                Item.MouseUp   += new MouseEventHandler((sender, e) =>
                {
                    MouseIsDown = false;
                    if (OptionsPageIsOpen)
                        Azem?.BringToFront();
                });
                
                // Avoid Applying MoveForm EventHandler to Text Containters (to retain the ability to drag-select text)
                if (Item.GetType() != typeof(TextBox) && Item.GetType() != typeof(RichTextBox))
                    Item.MouseMove += new MouseEventHandler((sender, e) => MoveForm());
            }

            Paint += Main.PaintBorder;
        }

        /// <summary>
        /// Initialize Dropdown Menu Used for Toggling of Folder Browser Method
        /// </summary>
        private void CreateBrowseModeDropdownMenu()
        {
            var extalignment = BinPathBrowseBtn.Size.Height;
            var alignment = BinPathBrowseBtn.Location;

            var ButtonSize = new Size(BinPathBrowseBtn.Size.Width + optionsMenuDropdownBtn.Size.Width, 25);

            DropdownMenu[0] = new Button() {
                Font = new Font("Gadugi", 7.25f, FontStyle.Bold),
                Text = "Directory Tree*",
                BackColor = Main.AppColour,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(alignment.X, alignment.Y + extalignment),
                Size = ButtonSize
            };
            DropdownMenu[1] = new Button() {
                Font = new Font("Gadugi", 7.25F, FontStyle.Bold),
                Text = "File Browser",
                BackColor = Main.AppColour,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(alignment.X, alignment.Y + extalignment + DropdownMenu[0].Size.Height),
                Size = ButtonSize
            };



            // Create and Assign Event Handlers
            DropdownMenu[0].Click += (why, does) =>
            {
                if (!Main.LegacyFolderSelectionDialogue) {
                    DropdownMenu[0].Text += '*';
                    DropdownMenu[1].Text = DropdownMenu[1].Text.Remove(DropdownMenu[1].Text.Length-1);

                    Main.LegacyFolderSelectionDialogue ^= true;
                }
            };
            DropdownMenu[1].Click += (my, back) =>
            {
                if (Main.LegacyFolderSelectionDialogue) {
                    DropdownMenu[1].Text += '*';
                    DropdownMenu[0].Text = DropdownMenu[0].Text.Remove(DropdownMenu[0].Text.Length-1);

                    Main.LegacyFolderSelectionDialogue ^= true;
                }
            };
            // hurt. there was a third event at first.


            // Add Controls to MainForm Control Collection
            Controls.Add(DropdownMenu[0]);
            Controls.Add(DropdownMenu[1]);

            // Ensure Controls Display Correctly
            DropdownMenu[0].Hide();
            DropdownMenu[1].Hide();
            DropdownMenu[0].BringToFront();
            DropdownMenu[1].BringToFront();
        }
        #endregion
        

        
        //================================\\
        //--|   Control Declarations   |--\\
        //================================\\
        #region Control Declarations

        public Button[] DropdownMenu = new Button[2];
        private TextBox binPathTextBox;
        private Button BinPathBrowseBtn;
        private RichTextBox OutputWindowRichTextBox;
        private Button optionsMenuDropdownBtn;
        private Button ChoosePropertyBtn;
        private Label ActiveScriptLabel;

        private Button ExitBtn;
        private Button MinimizeBtn;
        #endregion

        private Button AbortBtn;
        private Button ReloadScriptBtn;
        private CheckBox DisplayModeToggleCheckBox;
        private Button ClearBtn;
        private Label label1;
        private CheckBox redirectCheckBox;
    }
}

