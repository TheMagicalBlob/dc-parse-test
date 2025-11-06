using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //=================================\\
        //--|   Variable Declarations   |--\\
        //=================================\\
        #region [Variable Declarations]
        
        //#
        //## Script Parsing Globals
        //#
        #region [Script Parsing Globals]

        /// <summary>
        /// The loaded DC Script binary as a byte array. <br/>
        /// </summary>
        public static byte[] DCFile
        {
            get => _dcFile;

            set {
                // Array.Empty for intentional resets of the array, until this app is actually functional and I can rely on my shit code lol
                if (value == null || value == Array.Empty<byte>())
                {
                    _dcFile = null;
                    DCFileMainDataLength = 0;
                    return;
                }
                else if (value.Length < 0x2D)
                {
#if !DEBUG
                    MessageBox.Show($"ERROR: provided dc file was too small to be valid (0x{value.Length:X}).");
                    value = Array.Empty<byte>();
#else
                    throw new InvalidDataException($"Provided dc file was too small to be valid (0x{value.Length:X}).");
#endif
                }



                // Actually go brr if all's well
                _dcFile = value;

                if (value.Length > 0x2C)
                {
                    DCFileMainDataLength = BitConverter.ToInt64(DCFile, 8);
                }
            }
        }
        private static byte[] _dcFile;

        /// <summary>
        /// The address of the provided DC file's relocation table
        /// </summary>
        public static long DCFileMainDataLength;


        /// <summary>
        /// Static refference to the active DC binary's header struct.
        /// </summary>
        public static DCFileHeader DCScript;


        /// <summary>
        /// A collection of known id's used in hardcoded checks, in order to handle basic operation when missing an sidbase.bin file.
        /// </summary>
        public enum KnownSIDs : ulong
        {
            UNKNOWN_SID_64 = 0x910ADC74DA2A5F6Dul,
            array = 0x4F9E14B634C6B026ul,
            symbol_array = 0xC8F749F92779D489ul,
            ammo_to_weapon_array = 0x14F1A7D0C4E0E13Eul,

            map = 0x080F5919176D2D91ul,

            weapon_gameplay_def = 0x6E1BB1DB85CC7806ul,
            melee_weapon_gameplay_def = 0xD17D76E0322C34A7ul,

            look2 = 0xBF24E1B6BADE9DCCul,

            placeholder = 0xDEADBEEFDEADBEEFul,
        }
        #endregion

        #endregion

        






        


        //===================================\\
        //---|   Function Declarations   |---\\
        //===================================\\
        #region [Function Declarations]

        private static void LoadBinFile(string DCFilePath)
        {
            if (File.Exists(DCFilePath))
            {
                CloseBinFile();

                ActiveFilePath = DCFilePath;
                ActiveFileName = DCFilePath.Substring(DCFilePath.LastIndexOf('\\') + 1);

                Venat?.StartBinParseThread();
            }
            else
            {
                MessageBox.Show("Invalid path provided for dc file! Doing nothing instead. :)", "How did you even manage that?");
            }
        }
        


        /// <summary>
        /// Reset the GUI and all relevant globals to their original states. //! (ideally...)
        /// </summary>
        private static void CloseBinFile()
        {
            ReloadCloseButtonsMammet(false);

            ResetSelectionLabel();
            ResetStatusLabel();

            
            DCFile = null;
            
            if (Venat != null)
            {
                Panels.Reset();

                if (DCScript.Entries != null)
                {
                    Venat.OptionsMenuDropdownBtn.TabIndex -= DCScript.Entries.Length;
                    Venat.MinimizeBtn.TabIndex -= DCScript.Entries.Length;
                    Venat.ExitBtn.TabIndex -= DCScript.Entries.Length;
                }
            }
        }

        private static void CTCloseBinFile()
        {
            Venat?.Invoke(Venat.CloseBinFileMammet);
        }

        

        private void StartBinParseThread()
        {
            if (binThread != null && binThread.ThreadState != System.Threading.ThreadState.Unstarted)
            {
                try
                {
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
        private void ThreadedBinFileParse()
        {
            var binPath = ActiveFilePath?.ToString() ?? "null";

            try
            {
                //#
                //## Load & Parse provided DC file.
                //#
                DCFile = File.ReadAllBytes(binPath);

                // TODO: make sure there's no difference between path versions! //!
                // Check whether or not the script is a basic empty one
                if (SHA256.Create().ComputeHash(DCFile).SequenceEqual(EmptyDCFileHash))
                {
                    StatusLabelMammet(new[] { "Empty DC File Loaded." });
                    ResetSelectionLabel();
                    return;
                }



                DCScript = new DCFileHeader(DCFile, ActiveFileName);

                //#
                //## Setup Form
                //#
                echo("\nFinished!");
                StatusLabelMammet(new[] { "Finished Loading dc File, populating properties panel...", emptyStr, emptyStr });
                PropertiesPanelMammet(ActiveFileName, DCScript);

                ReloadCloseButtonsMammet(true);
                StatusLabelMammet(new[] { "Viewing Script" });

            }
            // File in use, probably
            catch (IOException dang)
            {
                catchError("Error loading DC file; file may be in use, or simply not exist.", $"\nERROR: Selected file is either in use, or doesn't exist. ({dang.Message})");
            }/*
            // Thread has been killed (not sure why I'm still bothering to check for this)
            catch (ThreadAbortException)
            {
                catchError("DC Parse aborted (seemingly intentionally?)", $"\nERROR: Selected file is either in use, or doesn't exist.");
            }
            catch (Exception fuck)
            {
                var intErr = $"ERROR: An unexpected {fuck.GetType()} occured while parsing the provided DC .bin file.";
                var mainErr = $"Unhandled Error Parsing DC File!";

                catchError(mainErr + $" ({fuck.GetType()})", '\n' + intErr);
                MessageBox.Show(intErr, mainErr);
            }
*/

            void catchError(string mainMessage, string internalMessage)
            {
                echo(internalMessage);
                CTCloseBinFile();

                StatusLabelMammet(new[] { mainMessage, emptyStr, emptyStr });
            }
        }



        /// <summary>
        /// //! WRITE A DESCRIPTION FOR ME!                                                                                          no.
        /// </summary>
        /// 
        /// <param name="DCFile"> The whole DC file, loaded as a byte array. </param>
        /// <param name="Type"> The type of the DC struct. </param>
        /// <param name="Address"> The address of the DC struct in the <paramref name="DCFile"/>. </param>
        /// <param name="Name"> The name (if there is any) of the DC structure entry </param>
        /// <returns> The loaded DC Structure, in object form. (or a string with basic details about the structure, if it hasn't at least been slightly-apped) </returns>
        private static object LoadMappedDCStructs(byte[] DCFile, SID Type, long Address, object Name = null)
        {
            var name = (SID)(Name ?? SID.Empty);

            switch (Type.RawID)
            {
                //#
                //## Mapped Structures
                //#
                // map == [ struct len, sid[]* ids, struct*[] * data ]
                case KnownSIDs.map: return new Map(DCFile, Address, name);

                case KnownSIDs.weapon_gameplay_def: return new weapon_gameplay_def(DCFile, Address, name);

                case KnownSIDs.melee_weapon_gameplay_def: return new MeleeWeaponGameplayDef(DCFile, Address, name);

                case KnownSIDs.symbol_array: return new SymbolArrayDef(DCFile, Address, name);

                case KnownSIDs.ammo_to_weapon_array: return new ammo_to_weapon_array(DCFile, Address, name);

                case KnownSIDs.look2: return new look2(DCFile, Address, name);


                //#
                //## Unmapped Structures
                //#
                default: return new UnmappedStructure(Type, Address, name);
            }
        }









        
        public static object ReadPropertyValueByType(byte[] Array, System.Reflection.PropertyInfo Property, int Offset)
        {
            var type = Property.PropertyType.Name;
            
            switch (type)
            {
                case "SID":
                    return SID.Parse(GetSubArray(Array, Offset));

                case "Byte":
                    return Array[Offset];

                case "Byte[]":
                    if (Property.Name.Contains("_s0x"))
                    {
                        var name = Property.Name.Substring(Property.Name.LastIndexOf("_s0x") + 4);
                        return GetSubArray(Array, Offset, int.Parse(name));
                    }
                    else {
                        echo("Array was provided with an invalid name format ({Property.Name}). Assuming length of 8 for byte array.");
                        return GetSubArray(Array, Offset);
                    }


                case "Single":
                    return BitConverter.ToSingle(Array, Offset);
                case "Double":
                    return BitConverter.ToDouble(Array, Offset);

                    
                case "Int16":
                    return BitConverter.ToInt16(Array, Offset);
                case "UInt16":
                    return BitConverter.ToUInt16(Array, Offset);
                    
                case "Int64":
                    return BitConverter.ToInt64(Array, Offset);
                case "UInt64":
                    return BitConverter.ToUInt64(Array, Offset);


                case "UInt32":
                    return BitConverter.ToUInt32(Array, Offset);
                case "Int32":
                default:
                    if (!type.Contains("Int"))
                    {
                        echo($"Unknown Type \"{type}\", Treating as signed Int32");
                    }

                    return BitConverter.ToInt32(Array, Offset);
            }
        }



        /// <summary>
        /// //!
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Offset"></param>
        /// <param name="ValueAsString"></param>
        public static void WritePropertyValueByType(Type Type, int Offset, string ValueAsString)
        {
            byte[] convertedValue;

            // Ensure value string has no hex number prefix
            if (ValueAsString.Length > 2)
            {
                if ($"{ValueAsString[0]}{ValueAsString[1]}" == "0x")
                {
                    var substring = ValueAsString.Substring(2);
                    echo($"Removing hexadecimal number specifier from value string ({ValueAsString} => {substring})");

                    ValueAsString = substring;
                }
            }

            if (StatusDetails?.Length > 1 && StatusDetails[1] == "Invalid Input Value")
            {
                UpdateStatusLabel(new[] { null, string.Empty });
            }



            // Convert the value to the relevant type, then write it to the loaded DC file array
            switch (Type.Name)
            {
                case "Byte":
                    convertedValue = new[] { byte.Parse(ValueAsString, System.Globalization.NumberStyles.HexNumber) };
                    break;


                case "Single":
                    convertedValue = BitConverter.GetBytes(float.Parse(ValueAsString, System.Globalization.NumberStyles.HexNumber));
                    break;

                case "Double":
                    convertedValue = BitConverter.GetBytes(double.Parse(ValueAsString, System.Globalization.NumberStyles.HexNumber));
                    break;

                    
                case "Short":
                    convertedValue = BitConverter.GetBytes(short.Parse(ValueAsString, System.Globalization.NumberStyles.HexNumber));
                    break;
                    
                case "Long":
                    convertedValue = BitConverter.GetBytes(long.Parse(ValueAsString, System.Globalization.NumberStyles.HexNumber));
                    break;

                case "Int":
                default:
                    if (Type.Name != "Int")
                    {
                        echo($"Unknown Type, Treating as signed int32");

                    }
                    

                    if (int.TryParse(ValueAsString, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var parsedValue))
                    {
                        convertedValue = BitConverter.GetBytes(parsedValue);
                    }
                    else {
                        convertedValue = null;
                    }
                    break;
            }
            
            
            // Handle invalid inputs
            if (convertedValue == null)
            {
                UpdateStatusLabel(new[] { null, "Invalid Input Value" });
                return;
            }


            WriteSubArray(DCFile, convertedValue, Offset);

            Changes.Add(new object[] { Offset, convertedValue });
        }
        #endregion










        //==================================\\
        //--|   SID Class Declaration   |---\\
        //==================================\\

        /// <summary>
        /// Small class used for handling string id's in a bit more of a convenient manner.
        /// </summary>
        public class SID
        {
            //#
            //## Instance Initializers
            //#

            /// <summary>
            /// Create a new SID instance from a provided byte array, and attempt to decode the id.
            /// </summary>
            /// <param name="EncodedSIDArray"> The encoded ulong string id, converted to a byte array. </param>
            private SID(byte[] EncodedSIDArray)
            {
                DecodedID = SIDBase.DecodeSIDHash(EncodedSIDArray);
                EncodedID = BitConverter.ToString(EncodedSIDArray).Replace("-", emptyStr);

                RawID = (KnownSIDs) BitConverter.ToUInt64(EncodedSIDArray, 0);
            }

            /// <summary>
            /// Create a new SID instance from a provided ulong hash, and attempt to decode the id.
            /// </summary>
            /// <param name="EncodedSID"> The encoded ulong string id. </param>
            private SID(ulong EncodedSID)
            {
                var EncodedSIDArray = BitConverter.GetBytes(EncodedSID);

                DecodedID = SIDBase.DecodeSIDHash(EncodedSIDArray);
                EncodedID = BitConverter.ToString(EncodedSIDArray).Replace("-", emptyStr);
                RawID = (KnownSIDs) EncodedSID;
            }


            /// <summary>
            /// I don't know how else to make that SID.Empty field, lol.
            /// </summary>
            private SID(string decodedSID, ulong encodedSID)
            {
                DecodedID = decodedSID;
                EncodedID = encodedSID.ToString("X");

                RawID = (KnownSIDs) encodedSID;
            }







            //#
            //## VARIABLE DECLARATIONS
            //#
            #region [Variable Declarations]

            /// <summary>
            /// The decoded string id.
            /// <br/><br/>
            /// If the id cannot be decoded, it will return either: 
            /// <br/> - the encoded ulong sid's string representation
            /// <br/> OR
            /// <br/> - UNKNOWN_SID_64
            /// <br/><br/>
            /// Depending on whether the ShowUnresolvedSIDs option is enabled or disabled respectively
            /// </summary>
            public string DecodedID
            {
                get {
                    if (_decodedID == "UNKNOWN_SID_64" && ShowUnresolvedSIDs)
                    {
                        return EncodedID;
                    }
#if DEBUG
                    else if (_decodedID == "INVALID_SID_64" && ShowInvalidSIDs)
                    {
                        return EncodedID;
                    }
#endif

                    return _decodedID;
                }

                set => _decodedID = value;
            }
            private string _decodedID;



            /// <summary>
            /// The string representation of the encoded ulong string id.
            /// </summary>
            public string EncodedID { get; set; }



            /// <summary>
            /// The raw ulong version of the encoded string id. (used for hardcoded checks in code)
            /// </summary>
            public KnownSIDs RawID { get; set; }



            /// <summary>
            /// Represents an item with an unspecified name.
            /// </summary>
            public static readonly SID Empty = new SID("unnamed", 0x5FE267C3F96ADB8C);
            #endregion






            //#
            //## FUNCTION DECLARATIONS
            //#
            #region [Function Declarations]
            /// <summary>
            /// Create a new SID instance from the provided <paramref name="EncodedSID"/>.
            /// </summary>
            /// <param name="EncodedSID"> The FNV1-a 64b hash to be decoded with the loaded lookup sidbase.bin table(s), converted to an array of bytes. </param>
            /// 
            /// <returns>
            /// A new SID instance containing the id's decoded string (or error reply), as well as the encoded string id in a hex number string format. <br/>
            /// Also contains the encoded id in it's original ulong format
            /// </returns>
            public static SID Parse(byte[] EncodedSID) => new SID(EncodedSID);

            
            /// <summary>
            /// Create a new SID instance from the provided <paramref name="EncodedSID"/>.
            /// </summary>
            /// <param name="EncodedSID"> The FNV1-a 64b hash to be decoded with the loaded lookup sidbase.bin table(s), as a default unsigned 64-bit integer. </param>
            /// 
            /// <returns>
            /// A new SID instance containing the id's decoded string (or error reply), as well as the encoded string id in a hex number string format. <br/>
            /// Also contains the encoded id in it's original ulong format
            /// </returns>
            public static SID Parse(ulong EncodedSID) => new SID(EncodedSID);
            #endregion
        }










        //======================================\\
        //--|   SIDBase Class Declaration   |---\\
        //======================================\\

        /// <summary> 
        /// Used for decoding any encoded string id's found.
        /// </summary>
        public class SIDBase
        {
            /// <summary>
            /// Initialize a new instance of the SIDBase class with the file at the path provided. <br/>
            ///
            /// </summary>
            /// <param name="SIDBasePath"> The path of the sidbase.bin to be loaded for this instance. </param>
            /// <exception cref="FileNotFoundException"> Thrown in the event that Jupiter aligns wi- what the fuck else would it be for. </exception>
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


                SIDHashTable = GetSubArray(rawSIDBase, 8, HashTableRawLength);
                SIDStringTable = GetSubArray(rawSIDBase, SIDHashTable.Length + 8, rawSIDBase.Length - (HashTableRawLength + 8));

                if (rawSIDBase.Length < 24)
                {
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
            /// List of SIDBase Class instances for the active sidbase.bin lookup tables.
            /// </summary>
            private static SIDBase[] SIDBases;







            //#
            //## FUNCTION DECLARATIONS
            //#
    #pragma warning disable IDE0011 // aDd BrAcEs
        
            /// <summary>
            /// Load a new sidbase from the path provided, adding it to the list of sidbases to search through. <br/>
            ///
            /// </summary>
            /// <param name="SIDBasePath"> The path of the sidbase.bin to be loaded for this instance. </param>
            public static void LoadSIDBase(string SIDBasePath)
            {
                if (File.Exists(SIDBasePath))
                {
                    if (SIDBases != null)
                    {
                        // Load it and add it to the list of previously-loaded lookup tables
                        SIDBases = SIDBases.Concat(new[] { new SIDBase(SIDBasePath) }).ToArray();
                    }
                    else {
                        // Load it and create the lookup table list
                        SIDBases = new[] { new SIDBase(SIDBasePath) };
                    }
                }
                else {
                    // Bitch 'n moan
                    MessageBox.Show($"File does not exist:\n " + SIDBasePath, "Invalid path provided for desired sidbase.bin!");
                }
            }


        
            /// <summary>
            /// Get a sub-array of the specified <paramref name="length"/> from a larger <paramref name="array"/> of bytes, starting at the <paramref name="index"/> specified.
            /// </summary>
            /// <param name="array"> The array from which to take the sub-array. </param>
            /// <param name="index"> The start index of the sub-array within <paramref name="array"/>. </param>
            /// <param name="length"> The length of the sub-array. </param>
            /// <returns> What the hell do you think. </returns>
            private static byte[] GetSubArray(byte[] array, int index, int length = 8)
            {
                if (length == 0)
                {
                    return Array.Empty<byte>();
                }


                // Build return string.
                for (var ret = new byte[length];; ret[length - 1] = array[index + (length-- - 1)])
                {
                    if (length <= 0)
                    {
                        return ret;
                    }
                }
            }



            /// <summary>
            /// Attempt to decode a provided 64-bit FNV-1a hash via a provided lookup file (sidbase.bin)
            /// </summary>
            /// <param name="bytesToDecode"> The hash to decode, as an array of bytes </param>
            /// <exception cref="IndexOutOfRangeException"> Thrown in the event of an invalid string pointer read from the sidbase after the provided hash is located. </exception>
            private string LookupSIDHash(byte[] bytesToDecode)
            {
                if (bytesToDecode.Sum(@byte => @byte) == 0)
                {
                    echo($"Null SID provided.");
                    return "(null sid)";
                }

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
                        {
                            break;
                        }



                        // Handle missing sid's.
                        if (scanAddress == previousAddress)
                        {
                            return "UNKNOWN_SID_64";
                        }

                        previousAddress = scanAddress;
                    }





                // Read the string pointer
                readString:
                    var stringPtr = (int)BitConverter.ToInt64(SIDHashTable, scanAddress + 8); // Get the string pointer for the read hasha, located immediately after said hash
                    stringPtr -= HashTableRawLength + 8; // Adjust the string pointer to account for the lookup table being a separate array, and table length being removed

                    if (stringPtr >= SIDStringTable.Length)
                    {
                        throw new IndexOutOfRangeException($"ERROR: Invalid Pointer Read for String Data!\n    str* 0x{stringPtr:X} >= len 0x{SIDHashTable.Length + SIDStringTable.Length + 8:X}.");
                    }


                    // Parse and add the string to the array
                    var stringBuffer = string.Empty;

                    while (SIDStringTable[stringPtr] != 0)
                    {
                        stringBuffer += Encoding.UTF8.GetString(SIDStringTable, (int) stringPtr++, 1);
                    }


                    return stringBuffer;
                }
                else {
                    echo($"Invalid SID provided; unexpected length of \"{bytesToDecode?.Length ?? 0}\". Must be 8 bytes.");
                    return "INVALID_SID_64";
                }
            }



            public static string DecodeSIDHash(byte[] EncodedSID)
            {
                var id = "(No SIDBases Loaded.)";

                foreach (var table in SIDBases ?? Array.Empty<SIDBase>())
                {
                    id = table?.LookupSIDHash(EncodedSID) ?? "(Null SIDBase Instance!!!)";

                    if (id != "UNKNOWN_SID_64")
                    {
                        break;
                    }
                }

                return id;
            }
        


            private static void echo(object message = null)
            {
    #if DEBUG
                string str;

                Console.WriteLine(str = message?.ToString() ?? string.Empty);

                if (!Console.IsInputRedirected)
                {
                    Debug.WriteLine(str);
                }
    #endif
            }
        }
    }
}
