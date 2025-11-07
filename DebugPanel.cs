using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;

namespace NaughtyDogDCReader
{
    public partial class DebugPanel : Form
    {
        public DebugPanel()
        {
            InitializeComponent();

            var hSeparatorLines = new List<Point[]>();
            var vSeparatorLines = new List<Point[]>();

            var that = this;

            // Set appropriate event handlers for the controls on the form as well
            foreach (Control item in that.Controls.OfType<NaughtyDogDCReader.Label>())
            {
                if (item.Name == "SwapBrowseModeBtn") // lazy fix to avoid the mouse down event confliciting with the button
                    continue;

                
                // Apply the seperator drawing function to any seperator lines
                if (item.GetType() == typeof(NaughtyDogDCReader.Label) && ((NaughtyDogDCReader.Label)item).IsSeparatorLine)
                {
                    if (item.Size.Width > item.Size.Height)
                    {
                        // Horizontal Lines
                        hSeparatorLines.Add(new Point[2] { 
                            new Point(((NaughtyDogDCReader.Label)item).StretchToFitForm ? 1 : item.Location.X, item.Location.Y + 7),
                            new Point(((NaughtyDogDCReader.Label)item).StretchToFitForm ? item.Parent.Width - 2 : item.Location.X + item.Width, item.Location.Y + 7)
                        });

                        Controls.Remove(item);
                    }
                    else {
                        // Vertical Lines
                        vSeparatorLines.Add(new [] {
                            new Point(item.Location.X + 3, ((NaughtyDogDCReader.Label)item).StretchToFitForm ? 1 : item.Location.Y),
                            new Point(item.Location.X + 3, ((NaughtyDogDCReader.Label)item).StretchToFitForm ? item.Parent.Height - 2 : item.Height)
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
                if (item.GetType() != typeof(NaughtyDogDCReader.TextBox) && item.GetType() != typeof(NaughtyDogDCReader.RichTextBox))
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
            
            
            CloseBtn.Click += (a, b) =>
            {
                Bingus.Visible = false;
                Venat?.Update();
            };

            showBasicPropertiesWindow.Checked = Venat.propertiesWindow.Visible;
        }


        //#
        //## Debug Variable Declarations
        //#
        #region [debug variable declarations]

        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        public Point[][] HSeparatorLines;
        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        public Point[][] VSeparatorLines;
        #endregion



        //#
        //## Debug Function Declarations
        //#

        private void debugMiscBtn_Click(object sender, EventArgs e)
        {
            var ffs = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            foreach (var item in ffs)
            {
                echo($"[{item}]: {(byte) item} => {(byte) item.ToString().ToUpper()[0]}");
            }

            for (byte beh = 90; beh < 98; beh++)
            {
                echo($"{(char) beh}");
            }
            echo((char) 123);
        }

        private void debugDisableLinesBtn_CheckedChanged(object sender, EventArgs e)
        {
#if DEBUG
            noDraw ^= true;
            CreateGraphics().Clear(BackColor);
            Refresh();
#endif
        }


        // verify tab index adjustment is working as expected
        private void debugTabCheckBtn_Click(object sender, EventArgs e)
        {
            void eh(System.Windows.Forms.Control.ControlCollection controls)
            {
                foreach (Control cunt in controls)
                {
                    if (cunt.HasChildren)
                    {
                        eh(cunt.Controls);
                    }
                    echo($"# [{cunt.Name}: {cunt.TabIndex}]");
                }

            }

            eh(this.Controls);
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void debugShowInvalidSIDsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
#if DEBUG
            ShowInvalidSIDs = ((CheckBox) sender).Checked;
#endif
        }

        private void showBasicPropertiesWindow_CheckedChanged(object sender, EventArgs e)
        {
            var @checked = ((CheckBox)sender).Checked;

            Venat.propertiesWindow.Visible = @checked;
            Venat.propertiesEditor.Visible = !@checked;
        }
    }
}
