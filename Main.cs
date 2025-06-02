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

            sidbasePathTextBox.TextChanged += (sender, _) =>
            {
                if (File.Exists(((TextBox)sender).Text))
                {
                    sidbase = File.ReadAllBytes(((TextBox)sender).Text);
                }
            };
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

        private static void echo(object str = null)
        {
            Console.WriteLine(str);

            if (Azem.DebugOutputCheckBox.Checked)
            {
                Print(str);
            }
        }
        //private static string read () => Console.ReadLine();
        private static void exit() => Environment.Exit(0);

        private static void CTUpdateLabel(object str)
        {
            Venat?.Invoke(labelMammet, new object[] { str });
        }

        
        private static void UpdateLabel(object str)
        {
            activeScriptLabel.Text = "Selected Script: " + str;
        }

        

        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]

        private byte[] sidbase;

        private int  unkInteger0;
        private long unkInteger1;
        private long endPointer;
        private int  tableLength;
        private bool abort = false;
        private static Label activeScriptLabel;
        private static Button abortBtn;
        public delegate void binThreadFormWand(object obj); //! god I need to read about delegates lmao
        private static binThreadFormWand labelMammet = new binThreadFormWand(UpdateLabel);
        private static binThreadFormWand buttonMammet = new binThreadFormWand((obj) => { abortBtn.Visible = (bool)obj; });
        public static binThreadFormWand outputMammet = new binThreadFormWand((obj) => { _OutputWindow.AppendLine(obj.ToString()); });
        #endregion


        
        /// <summary>
        /// Post-InitializeComponent Configuration. <br/><br/>
        /// Create Assign Anonomous Event Handlers to Parent and Children.
        /// </summary>
        public void InitializeAdditionalEventHandlers()
        {
            MinimizeBtn.Click      += new EventHandler((sender, e) => ActiveForm.WindowState      = FormWindowState.Minimized     );
            MinimizeBtn.MouseEnter += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(90, 100, 255  ));
            MinimizeBtn.MouseLeave += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(0 , 0  , 0    ));
            ExitBtn.Click          += new EventHandler((sender, e) => Environment.Exit(                            0             ));
            ExitBtn.MouseEnter     += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(230, 100, 100 ));
            ExitBtn.MouseLeave     += new EventHandler((sender, e) => ((Control)sender).ForeColor = Color.FromArgb(0  , 0  , 0   ));


            // Set Event Handlers for Form Dragging
            MouseDown += new MouseEventHandler((sender, e) =>
            {
                MouseDif = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);

                MouseIsDown = true;
                DropdownMenu[1].Visible = DropdownMenu[0].Visible = false;
            });

            MouseUp   += new MouseEventHandler((sender, e) =>
            {
                MouseIsDown = false;

                if (OptionsPageIsOpen)
                    Azem?.BringToFront();
            });

            MouseMove += new MouseEventHandler((sender, e) => MoveForm());


            // Set appropriate event handlers for the controls on the form as well
            foreach (Control Item in Controls)
            {
                if (Item.Name == "SwapBrowseModeBtn") // lazy fix to avoid the mouse down event confliciting with the button
                    continue;
                
                Item.MouseDown += new MouseEventHandler((sender, e) => {
                    MouseDif = new Point(MousePosition.X - Venat.Location.X, MousePosition.Y - Venat.Location.Y);
                    MouseIsDown = true;
                    DropdownMenu[1].Visible = DropdownMenu[0].Visible = false;
                });
                Item.MouseUp   += new MouseEventHandler((sender, e) => { MouseIsDown = false; if (OptionsPageIsOpen) Azem?.BringToFront(); });
                
                // Avoid Applying MoveForm EventHandler to Text Containters (to retain the ability to drag-select text)
                if (Item.GetType() != typeof(TextBox) && Item.GetType() != typeof(RichTextBox))
                    Item.MouseMove += new MouseEventHandler((sender, e) => MoveForm());
            }

            Paint += PaintBorder;
        }

        

        private void OptionsMenuDropdownBtn_Click(object sender, EventArgs e)
        {
            Azem.Visible ^= true;
		    Azem.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width)/2, Venat.Location.Y + 80);
            Azem.Update();

            DropdownMenu[1].Visible = DropdownMenu[0].Visible = false;
        }

        
        /// <summary>
        /// Initialize Dropdown Menu Used for Toggling of Folder Browser Method
        /// </summary>
        private void CreateBrowseModeDropdownMenu()
        {
            var extalignment = BrowseBtn.Size.Height;
            var alignment = BrowseBtn.Location;

            var ButtonSize = new Size(BrowseBtn.Size.Width + optionsMenuDropdownBtn.Size.Width, 25);

            DropdownMenu[0] = new Button() {
                Font = new Font("Gadugi", 7.25f, FontStyle.Bold),
                Text = "Directory Tree*",
                BackColor = AppColour,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(alignment.X, alignment.Y + extalignment),
                Size = ButtonSize
            };
            DropdownMenu[1] = new Button() {
                Font = new Font("Gadugi", 7.25F, FontStyle.Bold),
                Text = "File Browser",
                BackColor = AppColour,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(alignment.X, alignment.Y + extalignment + DropdownMenu[0].Size.Height),
                Size = ButtonSize
            };



            // Create and Assign Event Handlers
            DropdownMenu[0].Click += (why, does) => {
                if (!LegacyFolderSelectionDialogue) {
                    DropdownMenu[0].Text += '*';
                    DropdownMenu[1].Text = DropdownMenu[1].Text.Remove(DropdownMenu[1].Text.Length-1);

                    LegacyFolderSelectionDialogue ^= true;
                }
            };
            DropdownMenu[1].Click += (my, back) => {
                if (LegacyFolderSelectionDialogue) {
                    DropdownMenu[1].Text += '*';
                    DropdownMenu[0].Text = DropdownMenu[0].Text.Remove(DropdownMenu[0].Text.Length-1);

                    LegacyFolderSelectionDialogue ^= true;
                }
            };



            // Add Controls to MainForm Control Collection
            Controls.Add(DropdownMenu[0]);
            Controls.Add(DropdownMenu[1]);

            // Ensure Controls Display Correctly
            DropdownMenu[0].Hide();
            DropdownMenu[1].Hide();
            DropdownMenu[0].BringToFront();
            DropdownMenu[1].BringToFront();
        }


        /// <summary>
        /// Use the Buffer class to copy and return a smaller sub-array from a provided <paramref name="array"/>.
        /// </summary>
        /// <param name="array"> The larger array from which to take the sub-array returned. </param>
        /// <param name="index"> The start index in <paramref name="array"/> from which the copying starts. </param>
        /// <param name="len"> The length of the sub-array to be returned. Defaults to 8 bytes. </param>
        /// <returns> A byte array of 8 bytes (or an optional different length) copied from the specified <paramref name="index"/> in <paramref name="array"/>. </returns>
        private static byte[] GetSubArray(byte[] array, int index, int len = 8)
        {
            var ret = new byte[8];
            Buffer.BlockCopy(array, index, ret, 0, len);

            return ret;
        }


        /// <summary>
        /// Parse the current sidbase.bin
        /// </summary>
        /// <param name="sidbase"></param>
        /// <param name="bytesToDecode"></param>
        /// <returns></returns>
        private static string DecodeSIDHash(byte[] sidbase, byte[] bytesToDecode)
        {
            if (bytesToDecode.Length != 8)
            {
                echo($"Invalid SID provided; unexpected length of \"{bytesToDecode?.Length ?? 0}\". Must be 8 bytes.");
                return "INVALID_SID";
            }

            for (long mainArrayIndex = 0, subArrayIndex = 0; mainArrayIndex < sidbase.Length; subArrayIndex = 0, mainArrayIndex++)
            {
                if (sidbase[mainArrayIndex] != (byte)bytesToDecode[subArrayIndex])
                {
                    continue;
                }


                // Scan for the rest of the bytes
                while ((subArrayIndex < 8 && mainArrayIndex < sidbase.Length) && sidbase[mainArrayIndex] == (byte)bytesToDecode[subArrayIndex]) // while (subArrayIndex < 8 && sidbase[mainArrayIndex++] == (byte)bytesToDecode[subArrayIndex++]) how the fuck does this behave differently?? I need sleep.
                {
                    mainArrayIndex++;
                    subArrayIndex++;
                }

                // continue if there was only a partial match
                if (subArrayIndex != 8)
                {
                    continue;
                }


                // Read the string pointer
                mainArrayIndex = BitConverter.ToInt64(sidbase, (int)mainArrayIndex);

                // Parse and add the string to the array
                var stringBuffer = string.Empty;
                while (sidbase[mainArrayIndex] != 0)
                {
                    stringBuffer += Encoding.UTF8.GetString(sidbase, (int)mainArrayIndex++, 1);
                }

                
                return stringBuffer;
            }

            return BitConverter.ToString(bytesToDecode).Replace("-", string.Empty);
        }


        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            #if DEBUG
            binPathTextBox.Set(@"O:\Modding\The Last of Us Part II\Script Editing\1.07\-weapon stuff\unedited\weapon-gameplay.bin");
            //binPathTextBox.Set(@"O:\Modding\The Last of Us Part II\Script Editing\1.07\-weapon stuff\unedited\weapon-upgrades.bin");
            //binPathTextBox.Set(@"O:\Modding\The Last of Us Part II\Script Editing\1.07\-weapon stuff\unedited\weapon-damages.bin");

#else
            using (var Browser = new OpenFileDialog { Title = "Please select a non-state-script from \"bin/dc1\"." })
            {
                if (Browser.ShowDialog() == DialogResult.OK)
                {
                    binPathTextBox.Set(Browser.FileName);
                }
            }
#endif
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
                var path = pathObj as string;

                var binFile = File.ReadAllBytes(path);
                var binName = path.Substring(path.LastIndexOf('\\') + 1);
            


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
                if (DecodeSIDHash(sidbase, GetSubArray(binFile, addr)) != "array")
                {
                    Console.WriteLine($"Unexpected SID \"{DecodeSIDHash(sidbase, GetSubArray(binFile, addr))}\" at {addr:X}, Aborting.");
                    return;
                }
            
                var activeLabel = binName + "; Reading Script...";
                CTUpdateLabel(activeLabel);
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
                for (int tableIndex = 0; tableIndex < tableLength && !abort; tableIndex++)
                {
                    CTUpdateLabel(activeLabel + $" ({tableIndex} / {tableLength})");
                
                    addr += 8;
                    name = DecodeSIDHash(sidbase, GetSubArray(binFile, addr));
                
                    addr += 8;
                    type = DecodeSIDHash(sidbase, GetSubArray(binFile, addr));
                
                    addr += 8;
                    address = BitConverter.ToInt64(GetSubArray(binFile, addr), 0);

                    Print($"Item #{tableIndex}: [ Label: {name} Type: {type} Address: {address:X} ]");

                    switch (type)
                    {
                            
                        // map == [ struct len, struct ids, struct data ]
                        case "map":
                            var mapLength = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                            var mapSymbolArray = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);
                            long mapStructArray = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 16), 0);

                            Print($"  Found Map: [ Length: {mapLength}; Symbol Array Address: 0x{mapSymbolArray:X}; Struct Array Address: 0x{mapStructArray:X} ]");
                            for (int arrayIndex = 0; arrayIndex < mapLength && !abort; mapStructArray += 8, mapSymbolArray += 8, arrayIndex++)
                            {
                                var structAddr = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructArray), 0);

                                if (DecodeSIDHash(sidbase, GetSubArray(binFile, structAddr - 8)) == "weapon-gameplay-def")
                                {
                                    var weaponDefenitionAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)BitConverter.ToInt64(GetSubArray(binFile, (int)structAddr + 0x10), 0) + 0x98), 0);

                                    Print($"   Weapon Gameplay Definition #{arrayIndex.ToString().PadLeft(2, '0')}: Struct Addr: 0x{structAddr.ToString("X").PadLeft(8, '0')} {DecodeSIDHash(sidbase, GetSubArray(binFile, (int)mapSymbolArray))} Ammo Count: {weaponDefenitionAddr}");
                                }

                                else 
                                    Print($"   {DecodeSIDHash(sidbase, GetSubArray(binFile, structAddr - 8))} #{arrayIndex.ToString().PadLeft(2, '0')}: Struct Addr: 0x{structAddr.ToString("X").PadLeft(8, '0')} {DecodeSIDHash(sidbase, GetSubArray(binFile, (int)mapSymbolArray))}");
                            }
                            Print("");
                            break;

                        case "symbol-array":
                            break;
                            var sArrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                            var sArrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);

                            Print($"  Item Details: [ Length: {sArrayLen} ]");
                            for (int i = 0; i < sArrayLen && !abort; sArrayAddr += 8, i++)
                            {
                                Print($"  Symbol: {DecodeSIDHash(sidbase, GetSubArray(binFile, (int)sArrayAddr))}");
                            }
                            Print("");
                            break;
                        case "ammo-to-weapon-array":
                            break;
                            var atwArrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                            var atwArrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);

                            Print($"  Item Details: [ Length: {atwArrayLen} ]");
                            for (int i = 0; i < atwArrayLen && !abort; atwArrayAddr += 16, i++)
                            {
                                Print($" Weapon: {DecodeSIDHash(sidbase, GetSubArray(binFile, (int)atwArrayAddr + 8))} Ammo Type: {DecodeSIDHash(sidbase, GetSubArray(binFile, (int)atwArrayAddr))}");
                            }
                            Print("");
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
            }
            catch(ThreadAbortException){}
        }
        #endregion
    }
}
