using System;
using System.ComponentModel.Design;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static weapon_data.Common;

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

            binPathTextBox.TextChanged += (sender, _) => BinPathBox_Set(((TextBox)sender).Text);

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
            else if (File.Exists($"{Directory.GetCurrentDirectory()}\\sid1\\sidbase.bin"))
            {
                sidbase = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}\\sid1\\sidbase.bin");
            }
            else
                sidbase = File.ReadAllBytes(@"O:\Modding\The Last of Us Part II\sid\sidbase.bin");

            #if DEBUG
            #endif
        }




        /// <summary>
        /// Use the Buffer class to copy and return a smaller sub-array from a provided <paramref name="array"/>.
        /// </summary>
        /// <param name="array"> The larger array from which to take the sub-array returned. </param>
        /// <param name="index"> The start index in <paramref name="array"/> from which the copying starts. </param>
        /// <param name="len"> The length of the sub-array to be returned. Defaults to 8 bytes. </param>
        /// <returns> A byte array of 8 bytes (or an optional different length) copied from the specified <paramref name="index"/> in <paramref name="array"/>. </returns>
        public static byte[] GetSubArray(byte[] array, int index, int len = 8)
        {
            var ret = new byte[8];
            Buffer.BlockCopy(array, index, ret, 0, len);

            return ret;
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

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            using (var Browser = new OpenFileDialog { Title = "Please select a non-state-script from \"bin/dc1\"." })
            {
                if (Browser.ShowDialog() == DialogResult.OK)
                {
                    binPathTextBox.Set(Browser.FileName);
                }
            }
        }
        
        private void BinPathBox_Set(string text)
        {
            if (File.Exists(text))
            {
                if (binThread != null && binThread.ThreadState != ThreadState.Unstarted)
                {
                    try {
                        binThread.Abort();
                    }
                    catch (ThreadAbortException) { echo("Bin Thread Killed\n"); }
                    catch (Exception dang) { echo($"Unexpected error of type \"{dang.GetType()}\" thrown when aborting binThread.\n"); }
                }


                binThread = new Thread(new ParameterizedThreadStart(LoadBinFile));
                binThread.Start(text);
            }
        }

        private void abortBtn_Click(object sender, EventArgs e)
        {
            abort = true;
            echo("Bin Thread Killed\n");
        }

        #endregion




        //==================================\\
        //--|   Function Delcarations   |---\\
        //==================================\\
        #region [Function Delcarations]
        private Thread binThread;
        private void LoadBinFile(object pathObj)
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
                
                ActiveForm.Invoke(buttonMammet, new object[] { true });

                // Read additional header info
                endPointer = BitConverter.ToInt64(binFile, 0x8);
                unkInteger0 = BitConverter.ToInt32(binFile, 0x10);
                tableLength = BitConverter.ToInt32(binFile, 0x14);
                unkInteger1 = BitConverter.ToInt64(binFile, 0x18);

                if (abort) {
                    goto yeet;
                }

                // Parse symbol table or whatever
                int addr = 0x20;
                if (DecodeSIDHash(GetSubArray(binFile, addr)) != "array")
                {
                    Console.WriteLine($"Unexpected SID \"{DecodeSIDHash(GetSubArray(binFile, addr))}\" at {addr:X}, Aborting.");
                    return;
                }
            
                var activeLabel = binName + "; Reading Script...";
                CTUpdateLabel(activeLabel);
                InitializeDcStructListsByScriptName(binName);

                echo ($"Reading the whatever it's called. (length: {tableLength})");
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
                
                PrintNL($"Parsing DC Content Table (Length: {tableLength.ToString().PadLeft(2, '0')})");

                for (int tableIndex = 0; tableIndex < tableLength && !abort; tableIndex++)
                {
                    CTUpdateLabel(activeLabel + $" ({tableIndex} / {tableLength})");
                
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
                            
                            for (int arrayIndex = 0; arrayIndex < mapLength && !abort; mapStructArray += 8, mapSymbolArray += 8, arrayIndex++)
                            {
                                Venat?.Invoke(outputMammetSameLine, new object[] { $"  Parsing Map Structures... {arrayIndex} / {mapLength - 1}" });
                                PrintLL($"  Parsing Map Structures... {arrayIndex} / {mapLength - 1}");
                                var structAddr = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructArray), 0);
                                var structType = DecodeSIDHash(GetSubArray(binFile, structAddr - 8));

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
                            if (weaponDefinitions.Count > 0)
                            {
                                foreach(var newDef in weaponDefinitions)
                                    PrintNL($"{newDef.GameplayDefinitionType}:{newDef.Name} Ammo Count: {newDef.AmmoCount}");
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
                Venat.Invoke(buttonMammet, new object[] { false });

                abort = false;
            }
            catch(ThreadAbortException){}
        }
        #endregion
    }
}
