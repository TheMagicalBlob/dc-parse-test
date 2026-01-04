using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NaughtyDogDCReader
{
    public partial class Main : Form
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"> The path to the DC Script to be loaded on-boot. </param>
        public Main(string path) => main(path);

        /// <summary>
        /// 
        /// </summary>
        public Main() => main(null);



        







        //==================================\\
        //--|   Function Declarations   |---\\
        //==================================\\
        #region [Function Delcarations]
        #pragma warning disable IDE1006
        private void main(string DCFilePath)
        {
            InitializeComponent();
            InitializeAdditionalEventHandlers_Main(this);

            VersionLabel.Text += Version;
            logWindow.Clear();
            propertiesWindow.Clear();



            // Set global object refs used in various static functions (maybe change that...)
            Refresh();
            Venat = this;
            Azem = new OptionsPage();
            Panels = new PropertyHandlers();
            Bingus = new DebugPanel();


            PropertiesPanel = propertiesPanel;
            PropertiesWindow = propertiesWindow;
            PropertiesEditor = propertiesEditor;

            LogWindow = logWindow;

            ScriptStatusLabel = scriptStatusLabel;
            ScriptSelectionLabel = scriptSelectionLabel;
            Update();



            // Check various expected paths for the required sidbase.bin file
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
                echo($"No valid sidbase.bin file was found in/around \"{workingDirectory}\".");
                UpdateStatusLabel(new[] { "WARNING: No sidbase.bin found; please provide one to decode hashed strings." });
            }


            BaseAbortButtonWidth = CloseBtn.Size.Width;



            // Immediately load the provided script if the tool was started with the path one as the first argument
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

#pragma warning restore IDE1006

        #endregion (function declarations)









        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void BinPathBrowseBtn_Click(object sender, EventArgs e)
        {
#if true
            using (var Browser = new OpenFileDialog
            {
                Title = "Please select a script from \"bin/dc1\"."
            })
            if (Browser.ShowDialog() == DialogResult.OK)
            {
                LoadBinFile(Browser.FileName);
            }
#else
            LoadBinFile(
                //@"C:\Users\blob\LocalModding\Bin Reversing\_Scripts\characters.bin"
                //@"C:\Users\blob\LocalModding\Bin Reversing\working (1.07)\weapon-mods.bin"
                @"C:\Users\blob\LocalModding\Bin Reversing\_Scripts\weapon-gameplay.bin"
            );
#endif
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
            Azem.Visible ^= true;
            Azem.Location = new Point(Venat.Location.X + ((Venat.Size.Width - Azem.Size.Width) / 2), Venat.Location.Y + SubformVerticalOffset);
            Azem.Update();
        }


        private void ToggleDebugPanel(object sender, EventArgs e)
        {
            Bingus.Visible ^= true;
            Bingus.Location = new Point(Venat.Location.X + ((Venat.Size.Width - Azem.Size.Width) / 2), Venat.Location.Y + SubformVerticalOffset);
            Bingus.Update();
        }


        private void ReloadBinFile(object sender, EventArgs e)
        {
            var filePath = ActiveFilePath;

            if (File.Exists(filePath))
            {
                CloseBinFile();
                LoadBinFile(filePath);
            }
            else {
                UpdateStatusLabel(new[] { "ERROR: Unable to reload DC File. (File no longer exists.)", emptyStr, emptyStr });
            }
        }


        

        /// <summary>
        /// Reset the GUI and all relevant globals to their original states. //! (ideally...)
        /// </summary>
        private void CloseBtn_Click(object _, EventArgs __) => CloseBinFile();



        /// <summary>
        /// Testing random input crap
        /// </summary>
        private void FormKeyboardInputHandler(string sender, Keys arg, bool ctrl, bool shift)
        {
            echo($"Input [{arg}] Received by Control [{sender}]");

            //! temp
            switch (arg)
            {
                case Keys.Back:
                    Panels.GoBack();   
                    break;

                default:
                    echo($"");
                    break;
            }
        }

        

        private void propertyBackBtn_Click(object sender, EventArgs e)
        {
            Panels.GoBack();   
        }

        private void propertyForwardBtn_Click(object sender, EventArgs e)
        {
            Panels?.LoadPropertyForHighlightedPropertyButton();
        }
        #endregion
    }
}
