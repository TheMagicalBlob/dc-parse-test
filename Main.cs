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

            activeScriptLabel = ActiveScriptLabel;
            abortBtn = AbortOrCloseBtn;
            OutputWindow = PropertiesWindowRichTextBox;

            BaseAbortButtonWidth = abortBtn.Size.Width;
            
            propertiesPanelMammet = new PropertiesPanelWand(PopulatePropertiesPanel);
        }


        


        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

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

        
        private List<object[]> shownControls;
        private void debugShowAllBtn_Click(object sender, EventArgs e)
        {
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
                    ((Control)shownControls[i][0]).Visible = (shownControls[1][1] as bool[])[0];
                    ((Control)shownControls[i][0]).Enabled = (shownControls[1][1] as bool[])[1];
                }

                shownControls = null;
            }
        }

        private void redirectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            #if DEBUG
            redirect = ((CheckBox)sender).Checked;
            #endif
        }

        private void bleghBtn_Click(object sender, EventArgs e)
        {
            AbortButtonMammet(!abortBtn.Enabled);
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
            try
            {
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

                var outputLine = Venat?.GetOutputWindowLines().Length - 1 ?? 0;

                for (int tableIndex = 0, addr = 0x28; tableIndex < DCHeader.HeaderItems.Length; tableIndex++, addr += 24)
                {
                    Venat?.CTUpdateLabel(ActiveLabel + $" ({tableIndex} / {DCHeader.TableLength})");

                    PrintLL($"Item #{tableIndex}: {DCHeader.HeaderItems[tableIndex].Name}", outputLine);
                    echo($"Item #{tableIndex}: [ Label: {DCHeader.HeaderItems[tableIndex].Name} Type: {DCHeader.HeaderItems[tableIndex].Type} Data Address: {DCHeader.HeaderItems[tableIndex].StructAddress:X} ]");

                    DCEntries[tableIndex] = LoadDCStructByType(DCFile, DCHeader.HeaderItems[tableIndex].Type, (int)DCHeader.HeaderItems[tableIndex].StructAddress, DCHeader.HeaderItems[tableIndex].Name);
                }




                //#
                //## Setup Form
                //#

                PrintNL("\nFinished!");
                CTUpdateLabel(ActiveFileName + " Finished Loading dc File");

                ReloadButtonMammet(true);
                AbortButtonMammet(1);


                PropertiesPanelMammet(ActiveFileName, DCEntries);
            }
            catch (IOException) {
                PrintNL($"\nERROR: Selected File is Being Used by Another Process.");
                CTUpdateLabel(ActiveFileName + " Error Loading dc File!!!");
                AbortButtonMammet(false, 0);
            }
            catch (ThreadAbortException) {
                AbortButtonMammet(false, 0);
            }
            # if !DEBUG
            catch (Exception fuck) {
                PrintNL($"\nAn Unexpected {fuck.GetType()} Occured While Attempting to Parse the DC File.");
                MessageBox.Show($"An Unexpected {fuck.GetType()} Occured While");
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

                case "symbol-array":                return new SymbolArrayDef(Name, binFile, Address);

                case "ammo-to-weapon-array":        return new AmmoToWeaponArray(binFile, Address, Name);


                    
                //#
                //## Unmapped Structures
                //#
                default: return $"Unknown Structure: {Type}\n    Struct Addr: 0x{Address.ToString("X").PadLeft(8, '0')}\n    Struct Name: {Name}";
            }
        }

        public static void ReloadButtonMammet(bool enabled)
        {
            Venat?.Invoke(Venat.reloadButtonMammet, new[] { enabled });
        }

        public static void AbortButtonMammet(params object[] args)
        {
            Venat?.Invoke(Venat.abortButtonMammet, new[] { args ?? new object[] { false, 0 } });
        }

        public static void PropertiesPanelMammet(string dcFileName, object[] dcEntries)
        {
            Venat?.Invoke(Venat.propertiesPanelMammet, new object [] { dcFileName, dcEntries });
        }

        private static void CloseBinFile()
        {
            DCFile = null;
            Emmet.Controls.Clear();
        }

        private void debugDisableLinesBtn_CheckedChanged(object sender, EventArgs e)
        {
            noDraw ^= true;
            CreateGraphics().Clear(BackColor);
            Refresh();
        }
        #endregion
    }
}
