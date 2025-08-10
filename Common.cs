using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace weapon_data
{
    public partial class Main
    {

        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]


        //#
        //## Script Parsing Globals
        //#

        /// <summary>
        /// An array of bytes containing the entire provided sidbase.bin file.<br/>
        /// Used for decoding any read SID's that haven't already been decoded and chached.
        /// </summary>
        public static byte[] SIDBase
        {
            get => _sidbase;

            set {
                _sidbase = value;

                if (value.Length > 0x19)
                {
                    SIDBaseTableLength = BitConverter.ToInt64(SIDBase, 0) * 16;
                }
                else
                {
                    //! Implement an error, since the file would obviously be corrupted.
#if DEBUG
                    echo("ERROR: Invalid length for sidbase.bin (< 0x19- is it corrupted?)");
#else
                    MessageBox.Show("ERROR: Invalid length for sidbase.bin (< 0x19- is it corrupted?)", "The provided sidbase was unable to be loaded.");
#endif
                }
            }
        }
        private static byte[] _sidbase;

        /// <summary>
        /// The length (in bytes) of the sidbase.bin's lookup table (relative to the first entry), read from the first 8 bytes in the file.
        /// </summary>
        public static long SIDBaseTableLength;

        

        /// <summary>
        /// An array of bytes containing the entire provided DC .bin file. <br/>
        /// </summary>
        public static byte[] DCFile
        {
            get => _dcFile;

            set {
                _dcFile = value == Array.Empty<byte>() ? null : value; // array.empty for intentional resets of the array, just avoid oops

                if (value?.Length > 0x2C)
                {
                    DCFileMainDataLength = BitConverter.ToInt64(DCFile, 8);
                }
                else if ((value ?? null) != Array.Empty<byte>()) {
                    //! Implement an error, since the file would obviously be corrupted.
                }
            }
        }
        private static byte[] _dcFile;

        /// <summary>
        /// The address of the provided DC file's relocation table
        /// </summary>
        public static long DCFileMainDataLength;


        
        public static DCFileHeader DCScript;

        public List<SID> DecodedSIDs = new List<SID>();

        public static bool Abort
        {
            set {
                if (value && binThread != null)
                {
                    echo ("Aborting...");

                    binThread.Abort();
                    AbortButtonMammet(false);
                }
            }
        }





        
        //#
        //## Form Functionality Globals
        //#
        /// <summary> Return the current state of the options page. </summary>
        public static bool OptionsPageIsOpen { get => Azem?.Visible ?? false; }

        #if DEBUG
        public static bool DebugPanelIsOpen { get => Bingus?.Visible ?? false; }
        #endif

        /// <summary> If true, show the string representation of the raw SID's instead of UNKNOWN_SID_64 when an id can not be decoded. </summary>
        public static bool ShowUnresolvedSIDs = true;

        # if DEBUG
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



        /// <summary> The name of the provided DC file. </summary>
        public static string ActiveFileName
        {
            get => _activeFileName;

            private set {
                _activeFileName = value ?? "null";

                UpdateSelectionLabel(new[] { ActiveFileName, null, null });
            }
        }
        private static string _activeFileName = "No Script Selected";


        /// <summary> The absolute path to the provided DC file. </summary>
        public static string ActiveFilePath
        {
            get => _activeFilePath;

            set {
                _activeFilePath = value ?? "null";

                if (System.IO.File.Exists(ActiveFilePath))
                {
                    Venat?.LoadBinFile(ActiveFilePath);

                    ActiveFileName = value.Substring(value.LastIndexOf('\\') + 1);
                }
            }
        }
        private static string _activeFilePath = "No Script Selected";


        /// <summary> //! </summary>
        private static string[] StatusDetails
        {
            get => _statusDetails;

            set {
                if (value == null || value.Length < 1)
                {
                    _statusDetails = Array.Empty<string>();
                    ScriptStatusLabel.Text = "Status: [None]";
                    return;
                }



                // Update changed array members only
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] != null)
                    {
                        _statusDetails[i] = value[i];
                    }
                }

                

                ScriptStatusLabel.Text = $"Status: {StatusDetails[0]} ";
            
                for (int i = 1; i < StatusDetails.Length; i++)
                {
                    if ((StatusDetails[i]?.Length ?? 0) > 0)
                    {
                        ScriptStatusLabel.Text += " | " + StatusDetails[i];
                        Venat?.Update();
                    }
                }
            }
        }
        private static string[] _statusDetails = new string [] { null, null, null };

        
        /// <summary> //! </summary>
        private static string[] SelectionDetails
        {
            get => _selectionDetails;

            set {
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
                for (int i = 0; i < value.Length; i++)
                {
                    if (i < value.Length && value[i] != null)
                    {
                        _selectionDetails[i] = value[i];
                    }
                }

                ScriptSelectionLabel.Text = $"Selection: {SelectionDetails[0]} ";
            
                for (int i = 1; i < SelectionDetails.Length; i++)
                {
                    if ((SelectionDetails[i]?.Length ?? 0) > 0)
                    {
                        ScriptSelectionLabel.Text += " | " + SelectionDetails[i];
                        Venat?.Update();
                    }
                }
            }
        }
        private static string[] _selectionDetails = new string [] { null, null, null };
        


        /// <summary> MainPage Form Pointer/Refference. </summary>
        public static Main Venat;

        /// <summary> OptionsPage Form Pointer/Refference. </summary>
        public static OptionsPage Azem;

        /// <summary> Properties Panel GroupBox Pointer/Refference. </summary>
        public static GroupBox PropertiesPanel;
        
        /// <summary> Output Window Pointer/Ref Because I'm Lazy. </summary>
        public static RichTextBox PropertiesWindow;

        /// <summary> Log Window Pointer/Ref. </summary>
        public static RichTextBox LogWindow;

        public static DebugPanel Bingus;
        
        public static Label ScriptStatusLabel;
        public static Label ScriptSelectionLabel;
        public static Button AbortOrCloseBtn;
        


        /// <summary> A collection of known id's used in hardcoded checks, in order to handle basic operation when missing an sidbase.bin file. </summary>
        public enum KnownSIDs : ulong
        {
            UNKNOWN_SID_64 =       0x910ADC74DA2A5F6D,
            array =                0x4F9E14B634C6B026,
            symbol_array =         0xDFD21E68AC12C54B,
            ammo_to_weapon_array = 0xEF3BE7EF6F790D34,
            weapon_gameplay_defs = 0x8B099027E05B4597,
            map =                  0x080F5919176D2D91,

            melee_weapon_gameplay_def = 0x730ADC6EDAF0A96D,


            placeholder =          0xDEADBEEFDADDEAD2,
        }
        





        //#
        //## Threading-Related Variables (threads, delegates, and mammets)
        //#
        private static Thread binThread;

        /// <summary> Cross-thread form interaction delegate. </summary>
        public delegate void binThreadFormWand(params object[] args); //! god I need to read about delegates lmao
        /// <summary> //! </summary>
        private delegate void binThreadLabelWand(string[] details);
        /// <summary> //! </summary>
        public delegate void binThreadOutputWand(string msg, int line);
        /// <summary> //! </summary>
        public delegate string[] binThreadFormWandOutputRead();
        


        public binThreadOutputWand propertiesWindowNewLineMammet = new binThreadOutputWand((args, _) =>
        {
            PropertiesWindow.AppendLine(args, false);
            PropertiesWindow.Update();
        });

        public binThreadOutputWand propertiesWindowSpecificLineMammet = new binThreadOutputWand((msg, line) =>
        {
            PropertiesWindow.UpdateLine(msg, line, false);
            PropertiesWindow.Update();
        });

        

        private binThreadLabelWand statusLabelMammet = new binThreadLabelWand((details) =>
        {
            StatusDetails = details;
        });

        private binThreadLabelWand selectionLabelMammet = new binThreadLabelWand((details) =>
        {
            SelectionDetails = details;
        });



        private binThreadFormWand abortButtonMammet  = new binThreadFormWand((args) =>
        {
            if (args == null || args.Length < 1 || AbortOrCloseBtn == null)
            {
                return;
            }

            foreach (object obj in args)
            {
                if (obj == null || obj.GetType() == typeof(int))
                {
                    var newIndex = (int) (obj ?? (AbortOrCloseBtn.Text == "Abort" ? 1 : 0));

                    if (newIndex > 1) { //! make sure this check is unnecessary, then remove it
                        echo($"ERROR: abortButtonMammet was provided an invalid index of \"{newIndex}\" for the button mode. Button has been defaulted to Abort.");
                        newIndex = 0;
                    }

                    AbortOrCloseBtn.Text = new[] { "Abort", "Close File" } [newIndex];
                
                    AbortOrCloseBtn.Size = new Size(BaseAbortButtonWidth + AbortButtonWidthDifference * newIndex, AbortOrCloseBtn.Size.Height);

                    AbortOrCloseBtn.Location = new Point(AbortOrCloseBtn.Location.X + AbortButtonWidthDifference * (-2 * newIndex + 1), AbortOrCloseBtn.Location.Y);
                    return;
                }
                else if (obj.GetType() == typeof(bool))
                {
                    AbortOrCloseBtn.Enabled = (bool)obj;
                    AbortOrCloseBtn.Font = new Font(AbortOrCloseBtn.Font.FontFamily, AbortOrCloseBtn.Font.Size, (FontStyle)(((bool) obj ? 0 : 8)));
                }
                else
                    echo($"ERROR: Unexpected argument type of \"{obj.GetType()}\" provided to abortButtonMammet");
                }
        });

        public binThreadFormWand reloadButtonMammet = new binThreadFormWand((Hmmm) =>
        {
            if (Venat == null) {
                return;
            }

            Venat.ReloadScriptBtn.Enabled ^= true;
            Venat.ReloadScriptBtn.Font = new Font(Venat.ReloadScriptBtn.Font.FontFamily, Venat.ReloadScriptBtn.Font.Size, Venat.ReloadScriptBtn.Font.Style ^ FontStyle.Strikeout);
        });






        //#
        //## Global Look/Feel-Related Variables
        //#

        public static Color AppColour = Color.FromArgb(125, 183, 245);
        public static Color AppColourLight = Color.FromArgb(210, 240, 250);

        public static Pen pen = new Pen(AppColourLight); // Colouring for Border Drawing

        public static readonly Font MainFont        = new Font("Gadugi", 8.25f, FontStyle.Bold); // For the vast majority of controls; anything the user doesn't edit, really.
        public static readonly Font TextFont        = new Font("Segoe UI Semibold", 9f); // For option controls with customized contents
        public static readonly Font DefaultTextFont = new Font("Segoe UI Semibold", 9f, FontStyle.Italic); // For option controls in default states

        /// <summary> Disable drawing of form border/separator lines </summary>
        #if DEBUG
        public static bool noDraw;
        #endif
        #endregion

        

        



        //==========================================\\
        //---|   Global Function Delcarations   |---\\
        //==========================================\\
        #region [Global Function Delcarations]

        //#
        //## Form Functionality Functions
        //#
        #region [Form Functionality Functions]

        /// <summary>
        /// Handle Form Dragging for Borderless Form.
        /// </summary>
        public static void MoveForm()
        {
            if(MouseIsDown && Venat != null)
            {
                Venat.Location = new Point(MousePosition.X - MouseDif.X, MousePosition.Y - MouseDif.Y);
                if (Azem != null)
                    Azem.Location = new Point(MousePosition.X - MouseDif.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 40);
                
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
                Azem?.BringToFront();
        }


        
        /// <summary>
        /// Draw a thin border over the for edges on repaint.
        /// <br/>Draw a thin line from one end of the painted control to the other.
        ///</summary>
        public static void DrawFormDecorations(Form venat, PaintEventArgs yoshiP)
        {
            # if DEBUG
            if (noDraw)
            {
                return;
            }
            #endif

            yoshiP.Graphics.Clear(venat.BackColor); // Clear line bounds with the current form's background colour

            
            //## Draw Vertical Lines
            foreach (var line in (venat as dynamic).VSeparatorLines ?? Array.Empty<Point[]>())
            {
                yoshiP?.Graphics.DrawLine(pen, line[0], line[1]);
            }

            //## Draw Horizontal Lines
            foreach (var line in (venat as dynamic).HSeparatorLines ?? Array.Empty<Point[]>())
            {
                yoshiP?.Graphics.DrawLine(pen, line[0], line[1]);
            }

            // Draw a thin (1 pixel) border around the form with the current Pen
            yoshiP?.Graphics.DrawLines(pen, new [] {
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
            foreach (var line in controls.OfType<weapon_data.Label>())
            {
                if (line.IsSeparatorLine)
                {
                    if (line.Size.Width > line.Size.Height)
                    {
                        // Horizontal Lines
                        hSeparatorLineScanner.Add(new Point[2] { 
                            new Point(((weapon_data.Label)line).StretchToFitForm ? 1 : line.Location.X, line.Location.Y + 7),
                            new Point(((weapon_data.Label)line).StretchToFitForm ? line.Parent.Width - 2 : line.Location.X + line.Width, line.Location.Y + 7)
                        });

                        Controls.Remove(line);
                    }
                    else {
                        // Vertical Lines (the + 3 is to center the line with the displayed lines in the editor)
                        vSeparatorLineScanner.Add(new [] {
                            new Point(line.Location.X + 3, ((weapon_data.Label)line).StretchToFitForm ? 1 : line.Location.Y),
                            new Point(line.Location.X + 3, ((weapon_data.Label)line).StretchToFitForm ? line.Parent.Height - 2 : line.Location.Y + line.Height)
                        });

                        Controls.Remove(line);
                    }
                }
            }

            if (hSeparatorLineScanner.Count > 0) {
                HSeparatorLines = hSeparatorLineScanner.ToArray();
            }
            if (vSeparatorLineScanner.Count > 0) {
                VSeparatorLines = vSeparatorLineScanner.ToArray();
            }


            
            
            // Set appropriate event handlers for the controls on the form as well
            foreach (var item in controls)
            {
                if (item.Name == "SwapBrowseModeBtn") // lazy fix to avoid the mouse down event confliciting with the button
                    continue;

     
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
                if (item.GetType() == typeof(weapon_data.TextBox) || item.GetType() == typeof(weapon_data.RichTextBox))
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
                else {
                    item.MouseMove += new MouseEventHandler((sender, e) => MoveForm());
                }
            }




            MinimizeBtn.Click      += new EventHandler((sender, e) => Venat.WindowState           = FormWindowState.Minimized     );
            MinimizeBtn.MouseEnter += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(90, 100, 255  ));
            MinimizeBtn.MouseLeave += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(0 , 0  , 0    ));
            ExitBtn.Click          += new EventHandler((sender, e) => Environment.Exit(                            0             ));
            ExitBtn.MouseEnter     += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(230, 100, 100 ));
            ExitBtn.MouseLeave     += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(0  , 0  , 0   ));


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
            
            KeyDown += (sender, arg) => FormKeyboardInputHandler(((Control)sender).Name, arg.KeyData, arg.Control, arg.Shift);

            Paint += (venat, yoshiP) => DrawFormDecorations((Form)venat, yoshiP);
        }



        /// <summary>
        /// Initialize Dropdown Menu Used for Toggling of Folder Browser Method
        /// </summary>
        private void CreateBrowseModeDropdownMenu()
        {
            var extalignment = BinFileBrowseBtn.Size.Height;
            var alignment = BinFileBrowseBtn.Location;

            var ButtonSize = new Size(BinFileBrowseBtn.Size.Width + optionsMenuDropdownBtn.Size.Width, 25);

            DropdownMenu[0] = new Button() {
                Font = new Font("Gadugi", 7.25f, FontStyle.Bold),
                Text = "Directory Tree*",
                BackColor = AppColour,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(alignment.X, alignment.Y + extalignment),
                Size = ButtonSize
            };
            DropdownMenu[1] = new Button() {
                Font = new Font("Gadugi", 7.25F, FontStyle.Bold),
                Text = "File Browser",
                BackColor = AppColour,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(alignment.X, alignment.Y + extalignment + DropdownMenu[0].Size.Height),
                Size = ButtonSize
            };


            // Create and Assign Event Handlers
            DropdownMenu[0].Click += (why, does) =>
            {
                if (!LegacyFolderSelectionDialogue) {
                    DropdownMenu[0].Text += '*';
                    DropdownMenu[1].Text = DropdownMenu[1].Text.Remove(DropdownMenu[1].Text.Length-1);

                    LegacyFolderSelectionDialogue ^= true;
                }
            };
            DropdownMenu[1].Click += (my, back) =>
            {
                if (LegacyFolderSelectionDialogue) {
                    DropdownMenu[1].Text += '*';
                    DropdownMenu[0].Text = DropdownMenu[0].Text.Remove(DropdownMenu[0].Text.Length-1);

                    LegacyFolderSelectionDialogue ^= true;
                }
            };
            // still hurt. there was a third event at first.


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





        //#
        //## Logging/Output functionaliy
        //#
        #region [Logging/Output Functionality]

        /// <summary>
        /// Echo a provided string (or string representation of an object) to the standard console output.
        /// <br/> Appends an empty new line if no message is provided.
        /// </summary>
#pragma warning disable IDE1006 // bug off, this one's lowercase
        public static void echo(object message = null)
        {
            # if DEBUG
            string str;

            Console.WriteLine(str = message?.ToString() ?? string.Empty);

            if (!Console.IsInputRedirected)
            {
                Debug.WriteLine(str);
            }
            #endif
        }
#pragma warning restore IDE1006

        

        /// <summary>
        /// Overrite a specific line in the properties output window with the provided <paramref name="message"/>
        /// <br/> Appends an empty new line if no message is provided.
        /// </summary>
        public void PrintPropertyDetailSL(object message = null, int line = 0)
        {
            if (message == null)
                message = string.Empty;

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(Venat.propertiesWindowSpecificLineMammet, new object[] { message?.ToString() ?? "null", line < 0 ? 0 : line });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailSL Invokation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }


        /// <summary>
        /// Replace a specified line in the properties output window with <paramref name="message"/>.
        /// <br/> Clears the line if no message is provided.
        /// </summary>
        public static void PrintPropertyDetailNL(object message = null)
        {
            if (message == null)
                message = string.Empty;

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(Venat.propertiesWindowNewLineMammet, new object[] { message?.ToString() ?? "null", null });
            }
            catch (Exception dang)
            {
                var err = $"Missed PrintPropertyDetailNL Invokation due to a(n) {dang.GetType()}.";
                echo(err);
            }
        }
        
        
        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="details"> The string[] to update the label's text with. </param>
        public static void UpdateStatusLabel(string[] details)
        {
            if ((details?.Length ?? 0) < 1)
            {
                echo($"ERROR: Empty or null string array provided for status label details." );
                return;
            }

            StatusDetails = details;
        }

        /// <summary>
        /// Reset the ScriptStatusLabel to it's default value.
        /// </summary>
        public static void ResetStatusLabel()
        {
            StatusDetails = null;
        }


        
        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="details"> The string to update the label's text with. </param>
        public static void UpdateSelectionLabel(string[] details)
        {
            if ((details?.Length ?? 0) < 1)
            {
                echo($"ERROR: Empty or null string array provided for selection label details." );
                return;
            }

            SelectionDetails = details;
        }

        /// <summary>
        /// Reset the ScriptSelectionLabel to it's default value.
        /// </summary>
        public static void ResetSelectionLabel()
        {
            SelectionDetails = Array.Empty<string>();
        }
        #endregion




        //#
        //## Miscellaneous Functions
        //#
        #region [Miscellaneous Functions]
        
        /// <summary>
        /// (//! Ideally...) Reset the GUI and all relevant globals to their original states.
        /// </summary>
        private static void CloseBinFile()
        {
            DCFile = null;
            PropertiesPanel.Controls.Clear();
            PropertiesWindow.Clear();
            LogWindow.Clear();

            if (Venat == null)
            {
                Venat.HeaderItemButtons = null;
                Venat.SubItemButtons = null;
                Venat.HeaderSelection = null;

                Venat.optionsMenuDropdownBtn.TabIndex -= DCScript.Entries.Length;
                Venat.MinimizeBtn.TabIndex -= DCScript.Entries.Length;
                Venat.ExitBtn.TabIndex -= DCScript.Entries.Length;
            }

            ResetSelectionLabel();
            ResetStatusLabel();
        }


        /// <summary>
        /// Use the Buffer class to copy and return a smaller sub-array from a provided <paramref name="array"/>.
        /// </summary>
        /// <param name="array"> The larger array from which to take the sub-array returned. </param>
        /// <param name="index"> The start index in <paramref name="array"/> from which the copying starts. </param>
        /// <param name="len"> The length of the sub-array to be returned. Defaults to 8 bytes. </param>
        /// <returns> A byte array of 8 bytes (or an optional different length) copied from the specified <paramref name="index"/> in <paramref name="array"/>. </returns>
        public static byte[] GetSubArray(byte[] array, int index, int len = 8)
        {
            var ret = new byte[8];
            Buffer.BlockCopy(array, index, ret, 0, len);

            return ret;
        }


        /// <summary>
        /// Parse the current sidbase.bin for the string representation of the provided 64-bit fnv1a hash.
        /// </summary>
        /// <param name="EncodedSIDArray"> The 8-byte array of bytes to decode. </param>
        /// <returns> Either the decoded version of the provided hash, or the string representation of said SID if it could not be decoded. </returns>
        private static string DecodeSIDHash(byte[] EncodedSIDArray)
        {            
            if (EncodedSIDArray.Length == 8)
            {
                var encodedSIDString = BitConverter.ToString(EncodedSIDArray).Replace("-", string.Empty);

                for (long mainArrayIndex = 0, subArrayIndex = 0; mainArrayIndex < SIDBaseTableLength; subArrayIndex = 0, mainArrayIndex+=8)
                {
                    if (SIDBase[mainArrayIndex] != EncodedSIDArray[subArrayIndex])
                    {
                        continue;
                    }


                    // Scan for the rest of the bytes
                    while ((subArrayIndex < 8 && mainArrayIndex < SIDBase.Length) && SIDBase[mainArrayIndex + subArrayIndex] == EncodedSIDArray[subArrayIndex]) // while (subArrayIndex < 8 && sidbase[mainArrayIndex++] == (byte)bytesToDecode[subArrayIndex++]) how the fuck does this behave differently?? I need sleep.
                    {
                        subArrayIndex++;
                    }

                    // continue if there was only a partial match
                    if (subArrayIndex != 8)
                    {
                        continue;
                    }
                

                    // Read the string pointer
                    var stringPtr = BitConverter.ToInt64(SIDBase, (int)(mainArrayIndex + subArrayIndex));
                    if (stringPtr >= SIDBase.Length)
                    {
                        throw new IndexOutOfRangeException($"ERROR: Invalid Pointer Read for String Data!\n    str* 0x{stringPtr:X} >= len 0x{SIDBase.Length:X}.");
                    }


                    // Parse and add the string to the array
                    var stringBuffer = string.Empty;

                    while (SIDBase[stringPtr] != 0)
                    {
                        stringBuffer += Encoding.UTF8.GetString(SIDBase, (int)stringPtr++, 1);
                    }

                
                    return stringBuffer;
                }
                
                return "0x" + BitConverter.ToString(EncodedSIDArray).Replace("-", string.Empty);
            }

            // Invalid Length for encoded id array
            else {
                echo($"Invalid SID provided; unexpected length of \"{EncodedSIDArray?.Length ?? 0}\". Must be 8 bytes.");
                return "INVALID_SID_64";
            }
        }

        private static string DecodeSIDHash(ulong EncodedSID) => DecodeSIDHash(BitConverter.GetBytes(EncodedSID));
        #endregion [miscellaneous functions]



        //#
        //## Mammet Shorthand Functions
        //#
        #region [Mammet Shorthand Functions]

        public static void AbortButtonMammet(params object[] args)
        {
            Venat?.Invoke(Venat.abortButtonMammet, new[] { args ?? new object[] { false, 0 } });
        }
        
        public static void ReloadButtonMammet(bool enabled)
        {
            Venat?.Invoke(Venat.reloadButtonMammet, new[] { new object[] { enabled } });
        }

        public static void PropertiesPanelMammet(object dcFileName, DCFileHeader dcEntries)
        {
            Venat?.Invoke(Venat.propertiesPanelMammet, new[] { dcFileName, dcEntries });
        }
        
        
        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="details">
        /// A string[3] containing the details for the status label.
        /// <br/> 
        /// </param>
        public static void StatusLabelMammet(string[] details = null)
        {
            Venat?.Invoke(Venat.statusLabelMammet, new [] { details ?? new[] { string.Empty, string.Empty, string.Empty } });
        }


        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="details">
        /// A string[3] containing the details for the slection label.
        /// <br/> 
        public static void SelectionLabelMammet(string[] details = null)
        {
            Venat?.Invoke(Venat.selectionLabelMammet, new [] { details ?? new[] { string.Empty, string.Empty, string.Empty } });
        }
        #endregion [mammet shorthand functions]

        #endregion [Global Functions]
    }


    
    

    //=====================================\\
    //---|   Custom Class Extensions   |---\\
    //=====================================\\
    #region [Custom Class Extensions]

    /// <summary>
    /// Custom RichTextBox class because bite me.
    /// </summary>
    public class RichTextBox : System.Windows.Forms.RichTextBox {

        /// <summary> Appends Text to The Currrent Text of A Text Box, Followed By The Standard Line Terminator.<br/>Scrolls To Keep The Newest Line In View. </summary>
        /// <param name="str"> The String To Output. </param>
        public void AppendLine(string str = "", bool scroll = true)
        {
            AppendText(str + '\n');
            Update();
                
            if (scroll) {
                ScrollToCaret();
            }
        }



        public void UpdateLine(string newMsg, int line, bool scroll = true)
        {
            while (line >= Lines.Length)
            {
                AppendText("\n");
            }

            var lines = Lines;
            lines[line] = newMsg ?? " ";

            Lines = lines;
            Update();

            if (scroll) {
                ScrollToCaret();
            }
        }
    }

    /// <summary> Custom TextBox Class to Better Handle Default TextBox Contents. </summary>
    public class TextBox : System.Windows.Forms.TextBox
    {
        /// <summary> Create a new winforms TextBox control. </summary>
        public TextBox()
        {
            TextChanged += SetDefaultText; // Save the first Text assignment as the DefaultText

            GotFocus += (sender, args) => ReadyControl();
            LostFocus += (sender, args) => ResetControl(false); // Reset control if nothing was entered, or the text is a portion of the default text
        }



        /// <summary>
        /// Default Control Text to Be Displayed When "Empty".
        /// </summary>
        private string DefaultText;
        public override string Text
        {
            get => base.Text;

            set {
                base.Text = value?.Replace("\"", string.Empty);
            }
        }



        // Help Better Keep Track of Whether the User's Changed the Text, Because I'm a Moron.
        public bool IsDefault() => Text == DefaultText;

        /// <summary>
        /// Yoink Default Text From First Text Assignment (Ideally right after being created).
        /// </summary>
        private void SetDefaultText(object _, EventArgs __)
        {
            DefaultText = Text;

            TextChanged -= SetDefaultText;
        }


        private void ReadyControl()
        {
            if(IsDefault()) {
                Clear();
            }
        }


        public void Reset() => ResetControl(true);
        private void ResetControl(bool forceReset)
        {
            if(Text.Length < 1 || DefaultText.Contains(Text) || forceReset)
            {
                Text = DefaultText;
            }

        }


        /// <summary>
        /// Set Control Text and State Properly (meh).
        /// </summary>
        public void Set(string text)
        {
            if (text != string.Empty && !DefaultText.Contains(text))
            {   
                Text = text;
            }
        }
    }

    public class Label : System.Windows.Forms.Label
    {
        public bool IsSeparatorLine
        {
            get => _isSeparatorLine;
            set => _isSeparatorLine = value;
        }
        private bool _isSeparatorLine = false;


        public bool StretchToFitForm
        {
            get => _stretchToFitForm & IsSeparatorLine;
            set => _stretchToFitForm = value;
        }
        private bool _stretchToFitForm = false;
    }
    #endregion
}
