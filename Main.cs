using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NaughtyDogDCReader
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            InitializeAdditionalEventHandlers_Main(this);

            VersionLabel.Text += Version;
            logWindow.Clear();
            propertiesWindow.Clear();



            // Set global object refs used in various static functions (maybe change that...)
            Update(); Refresh();
            // Form pointers
            Venat = this;
            Azem = new OptionsPage();
            Panels = new PropertiesHandler();
            Bingus = new DebugPanel();


            PropertiesPanel = propertiesPanel;
            PropertiesWindow = propertiesWindow;
            
            ScriptStatusLabel = scriptStatusLabel;
            ScriptSelectionLabel = scriptSelectionLabel;
            AbortOrCloseBtn = abortOrCloseBtn;
            Update(); Refresh();



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
                    SIDBase = new SIDBase(path);
                    return true;
                } 
                else return false;
            }))
            // Bitch if it isn't found so the user knows to load one manually
            {
                echo($"No valid sidbase.bin file was found in/around \"{workingDirectory}\".");
                UpdateStatusLabel(new[] { "WARNING: No sidbase.bin found; please provide one before loading a DC file." });
            }


            BaseAbortButtonWidth = AbortOrCloseBtn.Size.Width;
        }





        


        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void BinPathBrowseBtn_Click(object sender, EventArgs e)
        {
#if !DEBUG
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
                @"C:\Users\blob\LocalModding\Bin Reversing\working (1.07)\weapon-mods.bin"
                //@"C:\Users\blob\LocalModding\Bin Reversing\_Scripts\weapon-gameplay.bin"
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
                    LoadSIDBase(fileBrowser.FileName);   
                }
            }
        }


        private void ToggleOptionsMenu(object sender, EventArgs e)
        {
            Azem.Visible ^= true;
            Azem.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
            Azem.Update();
        }
        

        private void ToggleDebugPanel(object sender, EventArgs e)
        {
            Bingus.Visible ^= true;
		    Bingus.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
            Bingus.Update();
        }
        

        private void ReloadBinFile(object sender, EventArgs e)
        {
            var filePath = ActiveFilePath;
            CloseBinFile();

            if (File.Exists(filePath))
            {
                LoadBinFile(filePath);
            }
            else {
                UpdateStatusLabel(new[] { "ERROR: Unable to reload DC File. (File no longer exists.)", emptyStr, emptyStr });
            }
        }
        

        private void AbortOrCloseBtn_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Abort")
            {
                Abort = true;
            }
            else {
                CloseBinFile();
                AbortButtonMammet(0, false);
            }

            ReloadButtonMammet(false);
        }


        /// <summary>
        /// Testing random input crap
        /// </summary>
        private void FormKeyboardInputHandler(string sender, Keys arg, bool ctrl, bool shift)
        {
            echo($"Input [{arg}] Recieved by Control [{sender}]");
            return;

            /*
            switch (arg)
            {
                case Keys.Down:
                    if ((int)HeaderSelection.Tag == HeaderItemButtons.Length - 1)
                    {
                        HeaderItemButtons[0].Focus();
                    }
                    else {
                        HeaderItemButtons[(int)HeaderSelection.Tag + 1].Focus();
                    }
                break;

                case Keys.Up:
                    if ((int)HeaderSelection.Tag == 0)
                    {
                        HeaderItemButtons[HeaderItemButtons.Length - 1].Focus();
                    }
                    else {
                        HeaderItemButtons[(int)HeaderSelection.Tag - 1].Focus();
                    }
                break;


                #if DEBUG
                default:
                    echo($"Misc Input Recieved: [{arg}]");
                break;
                #endif
            }


            if (HeaderSelection == null && (HeaderItemButtons?.Any() ?? false))
            {
                HeaderItemButtons[arg == Keys.Down ? 0 : HeaderItemButtons.Length - 1].Focus();
            }
            else {
                if (arg == Keys.Down)
                {
                    if ((int)HeaderSelection.TabIndex == HeaderItemButtons.Length - 1)
                    {
                        HeaderItemButtons[0].Focus();
                    }
                    else {
                        HeaderItemButtons[(int)HeaderSelection.TabIndex - 1].Focus();
                    }
                }
                else if (arg == Keys.Up)
                {
                    if ((int)HeaderSelection.TabIndex == 0)
                    {
                        HeaderItemButtons[HeaderItemButtons.Length - 1].Focus();
                    }
                    else {
                        HeaderItemButtons[(int)HeaderSelection.TabIndex - 1].Focus();
                    }
                }
            }
            */
        }
        #endregion








        //==================================\\
        //--|   Function Delcarations   |---\\
        //==================================\\
        #region [Function Delcarations]
        
        private void StartBinParseThread()
        {   
            if (binThread != null && binThread.ThreadState != System.Threading.ThreadState.Unstarted)
            {
                try {
                    echo("Bin thread already active, killing thread.");
                    binThread.Abort();
                }
                catch (ThreadAbortException) { echo("Bin thread killed."); }
                catch (Exception dang) { echo($"Unexpected error of type \"{dang.GetType()}\" thrown when aborting bin thread."); }
                echo();
            }

            // Create and start the thread
            (binThread = new Thread(ThreadedBinFileParse)).Start();
        }


        /// <summary>
        /// //! Write Me
        /// </summary>
        /// <param name="pathObj"> The string object containing the path to the .bin file to be parsed. </param>
        private void ThreadedBinFileParse()
        {
            var binPath = ActiveFilePath?.ToString() ?? "null";
            
            try
            {
                //#
                //## Load & Parse provided DC file.
                //#
                DCFile = File.ReadAllBytes(binPath);

                if (SHA256.Create().ComputeHash(DCFile).SequenceEqual(EmptyDCFileHash))
                {
                    StatusLabelMammet(new [] { "Empty DC File Loaded." });
                    ResetSelectionLabel();
                    return;
                }


                AbortButtonMammet(true);

                DCScript = new DCFileHeader(DCFile, ActiveFileName);

                for (int headerItemIndex = 0, offset = 0x28; headerItemIndex < DCScript.Entries.Length; headerItemIndex++, offset += 24)
                {
                    StatusLabelMammet(new[] { null, $" ({headerItemIndex} / {DCScript.TableLength})", null });
                    echo($"Item #{headerItemIndex}: [ Label: {DCScript.Entries[headerItemIndex].Name} Type: {DCScript.Entries[headerItemIndex].Type} Data Address: {DCScript.Entries[headerItemIndex].StructAddress:X} ]");
                }



                //#
                //## Setup Form
                //#
                echo("\nFinished!");
                StatusLabelMammet(new[] { "Finished Loading dc File, populating properties panel...", emptyStr, emptyStr });

                ReloadButtonMammet(true);
                AbortButtonMammet(1);

                PropertiesPanelMammet(ActiveFileName, DCScript);
                ResetStatusLabel();
            }
            // File in use, probably
            catch (IOException)
            {
                echo($"\nERROR: Selected file is either in use, or doesn't exist.");

                StatusLabelMammet(new[] { "Error Loading dc File!!!", emptyStr, emptyStr });
                ResetSelectionLabel();
            }
            // Thread has been killed (normal)
            catch (ThreadAbortException)
            {
                StatusLabelMammet(new[] { "DC Parse Aborted", emptyStr, emptyStr });
                ResetSelectionLabel();
            }
            # if !DEBUG
            catch (Exception fuck) {
                echo($"\nERROR: An unexpected {fuck.GetType()} occured while attempting to parse the DC file.");
                MessageBox.Show($"An unexpected {fuck.GetType()} occured while parsing the provided DC .bin file.", "Unhandled Error Parsing DC File!!!");
                AbortButtonMammet(false, 0);
            }
            #endif
        }


        /// <summary>
        /// //! WRITE ME
        /// </summary>
        /// <param name="binFile"> The whole DC file, loaded as a byte array. </param>
        /// <param name="Type"> The type of the DC struct. </param>
        /// <param name="Address"> The address of the DC struct in the <paramref name="binFile"/>. </param>
        /// <param name="Name"> The name (if there is any) of the DC structure entry </param>
        /// <param name="silent"></param>
        /// <returns> The loaded DC Structure, in object form. (or a string with basic details about the structure, if it hasn't at least been slightly-apped) </returns>
        private static object LoadDCStructByType(byte[] binFile, SID Type, long Address, object NameObj = null)
        {
            var Name = (SID) (NameObj ?? SID.Empty);

            switch (Type.RawID)
            {
                //#
                //## Mapped Structures
                //#
                // map == [ struct len, sid[]* ids, struct*[] * data ]
                case KnownSIDs.map:                       return new DCMapDef(binFile, Address, Name);
                
                case KnownSIDs.weapon_gameplay_defs:      return new WeaponGameplayDef(binFile, Address, Name);
                
                case KnownSIDs.melee_weapon_gameplay_def: return new MeleeWeaponGameplayDef(binFile, Address, Name);
                
                case KnownSIDs.symbol_array:              return new SymbolArrayDef(binFile, Address, Name);
                
                case KnownSIDs.ammo_to_weapon_array:      return new AmmoToWeaponArray(binFile, Address, Name);

                case KnownSIDs.look2_def:                 return new Look2Def(binFile, Address, Name);

                    
                //#
                //## Unmapped Structures
                //#
                default: return new UnknownStruct(Type, Address, Name);
            }
        }
        #endregion [Function Declarations]
    }
}
