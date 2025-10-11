using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
            Venat = this;
            Azem = new OptionsPage();
            Panels = new PropertiesHandler();
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

            if (!new[] { $@"{workingDirectory}\sidbase.bin", $@"{workingDirectory}\sid\sidbase.bin", $@"{workingDirectory}\sid1\sidbase.bin", $@"{workingDirectory}\..\sidbase.bin" }
            .Any(path =>
            {
                if (File.Exists(path))
                {
                    SIDBase = new SIDBase(path);
                    return true;
                }
                return false;
            }))
            // Bitch if it isn't found so the user knows to load one manually
            {
                echo("No valid sidbase.bin file was provided");
                UpdateStatusLabel(new[] { null, "WARNING: No sidbase.bin found; please provide one before loading a DC file." });
            }


            BaseAbortButtonWidth = AbortOrCloseBtn.Size.Width;
        }



        


        //======================================\\
        //--|   Event Handler Declarations   |--\\
        //======================================\\
        #region [Event Handler Declarations]

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



        private void BinPathBrowseBtn_Click(object sender, EventArgs e)
        {
#if DEBUG
            LoadBinFile(
                @"C:\Users\blob\LocalModding\Bin Reversing\_Scripts\characters.bin"
                //@"C:\Users\blob\LocalModding\Bin Reversing\_Scripts\weapon-gameplay.bin"
            );
#else
            using (var Browser = new OpenFileDialog
            {
                Title = "Please select a script from \"bin/dc1\"."
            })
            if (Browser.ShowDialog() == DialogResult.OK)
            {
                LoadBinFile(Browser.FileName);
            }
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


        private void OptionsMenuDropdownBtn_Click(object sender, EventArgs e)
        {
            Azem.Visible ^= true;
            Azem.Location = new Point(Venat.Location.X + (Venat.Size.Width - Azem.Size.Width) / 2, Venat.Location.Y + 50);
            Azem.Update();
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

    /// <summary> 
    /// Used for decoding any encoded string id's found.
    /// </summary>
    public class SIDBase
    {
        public SIDBase(string SIDBasePath)
        {
            // Verify the provided path before proceeding
            if (!File.Exists(SIDBasePath))
            {
                throw new FileNotFoundException("The file at the path provided does not exist, please ensure that you're not a complete moron.");
            }


            var rawSIDBase = File.ReadAllBytes(SIDBasePath);

            // Read the table length to get the expected size of the hash table (don't really need it anymore)
            HashTableRawLength = BitConverter.ToInt32(rawSIDBase, 0) * 16;

            // Just-In-Case.
            if (HashTableRawLength >= int.MaxValue)
            {
                Console.Clear();
                MessageBox.Show($"ERROR: Sidbase is too large for 64-bit addresses, blame Microsoft for limiting me to that, then blame me for not bothering to try splitting the sidbases.");
                Environment.Exit(0);
            }


            SIDHashTable   = GetSubArray(rawSIDBase, 8, HashTableRawLength);
            SIDStringTable = GetSubArray(rawSIDBase, SIDHashTable.Length + 8, rawSIDBase.Length - (HashTableRawLength + 8));
  
            if (rawSIDBase.Length < 24) {
                //! Implement an error, since the file would obviously be corrupted.
#if DEBUG
                echo("ERROR: Invalid length for sidbase.bin (< 0x19- is it corrupted?)");
#else
                MessageBox.Show("ERROR: Invalid length for sidbase.bin (< 0x19- is it corrupted?)", "The provided sidbase was unable to be loaded.");
#endif
            }
        }

        


        //#
        //## VARIABLE DECLARATIONS
        //#

        /// <summary>
        /// The Lookup table of the sidbase, containing the hashes & their decoded string pointers, the latter of which get adjusted to be used with the string table.
        /// </summary>
        private readonly byte[] SIDHashTable;
        
        /// <summary>
        /// The raw, null-separated string data of the sidbase.
        /// </summary>
        private readonly byte[] SIDStringTable;


        /// <summary>
        /// The length of the sidbase.bin's lookup table (in bytes)<br/>
        /// </summary>
        public readonly int HashTableRawLength;

        
        /// <summary>
        /// The amount of items in the sidbase.bin's lookup table.<br/>
        /// No reason to read it as a long integer, as if it's that big; we've already fucked off by now.
        /// </summary>
        public readonly int HashTableCount;



        
        
        //#
        //## FUNCTION DECLARATIONS
        //#

        /// <summary>
        /// Get a sub-array of the specified <paramref name="length"/> from a larger <paramref name="array"/> of bytes, starting at the <paramref name="index"/> specified.
        /// </summary>
        /// <param name="array"> The array from which to take the sub-array. </param>
        /// <param name="index"> The start index of the sub-array within <paramref name="array"/>. </param>
        /// <param name="length"> The length of the sub-array. </param>
        /// <returns> What the hell do you think. </returns>
        private static byte[] GetSubArray(byte[] array, int index, int length = 8)
        {
            var ret = new byte[length];

            for (; length > 0; ret[length - 1] = array[index + (length-- - 1)]);
            return ret;
        }


        
        /// <summary>
        /// Attempt to decode a provided 64-bit FNV-1a hash via a provided lookup file (sidbase.bin)
        /// </summary>
        /// <param name="bytesToDecode"> The hash to decode, as an array of bytes </param>
        /// <exception cref="IndexOutOfRangeException"> Thrown in the event of an invalid string pointer read from the sidbase after the provided hash is located. </exception>
        public string DecodeSIDHash(byte[] bytesToDecode)
        {
            if (bytesToDecode.Length == 8)
            {
                ulong
                    currentHash,
                    expectedHash
                ;
                int
                    previousAddress = 0xBADBEEF, // Used for checking whether the hash could not be decoded
                    scanAddress = HashTableRawLength / 2,
                    currentRange = scanAddress
                ;


                expectedHash = BitConverter.ToUInt64(bytesToDecode, 0);

                // check whether or not the chunk can be evenly split; if not, check
                // the odd one out for the expected hash, then exclude it and continue as normal if it isn't a match.
                if (((HashTableRawLength >> 4) & 1) == 1)
                {
                    var checkedHash = BitConverter.ToUInt64(SIDHashTable, HashTableRawLength - 0x10);

                    if (checkedHash == expectedHash)
                    {
                        scanAddress = HashTableRawLength - 0x10;
                        goto readString;
                    }

                    scanAddress = currentRange -= 8;
                }
                

                while (true)
                {
                    // Adjust the address to maintain alignment
                    if (((scanAddress >> 4) & 1) == 1)
                    {
                        if (BitConverter.ToUInt64(SIDHashTable, scanAddress) == expectedHash)
                        {
                            goto readString;
                        }
                    
                        scanAddress -= 0x10;
                    }
                    if (((currentRange >> 4) & 1) == 1)
                    {
                        currentRange += 0x10;
                    }


                    currentHash = BitConverter.ToUInt64(SIDHashTable, scanAddress);

                    if (expectedHash < currentHash)
                    {
                        scanAddress -= currentRange / 2;
                        currentRange /= 2;
                    }
                    else if (expectedHash > currentHash)
                    {
                        scanAddress += currentRange / 2;
                        currentRange /= 2;
                    }
                    else
                        break;



                    // Handle missing sid's.
                    if (scanAddress == previousAddress)
                    {
                        return "UNKNOWN_SID_64";
                    }

                    previousAddress = scanAddress;
                }





                // Read the string pointer
                readString:
                var stringPtr = (int) BitConverter.ToInt64(SIDHashTable, scanAddress + 8); // Get the string pointer for the read hasha, located immediately after said hash
                stringPtr -= HashTableRawLength + 8; // Adjust the string pointer to account for the lookup table being a separate array, and table length being removed
                
                if (stringPtr >= SIDStringTable.Length)
                {
                    throw new IndexOutOfRangeException($"ERROR: Invalid Pointer Read for String Data!\n    str* 0x{stringPtr:X} >= len 0x{SIDHashTable.Length + SIDStringTable.Length + 8:X}.");
                }


                // Parse and add the string to the array
                var stringBuffer = string.Empty;

                while (SIDStringTable[stringPtr] != 0)
                {
                    stringBuffer += Encoding.UTF8.GetString(SIDStringTable, (int)stringPtr++, 1);
                }

                
                return stringBuffer;
            }
            else {
                echo($"Invalid SID provided; unexpected length of \"{bytesToDecode?.Length ?? 0}\". Must be 8 bytes.");
                return "INVALID_SID_64";
            }
        }

        public string DecodeSIDHash(ulong EncodedSID) => DecodeSIDHash(BitConverter.GetBytes(EncodedSID));

        

        private static void echo(object message = null)
        {
            # if DEBUG
            string str;

            Console.WriteLine(str = message?.ToString() ?? string.Empty);

            if (!Console.IsInputRedirected)
            {
                Debug.WriteLine(str);
            }
            #endif
        }


        #if false
        private string BAD_DecodeSIDHash(byte[] bytesToDecode)
        {
            var ret = "UNKNOWN_SID_64";


            if (bytesToDecode.Length == 8)
            {
                var expectedHash = BitConverter.ToUInt64(bytesToDecode, 0);
                
                ulong currentHash;

                int
                    scanAddress,
                    previousAddress = 0xBADBEEF,
                    currentRange
                ;

                scanAddress = currentRange = FullHashTableLength / 2;
                
                // check whether or not the chunk can be evenly split; if not, check
                // the odd one out for the expected hash, then exclude it and continue as normal if it isn't a match.
                if (((FullHashTableLength >> 4) & 1) == 1)
                {
                    FullHashTableLength -= 0x10;

                    var checkedHash = BitConverter.ToUInt64(SIDHashTable, (int) FullHashTableLength);
                    if (checkedHash == expectedHash)
                    {
                        scanAddress = FullHashTableLength;
                        goto readString;
                    }
                }


                while (true)
                {
                    // check for uneven split again
                    if (((scanAddress >> 4) & 1) == 1)
                    {
                        var checkedHash = BitConverter.ToUInt64(SIDHashTable, (int) scanAddress);
                        
                        if (checkedHash == expectedHash)
                        {
                            goto readString;
                        }
                        else
                            scanAddress -= 0x10;
                    } 
                    if (((currentRange >> 4) & 1) == 1)
                    {
                        currentRange += 0x10;
                    }

                    

                    currentHash = BitConverter.ToUInt64(SIDHashTable, (int) scanAddress);

                    if (expectedHash < currentHash)
                    {
                        scanAddress -= currentRange / 2;
                        currentRange /= 2;
                    }
                    else if (expectedHash > currentHash)
                    {
                        scanAddress += (currentRange) / 2;
                    }
                    else
                        break;
                    

                    // Handle missing sid's. How did I forget about that?
                    if (scanAddress == previousAddress)
                    {
                        return ret;
                    }

                    previousAddress = scanAddress;
                }
                





                // Read the string pointer
                readString:
                var stringPtr = (int) BitConverter.ToInt64(SIDHashTable, (int)(scanAddress + 8)) - ((uint) FullHashTableLength + 8);
                if (stringPtr >= SIDStringTable.Length)
                {
                    throw new IndexOutOfRangeException($"ERROR: Invalid Pointer Read for String Data!\n    str* 0x{stringPtr:X} >= len 0x{SIDHashTable.Length + SIDStringTable.Length + 8:X}.");
                }


                // Parse and add the string to the array
                var stringBuffer = string.Empty;

                while (SIDStringTable[stringPtr] != 0)
                {
                    stringBuffer += Encoding.UTF8.GetString(SIDStringTable, (int)stringPtr++, 1);
                }

                
                ret = stringBuffer;
            }
            else {
                echo($"Invalid SID provided; unexpected length of \"{bytesToDecode?.Length ?? 0}\". Must be 8 bytes.");
                ret = "INVALID_SID_64";
            }

            return ret;
        }
        #endif
    }
}
