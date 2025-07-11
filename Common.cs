using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

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
                else {
                    //! Implement an error, since the file would obviously be corrupted.
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
                _dcFile = value;

                if (value?.Length > 0x2C)
                {
                    DCFileMainDataLength = BitConverter.ToInt64(DCFile, 8);
                }
                else if ((value ?? null) != Array.Empty<byte>()){
                    //! Implement an error, since the file would obviously be corrupted.
                }
            }
        }
        private static byte[] _dcFile;

        /// <summary>
        /// The address of the provided DC file's relocation table
        /// </summary>
        public static long DCFileMainDataLength;


        
        public static DCFileHeader DCHeader;

        public static object[] DCEntries;

        public List<object[]> DecodedIDS = new List<object[]>(1000);

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

        /// <summary> Boolean global for keeping track of the current mouse state. </summary>
        public static bool MouseIsDown = false;



        /// <summary> Store Expected Options Form Offset. </summary>
        public static Point OptionsFormLocation;

        /// <summary> Variable for Smooth Form Dragging. </summary>
        public static Point MouseDif;

        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        private static Point[][] HSeparatorLines;
        
        /// <summary> An array of Point() arrays with the start and end points of a line to draw. </summary>
        private static Point[][] VSeparatorLines;



        /// <summary> MainPage Form Pointer/Refference. </summary>
        public static Main Venat;

        /// <summary> OptionsPage Form Pointer/Refference. </summary>
        public static OptionsPage Azem;

        /// <summary> Properties Panel Form Pointer/Refference. </summary>
        public static GroupBox Emmet;

        /// <summary> OutputWindow Pointer/Ref Because I'm Lazy. </summary>
        public static RichTextBox OutputWindow;


        
        /// <summary> Boolean global to set the type of dialogue to use for the GamedataFolder path box's browse button. </summary>
        public static bool LegacyFolderSelectionDialogue = true;



        /// <summary> The name of the . </summary>
        public static string ActiveFileName;


        /// <summary> The difference in size (horizontally, in pixels) of the Abort/Close File button when it changes from one to the other. </summary>
        private static readonly int AbortButtonWidthDifference = 20; //! Lazy

        /// <summary> The initial width (in pixels) of the Abort button. Used when switching from "abort/close file" modes. </summary>
        private static int BaseAbortButtonWidth;

        
        public static Label scriptStatusLabel;
        public static Label scriptSelectionLabel;
        public static Button abortBtn;


        public static string[] StatusDetails
        {
            get => _statusDetails;

            private set
            {
                if (value == null || value.Length < 1)
                {
                    _selectionDetails = Array.Empty<string>();
                    scriptSelectionLabel.Text = string.Empty;
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

                UpdateStatusLabel(_statusDetails);
            }
        }
        public static string[] _statusDetails = new [] { string.Empty, string.Empty, string.Empty };



        public static string[] SelectionDetails
        {
            get => _selectionDetails;

            private set
            {
                if (value == null || value.Length < 1)
                {
                    _selectionDetails = Array.Empty<string>();
                    scriptSelectionLabel.Text = string.Empty;
                    return;
                }


                // Update changed array members only
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] != null)
                    {
                        _selectionDetails[i] = value[i];
                    }
                }

                UpdateSelectionLabel(_selectionDetails);
            }
        }
        public static string[] _selectionDetails = new [] { string.Empty, string.Empty };


        
        //#
        //## Threading-Related Variables
        //#
        private static Thread binThread;

        public delegate void binThreadFormWand(params object[] args); //! god I need to read about delegates lmao
        private delegate void binThreadLabelWand(string[] details);
        public delegate void binThreadOutputWand(string msg, int line = 0);
        public delegate string[] binThreadFormWandOutputRead();

        

        public binThreadOutputWand propertiesWindowNewLineMammet = new binThreadOutputWand((args, _) =>
        {
            OutputWindow.AppendLine(args[0].ToString());
            OutputWindow.Update();
        });

        public binThreadOutputWand propertiesWindowSpecificLineMammet = new binThreadOutputWand((msg, line) =>
        {
            OutputWindow.UpdateLine(msg, line);
            OutputWindow.Update();
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
            if (args == null || args.Length < 1 || abortBtn == null)
            {
                return;
            }

            foreach (object obj in args)
            {
                if (obj == null || obj.GetType() == typeof(int))
                {
                    int newIndex;

                    abortBtn.Text = new[] { "Abort", "Close File" } [newIndex = (int) (obj ?? (abortBtn.Text == "Abort" ? 1 : 0))];
                
                    abortBtn.Size = new Size(BaseAbortButtonWidth + AbortButtonWidthDifference * newIndex, abortBtn.Size.Height);
                    abortBtn.Location = new Point(abortBtn.Location.X + AbortButtonWidthDifference * (-2 * newIndex + 1), abortBtn.Location.Y);
                    return;
                }
                else if (obj.GetType() == typeof(bool))
                {
                    abortBtn.Enabled = (bool)obj;
                    abortBtn.Font = new Font(abortBtn.Font.FontFamily, abortBtn.Font.Size, (FontStyle)(((bool) obj ? 0 : 8)));
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


        
        /// <summary>
        /// Draw a thin border over the for edges on repaint.
        /// <br/>Draw a thin line from one end of the painted control to the other.
        ///</summary>
        public static void DrawFormDecorations(Form venat, PaintEventArgs yoshiP)
        {
            if (noDraw)
            {
                return;
            }

            yoshiP.Graphics.Clear(venat.BackColor); // Clear line bounds with the current form's background colour

            
            //## Draw Vertical Lines
            foreach (var line in VSeparatorLines ?? Array.Empty<Point[]>())
            {
                yoshiP?.Graphics.DrawLine(pen, line[0], line[1]);
            }

            //## Draw Horizontal Lines
            foreach (var line in HSeparatorLines ?? Array.Empty<Point[]>())
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
                    item.KeyDown += (sender, arg) => FormKeyboardInputHandler(((Control)sender).Name, arg.KeyData, arg.Control, arg.Shift);
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
            
            
            MinimizeBtn.Click      += new EventHandler((sender, e) => Venat.WindowState           = FormWindowState.Minimized     );
            MinimizeBtn.MouseEnter += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(90, 100, 255  ));
            MinimizeBtn.MouseLeave += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(0 , 0  , 0    ));
            ExitBtn.Click          += new EventHandler((sender, e) => { noDraw = true;  Environment.Exit(          0           );});
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
            var extalignment = BinPathBrowseBtn.Size.Height;
            var alignment = BinPathBrowseBtn.Location;

            var ButtonSize = new Size(BinPathBrowseBtn.Size.Width + optionsMenuDropdownBtn.Size.Width, 25);

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

            Console.WriteLine(str = message?.ToString() ?? "null");

            if (Console.IsInputRedirected)
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
        public void PrintLL(object message = null, int line = 0)
        {
            if (message == null)
                message = string.Empty;

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(propertiesWindowSpecificLineMammet, message, line < 0 ? 0 : line);
            }
            catch (Exception dang)
            {
                var err = $"Missed echo Invokation due to a {dang.GetType()}";
                echo(err);
            }
        }


        /// <summary>
        /// Append a <paramref name="message"/> to the properties output window.
        /// <br/> Appends an empty new line if no message is provided.
        /// </summary>
        public static void PrintNL(object message = null)
        {
            if (message == null)
                message = string.Empty;

#if DEBUG
            // Debug Output
            echo(message);
#endif

            // This occasionally crashes in a manner that's really annoying to replicate, so meh
            try {
                Venat?.Invoke(Venat.propertiesWindowNewLineMammet, new [] { new object [] { message.ToString() } });
            }
            catch (Exception dang)
            {
                var err = $"Missed echo Invokation due to a {dang.GetType()}";
                echo(err);
            }
        }
        #endregion


        
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
        /// <param name="bytesToDecode"> The 8-byte array of bytes to decode. </param>
        /// <returns> Either the decoded version of the provided hash, or the string representation of said SID if it could not be decoded. </returns>
        public static string DecodeSIDHash(byte[] bytesToDecode)
        {            
            if (bytesToDecode.Length == 8)
            {
                for (long mainArrayIndex = 0, subArrayIndex = 0; mainArrayIndex < SIDBaseTableLength; subArrayIndex = 0, mainArrayIndex+=8)
                {
                    if (SIDBase[mainArrayIndex] != (byte)bytesToDecode[subArrayIndex])
                    {
                        continue;
                    }


                    // Scan for the rest of the bytes
                    while ((subArrayIndex < 8 && mainArrayIndex < SIDBase.Length) && SIDBase[mainArrayIndex + subArrayIndex] == (byte)bytesToDecode[subArrayIndex]) // while (subArrayIndex < 8 && sidbase[mainArrayIndex++] == (byte)bytesToDecode[subArrayIndex++]) how the fuck does this behave differently?? I need sleep.
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

                return BitConverter.ToString(bytesToDecode).Replace("-", string.Empty);
            }
            else {
                echo($"Invalid SID provided; unexpected length of \"{bytesToDecode?.Length ?? 0}\". Must be 8 bytes.");
                return "INVALID_SID_64";
            }
        }

        #endregion
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

                Font = Main.TextFont;
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
