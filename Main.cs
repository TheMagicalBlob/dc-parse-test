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
#if DEBUG
            Bingus = new DebugOptionsPanel();
#endif



            //##-> Create the various delegates for the Properties Handler, so we can do shit across multiple threads
            populatePropertyList = PopulatePropertyList;
            spawnVariableEditorBox = SpawnVariableEditorBox;
            editStructureInHexEditor = EditStructureInHexEditor;

            selectionLabelMammet = UpdateSelectionLabel;
            selectionLabelResetMammet = ResetSelectionLabel;
            LogUpdateMammet = Log;
            LogSameLineMammet = _Log;

            setReloadCloseButtonStatus = SetReloadCloseButtonStatus;
            CloseBinFileMammet = CloseBinFile;



            //##-> Create and decorate the form
            InitializeGUI(DCFilePath);

            Refresh(); Update();



            //##-> PropertyList/Editor-Related Variable Declarations
            DefaultPropertyListButtonHeight = 23;
            DefaultPropertyEditorRowHeight = 23;

            Changes = new List<object[]>();
            History = new List<object[]>();



            //##-> Check various expected paths for the required sidbase.bin file
            LoadSidbaseFromExpectedPath();


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

        /// <summary>
        /// Create and design the contents of the GUI, as well as save various control refferences
        /// </summary>
        /// <param name="DCFilePath"></param>
        private void InitializeGUI(string DCFilePath)
        {
            InitializeComponent();
            InitializeAdditionalEventHandlers(this);

            VersionLabel.Text += Version;
            logWindow.Clear();



            // Set global object refs used in various static functions
            PropertySelectionPanel = propertyListPanel;
            PropertyEditorPanel = propertyEditorPanel;
            LogWindow = logWindow;

            ActiveScriptLabel = activeScriptLabel;
            ScriptSelectionLabel = scriptSelectionLabel;

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






        /// <summary>
        /// Search the program's current working directory and parent directory for an existing sidbase.bin, and load any that are found.
        /// </summary>
        public void LoadSidbaseFromExpectedPath()
        {
            var workingDirectory = Directory.GetCurrentDirectory();

            var paths = new[]
            {
                $@"{workingDirectory}\sidbase.bin",
                $@"{workingDirectory}\sid\sidbase.bin",
                $@"{workingDirectory}\sid1\sidbase.bin",

                $@"{workingDirectory}\..\sidbase.bin",
                $@"{workingDirectory}\..\sid\sidbase.bin",
                $@"{workingDirectory}\..\sid1\sidbase.bin",
            };

            if (!paths.Any(path =>
            {
                if (File.Exists(path))
                {
                    SIDBase.LoadSIDBase(path);
                    Log($"Loaded sidbase.bin from:\n\"{path}\".");
                    return true;
                }
                else {
                    return false;
                }
            }))
            {
                echo($"No valid sidbase.bin file was found in \"{workingDirectory}\" or its parent directory.");
                Log("!! WARNING: No sidbase.bin found within the app folder; please load one to view script modules properly.");
            }
        }
        #endregion (function declarations)












        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void BrowseForDCScript(object sender, EventArgs e)
        {
            using (var Browser = new OpenFileDialog())
            {
                Browser.Title = "Please select a NaughtyDog DC script module.";

                if (Browser.ShowDialog() == DialogResult.OK)
                {
                    LoadBinFile(Browser.FileName);
                }
            }
        }






        private void BrowseForSidbase(object sender, EventArgs e)
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
#if DEBUG
            if (Bingus == null)
            {
                var msg = "Debug Panel not created.";

                echo(msg);
                return;
            }

            Bingus.Visible ^= true;
            Bingus.Location = new Point(Venat.Location.X + ((Venat.Size.Width - Azem.Size.Width) / 2), Venat.Location.Y + SubformVerticalOffset);
            Bingus.Update();
#endif
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
        private void CloseBinFileButtonPressed(object _, EventArgs __) => CloseBinFile();



        private void DeleteMe1(object sender, EventArgs e) {  }



        private void DeleteMe2(object sender, EventArgs e) { }
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
        /// Attempt to space-out a struct name by prepending a space to any capitalized letter that follows a lowercase one.
        /// 
        /// //! POINTLESS NOW- wrote this before I reworked the structs and shite like WeaponGameplayDef became weapon_gameplay_def
        /// </summary>
        /// <returns> The provided <paramref name="StructName">, now spaced out rather than camel/pascal-case. </returns>
        private string SpaceOutStructName(string StructName)
        {
            if (!System.Globalization.CultureInfo.CurrentCulture.ToString().ToLower().StartsWith("en"))
            {
                echo("Warning- Unexpected culture info. No idea whether this matters. I'm fairly sure it doesn't in this case, though.");
            }

            var str = string.Empty;

            for (var charIndex = 0; charIndex < StructName.Length; charIndex++) // Why the hell did i use decimal numbers instead of hex? Douche.
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
    }
}
