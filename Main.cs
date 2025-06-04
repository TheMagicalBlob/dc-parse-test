using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace weapon_data
{
    public partial class Main: Form
    {
        public Main()
        {
            InitializeComponent();
            CreateBrowseModeDropdownMenu();
            InitializeAdditionalEventHandlers();

            Venat = this;

            binPathTextBox.TextChanged += (sender, _) => CheckbinPathBoxText(((TextBox)sender).Text);

            Update();
            Refresh();

            Azem = new OptionsPage();
            Update();
            Refresh();
            activeScriptLabel = ActiveScriptLabel;
            abortBtn = AbortBtn;
            _OutputWindow = richTextBox1;

            if (File.Exists($"{Directory.GetCurrentDirectory()}\\sidbase.bin"))
            {
                sidbase = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\sidbase.bin");
            }
            else if (File.Exists($"{Directory.GetCurrentDirectory()}\\sid\\sidbase.bin"))
            {
                sidbase = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\sid\\sidbase.bin");
            }
            
            else if (File.Exists($"{Directory.GetCurrentDirectory()}\\sid1\\sidbase.bin"))
            {
                sidbase = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\sid1\\sidbase.bin");
            }
        }


        


        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void OptionsMenuDropdownBtn_Click(object sender, EventArgs e)
        {
            Azem.Visible ^= true;
		    Azem.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width)/2, Venat.Location.Y + 80);
            Azem.Update();

            DropdownMenu[1].Visible = DropdownMenu[0].Visible = false;
        }

        private void BinPathBrowseBtn_Click(object sender, EventArgs e)
        {
            using (var Browser = new OpenFileDialog {
                Title = "Please select a non-state-script from \"bin/dc1\"."
            })
            {
                if (Browser.ShowDialog() == DialogResult.OK)
                {
                    binPathTextBox.Set(Browser.FileName);
                }
            }
        }
        
        private void ReloadBinFile(object sender, EventArgs e)
        {
            if (File.Exists(binPathTextBox.Text))
            {
                LoadBinFile(binPathTextBox.Text);
            }
        }
        
        private void CheckbinPathBoxText(string boxText)
        {
            if (File.Exists(boxText))
            {
                LoadBinFile(boxText);
            }
        }

        private void AbortBinFileParse(object sender, EventArgs e)
        {
            abort = true;
            echo("Bin Thread Killed\n");
        }
        
        private void ClearBtn_Click(object sender, EventArgs e) => _OutputWindow.Clear();
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
        private void ParseBinFile(object pathObj)
        {
            try
            {
                var binPath = pathObj as string;

                var binFile = File.ReadAllBytes(binPath);
                var binName = binPath.Substring(binPath.LastIndexOf('\\') + 1);
            
                ActiveForm.Invoke(abortButtonMammet, new object[] { true });

                
                DCHeader = new DCFileHeader(binName, binFile);

                if (abort) {
                    goto yeet;
                }

                
                for (int tableIndex = 0, addr = 0x28; tableIndex < DCHeader.Structures.Length && !Venat.abort; tableIndex++, addr += 24)
                {
                    Venat.CTUpdateLabel(ActiveLabel + $" ({tableIndex} / {DCHeader.TableLength})");
                    PrintNL($"Item #{tableIndex}: [ Label: {DCHeader.Structures[tableIndex].Name} Type: {DCHeader.Structures[tableIndex].Type} Data Address: {DCHeader.Structures[tableIndex].Pointer:X} ]");
                
                    switch (DCHeader.Structures[tableIndex].Type)
                    {
                        // map == [ struct len, sid[]* ids, struct*[] * data ]
                        case "map":
                            var mapLength = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCHeader.Structures[tableIndex].Pointer), 0);
                            var mapSymbolArray = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCHeader.Structures[tableIndex].Pointer + 8), 0);
                            long mapStructArray = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCHeader.Structures[tableIndex].Pointer + 16), 0);


                            PrintNL($"  Found Map: [ Length: {mapLength}; Symbol Array Address: 0x{mapSymbolArray:X}; Struct Array Address: 0x{mapStructArray:X} ]");
                            
                            for (int arrayIndex = 0, pad = mapLength.ToString().Length; arrayIndex < mapLength && !Venat.abort; mapStructArray += 8, mapSymbolArray += 8, arrayIndex++)
                            {
                                PrintLL($"  Parsing Map Structures... {arrayIndex} / {mapLength - 1}");


                                var structAddr = (int)BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)mapStructArray), 0);
                                var structType = Venat.DecodeSIDHash(Venat.GetSubArray(binFile, structAddr - 8));

                                // Check for and parse known DCHeader.Structures, or just print the basic data if it's not a handled structure
                                switch (structType)
                                {
                                    case "weapon-gameplay-def":
                                        Venat.WeaponDefinitions.Add(new WeaponGameplayDef(Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)mapSymbolArray)), binFile, structAddr));
                                        break;
                                        case "melee-weapon-gameplay-def":
                                            break;

                                    default:
                                        Venat.UnknownDefinitions.Add($"Unknown Structure #{arrayIndex.ToString().PadLeft(pad, '0')}: {structType}\n    Struct Addr: 0x{structAddr.ToString("X").PadLeft(8, '0')}\n    Struct Name: {Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)mapSymbolArray))}");
                                        break;
                                }
                            }
                            PrintNL();

                            //! Print Parsed Data
                            if (Venat.WeaponDefinitions.Count > 0)
                            {
                                var ext = "  ->  ";
                                foreach (var newDef in Venat.WeaponDefinitions)
                                {
                                    PrintNL($"{ext}Weapon Definition @: 0x{newDef.Address:X} {newDef.Name}");


                                    PrintNL($"{ext}Is Firearm?: {newDef.IsFirearm}");

                                    if (newDef.IsFirearm)
                                    {
                                        PrintNL($"{ext}Ammo Count: {newDef.AmmoCount}");
                                    }

                                    PrintNL($"{ext}Reticle: {newDef.Hud2ReticleName}");
                                    PrintNL($"{ext}SimpleReticle: {newDef.Hud2SimpleReticleName}");
                                
                                    PrintNL();
                                }
                                PrintNL();
                            }
                        
                            //! Print unknown remaining DCHeader.Structures
                            if (Venat.UnknownDefinitions.Count > 0)
                            {
                                var ext = "  ->  ";
                                foreach (var newDef in Venat.UnknownDefinitions)
                                {
                                    PrintNL(ext + newDef);
                                }
                                PrintNL();
                            }

                            PrintNL("");
                            break;


                        case "symbol-array":
                        break;
                            var sArrayLen = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCHeader.Structures[tableIndex].Pointer), 0);
                            var sArrayAddr = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCHeader.Structures[tableIndex].Pointer + 8), 0);

                            PrintNL($"  Item Details: [ Length: {sArrayLen} ]");
                            for (int i = 0; i < sArrayLen && !Venat.abort; sArrayAddr += 8, i++)
                            {
                                PrintNL($"  Symbol: {Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)sArrayAddr))}");
                            }
                            PrintNL("");
                            break;


                        case "ammo-to-weapon-array":
                        break;
                            var atwArrayLen = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCHeader.Structures[tableIndex].Pointer), 0);
                            var atwArrayAddr = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCHeader.Structures[tableIndex].Pointer + 8), 0);

                            PrintNL($"  Item Details: [ Length: {atwArrayLen} ]");
                            for (int i = 0; i < atwArrayLen && !Venat.abort; atwArrayAddr += 16, i++)
                            {
                                PrintNL($" Weapon: {Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)atwArrayAddr + 8))} Ammo Type: {Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)atwArrayAddr))}");
                            }
                            PrintNL("");
                            break;
                    }
                }

                ActiveForm.Invoke(reloadButtonMammet, new object[] { true });

                

                //#
                //## Do shit
                //#



                yeet:
                PrintNL("Finished!");
                CTUpdateLabel(binName + " Finished Loading dc File");
                Venat.Invoke(abortButtonMammet, new object[] { false });

                abort = false;
            }
            catch(ThreadAbortException) {
                ActiveForm.Invoke(abortButtonMammet, new object[] { false });
            }
        }
        #endregion
    }
}
