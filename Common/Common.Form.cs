using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        //#
        //## Form Functionality Globals
        //#
        #region [Form Functionality Globals]
        /// <summary> Return the current state of the options page. </summary>
        public static bool OptionsPageIsOpen => Azem?.Visible ?? false;

#if DEBUG
        public static bool DebugPanelIsOpen => Bingus?.Visible ?? false;
#endif

        /// <summary> If true, show the string representation of the raw SID's instead of UNKNOWN_SID_64 when an id can not be decoded. </summary>
        public static bool ShowUnresolvedSIDs = true;

#if DEBUG
        /// <summary> If true, show the string representation of the raw SID's instead of INVALID_SID_64 when an invalid sid has been provided. </summary>
        public static bool ShowInvalidSIDs = true;
#endif


        /// <summary> Boolean global for keeping track of the current mouse state. </summary>
        public static bool MouseIsDown = false;

        /// <summary> Boolean global to set the type of dialogue to use for the GamedataFolder path box's browse button. </summary>
        public static bool LegacyFolderSelectionDialogue = true;

        /// <summary> Store Expected Options Form Offset. </summary>
        public static Point OptionsFormLocation;

        /// <summary> Variable for Smooth Form Dragging. </summary>
        public static Point MouseDif;

        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        private Point[][] HSeparatorLines;

        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        private Point[][] VSeparatorLines;

        /// <summary> The difference in size (horizontally, in pixels) of the Abort/Close File button when it changes from one to the other. </summary>
        private static readonly int AbortButtonWidthDifference = 20; //! Lazy

        /// <summary> The initial width (in pixels) of the Abort button. Used when switching from "abort/close file" modes. </summary>
        private static int BaseAbortButtonWidth;

        /// <summary>
        /// //! Make sure this is actually consistent across each patch version
        /// </summary>
        private static readonly byte[] EmptyDCFileHash = new byte[] { 0x1c, 0xd3, 0xe2, 0x12, 0xe6, 0xed, 0xda, 0xac, 0xd4, 0x3c, 0xac, 0x53, 0x55, 0x34, 0x19, 0x85, 0x2e, 0x3a, 0x7c, 0x1b, 0x28, 0x36, 0x15, 0xef, 0xea, 0x20, 0x74, 0x5e, 0x98, 0xe8, 0x7b, 0x95 };

        public const string emptyStr = "";






        /// <summary>
        /// The absolute path to the provided DC file.
        /// </summary>
        public static string ActiveFilePath
        {
            get => _activeFilePath;

            set {
                _activeFilePath = value;

                ActiveFileName = ActiveFilePath.Substring(ActiveFilePath.LastIndexOf('\\') + 1);
            }
        }
        private static string _activeFilePath = "No Script Selected";


        

        /// <summary>
        /// The name of the provided DC file.
        /// </summary>
        public static string ActiveFileName
        {
            get => _activeFileName;

            private set
            {
                _activeFileName = value ?? "null";

                UpdateStatusLabel(new[] { "Viewing Script " + ActiveFileName, null, null });
            }
        }
        private static string _activeFileName = "No Script Selected";





        /// <summary>
        /// //!
        /// </summary>
        private static string[] StatusDetails
        {
            get => _statusDetails;

            set
            {
                if (value == null || value.Length < 1)
                {
                    _statusDetails = Array.Empty<string>();
                    ScriptStatusLabel.Text = "Status: [Inactive]";
                    return;
                }



                if (_statusDetails != Array.Empty<string>() && value.Length <= _statusDetails.Length)
                {
                    // Update changed array members only
                    for (var i = 0; i < value.Length; i++)
                    {
                        if (value[i] != null)
                        {
                            _statusDetails[i] = value[i];
                        }
                    }
                }
                else
                {
                    _statusDetails = value;
                }



                ScriptStatusLabel.Text = $"Status: {_statusDetails[0]} ";

                for (var i = 1; i < _statusDetails.Length; i++)
                {
                    if ((StatusDetails[i]?.Length ?? 0) > 0)
                    {
                        ScriptStatusLabel.Text += " | " + _statusDetails[i];
                        Venat?.Update();
                    }
                }
            }
        }
        private static string[] _statusDetails = Array.Empty<string>();




        /// <summary>
        /// //!
        /// </summary>
        private static string[] SelectionDetails
        {
            get => _selectionDetails;

            set
            {
                if (value == null || value.Length < 1)
                {
                    _selectionDetails = Array.Empty<string>();
                    ScriptSelectionLabel.Text = "Selection: [None]";
                    return;
                }


                if ((SelectionDetails?.Length ?? 0) < 1)
                {
                    _selectionDetails = value;
                }

                else if (value.Length > _selectionDetails.Length)
                {
                    var buff = new string[value.Length];
                    Buffer.BlockCopy(SelectionDetails, 0, buff, 0, SelectionDetails.Length);

                    _selectionDetails = buff;
                }


                // Update changed array members only
                for (var i = 0; i < value.Length; i++)
                {
                    if (i < value.Length && value[i] != null)
                    {
                        _selectionDetails[i] = value[i];
                    }
                }

                ScriptSelectionLabel.Text = $"Selection: {SelectionDetails[0]} ";

                for (var i = 1; i < SelectionDetails.Length; i++)
                {
                    if ((SelectionDetails[i]?.Length ?? 0) > 0)
                    {
                        ScriptSelectionLabel.Text += " | " + SelectionDetails[i];
                        Venat?.Update();
                    }
                }
            }
        }
        private static string[] _selectionDetails = Array.Empty<string>();



        /// <summary> MainPage Form Pointer/Reference. </summary>
        public static Main Venat;

        /// <summary> OptionsPage Form Pointer/Reference. </summary>
        public static OptionsPage Azem;

        /// <summary> Debug options panel form Pointer/Reference. </summary>
        public static DebugPanel Bingus;
        

        /// <summary> StructBSIdkNameItLater Class Pointer/Reference. </summary>
        public static PropertyHandlers Panels;

        /// <summary> Properties Panel GroupBox Pointer/Reference. </summary>
        public static GroupBox PropertiesPanel;

        /// <summary> Properties Editor Pointer/Reference. </summary>
        public static GroupBox PropertiesEditor;


        /// <summary> Properties Window (the output one) Pointer/Reference Because I'm Lazy. </summary>
        public static RichTextBox PropertiesWindow;

        /// <summary> Log Window Pointer/Reference.  </summary>
        public static RichTextBox LogWindow;


        public static Label ScriptStatusLabel;

        public static Label ScriptSelectionLabel;


        /// <summary>
        /// A list of changes made to the loaded DC file, for undoing crap.
        /// <br/> <br/>
        /// 0: Address
        ///<br/>
        /// 1: Original Data
        /// </summary>
        public static List<object[]> Changes;


        public delegate void SubformExitFunction(object _, EventArgs __);
        #endregion
        #endregion







        
        //===================================\\
        //---|   Function Delcarations   |---\\
        //===================================\\
        #region [Function Delcarations]

        //#
        //## Form Functionality Functions
        //#
        #region [Form Functionality Functions]

        /// <summary>
        /// Create and subscribe to various event handlers for additional form functionality. (fck your properties panel's event handler window, let me write code)
        /// </summary>
        public static void InitializeAdditionalEventHandlers(Form parent, Button CloseBtn, SubformExitFunction ExitFunction, ref Point[][] HSeparatorLines, ref Point[][] VSeparatorLines)
        {
            var controls = parent.Controls.Cast<Control>().ToArray();

            var hSeparatorLineScanner = new List<Point[]>();
            var vSeparatorLineScanner = new List<Point[]>();


            // Apply the seperator drawing function to any seperator lines
            foreach (var line in controls.OfType<NaughtyDogDCReader.Label>())
            {
                if (line.IsSeparatorLine)
                {
                    // Horizontal Lines
                    hSeparatorLineScanner.Add(new Point[2] {
                        new Point(line.StretchToFitForm ? 1 : line.Location.X, line.Location.Y + 7),
                        new Point(line.StretchToFitForm ? line.Parent.Width - 2 : line.Location.X + line.Width, line.Location.Y + 7)
                    });

                    parent.Controls.Remove(line);
                }
            }

            if (hSeparatorLineScanner.Count > 0)
            {
                HSeparatorLines = hSeparatorLineScanner.ToArray();
            }/*
            if (vSeparatorLineScanner.Count > 0)
            {
                VSeparatorLines = vSeparatorLineScanner.ToArray();
            }*/


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

        public static void MouseDownFunc(object sender = null, EventArgs e = null)
        {
            if (Venat != null)
            {
                MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                MouseIsDown = true;
            }
        }

        public static void MouseUpFunc(object sender = null, EventArgs e = null)
        {
            MouseIsDown = false;

            if (OptionsPageIsOpen)
            {
                Azem?.BringToFront();
            }
        }



        /// <summary>
        /// Draw a thin border over the for edges on repaint.
        /// <br/>Draw a thin line from one end of the painted control to the other.
        ///</summary>
        public static void DrawFormDecorations(Form venat, PaintEventArgs yoshiP)
        {
#if DEBUG
            if (noDraw)
            {
                return;
            }
#endif

            yoshiP.Graphics.Clear(venat.BackColor); // Clear line bounds with the current form's background colour


            //## Draw Vertical Lines
            foreach (var line in (venat as dynamic).VSeparatorLines ?? Array.Empty<Point[]>())
            {
                yoshiP?.Graphics.DrawLine(FormDecorationPen, line[0], line[1]);
            }

            //## Draw Horizontal Lines
            foreach (var line in (venat as dynamic).HSeparatorLines ?? Array.Empty<Point[]>())
            {
                yoshiP?.Graphics.DrawLine(FormDecorationPen, line[0], line[1]);
            }

            // Draw a thin (1 pixel) border around the form with the current Pen
            yoshiP?.Graphics.DrawLines(FormDecorationPen, new[]
            {
                Point.Empty,
                new Point(venat.Width-1, 0),
                new Point(venat.Width-1, venat.Height-1),
                new Point(0, venat.Height-1),
                Point.Empty
            });
        }



        /// <summary>
        /// Post-InitializeComponent Configuration. <br/><br/>
        /// Create Assign Anonymous Event Handlers to Parent and Children.
        /// </summary>
        public void InitializeAdditionalEventHandlers_Main(Main Venat)
        {
            var controls = Venat.Controls.Cast<Control>().ToArray();

            var hSeparatorLineScanner = new List<Point[]>();
            var vSeparatorLineScanner = new List<Point[]>();



            // Apply the separator drawing function to any separator lines
            foreach (var line in controls.OfType<NaughtyDogDCReader.Label>())
            {
                if (line.IsSeparatorLine)
                {
                    if (line.Size.Width > line.Size.Height)
                    {
                        // Horizontal Lines
                        hSeparatorLineScanner.Add(new Point[2] {
                            new Point(((NaughtyDogDCReader.Label) line).StretchToFitForm ? 1 : line.Location.X, line.Location.Y + 7),
                            new Point(((NaughtyDogDCReader.Label) line).StretchToFitForm ? line.Parent.Width - 2 : line.Location.X + line.Width, line.Location.Y + 7)
                        });

                        Controls.Remove(line);
                    }
                    else {
                        // Vertical Lines (the + 3 is to center the line with the displayed lines in the editor)
                        vSeparatorLineScanner.Add(new Point[2] {
                            new Point(line.Location.X + 3, ((NaughtyDogDCReader.Label) line).StretchToFitForm ? 1 : line.Location.Y),
                            new Point(line.Location.X + 3, ((NaughtyDogDCReader.Label) line).StretchToFitForm ? line.Parent.Height - 2 : line.Location.Y + line.Height)
                        });

                        Controls.Remove(line);
                    }
                }
            }

            if (hSeparatorLineScanner.Count > 0)
            {
                HSeparatorLines = hSeparatorLineScanner.ToArray();
            }
            if (vSeparatorLineScanner.Count > 0)
            {
                VSeparatorLines = vSeparatorLineScanner.ToArray();
            }




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
        #endregion
        #endregion
    }
}
