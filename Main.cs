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
            


                // Read file magic from header
                if (!binFile.Take(8).ToArray().SequenceEqual(new byte[] { 0x30, 0x30, 0x43, 0x44, 0x01, 0x00, 0x00, 0x00 }))
                {
                    Console.WriteLine($"Invalid File Provided: Invalid file magic.");
                    return;
                }
                
                ActiveForm.Invoke(abortButtonMammet, new object[] { true });
                ActiveForm.Invoke(reloadButtonMammet, new object[] { true });


                DCHeader = new DCFileHeader(binName, binFile);

                if (abort) {
                    goto yeet;
                }

                // Parse symbol table or whatever
                int addr = 0x20;
                var eugh = DecodeSIDHash(GetSubArray(binFile, addr));
                if (eugh != "array")
                {
                    Console.WriteLine($"Unexpected SID \"{eugh}\" at {addr:X}, Aborting.");
                    return;
                }
            
                var activeLabel = binName + "; Reading Script...";
                CTUpdateLabel(activeLabel);
                InitializeDcStructListsByScriptName(binName);

                echo ($"Reading the whatever it's called. (length: {DCHeader.TableLength})");
                if (abort) {
                    goto yeet;
                }
                
                
        #if DEBUG
                var pre = DateTime.Now.ToString();
        #endif
                long address;

                string name;
                string type;

                if (abort) {
                    goto yeet;
                }
                
                PrintNL($"Parsing DC Content Table (Length: {DCHeader.TableLength.ToString().PadLeft(2, '0')})");


                for (int tableIndex = 0; tableIndex < DCHeader.TableLength && !abort; tableIndex++)
                {
                    CTUpdateLabel(activeLabel + $" ({tableIndex} / {DCHeader.TableLength})");
                
                    addr += 8;
                    name = DecodeSIDHash(GetSubArray(binFile, addr));
                
                    addr += 8;
                    type = DecodeSIDHash(GetSubArray(binFile, addr));
                
                    addr += 8;
                    address = BitConverter.ToInt64(GetSubArray(binFile, addr), 0);

                    PrintNL($"Item #{tableIndex}: [ Label: {name} Type: {type} Address: {address:X} ]");

                    switch (type)
                    {
                        // map == [ struct len, sid[]* ids, struct*[] * data ]
                        case "map":
                            var mapLength = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                            var mapSymbolArray = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);
                            long mapStructArray = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 16), 0);


                            PrintNL($"  Found Map: [ Length: {mapLength}; Symbol Array Address: 0x{mapSymbolArray:X}; Struct Array Address: 0x{mapStructArray:X} ]");
                            
                            for (int arrayIndex = 0, tst = 0; arrayIndex < mapLength && !abort; mapStructArray += 8, mapSymbolArray += 8, arrayIndex++)
                            {
                                PrintLL($"  Parsing Map Structures... {arrayIndex} / {mapLength - 1}");


                                var structAddr = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructArray), 0);
                                var structType = DecodeSIDHash(GetSubArray(binFile, structAddr - 8));

                                // Check for and parse known structures, or just print the basic data if it's not a handled structure
                                switch (structType)
                                {
                                    case "weapon-gameplay-def":
                                        weaponDefinitions.Add(new WeaponGameplayDef(DecodeSIDHash(GetSubArray(binFile, (int)mapSymbolArray)), binFile, structAddr));
                                        break;

                                    default:
                                        PrintNL($"   {structType} #{arrayIndex.ToString().PadLeft(2, '0')}: Struct Addr: 0x{structAddr.ToString("X").PadLeft(8, '0')} {DecodeSIDHash(GetSubArray(binFile, (int)mapSymbolArray))}");
                                        break;
                                }
                            }

                            //! Print Parsed Data
                            if (weaponDefinitions.Count > 0)
                            {
                                var ext = "  ->  ";
                                foreach (var newDef in weaponDefinitions)
                                {
                                    PrintNL($"{ext}0x{newDef.Address:X} {newDef.Name}; Is Firearm?: {newDef.IsFirearm}");

                                    if (newDef.IsFirearm)
                                    {
                                        PrintNL($"{ext}Ammo Count: {newDef.AmmoCount}");
                                    }
                                }
                            }
                            PrintNL("");
                            break;

                        case "symbol-array":
                            //break;
                            var sArrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                            var sArrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);

                            PrintNL($"  Item Details: [ Length: {sArrayLen} ]");
                            for (int i = 0; i < sArrayLen && !abort; sArrayAddr += 8, i++)
                            {
                                PrintNL($"  Symbol: {DecodeSIDHash(GetSubArray(binFile, (int)sArrayAddr))}");
                            }
                            PrintNL("");
                            break;
                        case "ammo-to-weapon-array":
                            //break;
                            var atwArrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                            var atwArrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);

                            PrintNL($"  Item Details: [ Length: {atwArrayLen} ]");
                            for (int i = 0; i < atwArrayLen && !abort; atwArrayAddr += 16, i++)
                            {
                                PrintNL($" Weapon: {DecodeSIDHash(GetSubArray(binFile, (int)atwArrayAddr + 8))} Ammo Type: {DecodeSIDHash(GetSubArray(binFile, (int)atwArrayAddr))}");
                            }
                            PrintNL("");
                            break;
                    }
                }

                #if DEBUG
                var post = DateTime.Now.ToString();
                echo (pre);
                echo (post);
                #endif
                
                yeet:
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
