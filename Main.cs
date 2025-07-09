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
            InitializeAdditionalEventHandlers();
            OutputWindow.KeyDown += (sender, arg) => //!
            {
                if (arg.KeyData == Keys.Escape)
                {
                    Focus();
                }
            };


            Update(); Refresh();
            Venat = this;
            Azem = new OptionsPage();
            Emmet = PropertiesPanel;
            Update(); Refresh();



            if (File.Exists($"{Directory.GetCurrentDirectory()}\\sidbase.bin"))
            {
                SIDBase = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\sidbase.bin");
            }
            else if (File.Exists($"{Directory.GetCurrentDirectory()}\\sid\\sidbase.bin"))
            {
                SIDBase = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\sid\\sidbase.bin");
            }
            
            else if (File.Exists($"{Directory.GetCurrentDirectory()}\\sid1\\sidbase.bin"))
            {
                SIDBase = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\sid1\\sidbase.bin");
            }

            
            VersionLabel.Text = "Ver." + Version;

            ActiveFileName = "No Script Selected";

            scriptStatusLabel = ScriptStatusLabel;
            scriptSelectionLabel = ScriptSelectionLabel;
            abortBtn = AbortOrCloseBtn;
            OutputWindow = PropertiesWindowRichTextBox;

            BaseAbortButtonWidth = abortBtn.Size.Width;
            
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
                    if ((int)Selection.Tag == HeaderItemButtons.Count - 1)
                    {
                        HeaderItemButtons[0].Focus();
                    }
                    else {
                        HeaderItemButtons[(int)Selection.Tag + 1].Focus();
                    }
                break;

                case Keys.Up:
                    if ((int)Selection.Tag == 0)
                    {
                        HeaderItemButtons[HeaderItemButtons.Count - 1].Focus();
                    }
                    else {
                        HeaderItemButtons[(int)Selection.Tag - 1].Focus();
                    }
                break;


                #if DEBUG
                default:
                    echo($"Misc Input Recieved: [{arg}]");
                break;
                #endif
            }
            if (Selection == null && (HeaderItemButtons?.Any() ?? false))
            {
                HeaderItemButtons[arg == Keys.Down ? 0 : HeaderItemButtons.Count - 1].Focus();
            }
            else {
                if (arg == Keys.Down)
                {
       
                }
                else if (arg == Keys.Up)
                {
                    if ((int)Selection.Tag == 0)
                    {
                        HeaderItemButtons[HeaderItemButtons.Count - 1].Focus();
                    }
                    else {
                        HeaderItemButtons[(int)Selection.Tag - 1].Focus();
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
                binPathTextBox.Set(Browser.FileName);
            }
        }
        

        private void ChoosePropertyBtn_Click(object sender, EventArgs e)
        {
            if (Emmet != null)
            {
                Emmet.Visible ^= true;
		        Emmet.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
                Emmet.Update();
            }
        }

        private void OptionsMenuDropdownBtn_Click(object sender, EventArgs e)
        {
            if (Emmet != null)
            {
                Azem.Visible ^= true;
		        Azem.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
                Azem.Update();
            }
        }
        
        private void ReloadBinFile(object sender, EventArgs e)
        {
            if (File.Exists(binPathTextBox.Text))
            {
                LoadBinFile(binPathTextBox.Text);
            }
        }
        
        private void CheckbinPathBoxText(object sender, EventArgs _)
        {
            var boxText = ((TextBox)sender).Text;
            if (File.Exists(boxText))
            {
                LoadBinFile(boxText);
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
            }

            AbortButtonMammet(0, false);
            ReloadButtonMammet(false);
        }

        #if DEBUG
        private List<object[]> shownControls;
        #endif
        private void debugShowAllBtn_Click(object sender, EventArgs e)
        {
            #if DEBUG
            if (shownControls == null)
            {
                shownControls = new List<object[]>();
                
                foreach (Control control in Controls)
                {
                    if (!control.Visible || !control.Enabled)
                    {
                        shownControls.Add(new object[] { control, new[] { control.Visible, control.Enabled } });
                    }
                    control.Visible = true;
                    control.Enabled = true;
                }

                if (Azem != null)
                {
                    foreach (Control control in Azem.Controls)
                    {
                        if (!control.Visible || !control.Enabled)
                        {
                            shownControls.Add(new object[] { control, new[] { control.Visible, control.Enabled } });
                        }
                        control.Visible = true;
                        control.Enabled = true;
                    }
                }

                if (Emmet != null)
                {
                    foreach (Control control in Emmet.Controls)
                    {
                        if (!control.Visible || !control.Enabled)
                        {
                            shownControls.Add(new object[] { control, new[] { control.Visible, control.Enabled } });
                        }
                        control.Visible = true;
                        control.Enabled = true;
                    }
                }
            }
            else {
                for (int i = 0; i < shownControls.Count; i++)
                {
                    ((Control)shownControls[i][0]).Visible = (shownControls[i][1] as bool[])[0];
                    ((Control)shownControls[i][0]).Enabled = (shownControls[i][1] as bool[])[1];
                }

                shownControls = null;
            }
            #endif
        }


        private void debugMiscBtn_Click(object sender, EventArgs e)
        {
            var ffs = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            foreach (var item in ffs)
            {
                echo($"[{item}]: {(byte) item} => {(byte) item.ToString().ToUpper() [0]}");
            }

            for(byte beh = 90; beh < 98; beh++)
            {
                echo($"{(char)beh}");
            }
            echo((char)123);
        }

        
        private void debugDisableLinesBtn_CheckedChanged(object sender, EventArgs e)
        {
            #if DEBUG
            noDraw ^= true;
            CreateGraphics().Clear(BackColor);
            Refresh();
            #endif
        }
        

        private void debugTabCheckBtn_Click(object sender, EventArgs e)
        {
            void eh(System.Windows.Forms.Control.ControlCollection controls)
            {
                foreach (Control cunt in controls)
                {
                    if (cunt.HasChildren)
                    {
                        eh (cunt.Controls);
                    }
                    PrintNL($"# [{cunt.Name}: {cunt.TabIndex}]");
                }

            }

            eh(this.Controls);
        }

        private void debugLineTestBtn_Click(object sender, EventArgs e)
        {
            PrintLL(debugLineTestTextBox.Text, (int)debugLineTestIntBox.Value);
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
                    binThread.Abort();
                }
                catch (ThreadAbortException) { echo("Bin Thread Killed\n"); }
                catch (Exception dang) { echo($"Unexpected error of type \"{dang.GetType()}\" thrown when aborting binThread.\n"); }
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
            
            try {
                //#
                //## Load provided DC file.
                //#
                DCFile = File.ReadAllBytes(binPath);
                ActiveFileName = binPath.Substring(binPath.LastIndexOf('\\') + 1);


                //#
                //## Parse provided DC file.
                //#
                AbortButtonMammet(true);

                DCHeader = new DCFileHeader(DCFile, ActiveFileName);
                DCEntries = new object[DCHeader.TableLength];

                for (int fuck = 0, sake = 0x28; fuck < DCHeader.HeaderItems.Length; fuck++, sake += 24)
                {
                    Venat?.CTUpdateStatusLabel($" ({fuck} / {DCHeader.TableLength})");

                    echo($"Item #{fuck}: [ Label: {DCHeader.HeaderItems[fuck].Name} Type: {DCHeader.HeaderItems[fuck].Type} Data Address: {DCHeader.HeaderItems[fuck].StructAddress:X} ]");

                    DCEntries[fuck] = LoadDCStructByType(DCFile, DCHeader.HeaderItems[fuck].Type, (int)DCHeader.HeaderItems[fuck].StructAddress, DCHeader.HeaderItems[fuck].Name);
                }




                //#
                //## Setup Form
                //#

                echo("\nFinished!");
                CTUpdateStatusLabel(ActiveFileName + " Finished Loading dc File");

                ReloadButtonMammet(true);
                AbortButtonMammet(1);


                PropertiesPanelMammet(ActiveFileName, DCEntries);
            }
            // File in 
            catch (IOException) {
                echo($"\nERROR: Selected File is Being Used by Another Process.");
                CTUpdateStatusLabel(ActiveFileName + " Error Loading dc File!!!");
                AbortButtonMammet(false, 0);
            }
            catch (ThreadAbortException) {
                AbortButtonMammet(false, 0);
            }
            # if !DEBUG
            catch (Exception fuck) {
                echo($"\nAn Unexpected {fuck.GetType()} Occured While Attempting to Parse the DC File.");
                MessageBox.Show($"An unexpected {fuck.GetType()} occured while parsing the provided DC .bin file");
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

        private static void CloseBinFile()
        {
            DCFile = null;
            Emmet.Controls.Clear();
            OutputWindow.Clear();

            if (Venat != null)
            {
                Venat.HeaderItemButtons?.Clear();
                Venat.HeaderItemButtons = null;
                Venat.Selection = null;

                Venat.optionsMenuDropdownBtn.TabIndex -= DCEntries.Length;
                Venat.MinimizeBtn.TabIndex -= DCEntries.Length;
                Venat.ExitBtn.TabIndex -= DCEntries.Length;
            }
        }




        //#
        //## Mammet Shorthand Function Declarations
        //#
        #region [mammet shorthand function declarations]

        public static void ReloadButtonMammet(bool enabled)
        {
            Venat?.Invoke(Venat.reloadButtonMammet, new[] { new object[] { enabled } });
        }

        public static void AbortButtonMammet(params object[] args)
        {
            Venat?.Invoke(Venat.abortButtonMammet, new[] { args ?? new object[] { false, 0 } });
        }

        public static void PropertiesPanelMammet(object dcFileName, object[] dcEntries)
        {
            Venat?.Invoke(Venat.propertiesPanelMammet, new[] { dcFileName, dcEntries });
        }
        
        
        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="status">  </param>
        /// <param name="subStatus1">  </param>
        /// <param name="subStatus2">  </param>
        public void CTUpdateStatusLabel(object status = null, object subStatus1 = null, object subStatus2 = null)
        {
            Venat.Invoke(statusLabelMammet, new [] { new object[] { status, subStatus1, subStatus2 } });
        }

        
        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="str"> The string to update the label's text with. </param>
        public static void UpdateStatusLabel(string str)
        {
            scriptStatusLabel.Text = $"Selected Script: {ActiveFileName} {str}";
        }


        /// <summary>
        /// Update the yellow status/info label from a different thread through the statusLabelMammet
        /// </summary>
        /// <param name="status">  </param>
        /// <param name="subStatus1">  </param>
        /// <param name="subStatus2">  </param>
        public void CTUpdateSelectionLabel(object status = null, object subStatus1 = null, object subStatus2 = null)
        {
            //Venat.Invoke(selectionLabelMammet, new [] { new object[] { status, subStatus1, subStatus2 } });
        }

        
        /// <summary>
        /// Update the yellow status/info label with the provided string
        /// </summary>
        /// <param name="str"> The string to update the label's text with. </param>
        public static void UpdateSelectionLabel(string str)
        {
            scriptSelectionLabel.Text = $"Selected Script: {ActiveFileName}{str}";
        }
        #endregion

        #endregion [Function Declarations]
    }
}
