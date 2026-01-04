using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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
        /// Static reference to the active DC binary's header struct.
        /// </summary>
        public static DCModule ActiveDCScript;


        
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


        public static readonly Type[] AdditionalDataTypes = new []
        {
            typeof(Array),
            typeof(SID),
            typeof(weapon_gameplay_def)
        };




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
            SetReloadCloseButtonsEnabledStatus(false);

            ResetSelectionLabel();
            ResetStatusLabel();

            
            DCFile = null;
            
            Panels?.ResetPanels();
        }



        private static void CTCloseBinFile()
        {
            Venat?.Invoke(Venat.CloseBinFileMammet);
        }

        




        private void StartBinParseThread()
        {
            if (DCFileHandlerThread != null && DCFileHandlerThread.ThreadState != System.Threading.ThreadState.Unstarted)
            {
                try
                {
                    echo("Bin thread already active, killing thread.");
                    DCFileHandlerThread.Abort();
                }
                catch (ThreadAbortException) { echo("Bin thread killed."); }
                catch (Exception dang) { echo($"Unexpected error of type \"{dang.GetType()}\" thrown when aborting bin thread."); }
                echo();
            }


            // Create and start the thread
            DCFileHandlerThread = new Thread(DCFileHandlerFunction)
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
            //#
            //## Load provided DC file.
            //#
            DCFile = File.ReadAllBytes(FilePath);

            // Check whether or not the script is a basic empty one  TODO: make sure there's no difference between path versions! //!
            if (SHA256.Create().ComputeHash(DCFile).SequenceEqual(EmptyDCFileHash))
            {
                UpdateStatusLabel(new[] { "Empty DC File Loaded." });
                ResetSelectionLabel();
                return;
            }

            // Parse the script's header entries
            ActiveDCScript = new DCModule(DCFile, ActiveFileName);



            //#
            //## Setup Form
            //#
            echo("\nFinished!");
            UpdateStatusLabel(new[] { "Finished Loading dc File, populating properties panel...", emptyStr, emptyStr });
            PopulatePropertiesPanelWithHeaderItemContents(ActiveFileName, ActiveDCScript);

            SetReloadCloseButtonsEnabledStatus(true);
            UpdateStatusLabel(new[] { "Viewing Script" });
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
                case KnownSIDs.map: return new map(DCFile, Address, name); // map == [ struct len, sid[]* ids, struct*[] * data ]

                case KnownSIDs.weapon_gameplay_def: return new weapon_gameplay_def(DCFile, Address, name);

                case KnownSIDs.melee_weapon_gameplay_def: return new MeleeWeaponGameplayDef(DCFile, Address, name);

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
        //## 
        //#
        #region []

        /// <summary>
        /// Get a sub-array of the specified <paramref name="length"/> from a larger <paramref name="array"/> of bytes, starting at the <paramref name="Address"/> specified.
        /// </summary>
        /// <param name="array"> The array from which to take the sub-array. </param>
        /// <param name="Address"> The start address of the sub-array within <paramref name="array"/>. </param>
        /// <param name="length"> The length of the sub-array. </param>
        /// <returns> What the hell do you think. </returns>
        public static byte[] GetSubArray(byte[] array, int Address, int length = 8)
        {
            if (length == 0)
            {
                return Array.Empty<byte>();
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

            if (startAddress > buffer.Length)
            {
                return string.Empty;
            }

            do {
                str += (char) buffer[startAddress++];
            }
            while (startAddress < buffer.Length && buffer[startAddress] != terminator);

            return str;
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


            // Make sure I haven't accidentally passed property info instead again
            if (objectType.Name.Contains("PropertyInfo"))
            {
                throw new Exception($"{nameof(ObjectIsStruct)} called with object passed incorrectly.");
            }



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
        #endregion
        #endregion (function declarations)
    }
}
