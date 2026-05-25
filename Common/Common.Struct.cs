using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //=======================================================\\
        //--|   Script Parsing Global Variable Declarations   |--\\
        //=======================================================\\
        #region [Script Parsing Global Variable Declarations]


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

                DCFileMainDataLength = BitConverter.ToInt64(DCFile, 8);
                DCModuleStartAddress = BitConverter.ToInt64(DCFile, 0x18) * 24;
            }
        }
        private static byte[] _dcFile;

        /// <summary>
        /// The address of the provided DC file's relocation table
        /// </summary>
        public static long DCFileMainDataLength;


        /// <summary>
        /// Start address for the module contents, right after the initial entry map
        /// </summary>
        public static long DCModuleStartAddress;


        /// <summary>
        /// Static reference to the active DC binary's header struct.
        /// </summary>
        public static DCModule ActiveDCModule;


        
        public static readonly Type[] BasicNumericalTypes = new[]
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            //typeof(nint),
            //typeof(nuint),
            typeof(long),
            typeof(ulong)
        };


        public static readonly Type[] AdvancedNumericalTypes = new[]
        {
            typeof(decimal),
            typeof(double),
            typeof(float)
        };


        //! tf is this one for?
        public static readonly Type[] AdditionalDataTypes = new []
        {
            typeof(Array),
            typeof(SID),
        };




        /// <summary>
        /// //! OVERUSED
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



        /// <summary>
        /// A collection of type id's for unmapped structures that have had their sizes
        /// </summary>
        public static System.Collections.Generic.List<object[]> ParsedSizes;
        #endregion


        






        


        //===================================\\
        //---|   Function Declarations   |---\\
        //===================================\\
        #region [Function Declarations]

        /// <summary>
        /// //!
        /// </summary>
        /// <param name="DCFilePath"></param>
        private void LoadBinFile(string DCFilePath)
        {
            if (File.Exists(DCFilePath))
            {
                CloseBinFile();

                ActiveFilePath = DCFilePath;
                Venat?.StartBinParseThread();

                ParsedSizes = new System.Collections.Generic.List<object[]>();
            }
            else {
                MessageBox.Show("Invalid path provided for dc file! Doing nothing instead. :)", "How did you even manage that?");
            }
        }
        





        /// <summary>
        /// Reset the GUI and all relevant globals to their original states. //! (ideally...)
        /// </summary>
        private static void CloseBinFile()
        {
            SetReloadCloseButtonsEnabledStatus(false);

            CTResetSelectionLabel();
            
            DCFile = null;
            
            Venat.ResetPanels();
        }






        /// <summary>
        /// //!
        /// </summary>
        private void StartBinParseThread()
        {
            if (DCFileHandlerThread != null && DCFileHandlerThread.ThreadState != System.Threading.ThreadState.Unstarted)
            {
                try {
                    echo("Bin thread already active, killing thread.\n");
                    DCFileHandlerThread.Abort();
                }
                catch (ThreadAbortException) { echo("Bin thread killed.\n"); }
                catch (Exception dang) { echo($"Unexpected error of type \"{dang.GetType()}\" thrown when aborting bin thread.\n"); }
            }


            // Create and start the thread
            DCFileHandlerThread = new Thread(CTLoadProvidedDCFile)
            {
                IsBackground = true,
                Name = nameof(DCFileHandlerThread)
            };

            DCFileHandlerThread.Start();
        }






        /// <summary>
        /// Load the header info for the NaughtyDog DCScript at the provided <paramref name="FilePath"/>.
        /// </summary>
        /// <param name="FilePath"></param>
        public static void LoadProvidedDCFile(string FilePath)
        {
            // Load provided DC file.
            DCFile = File.ReadAllBytes(FilePath);

            // Check whether or not the script is a basic empty one  TODO: make sure there's no difference between path versions! //!
            if (SHA256.Create().ComputeHash(DCFile).SequenceEqual(EmptyDCFileHash))
            {
                CTLog("Empty DC file provided. Nothing to load.");
                return;
            }

            // Parse the script's header entries
            ActiveDCModule = new DCModule(DCFile, ActiveFileName);



            // Setup Form
            echo("Finished Loading dc File, populating properties panel...");
            CTPopulatePropertyList(ActiveDCModule, ActiveFileName);

            SetReloadCloseButtonsEnabledStatus(true);
            CTLog("Viewing Script");
        }






        /// <summary>
        /// //!
        /// </summary>
        /// 
        /// <param name="DCFile"> The whole DC file, loaded as a byte array. </param>
        /// <param name="Type"> The type of the DC struct. </param>
        /// <param name="Address"> The address of the DC struct in the <paramref name="DCFile"/>. </param>
        /// <param name="Name"> The name (if there is any) of the DC structure entry </param>
        /// <returns> The loaded DC Structure, in object form. (or a string with basic details about the structure, if it hasn't at least been slightly-apped) </returns>
        internal static object LoadMappedDCStructs(byte[] DCFile, SID Type, long Address, SID Name = null)
        {
            var name = Name ?? SID.Empty;

            switch (Type.RawID)
            {
                //#
                //## Mapped Structures
                //#
                case KnownSIDs.map: return new map(DCFile, Address, name); // map == [ struct len, sid[]* ids, struct*[] * data ]

                case KnownSIDs.weapon_gameplay_def: return new weapon_gameplay_def(DCFile, Address, name);

                //case KnownSIDs.melee_weapon_gameplay_def: return new MeleeWeaponGameplayDef(DCFile, Address, name);

                case KnownSIDs.symbol_array: return new symbol_array(DCFile, Address, name);

                case KnownSIDs.ammo_to_weapon_array: return new ammo_to_weapon_array(DCFile, Address, name);

                case KnownSIDs.look2: return new look2(DCFile, Address, name);


                //#
                //## Unmapped Structures
                //#
                default: return new UnmappedStructure(Type, Address, name);
            }
        }










        //#
        //## Miscellaneous Small Struct-Related Functions
        //#
        #region [Miscellaneous Small Struct-Related Functions]

        /// <summary>
        /// Get a sub-array of the specified <paramref name="length"/> from a larger <paramref name="array"/> of bytes, starting at the <paramref name="Address"/> specified.
        /// </summary>
        /// <param name="array"> The array from which to take the sub-array. </param>
        /// <param name="Address"> The start address of the sub-array within <paramref name="array"/>. </param>
        /// <param name="length"> The length of the sub-array. </param>
        /// <returns> What the hell do you think. </returns>
        public static byte[] GetSubArray(byte[] array, long Address, int length = 8)
        {
            if (length == 0)
            {
                return Array.Empty<byte>();
            }
            if (Address + length > array.Length)
            {
                throw new IndexOutOfRangeException($"Provided length and address exceed the length of the array (0x{Address:X} + 0x{length:X} >= 0x{array.Length:X})");
            }



            // Build return string.
            for (var ret = new byte[length];; ret[length - 1] = array[Address + (length-- - 1)])
            {
                if (length <= 0)
                {
                    return ret;
                }
            }
        }






        /// <summary>
        /// //!
        /// </summary>
        /// <param name="array"></param>
        /// <param name="subarray"></param>
        /// <param name="Address"></param>
        public static void WriteSubArray(byte[] array, byte[] subarray, int Address)
        {
            for (var length = subarray.Length - 1;; array[Address + length] = subarray[length--])
            {
                if (length < 1)
                {
                    return;
                }
            }
        }


        



        /// <summary>
        /// Reads a string from <paramref name="buffer"/> at the specified <paramref name="startAddress"/>, until the string terminator is read. <br/>
        /// Encoding: Converts the bytes to a char, so whatever string encoding format that results in.
        /// </summary>
        /// <param name="buffer"> The array of bytes from which to read the returned string. </param>
        /// <param name="startAddress"> The address in <paramref name="buffer"/> at which to start reading the returned string. </param>
        /// <param name="terminator"> The terminator for the string (defaults to the standard string terminator; 0x00). </param>
        /// <returns> Home with the Milk. </returns>
        public static string ReadString(byte[] buffer, int startAddress, byte terminator = 0)
        {
            var str = string.Empty;

            // Index overflow | terminator is immediately read
            if (startAddress >= buffer.Length || buffer[startAddress] == terminator)
            {
                return string.Empty;
            }



            do {
                str += (char) buffer[startAddress++];
            }
            while (startAddress < buffer.Length && buffer[startAddress] != terminator);

            return str;
        }

        




        /// <summary>
        /// //!
        /// </summary>
        /// <param name="Array"></param>
        /// <param name="Property"></param>
        /// <param name="Address"></param>
        /// <returns></returns>
        public static object ReadPropertyValueByType(byte[] Array, System.Reflection.PropertyInfo Property, int Address)
        {
            var type = Property.PropertyType.Name;

            // Basic integrity check
            if (Address >= Array.Length)
            {
#if DEBUG
                throw new IndexOutOfRangeException($"Provided address was outside the bounds of the array for {type}. ({Address:X} >= {Array.Length:X})");
#else
                Log($"Error reading value for {type}.");
                return null;
#endif
            }

            
            switch (type)
            {
                case "SID":
                    return SID.Parse(GetSubArray(Array, Address));

                case "Byte":
                    return Array[Address];

                case "Byte[]":
                    var len = 8;

                    // Attempt to parse the array size from the name if it's specified in the property name (//! the hell? I need to check that setup)
                    if (Property.Name.Contains("_s0x"))
                    {
                        var size = Property.Name.Substring(Property.Name.LastIndexOf("_s0x") + 4);

                        if (!int.TryParse(size, out len))
                        {
                            echo($"Array was provided with an invalid name format ({Property.Name} != _s0x*). Assuming length of 8 for byte array.");
                        }
                    }
                    else {
                        echo($"Array was provided with an invalid name format ({Property.Name} mising _s0x size specifier). Assuming length of 8 for byte array.");
                    }

                    return GetSubArray(Array, Address, len);


                case "Single":
                    return BitConverter.ToSingle(Array, Address);
                case "Double":
                    return BitConverter.ToDouble(Array, Address);

                    
                case "Int16":
                    return BitConverter.ToInt16(Array, Address);
                case "UInt16":
                    return BitConverter.ToUInt16(Array, Address);
                    
                case "Int64":
                    return BitConverter.ToInt64(Array, Address);
                case "UInt64":
                    return BitConverter.ToUInt64(Array, Address);


                case "UInt32":
                    return BitConverter.ToUInt32(Array, Address);
                
                case "Int32":
                default:
                    if (!type.ToLower().Contains("int"))
                    {
                        echo($"Unknown Type \"{type}\", Treating as signed Int32");
                    }

                    return BitConverter.ToInt32(Array, Address);
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
                Log($"!!ERROR: error getting converted value for object of type \"{Type.Name}\"\n!! (provided str: {ValueAsString})");
                return;
            }


            WriteSubArray(DCFile, convertedValue, Offset);

            Changes.Add(new object[] { Offset, convertedValue });
        }






        /// <summary>
        /// Determine whether the provided object is a structure.
        /// </summary>
        public static bool ObjectIsStruct(object Object, bool CheckArrayContents = false)
        {
            if (Object == null)
            {
                echo($"Null object provided for {nameof(ObjectIsStruct)}(); defaulting to false.");
                return false;
            }

            var objectType = Object.GetType();



            // Grab the type of the elements inside of an array for struct checking (//! verify consistent functionality!!)
            if (CheckArrayContents && objectType.IsArray)
            {
                objectType = objectType.GetElementType();

                // I think this works...
                if (objectType == typeof(object))
                {
                    foreach (var item in Object as Array)
                    {
                        if (item.GetType() != typeof(object))
                        {
                            var chk = ObjectIsStruct(item);

                            if (chk)
                            {
                                echo($"WARNING: Array is a generic array, but one or more elements was a struct. Returning true for now. //!");
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }


            var @bool = !objectType.IsClass && !objectType.IsSerializable;
            echo ($"Type \"{objectType}\" is {(@bool ? string.Empty : "not ")}a struct.");
            return @bool;
        }





        public static int FindStructSize(long Address, SID Name)
        {
            // Attempt to find struct length
            //! TEMP - Replace/Optimize Me!!!
            if (ParsedSizes.Count > 0)
            {
                foreach (var entry in ParsedSizes)
                {
                    // Struct has already had it's size parsed
                    if (entry[0] == Name)
                    {
                        return (int) entry[1];
                    }
                }
            }




            var i = Address + 8;
            long firstFind = -1, secondFind = -1;
            var reverseSearchDirection = false;

            echo($"# Attempting to find size of struct \"{Name.DecodedID}\"" +
                 $"# starting @0x{Address:X}");

            for (var x = 0;; x++)
            {
                // Note: We assume the structure is at least 8 bytes in length, not including the preceeding type id located 8 bytes before the struct data
                // We also assume the structures are even layed out near eachother. I have no idea whether or not that's consistently the case, or just occasionally
                if (x >= DCFile.Length)
                {
                    throw new Exception($" -> !!! Infinite loop detected when attempting to parse dc file for size of struct \"{Name.DecodedID}\".");
                }




                if (BitConverter.ToUInt64(GetSubArray(DCFile, i), 0) == (ulong) Name.RawID)
                {
                    if (firstFind == -1)
                    {
                        echo($" -> Found another instance of struct id \"{Name.EncodedID}\"");
                        firstFind = i + 8;
                    }
                    else {
                        secondFind = i + 8;
                        break;
                    }
                }



                if (i + 0x10 >= DCFile.Length)
                    
                {
                    echo($" -> End-of-file reached; reversing search direction. ({i} >= {DCFile.Length})");
                    reverseSearchDirection = true;
                    i = Address - 0x10;
                    continue;
                }

                i += reverseSearchDirection ? -8 : 8;

                if (i <= DCModuleStartAddress)
                {
                    echo($" -> Minimum offset reached. ({i:X} <= {DCModuleStartAddress:X})"); // I just realized that's probably the only cause for that as I type this
                    break;
                }
            }


            
            if (firstFind != -1 && secondFind != -1)
            {
                // Ensure we've got a consistent result, and return it if we do
                if (firstFind - Address == (reverseSearchDirection ? Address - secondFind : secondFind - firstFind))
                {
                    var newSize = (int) (firstFind - Address);
                    echo($"# Guessed size of {newSize:X} for {Name.DecodedID}\n");
                    
                    ParsedSizes.Add(new object[] { Name, newSize });
                    return newSize;
                }

                echo($"# Offsets were different, couldn't guess size. {firstFind - Address:X} != {secondFind - firstFind:X} / {Address - secondFind:X}\n");
                return -1;
            }

            echo($"# Unable to guess struct size for {Name.DecodedID}\n");
            return -1;
        }
#endregion
#endregion (function declarations)
    }
}
