using System;
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
        }

        
        
        private void debugShowAllBtn_Click(object sender, EventArgs e)
        {
            foreach (Control control in Controls)
            {
                control.Visible = true;
            }

            if (Azem != null)
            {
                foreach (Control control in Azem.Controls)
                {
                    control.Visible = true;
                }
            }

            if (Emmet != null)
            {
                foreach (Control control in Emmet.Controls)
                {
                    control.Visible = true;
                }
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
            echo("print");
            Venat?.Invoke(abortButtonMammet, new object[] { null });
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
        ///  //! Write Me
        /// </summary>
        /// <param name="pathObj"> The string object containing the path to the .bin file to be parsed. </param>
        private void ParseBinFile(object pathObj)
        {
            var binPath = pathObj?.ToString() ?? "null";
            try {
                //#
                //## Load provided DC file.
                //#
                try {
                    DCFile = File.ReadAllBytes(binPath);
                }
                catch(IOException) {
                    PrintNL($"\nERROR: Selected File is Being Used by Another Process.");
                    goto error;
                }
                ActiveFileName = binPath.Substring(binPath.LastIndexOf('\\') + 1);

                
                //#
                //## Parse provided DC file.
                //#
                Venat?.Invoke(abortButtonMammet, new object[] { true });
            
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

                Venat?.Invoke(reloadButtonMammet, new object[] { true });
                Venat?.Invoke(abortButtonMammet, new object[] { 1 });
                return;

                error:;
                CTUpdateLabel(ActiveFileName + " Error Loading dc File!!!");
                Venat?.Invoke(abortButtonMammet, new object[] { false });
            }
            catch(ThreadAbortException) {
                Venat?.Invoke(abortButtonMammet, new object[] { 0 });
                Venat?.Invoke(abortButtonMammet, new object[] { false });
            }
            catch (Exception fuck) {
                PrintNL($"\nAn Unexpected {fuck.GetType()} Occured While Attempting to Parse the DC File.");
                MessageBox.Show($"An Unexpected {fuck.GetType()} Occured While");
            }
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
        private static object LoadDCStructByType(byte[] binFile, string Type, long Address, string Name = null, bool silent = false)
        {
            if (Name == null || Name.Length < 1)
            {
                Name = "unnamed";
            }
            if (!silent)
            {
                Venat?.PrintLL($"  # [{Type}]: {{ Struct Address: 0x{Address.ToString("X").PadLeft(8, '0')}; DC Size: 0x{binFile.Length.ToString("X").PadLeft(8, '0')}; Name: {Name} }}", Venat?.GetOutputWindowLines().Length - 1 ?? 1);
            }


            object ret = null;



            switch (Type)
            {
                //#
                //## Mapped Structures
                //#
                // map == [ struct len, sid[]* ids, struct*[] * data ]
                case "map":
                    ret = new DCMapDef(binFile, Type, Address);
                    break;

                case "weapon-gameplay-def":
                    ret = new WeaponGameplayDef(binFile, Address, Name);
                    break;
                case "melee-weapon-gameplay-def":
                    break;
                    //ret = new MeleeWeaponGameplayDef(Name, binFile, Address);

                case "symbol-array":
                    break;
                    ret = new SymbolArrayDef(Name, binFile, Address);


                case "ammo-to-weapon-array":
                    break;
                    ret = new AmmoToWeaponArray(binFile, Address, Name);


                    
                //#
                //## Unmapped Structures
                //#
                default:
                    ret = $"Unknown Structure: {Type}\n    Struct Addr: 0x{Address.ToString("X").PadLeft(8, '0')}\n    Struct Name: {Name}";
                    break;
            }


            //var returnType = ret.GetType();
            //echo($"Returning {returnType} {(returnType != typeof(string) ? ((dynamic)ret).Name : "string")}");
            return ret;
        }

        private static void CloseBinFile()
        {
            DCFile = null;
            Emmet.Controls.Clear();
        }
        #endregion
    }
}
