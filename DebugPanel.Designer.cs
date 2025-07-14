using System.Collections.Generic;
using System.Drawing;
using System;
using System.Windows.Forms;
using static weapon_data.Main;

namespace weapon_data
{
    partial class DebugPanel
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
            this.CloseBtn = new System.Windows.Forms.Button();
            this.dummy = new System.Windows.Forms.Button();
            this.debugTabCheckBtn = new System.Windows.Forms.Button();
            this.debugDisableLinesBtn = new System.Windows.Forms.CheckBox();
            this.debugMiscBtn = new System.Windows.Forms.Button();
            this.label1 = new weapon_data.Label();
            this.label2 = new weapon_data.Label();
            this.Title = new weapon_data.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
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
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
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
            // debugTabCheckBtn
            // 
            this.debugTabCheckBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.debugTabCheckBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.debugTabCheckBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 6.5F);
            this.debugTabCheckBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.debugTabCheckBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.debugTabCheckBtn.Location = new System.Drawing.Point(49, 34);
            this.debugTabCheckBtn.Name = "debugTabCheckBtn";
            this.debugTabCheckBtn.Size = new System.Drawing.Size(49, 19);
            this.debugTabCheckBtn.TabIndex = 46;
            this.debugTabCheckBtn.Text = "tabcheck";
            this.debugTabCheckBtn.UseVisualStyleBackColor = false;
            // 
            // debugDisableLinesBtn
            // 
            this.debugDisableLinesBtn.AutoSize = true;
            this.debugDisableLinesBtn.Location = new System.Drawing.Point(104, 36);
            this.debugDisableLinesBtn.Name = "debugDisableLinesBtn";
            this.debugDisableLinesBtn.Size = new System.Drawing.Size(54, 17);
            this.debugDisableLinesBtn.TabIndex = 45;
            this.debugDisableLinesBtn.Text = "noline";
            this.debugDisableLinesBtn.UseVisualStyleBackColor = true;
            // 
            // debugMiscBtn
            // 
            this.debugMiscBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.debugMiscBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.debugMiscBtn.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.debugMiscBtn.ForeColor = System.Drawing.SystemColors.WindowText;
            this.debugMiscBtn.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.debugMiscBtn.Location = new System.Drawing.Point(4, 33);
            this.debugMiscBtn.Name = "debugMiscBtn";
            this.debugMiscBtn.Size = new System.Drawing.Size(43, 21);
            this.debugMiscBtn.TabIndex = 44;
            this.debugMiscBtn.Text = "Misc.";
            this.debugMiscBtn.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.debugMiscBtn.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Cambria", 8F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label1.IsSeparatorLine = true;
            this.label1.Location = new System.Drawing.Point(2, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(411, 17);
            this.label1.StretchToFitForm = true;
            this.label1.TabIndex = 23;
            this.label1.Text = "---------------------------------------------------------------------------------" +
    "";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Cambria", 8F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.label2.IsSeparatorLine = true;
            this.label2.Location = new System.Drawing.Point(2, 151);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(411, 17);
            this.label2.StretchToFitForm = true;
            this.label2.TabIndex = 22;
            this.label2.Text = "---------------------------------------------------------------------------------" +
    "";
            // 
            // Title
            // 
            this.Title.AutoSize = true;
            this.Title.Font = new System.Drawing.Font("Gadugi", 9.25F, System.Drawing.FontStyle.Bold);
            this.Title.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(240)))), ((int)(((byte)(250)))));
            this.Title.IsSeparatorLine = false;
            this.Title.Location = new System.Drawing.Point(151, 3);
            this.Title.Name = "Title";
            this.Title.Size = new System.Drawing.Size(102, 17);
            this.Title.StretchToFitForm = false;
            this.Title.TabIndex = 0;
            this.Title.Text = "Debug Options";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button1.Location = new System.Drawing.Point(12, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(43, 21);
            this.button1.TabIndex = 47;
            this.button1.Text = "Misc.";
            this.button1.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(183)))), ((int)(((byte)(245)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Font = new System.Drawing.Font("Segoe UI Semibold", 6.5F);
            this.button2.ForeColor = System.Drawing.SystemColors.WindowText;
            this.button2.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button2.Location = new System.Drawing.Point(61, 72);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(49, 19);
            this.button2.TabIndex = 48;
            this.button2.Text = "tabcheck";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // DebugPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.ClientSize = new System.Drawing.Size(415, 179);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.debugTabCheckBtn);
            this.Controls.Add(this.debugDisableLinesBtn);
            this.Controls.Add(this.debugMiscBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dummy);
            this.Controls.Add(this.CloseBtn);
            this.Controls.Add(this.Title);
            this.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DebugPanel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        


        /// <summary>
        /// Post-InitializeComponent Configuration. <br/><br/>
        /// Create Assign Anonomous Event Handlers to Parent and Children.
        /// </summary>
        public void InitializeAdditionalEventHandlers()
        {
            var hSeparatorLines = new List<Point[]>();
            var vSeparatorLines = new List<Point[]>();


            // Set appropriate event handlers for the controls on the form as well
            foreach (Control item in Controls)
            {
                if (item.Name == "SwapBrowseModeBtn") // lazy fix to avoid the mouse down event confliciting with the button
                    continue;

                
                // Apply the seperator drawing function to any seperator lines
                if (item.GetType() == typeof(weapon_data.Label) && ((weapon_data.Label)item).IsSeparatorLine)
                {
                    if (item.Size.Width > item.Size.Height)
                    {
                        // Horizontal Lines
                        hSeparatorLines.Add(new Point[2] { 
                            new Point(((weapon_data.Label)item).StretchToFitForm ? 1 : item.Location.X, item.Location.Y + 7),
                            new Point(((weapon_data.Label)item).StretchToFitForm ? item.Parent.Width - 2 : item.Location.X + item.Width, item.Location.Y + 7)
                        });

                        Controls.Remove(item);
                    }
                    else {
                        // Vertical Lines
                        vSeparatorLines.Add(new [] {
                            new Point(item.Location.X + 3, ((weapon_data.Label)item).StretchToFitForm ? 1 : item.Location.Y),
                            new Point(item.Location.X + 3, ((weapon_data.Label)item).StretchToFitForm ? item.Parent.Height - 2 : item.Height)
                        });

                        Controls.Remove(item);
                    }
                }
                
                item.MouseDown += new MouseEventHandler((sender, e) =>
                {
                    MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                    MouseIsDown = true;
                });
                item.MouseUp   += new MouseEventHandler((sender, e) =>
                {
                    MouseIsDown = false;
                    if (OptionsPageIsOpen) {
                        Azem.BringToFront();
                    }
                });
                
                // Avoid applying MouseMove and KeyDown event handlers to text containters (to retain the ability to drag-select text)
                if (item.GetType() != typeof(weapon_data.TextBox) && item.GetType() != typeof(weapon_data.RichTextBox))
                {
                    item.MouseMove += new MouseEventHandler((sender, e) => MoveForm());
                }
                else {
                    item.KeyDown += (sender, arg) =>
                    {
                        if (arg.KeyData == Keys.Escape)
                        {
                            Focus();
                            item.FindForm().Focus();
                        }
                    };
                }
            }
            
            if (hSeparatorLines.Count > 0) {
                HSeparatorLines = hSeparatorLines.ToArray();
            }
            if (vSeparatorLines.Count > 0) {
                VSeparatorLines = vSeparatorLines.ToArray();
            }
            

            // Set Event Handlers for Form Dragging
            MouseDown += new MouseEventHandler((sender, e) =>
            {
                MouseDif = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);

                MouseIsDown = true;
            });

            MouseUp   += new MouseEventHandler((sender, e) =>
            {
                MouseIsDown = false;

                if (OptionsPageIsOpen)
                    Azem?.BringToFront();
            });

            MouseMove += new MouseEventHandler((sender, e) => MoveForm());
            
            Paint += (bingus, yoshiP) => DrawFormDecorations((Form)bingus, yoshiP);
        }
        #endregion


        
        //================================\\
        //--|   Control Declarations   |--\\
        //================================\\
        #region [Control Declarations]
        private Label Title;
        private Button CloseBtn;

        private Button dummy;
        #endregion
        private Label label2;
        private Label label1;
        private Button debugTabCheckBtn;
        private CheckBox debugDisableLinesBtn;
        private Button debugMiscBtn;
        private Button button1;
        private Button button2;
    }
}