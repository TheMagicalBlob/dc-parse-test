﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
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

        private static readonly byte[] EmptyDCFileHash = new byte[] { 0x1c, 0xd3, 0xe2, 0x12, 0xe6, 0xed, 0xda, 0xac, 0xd4, 0x3c, 0xac, 0x53, 0x55, 0x34, 0x19, 0x85, 0x2e, 0x3a, 0x7c, 0x1b, 0x28, 0x36, 0x15, 0xef, 0xea, 0x20, 0x74, 0x5e, 0x98, 0xe8, 0x7b, 0x95 };

        public const string emptyStr = "";




        /// <summary>
        /// The name of the provided DC file.
        /// </summary>
        public static string ActiveFileName
        {
            get => _activeFileName;

            private set
            {
                _activeFileName = value ?? "null";

                UpdateSelectionLabel(new[] { ActiveFileName, null, null });
            }
        }
        private static string _activeFileName = "No Script Selected";




        /// <summary>
        /// The absolute path to the provided DC file.
        /// </summary>
        public static string ActiveFilePath = "No Script Selected";




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



        /// <summary> MainPage Form Pointer/Refference. </summary>
        public static Main Venat;

        /// <summary> OptionsPage Form Pointer/Refference. </summary>
        public static OptionsPage Azem;
        
        /// <summary> Properties Window (the output one) Pointer/Refference Because I'm Lazy. </summary>
        public static RichTextBox PropertiesWindow;

        /// <summary> Properties Editor Pointer/Refference. </summary>
        public static GroupBox PropertiesEditor;

        /// <summary> Properties Panel GroupBox Pointer/Refference. </summary>
        public static GroupBox PropertiesPanel;

        /// <summary> StructBSIdkNameItLater Class Pointer/Refference. </summary>
        public static PropertiesHandler Panels;

        /// <summary> Log Window Pointer/Refference.  </summary>
        public static RichTextBox LogWindow;

        /// <summary> Debug options panel form Pointer/Refference. </summary>
        public static DebugPanel Bingus;

        public static Label ScriptStatusLabel;
        public static Label ScriptSelectionLabel;

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
        /// Create Assign Anonomous Event Handlers to Parent and Children.
        /// </summary>
        public void InitializeAdditionalEventHandlers_Main(Main Venat)
        {
            var controls = Venat.Controls.Cast<Control>().ToArray();

            var hSeparatorLineScanner = new List<Point[]>();
            var vSeparatorLineScanner = new List<Point[]>();



            // Apply the seperator drawing function to any seperator lines
            foreach (var line in controls.OfType<NaughtyDogDCReader.Label>())
            {
                if (line.IsSeparatorLine)
                {
                    if (line.Size.Width > line.Size.Height)
                    {
                        // Horizontal Lines
                        hSeparatorLineScanner.Add(new Point[2] {
                            new Point(((NaughtyDogDCReader.Label)line).StretchToFitForm ? 1 : line.Location.X, line.Location.Y + 7),
                            new Point(((NaughtyDogDCReader.Label)line).StretchToFitForm ? line.Parent.Width - 2 : line.Location.X + line.Width, line.Location.Y + 7)
                        });

                        Controls.Remove(line);
                    }
                    else
                    {
                        // Vertical Lines (the + 3 is to center the line with the displayed lines in the editor)
                        vSeparatorLineScanner.Add(new[] {
                            new Point(line.Location.X + 3, ((NaughtyDogDCReader.Label)line).StretchToFitForm ? 1 : line.Location.Y),
                            new Point(line.Location.X + 3, ((NaughtyDogDCReader.Label)line).StretchToFitForm ? line.Parent.Height - 2 : line.Location.Y + line.Height)
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
                if (item.Name == "SwapBrowseModeBtn") // lazy fix to avoid the mouse down event confliciting with the button
                {
                    continue;
                }

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


                // Avoid applying MouseMove and KeyDown event handlers to text containters (to retain the ability to drag-select text)
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
        










        //#
        //## Threading-Related Variables (threads, delegates, and mammets)
        //#
        private static Thread binThread;

        /// <summary> Cross-thread form interaction delegate. </summary>
        public delegate void binThreadFormWand(bool args); //! god I need to read about delegates lmao
        /// <summary> //! </summary>
        private delegate void binThreadLabelWand(string[] details);
        private delegate void generalBinThreadWand();
        /// <summary> //! </summary>
        public delegate string[] binThreadFormWandOutputRead();




        private readonly binThreadLabelWand statusLabelMammet = new binThreadLabelWand((details) =>
        {
            StatusDetails = details;
        });

        private readonly generalBinThreadWand statusLabelResetMammet = new generalBinThreadWand(() =>
        {
            StatusDetails = null;
        });


        private readonly binThreadLabelWand selectionLabelMammet = new binThreadLabelWand((details) =>
        {
            SelectionDetails = details;
        });

        private readonly generalBinThreadWand selectionLabelResetMammet = new generalBinThreadWand(() =>
        {
            SelectionDetails = null;
        });




        private readonly binThreadFormWand reloadCloseButtonsMammet = new binThreadFormWand((isEnabled) =>
        {
            if (Venat.CloseBtn == null)
            {
                echo($"ERROR: {nameof(Venat.CloseBtn)} was null!");
                return;
            }

            // Enable/Disable the button, and update the button with the strikeout style property
            Venat.CloseBtn.Enabled = isEnabled;
            Venat.CloseBtn.Font = new Font(MainFont.FontFamily, MainFont.Size, MainFont.Style | (isEnabled ? FontStyle.Regular : FontStyle.Strikeout));


            if (Venat.ReloadScriptBtn == null)
            {
                echo($"ERROR: {nameof(Venat.ReloadScriptBtn)} was null!");
                return;
            }

            Venat.ReloadScriptBtn.Enabled = isEnabled;
            Venat.ReloadScriptBtn.Font = new Font(MainFont.FontFamily, MainFont.Size, MainFont.Style | (isEnabled ? FontStyle.Regular : FontStyle.Strikeout));
        });


        
        private readonly generalBinThreadWand CloseBinFileMammet = new generalBinThreadWand(() =>
        {
            CloseBinFile();
        });










        //#
        //## Mammet Shorthand Functions
        //#
        #region [Mammet Shorthand Functions]

        public static void ReloadCloseButtonsMammet(bool enabled)
        {
            Venat?.Invoke(Venat.reloadCloseButtonsMammet, new object[] { enabled });
        }

        public static void PropertiesPanelMammet(object dcFileName, DCFileHeader dcEntries)
        {
            Venat?.Invoke(Panels.propertiesPanelMammet, new object[] { dcFileName, dcEntries });
        }


        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="details">
        /// A string[3] containing the details for the status label.
        /// <br/> 
        /// </param>
        public static void StatusLabelMammet(string[] details)
        {
            Venat?.Invoke(Venat.statusLabelMammet, new object[] { details });
        }


        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="details">
        /// A string[3] containing the details for the slection label.
        /// <br/> 
        public static void SelectionLabelMammet(string[] details)
        {
            Venat?.Invoke(Venat.selectionLabelMammet, new[] { details });
        }
        #endregion [mammet shorthand functions]
        #endregion
    }
}
