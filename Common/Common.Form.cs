using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //======================================================\\
        //---|   Form Functionality Function Delcarations   |---\\
        //======================================================\\
        #region [Form Functionality Function Delcarations]
        
        /// <summary>
        /// Post-InitializeComponent Configuration. <br/><br/>
        /// Create Assign Anonymous Event Handlers to Parent and Children.
        /// </summary>
        public void InitializeAdditionalEventHandlers(Main Venat)
        {
            var controls = Venat.Controls.Cast<Control>().ToArray();


            // Setup variables used for decorations like the SeparatorLines and border
            InitializeFormDecorations(Venat, controls);


            // Set appropriate event handlers for the controls on the form as well
            foreach (var item in controls)
            {
                item.KeyDown += (sender, arg) => FormKeyboardInputHandler(((Control) sender).Name, arg.KeyData, arg.Control, arg.Shift);

                item.MouseDown += new MouseEventHandler((sender, e) =>
                {
                    MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                    MouseIsDown = true;
                });
                item.MouseUp += new MouseEventHandler((sender, e) =>
                {
                    MouseIsDown = false;
                    if (OptionsPageIsOpen)
                    {
                        Azem.BringToFront();
                    }
                });



                // Avoid applying MouseMove and KeyDown event handlers to text containers (to retain the ability to drag-select text)
                if (item.GetType() == typeof(NaughtyDogDCReader.TextBox) || item.GetType() == typeof(NaughtyDogDCReader.RichTextBox))
                {
                    item.KeyDown += (sender, arg) =>
                    {
                        if (arg.KeyData == Keys.Escape)
                        {
                            BinFileBrowseBtn.Focus();
                            Focus();
                        }
                    };
                }
                // Add the event handler to everything that's not a text container
                else
                {
                    item.MouseMove += new MouseEventHandler((sender, e) => MoveForm());
                }
            }





            MinimizeBtn.Click += new EventHandler((sender, e) => Venat.WindowState = FormWindowState.Minimized);
            MinimizeBtn.MouseEnter += new EventHandler((sender, e) => ((Control) sender).ForeColor = Color.FromArgb(90, 100, 255));
            MinimizeBtn.MouseLeave += new EventHandler((sender, e) => ((Control) sender).ForeColor = Color.FromArgb(0, 0, 0));
            ExitBtn.Click += new EventHandler((sender, e) => Environment.Exit(0));
            ExitBtn.MouseEnter += new EventHandler((sender, e) => ((Control) sender).ForeColor = Color.FromArgb(230, 100, 100));
            ExitBtn.MouseLeave += new EventHandler((sender, e) => ((Control) sender).ForeColor = Color.FromArgb(0, 0, 0));


            // Set Event Handlers for Form Dragging
            MouseDown += new MouseEventHandler((sender, e) =>
            {
                MouseDif = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);

                MouseIsDown = true;
            });

            MouseUp += new MouseEventHandler((sender, e) =>
            {
                MouseIsDown = false;

                if (OptionsPageIsOpen)
                {
                    Azem?.BringToFront();
                }
            });

            MouseMove += new MouseEventHandler((sender, e) => MoveForm());

            KeyDown += (sender, arg) => FormKeyboardInputHandler(((Control) sender).Name, arg.KeyData, arg.Control, arg.Shift);

            Paint += (venat, yoshiP) => DrawFormDecorations((Form) venat, yoshiP);
        }






        /// <summary>
        /// Create and subscribe to various event handlers for additional form functionality.
        /// </summary>
        public void InitializeAdditionalEventHandlersForSubform(Form parent, Button CloseBtn, SubformExitFunction ExitFunction, ref Point[][] HSeparatorLines, ref Point[][] VSeparatorLines)
        {
            var controls = parent.Controls.Cast<Control>().ToArray();

            InitializeFormDecorations(parent, controls);

            //var hSeparatorLineScanner = new List<Point[]>();
            //var vSeparatorLineScanner = new List<Point[]>();


            //// Apply the seperator drawing function to any seperator lines
            //foreach (var line in controls.OfType<NaughtyDogDCReader.Label>())
            //{
            //    if (line.IsSeparatorLine)
            //    {
            //        // Horizontal Lines
            //        hSeparatorLineScanner.Add(new Point[2] {
            //            new Point(line.StretchToFitForm ? 1 : line.Location.X, line.Location.Y + 7),
            //            new Point(line.StretchToFitForm ? line.Parent.Width - 2 : line.Location.X + line.Width, line.Location.Y + 7)
            //        });

            //        parent.Controls.Remove(line);
            //    }
            //}

            //if (hSeparatorLineScanner.Count > 0)
            //{
            //    HSeparatorLines = hSeparatorLineScanner.ToArray();
            //}
            //if (vSeparatorLineScanner.Count > 0)
            //{
            //    VSeparatorLines = vSeparatorLineScanner.ToArray();
            //}


            parent.Paint += (venat, yoshiP) => DrawFormDecorations((Form) venat, yoshiP);





            // Set CloseBtn event handler to provided delagate
            CloseBtn.Click += new EventHandler(ExitFunction);


            // Set Event Handlers for Form Dragging
            parent.MouseDown += new MouseEventHandler((sender, e) =>
            {
                MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                MouseIsDown = true;

                //Venat.DropdownMenu[1].Visible = Venat.DropdownMenu[0].Visible = false;

            });
            parent.MouseUp += new MouseEventHandler((sender, e) =>
                MouseIsDown = false
            );
            parent.MouseMove += new MouseEventHandler((sender, e) => MoveForm());


            foreach (var item in controls)
            {
                item.MouseDown += new MouseEventHandler((sender, e) =>
                {
                    MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                    MouseIsDown = true;
                });

                item.MouseUp += new MouseEventHandler((sender, e) =>

                    MouseIsDown = false
                );

                // Avoid Applying MoveForm EventHandler to Text Containters (to retain the ability to drag-select text)
                if (item.GetType() != typeof(TextBox) && item.GetType() != typeof(RichTextBox))
                {
                    item.MouseMove += new MouseEventHandler((sender, e) => MoveForm());
                }
            }
        }


        /// <summary>
        /// Testing random input crap
        /// </summary>
        private void FormKeyboardInputHandler(string sender, Keys arg, bool ctrl, bool shift)
        {
            echo($"Input [{arg}] Received by Control [{sender}]");

            if (arg == Keys.Back)
            {
                ReturnToParent();
            }
        }




        /// <summary>
        /// Handle Form Dragging for Borderless Form.
        /// </summary>
        public static void MoveForm()
        {
            if (MouseIsDown && Venat != null)
            {
                Venat.Location = new Point(MousePosition.X - MouseDif.X, MousePosition.Y - MouseDif.Y);

                if (Azem != null)
                {
                    Azem.Location = new Point(MousePosition.X - MouseDif.X + ((Venat.Size.Width - Azem.Size.Width) / 2), Venat.Location.Y + 50);
                }

#if DEBUG
                else if (Bingus != null)
                {
                    Bingus.Location = new Point(MousePosition.X - MouseDif.X + ((Venat.Size.Width - Bingus.Size.Width) / 2), Venat.Location.Y + 50);
                }
#endif

                Venat.Update();
                Azem?.Update();
            }
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void MouseDownFunc(object sender = null, EventArgs e = null)
        {
            if (Venat != null)
            {
                MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                MouseIsDown = true;
            }
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void MouseUpFunc(object sender = null, EventArgs e = null)
        {
            MouseIsDown = false;

            if (OptionsPageIsOpen)
            {
                Azem?.BringToFront();
            }
        }
        #endregion
    }
}
