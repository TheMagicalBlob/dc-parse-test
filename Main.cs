using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace weapon_data
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            InitializeAdditionalEventHandlers_Main();

            VersionLabel.Text += Version;
            logWindow.Clear();
            propertiesWindow.Clear();



            // Set global object refs used in various static functions (maybe change that...)
            Update(); Refresh();
            Venat = this;
            Azem = new OptionsPage();
            Bingus = new DebugPanel();

            PropertiesPanel = propertiesPanel;
            PropertiesWindow = propertiesWindow;
            
            ScriptStatusLabel = scriptStatusLabel;
            ScriptSelectionLabel = scriptSelectionLabel;
            AbortOrCloseBtn = abortOrCloseBtn;
            LogWindow = logWindow;
            Update(); Refresh();



            // Check various expected paths for the required sidbase.bin file
            var workingDirectory = Directory.GetCurrentDirectory();
            if (!new[] { $@"{workingDirectory}\sidbase.bin", $@"{workingDirectory}\sid\sidbase.bin", $@"{workingDirectory}\dis1\sidbase.bin" }
            .Any(path =>
            {
                if (File.Exists(path))
                {
                    SIDBase = File.ReadAllBytes(path);
                    return true;
                }
                return false;
            }))
            {
                echo("No valid sidbase.bin file was provided");
                //! handle missing sidbase, since you've hardcoded the decoded strings, dumbass
                UpdateStatusLabel(new[] { null, "WARNING: No sidbase.bin found; please provide one before loading a DC file." });
            }



            BaseAbortButtonWidth = AbortOrCloseBtn.Size.Width;
            
            propertiesPanelMammet = new PropertiesPanelWand(PopulatePropertiesPanel);
        }



        


        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void FormKeyboardInputHandler(string sender, Keys arg, bool ctrl, bool shift)
        {
            echo($"Input [{arg}] Recieved by Control [{sender}]");

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
        }



        private void BinPathBrowseBtn_Click(object sender, EventArgs e)
        {
            using (var Browser = new OpenFileDialog
            {
                Title = "Please select a non-state-script from \"bin/dc1\"."
            })
            if (Browser.ShowDialog() == DialogResult.OK)
            {
                ActiveFilePath = Browser.FileName;
            }
        }

        private void SidBaseBrowseBtn_Click(object sender, EventArgs e)
        {

        }


        private void OptionsMenuDropdownBtn_Click(object sender, EventArgs e)
        {
            return; // disabled until i've actually implemented the thing

            Azem.Visible ^= true;
            Azem.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
            Azem.Update();
        }
        
        private void ReloadBinFile(object sender, EventArgs e)
        {
            CloseBinFile();

            if (File.Exists(ActiveFilePath))
            {
                LoadBinFile(ActiveFilePath);
            }
            else {
                UpdateStatusLabel(new[] { "ERROR: Unable to reload DC File. (File no longer exists.)", string.Empty, string.Empty });
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
        

        private void debugPanelBtn_Click(object sender, EventArgs e)
        {
            Bingus.Visible ^= true;
		    Bingus.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
            Bingus.Update();
        }
        #endregion






        //==================================\\
        //--|   Function Delcarations   |---\\
        //==================================\\
        #region [Function Delcarations]
        
        private void LoadBinFile(string binPath)
        {   
            if (binThread != null && binThread.ThreadState != ThreadState.Unstarted)
            {
                try {
                    echo("Bin thread already active, killing thread.");
                    binThread.Abort();
                }
                catch (ThreadAbortException) { echo("Bin thread killed."); }
                catch (Exception dang) { echo($"Unexpected error of type \"{dang.GetType()}\" thrown when aborting bin thread."); }
                echo();
            }


            binThread = new Thread(new ParameterizedThreadStart(ParseBinFile));
            binThread.Start(binPath);
        }


        /// <summary>
        /// //! Write Me
        /// </summary>
        /// <param name="pathObj"> The string object containing the path to the .bin file to be parsed. </param>
        private void ParseBinFile(object pathObj)
        {
            var binPath = pathObj?.ToString() ?? "null";
            
            try
            {
                //#
                //## Load & Parse provided DC file.
                //#
                DCFile = File.ReadAllBytes(binPath);
                AbortButtonMammet(true);

                DCScript = new DCFileHeader(DCFile, ActiveFileName);
                DCEntries = new DCHeaderItem[DCScript.TableLength];

                for (int headerItemIndex = 0, sake = 0x28; headerItemIndex < DCScript.Items.Length; headerItemIndex++, sake += 24)
                {
                    StatusLabelMammet(new[] { null, $" ({headerItemIndex} / {DCScript.TableLength})", null });
                    echo($"Item #{headerItemIndex}: [ Label: {DCScript.Items[headerItemIndex].Name} Type: {DCScript.Items[headerItemIndex].Type} Data Address: {DCScript.Items[headerItemIndex].StructAddress:X} ]");
                    DCEntries[headerItemIndex] = LoadBasicDCEntry();
                }



                //#
                //## Setup Form
                //#
                echo("\nFinished!");
                StatusLabelMammet(new[] { "Finished Loading dc File", string.Empty, string.Empty });

                ReloadButtonMammet(true);
                AbortButtonMammet(1);


                PropertiesPanelMammet(ActiveFileName, DCEntries);
            }
            // File in 
            catch (IOException) {
                echo($"\nERROR: Selected File is Being Used by Another Process.");
                StatusLabelMammet(new[] { "Error Loading dc File!!!", string.Empty, string.Empty });
            }
            catch (ThreadAbortException) {
                StatusLabelMammet(new[] { "DC Parse Aborted", string.Empty, string.Empty });
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
        private static object LoadDCStructByType(byte[] binFile, string Type, long Address, string Name = null)
        {
            if (Name == null || Name.Length < 1)
            {
                Name = "unnamed";
            }



            switch (Type)
            {
                //#
                //## Mapped Structures
                //#
                // map == [ struct len, sid[]* ids, struct*[] * data ]
                case "map":                         return new DCMapDef(binFile, Address, Name);

                case "weapon-gameplay-def":         return new WeaponGameplayDef(binFile, Address, Name);

                case "melee-weapon-gameplay-def":   return new MeleeWeaponGameplayDef(binFile, Address, Name);

                case "symbol-array":                return new SymbolArrayDef(binFile, Address, Name);

                case "ammo-to-weapon-array":        return new AmmoToWeaponArray(binFile, Address, Name);


                    
                //#
                //## Unmapped Structures
                //#
                default: return new UnknownStruct(Type, Address, Name);
            }
        }

        private static object LoadBasicDCEntry()
        {
            return null;
        }
        #endregion [Function Declarations]
    }
}
