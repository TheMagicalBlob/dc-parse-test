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
            CreateBrowseModeDropdownMenu();
            InitializeAdditionalEventHandlers();

            Update(); Refresh();
            Venat = this;
            Azem = new OptionsPage();
            Update(); Refresh();
            

            activeScriptLabel = ActiveScriptLabel;
            abortBtn = AbortBtn;
            OutputWindow = OutputWindowRichTextBox;


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

            redirectCheckBox.Checked = redirect;
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
            return;
            Azem.Visible ^= true;
		    Azem.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
            Azem.Update();

            DropdownMenu[1].Visible = DropdownMenu[0].Visible = false;
        }

        private void OptionsMenuDropdownBtn_Click(object sender, EventArgs e)
        {
            return;
            Azem.Visible ^= true;
		    Azem.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
            Azem.Update();

            DropdownMenu[1].Visible = DropdownMenu[0].Visible = false;
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
        
        private void AbortBtn_Click(object sender, EventArgs e)
        {
            echo("\naborting bin thread...");
            abort = true;
        }

        
        private void ClearBtn_Click(object sender, EventArgs e) => OutputWindow.Clear();


        private void redirectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            #if DEBUG
            redirect = ((CheckBox)sender).Checked;
            #endif
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
             try {
                var binPath = pathObj as string;

                var binFile = File.ReadAllBytes(binPath);
                var binName = binPath.Substring(binPath.LastIndexOf('\\') + 1);
            
                Venat?.Invoke(abortButtonMammet, new object[] { true });

                
                DCFile = new DCFileHeader(binName, binFile);

                if (abort) {
                    goto yeet;
                }

                
                for (int tableIndex = 0, addr = 0x28; tableIndex < DCFile.HeaderItems.Length && !(Venat?.abort ?? true); tableIndex++, addr += 24)
                {
                    Venat?.CTUpdateLabel(ActiveLabel + $" ({tableIndex} / {DCFile.TableLength})");
                    PrintNL($"Item #{tableIndex}: [ Label: {DCFile.HeaderItems[tableIndex].Name} Type: {DCFile.HeaderItems[tableIndex].Type} Data Address: {DCFile.HeaderItems[tableIndex].StructAddress:X} ]");

                    DCEntries[tableIndex] = LoadDcStruct(binFile, DCFile.HeaderItems[tableIndex].Type, (int)DCFile.HeaderItems[tableIndex].StructAddress);
                }

                Venat?.Invoke(reloadButtonMammet, new object[] { true });

                

                //#
                //## Do shit
                //#



                yeet:
                PrintNL("Finished!");
                CTUpdateLabel(binName + " Finished Loading dc File");
                Venat?.Invoke(abortButtonMammet, new object[] { false });

                abort = false;
            }
            catch(ThreadAbortException) {
                Venat?.Invoke(abortButtonMammet, new object[] { false });
            }
        }

        private static object LoadDcStruct(byte[] binFile, string Type, long Address, string Name = null, bool silent = false)
        {
            if (Name == null || Name.Length < 1)
            {
                Name = "unnamed";
            }

            if (!silent)
            PrintNL($" #[{Type}]: {{ Struct Address: 0x{Address.ToString("X").PadLeft(8, '0')}; DC Size: 0x{binFile.Length.ToString("X").PadLeft(8, '0')}; Name: {Name} }}");
            object ret;

            switch (Type)
            {
                // map == [ struct len, sid[]* ids, struct*[] * data ]
                case "map":
                    ret = new DCMapDef(binFile, Type, Address);
                    break;
                    //! Print Parsed Data
                    foreach (var item in ((DCMapDef)ret).Items)
                    {
                        switch ((string)item[0])
                        {
                            case "":
                                break;

                            default:
                                break;
                        }
                    }
                    break;

                case "weapon-gameplay-def":
                    Venat?.WeaponDefinitions.Add(new WeaponGameplayDef(Name, binFile, Address));
                    break;
                case "melee-weapon-gameplay-def":
                    break;

                case "symbol-array":
                //break;
                    Venat.SymbolDefinitions.Add(new SymbolArrayDef(Name, binFile, Address));
                    
                    //! Print unknown remaining DCHeader.Structures
                    if (Venat?.SymbolDefinitions?.Count > 0)
                    {
                        var ext = "  ->  ";
                        PrintNL($"Printing Symbol Array of Length {Venat?.SymbolDefinitions.Last().Symbols.Count}:");
                        foreach (var symbolDef in Venat?.SymbolDefinitions.Last().Symbols)
                        {
                            PrintNL(ext + symbolDef);
                        }
                        PrintNL();
                    }
                    PrintNL();
                    break;


                case "ammo-to-weapon-array":
                    break;
                    Venat.AmmoToWeaponDefinitions.Add(new AmmoToWeaponArray(Name, binFile, Address));
                    
                    //! Print unknown remaining DCHeader.Structures
                    if (Venat?.AmmoToWeaponDefinitions?.Count > 0)
                    {
                        var ext = "  ->  ";
                        PrintNL($"Printing Ammo-to-Weapon Array of Length {Venat?.AmmoToWeaponDefinitions.Last().Symbols.Count}:");
                        foreach (var symbolDef in Venat?.AmmoToWeaponDefinitions.Last().Symbols)
                        {
                            PrintNL(ext + $"{symbolDef[0]} -> {symbolDef[1]}");
                        }
                        PrintNL();
                    }
                    PrintNL();
                    break;

                default:
                    Venat?.UnknownDefinitions.Add($"Unknown Structure: {Type}\n    Struct Addr: 0x{Address.ToString("X").PadLeft(8, '0')}\n    Struct Name: {Name}");
                    break;
            }

            return null;
        }
        #endregion
    }
}
