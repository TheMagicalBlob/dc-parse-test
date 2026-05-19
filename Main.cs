using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;

namespace NaughtyDogDCReader
{
    public partial class Main : Form
    {
        /// <summary>
        /// Iniialize the GUI with a preselected script to be loaded immediately.
        /// </summary>
        /// <param name="DCFilePath"> The path to the DC Script to be loaded on-boot. </param>
        public Main(string DCFilePath = null)
        {
            //##-> Set global object refs used in various static functions
            Venat = this;
            Azem = new OptionsPage();
            Bingus = new DebugOptionsPanel();



            //##-> Create the various delegates for the Properties Handler, so we can do shit across multiple threads
            setupPropertyListPopulation = PopulatePropertyList;
            spawnVariableEditorBox = SpawnVariableEditorBox;
            editStructureInHexEditor = EditStructureInHexEditor;

            selectionLabelMammet = UpdateSelectionLabel;
            selectionLabelResetMammet = ResetSelectionLabel;
            LogUpdateMammet = Log;
            LogSameLineMammet = _Log;

            setReloadCloseButtonStatus = SetReloadCloseButtonStatus;
            CloseBinFileMammet = CloseBinFile;



            //##-> PropertyPanel Variable Declarations
            DefaultPropertyListButtonHeight = 23;
            DefaultPropertyEditorRowHeight = 23;

            Changes = new List<object[]>();
            History = new List<object[]>();



            //##-> Create and decorate the form
            InitGUI(DCFilePath);



            //##-> Check various expected paths for the required sidbase.bin file
            var workingDirectory = Directory.GetCurrentDirectory();
            if (!new[]
            {
                $@"{workingDirectory}\sidbase.bin",
                $@"{workingDirectory}\sid\sidbase.bin",
                $@"{workingDirectory}\sid1\sidbase.bin",
                $@"{workingDirectory}\..\sidbase.bin"
            }
            .Any(path =>
            {
                if (File.Exists(path))
                {
                    SIDBase.LoadSIDBase(path);
                    return true;
                }
                else
                {
                    return false;
                }
            }))
            // Bitch if it isn't found so the user knows to load one manually
            {
                echo($"No valid sidbase.bin file was found in \"{workingDirectory}\" or it's immediate subforlders.");
                Log("!! WARNING: No sidbase.bin found; please provide one to decode hashed strings.");
            }


            BaseAbortButtonWidth = CloseBtn.Size.Width;



            //##-> Immediately load the provided script if the tool was started with the path one as the first argument
            if (DCFilePath != null)
            {
                void DelayedDCFileLoad(object _, PaintEventArgs __)
                {
                    Paint -= DelayedDCFileLoad;

                    Update();
                    LoadBinFile(DCFilePath);
                }

                Paint += DelayedDCFileLoad;
            }
        }








        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]


        /// <summary> Return the current state of the options page. </summary>
        public static bool OptionsPageIsOpen => Azem?.Visible ?? false;

#if DEBUG
        public static bool DebugOptionsPageIsOpen => Bingus?.Visible ?? false;
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

            set
            {
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

                if (value != null)
                {
                    ActiveScriptLabel.Text = ActiveFileName;
                }
            }
        }
        private static string _activeFileName = "No Script Selected";




        /// <summary> MainPage Form Pointer/Reference. </summary>
        public static Main Venat;

        /// <summary> OptionsPage Form Pointer/Reference. </summary>
        public static OptionsPage Azem;

        /// <summary> Debug options panel form Pointer/Reference. </summary>
        public static DebugOptionsPanel Bingus;


        /// <summary> Properties Panel GroupBox Pointer/Reference. </summary>
        public static GroupBox PropertySelectionPanel;

        /// <summary> Properties Editor Pointer/Reference. </summary>
        public static GroupBox PropertyEditorPanel;

        /// <summary> Log Window Pointer/Reference.  </summary>
        public static RichTextBox LogWindow;


        public static Label ActiveScriptLabel;

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

        private readonly List<object[]> History;
        #endregion











        //==================================\\
        //--|   Function Declarations   |---\\
        //==================================\\
        #region [Function Delcarations]
#pragma warning disable IDE1006

        private void InitGUI(string DCFilePath)
        {
            InitializeComponent();
            InitializeAdditionalEventHandlers(this);

            VersionLabel.Text += Version;
            logWindow.Clear();



            // Set global object refs used in various static functions
            Refresh();
            PropertySelectionPanel = propertySelectionPanel;
            PropertyEditorPanel = propertyEditorPanel;
            LogWindow = logWindow;

            ActiveScriptLabel = activeScriptLabel;
            ScriptSelectionLabel = scriptSelectionLabel;
            Update();
        }
#pragma warning restore IDE1006



        public void SetReloadCloseButtonStatus(bool isEnabled)
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
        }
        #endregion (function declarations)












        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void BinPathBrowseBtn_Click(object sender, EventArgs e)
        {
            using (var Browser = new OpenFileDialog())
            {
                Browser.Title = "Please select a script from \"bin/dc1\".";

                if (Browser.ShowDialog() == DialogResult.OK)
                {
                    LoadBinFile(Browser.FileName);
                }
            }
        }


        private void SidBaseBrowseBtn_Click(object sender, EventArgs e)
        {
            using (var fileBrowser = new OpenFileDialog
            {
                Title = "Select the desired sidbase.bin to use.",
                Filter = "String ID Lookup Table|*.bin"
            })
            {
                if (fileBrowser.ShowDialog() == DialogResult.OK)
                {
                    SIDBase.LoadSIDBase(fileBrowser.FileName);
                }
            }
        }


        private void ToggleOptionsMenu(object sender, EventArgs e)
        {
            if (Azem == null)
            {
                var msg = "Options page doesn't exist. How did I even manage that?";

                echo(msg);
                Log(msg);
                return;
            }

            Azem.Visible ^= true;
            Azem.Location = new Point(Venat.Location.X + ((Venat.Size.Width - Azem.Size.Width) / 2), Venat.Location.Y + SubformVerticalOffset);
            Azem.Update();
        }


        private void ToggleDebugOptionsPage(object sender, EventArgs e)
        {
            if (Bingus == null)
            {
                var msg = "Debug Panel not created.";

                echo(msg);
                return;
            }

            Bingus.Visible ^= true;
            Bingus.Location = new Point(Venat.Location.X + ((Venat.Size.Width - Azem.Size.Width) / 2), Venat.Location.Y + SubformVerticalOffset);
            Bingus.Update();
        }


        private void ReloadBinFile(object sender, EventArgs e)
        {
            CloseBinFile();

            if (File.Exists(ActiveFilePath))
            {
                LoadBinFile(ActiveFilePath);
            }
            else
            {
                Log("!! ERROR: Unable to reload DC File. (File no longer exists.)");
            }
        }




        /// <summary>
        /// Reset the GUI and all relevant globals to their original states. //! (ideally...)
        /// </summary>
        private void CloseBtn_Click(object _, EventArgs __) => CloseBinFile();



        private void propertyBackBtn_Click(object sender, EventArgs e)
        {
            ReturnToParent();
        }

        private void propertyForwardBtn_Click(object sender, EventArgs e)
        {
            LoadPropertyForHighlightedPropertyButton();
        }
        #endregion










        //===============================================\\
        //--|   Miscellaneous Function Declarations   |--\\
        //===============================================\\
        #region [Miscellaneous Function Declarations]

        /// <summary>
        /// //!
        /// </summary>
        /// <param name="GroupBox"></param>
        /// <param name="HostBoxScrollBarReference"></param>
        private void CreateScrollBarForGroupBox(Control GroupBox, ref VScrollBar HostBoxScrollBarReference, int EntryCount)
        {
            var cumulativeButtonHeight = (DefaultPropertyListButtonHeight * EntryCount) - 1; // I don't know why it's off by a pixel and I'm sick of fucking with it.

            if (cumulativeButtonHeight < PropertySelectionPanel.Height)
            {
                return;
            }


            if (!Venat.Controls.Contains(HostBoxScrollBarReference))
            {
                if (HostBoxScrollBarReference == null)
                {
                    HostBoxScrollBarReference = new VScrollBar()
                    {
                        Name = "PropertiesPanelScrollBar",
                        Height = GroupBox.Height - 2,
                        Width = 20, // Default width's a bit fat
                        //LargeChange = DefaultPropertyButtonHeight * 4, // Not sure which context the LargeChange is even used in, honestly
                    };


                    HostBoxScrollBarReference.Location = new Point((GroupBox.Parent.Location.X + GroupBox.Width) - (HostBoxScrollBarReference.Width + 1), GroupBox.Parent.Location.Y);

                    HostBoxScrollBarReference.Scroll += (_, args) => ScrollPropertyListButtons(GroupBox, args);
                }

                Venat.Controls.Add(HostBoxScrollBarReference);
            }


            HostBoxScrollBarReference.BringToFront();

            HostBoxScrollBarReference.Maximum = cumulativeButtonHeight - GroupBox.Height + (NaughtyDogDCReader.GroupBox.GroupBoxContentsOffset * 2);

            HostBoxScrollBarReference.SmallChange = DefaultPropertyListButtonHeight;
        }






        /// <summary>
        /// Reset all instance members in the current PropertiesHandler (clear all added controls, reset static ones to default states, clear variables)
        /// </summary>
        public void ResetPanels()
        {
            PropertySelectionPanel.Controls.Clear();
            PropertyEditorPanel.Controls.Clear();

            FirstAndLastPropertyButtons = null;
            PropertySelection = null;

            Venat.Controls.Remove(PropertyListScrollBar);
            PropertyListScrollBar = null;

            Venat.Controls.Remove(PropertyEditorScrollBar);
            PropertyEditorScrollBar = null;
        }






        /// <summary>
        /// Prepend a space to any capitalized letter that follows a lowercase one.
        /// </summary>
        /// <returns> The provided <paramref name="StructName">, now spaced out rather than camel/pascal-case. </returns>
        private string SpaceOutStructName(string StructName)
        {
            if (!System.Globalization.CultureInfo.CurrentCulture.ToString().ToLower().StartsWith("en"))
            {
                echo("Warning- Unexpected culture info. No idea whether this matters. I'm fairly sure it doesn't in this case, though.");
            }

            var str = string.Empty;

            for (var charIndex = 0; charIndex < StructName.Length; charIndex++)
            {
                if (StructName[charIndex] <= 122u && StructName[charIndex] >= 97u)
                {
                    if (charIndex + 1 != StructName.Length)
                    {
                        if (StructName[charIndex + 1] >= 65u && StructName[charIndex + 1] <= 90u)
                        {
                            str += $"{StructName[charIndex]} ";
                            continue;
                        }
                    }
                }

                str += StructName[charIndex];
            }

            return str;
        }
        #endregion












        //==================================\\
        //--|   SID Class Declaration   |---\\
        //==================================\\

        /// <summary>
        /// Small class used for handling string id's in a bit more of a convenient manner.
        /// </summary>
        public class SID
        {
            //#
            //## Instance Initializers
            //#

            /// <summary>
            /// Create a new SID instance from a provided byte array, and attempt to decode the id.
            /// </summary>
            /// <param name="EncodedSIDArray"> The encoded ulong string id, converted to a byte array. </param>
            private SID(byte[] EncodedSIDArray)
            {
                DecodedID = SIDBase.DecodeSIDHash(EncodedSIDArray);
                EncodedID = BitConverter.ToString(EncodedSIDArray).Replace("-", string.Empty);

                RawID = (KnownSIDs) BitConverter.ToUInt64(EncodedSIDArray, 0);
            }

            /// <summary>
            /// Create a new SID instance from a provided ulong hash, and attempt to decode the id.
            /// </summary>
            /// <param name="EncodedSID"> The encoded ulong string id. </param>
            private SID(ulong EncodedSID)
            {
                var EncodedSIDArray = BitConverter.GetBytes(EncodedSID);

                DecodedID = SIDBase.DecodeSIDHash(EncodedSIDArray);
                EncodedID = BitConverter.ToString(EncodedSIDArray).Replace("-", string.Empty);
                RawID = (KnownSIDs) EncodedSID;
            }


            /// <summary>
            /// I don't know how else to make that SID.Empty field, lol.
            /// </summary>
            private SID(string decodedSID, ulong encodedSID)
            {
                DecodedID = decodedSID;
                EncodedID = encodedSID.ToString("X");

                RawID = (KnownSIDs) encodedSID;
            }







            //#
            //## VARIABLE DECLARATIONS
            //#
            #region [Variable Declarations]

            /// <summary>
            /// The decoded string id.
            /// <br/><br/>
            /// If the id cannot be decoded, it will return either: 
            /// <br/> - the encoded ulong sid's string representation
            /// <br/> OR
            /// <br/> - UNKNOWN_SID_64
            /// <br/><br/>
            /// Depending on whether the ShowUnresolvedSIDs option is enabled or disabled respectively
            /// </summary>
            public string DecodedID
            {
                get
                {
                    if (_decodedID == "UNKNOWN_SID_64" && ShowUnresolvedSIDs)
                    {
                        return EncodedID;
                    }
#if DEBUG
                    else if (_decodedID == "INVALID_SID_64" && ShowInvalidSIDs)
                    {
                        return EncodedID;
                    }
#endif

                    return _decodedID;
                }

                set => _decodedID = value;
            }
            private string _decodedID;



            /// <summary>
            /// The string representation of the encoded ulong string id.
            /// </summary>
            public string EncodedID { get; set; }



            /// <summary>
            /// The raw ulong version of the encoded string id. (used for hardcoded checks in code)
            /// </summary>
            public KnownSIDs RawID { get; set; }



            /// <summary>
            /// Represents an item with an unspecified name.
            /// </summary>
            public static readonly SID Empty = new SID("unnamed", 0x5FE267C3F96ADB8C);
            #endregion






            //#
            //## FUNCTION DECLARATIONS
            //#
            #region [Function Declarations]

            /// <summary>
            /// Create a new SID instance from the provided <paramref name="EncodedSID"/>.
            /// </summary>
            /// <param name="EncodedSID"> The FNV1-a 64b hash to be decoded with the loaded lookup sidbase.bin table(s), converted to an array of bytes. </param>
            /// 
            /// <returns>
            /// A new SID instance containing the id's decoded string (or error reply), as well as the encoded string id in a hex number string format. <br/>
            /// Also contains the encoded id in it's original ulong format
            /// </returns>
            public static SID Parse(byte[] EncodedSID) => new SID(EncodedSID);


            /// <summary>
            /// Create a new SID instance from the provided <paramref name="EncodedSID"/>.
            /// </summary>
            /// <param name="EncodedSID"> The FNV1-a 64b hash to be decoded with the loaded lookup sidbase.bin table(s), as a default unsigned 64-bit integer. </param>
            /// 
            /// <returns>
            /// A new SID instance containing the id's decoded string (or error reply), as well as the encoded string id in a hex number string format. <br/>
            /// Also contains the encoded id in it's original ulong format
            /// </returns>
            public static SID Parse(ulong EncodedSID) => new SID(EncodedSID);
            #endregion
        }












        //======================================\\
        //--|   SIDBase Class Declaration   |---\\
        //======================================\\

        /// <summary> 
        /// Used for decoding any encoded string id's found.
        /// </summary>
        public class SIDBase
        {
            /// <summary>
            /// Initialize a new instance of the SIDBase class with the file at the path provided. <br/>
            ///
            /// </summary>
            /// <param name="SIDBasePath"> The path of the sidbase.bin to be loaded for this instance. </param>
            /// <exception cref="FileNotFoundException"> Thrown in the event that Jupiter aligns wi- what the fuck else would it be for. </exception>
            public SIDBase(string SIDBasePath)
            {
                // Verify the provided path before proceeding
                if (!File.Exists(SIDBasePath))
                {
                    throw new FileNotFoundException("The file at the provided path does not exist, please ensure that you're not a complete moron.");
                }


                var rawSIDBase = File.ReadAllBytes(SIDBasePath);

                // Read the table length to get the expected size of the hash table (don't really need it anymore)
                HashTableRawLength = BitConverter.ToInt32(rawSIDBase, 0) * 16;

                // Just-In-Case.
                if (HashTableRawLength >= int.MaxValue)
                {
                    Console.Clear();
                    MessageBox.Show($"ERROR: Sidbase is too large for 64-bit addresses, blame Microsoft for limiting me to that, then blame me for not bothering to try splitting the sidbases.");
                    Environment.Exit(0);
                }


                SIDHashTable = GetSubArray(rawSIDBase, 8, HashTableRawLength);
                SIDStringTable = GetSubArray(rawSIDBase, SIDHashTable.Length + 8, rawSIDBase.Length - (HashTableRawLength + 8));

                if (rawSIDBase.Length < 24)
                {
                    //! Implement an error, since the file would obviously be corrupted.
#if DEBUG
                    echo("ERROR: Invalid length for sidbase.bin (< 0x19- is it corrupted?)");
#else
                MessageBox.Show("ERROR: Invalid length for sidbase.bin (< 0x19- is it corrupted?)", "The provided sidbase was unable to be loaded.");
#endif
                }
            }






            //#
            //## VARIABLE DECLARATIONS
            //#

            /// <summary>
            /// The Lookup table of the sidbase, containing the hashes & their decoded string pointers, the latter of which get adjusted to be used with the string table.
            /// </summary>
            private readonly byte[] SIDHashTable;

            /// <summary>
            /// The raw, null-separated string data of the sidbase.
            /// </summary>
            private readonly byte[] SIDStringTable;


            /// <summary>
            /// The length of the sidbase.bin's lookup table (in bytes)<br/>
            /// </summary>
            public readonly int HashTableRawLength;


            /// <summary>
            /// List of SIDBase Class instances for the active sidbase.bin lookup tables.
            /// </summary>
            private static SIDBase[] SIDBases;







            //#
            //## FUNCTION DECLARATIONS
            //#
#pragma warning disable IDE0011 // aDd BrAcEs

            /// <summary>
            /// Load a new sidbase from the path provided, adding it to the list of sidbases to search through. <br/>
            ///
            /// </summary>
            /// <param name="SIDBasePath"> The path of the sidbase.bin to be loaded for this instance. </param>
            public static void LoadSIDBase(string SIDBasePath)
            {
                if (File.Exists(SIDBasePath))
                {
                    if (SIDBases != null)
                    {
                        // Load it and add it to the list of previously-loaded lookup tables
                        SIDBases = SIDBases.Concat(new[] { new SIDBase(SIDBasePath) }).ToArray();
                    }
                    else
                    {
                        // Load it and create the lookup table list
                        SIDBases = new[] { new SIDBase(SIDBasePath) };
                    }
                }
                else
                {
                    // Bitch 'n moan
                    MessageBox.Show($"File does not exist:\n " + SIDBasePath, "Invalid path provided for desired sidbase.bin!");
                }
            }



            /// <summary>
            /// Get a sub-array of the specified <paramref name="length"/> from a larger <paramref name="array"/> of bytes, starting at the <paramref name="Address"/> specified.
            /// </summary>
            /// <param name="array"> The array from which to take the sub-array. </param>
            /// <param name="Address"> The start address of the sub-array within <paramref name="array"/>. </param>
            /// <param name="length"> The length of the sub-array. </param>
            /// <returns> What the hell do you think. </returns>
            private static byte[] GetSubArray(byte[] array, int Address, int length = 8)
            {
                if (length == 0)
                {
                    return Array.Empty<byte>();
                }
                if (Address + length > array.Length)
                {
                    //throw new IndexOutOfRangeException($"Provided length and address exceed the length of the array (0x{Address:X} + 0x{length:X} >= 0x{array.Length:X} - ({Address + length:X}))");
                }



                // Build return string.
                for (var ret = new byte[length]; ; ret[length - 1] = array[Address + (length-- - 1)])
                {
                    if (length <= 0)
                    {
                        return ret;
                    }
                }
            }



            /// <summary>
            /// Attempt to decode a provided 64-bit FNV-1a hash via a provided lookup file (sidbase.bin)
            /// </summary>
            /// <param name="bytesToDecode"> The hash to decode, as an array of bytes </param>
            /// <exception cref="IndexOutOfRangeException"> Thrown in the event of an invalid string pointer read from the sidbase after the provided hash is located. </exception>
            private string LookupSIDHash(byte[] bytesToDecode)
            {
                if (bytesToDecode.Sum(@byte => @byte) == 0)
                {
                    return "(null sid)";
                }

                if (bytesToDecode.Length == 8)
                {
                    ulong
                        currentHash,
                    expectedHash
                    ;
                    int
                        previousAddress = 0xBADBEEF, // Used for checking whether the hash could not be decoded
                    scanAddress = HashTableRawLength / 2,
                    currentRange = scanAddress
                    ;


                    expectedHash = BitConverter.ToUInt64(bytesToDecode, 0);

                    // check whether or not the chunk can be evenly split; if not, check
                    // the odd one out for the expected hash, then exclude it and continue as normal if it isn't a match.
                    if (((HashTableRawLength >> 4) & 1) == 1)
                    {
                        var checkedHash = BitConverter.ToUInt64(SIDHashTable, HashTableRawLength - 0x10);

                        if (checkedHash == expectedHash)
                        {
                            scanAddress = HashTableRawLength - 0x10;
                            goto readString;
                        }

                        scanAddress = currentRange -= 8;
                    }


                    while (true)
                    {
                        // Adjust the address to maintain alignment
                        if (((scanAddress >> 4) & 1) == 1)
                        {
                            if (BitConverter.ToUInt64(SIDHashTable, scanAddress) == expectedHash)
                            {
                                goto readString;
                            }

                            scanAddress -= 0x10;
                        }
                        if (((currentRange >> 4) & 1) == 1)
                        {
                            currentRange += 0x10;
                        }


                        currentHash = BitConverter.ToUInt64(SIDHashTable, scanAddress);

                        if (expectedHash < currentHash)
                        {
                            scanAddress -= currentRange / 2;
                            currentRange /= 2;
                        }
                        else if (expectedHash > currentHash)
                        {
                            scanAddress += currentRange / 2;
                            currentRange /= 2;
                        }
                        else
                        {
                            break;
                        }



                        // Handle missing sid's.
                        if (scanAddress == previousAddress)
                        {
                            return "UNKNOWN_SID_64";
                        }

                        previousAddress = scanAddress;
                    }






                    //#
                    //## Read the string pointer
                    //#
                readString:

                    // Get the string pointer for the read hasha, located immediately after said hash
                    var stringPtr = (int)BitConverter.ToInt64(SIDHashTable, scanAddress + 8);

                    // Adjust the string pointer to account for the lookup table being a separate array, and table length being removed
                    stringPtr -= HashTableRawLength + 8;

                    if (stringPtr >= SIDStringTable.Length)
                    {
                        throw new IndexOutOfRangeException($"ERROR: Invalid Pointer Read for String Data!\n    str* 0x{stringPtr:X} >= len 0x{SIDHashTable.Length + SIDStringTable.Length + 8:X}.");
                    }


                    // Parse and add the string to the array
                    var stringBuffer = string.Empty;

                    while (SIDStringTable[stringPtr] != 0)
                    {
                        stringBuffer += Encoding.UTF8.GetString(SIDStringTable, (int) stringPtr++, 1);
                    }


                    return stringBuffer;
                }
                else
                {
                    echo($"Invalid SID provided; unexpected length of \"{bytesToDecode?.Length ?? 0}\". Must be 8 bytes.");
                    return "INVALID_SID_64";
                }
            }



            public static string DecodeSIDHash(byte[] EncodedSID)
            {
                var id = "(No SIDBases Loaded.)";

                foreach (var table in SIDBases ?? Array.Empty<SIDBase>())
                {
                    id = table?.LookupSIDHash(EncodedSID) ?? "(Null SIDBase Instance!!!)";

                    if (id != "UNKNOWN_SID_64")
                    {
                        break;
                    }
                }

                return id;
            }



            private static void echo(object message = null)
            {
#if DEBUG
                string str;

                Console.WriteLine(str = message?.ToString() ?? string.Empty);

                if (!Console.IsInputRedirected)
                {
                    Debug.WriteLine(str);
                }
#endif
            }
        }
    }
}
