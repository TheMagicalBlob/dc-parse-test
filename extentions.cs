using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using static NaughtyDogDCReader.Main;

namespace NaughtyDogDCReader
{
    //=====================================\\
    //---|   Custom Class Extensions   |---\\
    //=====================================\\
    #region [Custom Class Extensions]

    /// <summary>
    /// Custom RichTextBox class because bite me.
    /// </summary>
    public class RichTextBox : System.Windows.Forms.RichTextBox
    {
        /// <summary>
        /// Appends Text to The Current Text of A Text Box, Followed By The Standard Line Terminator.
        /// <br/>Scrolls To Keep The Newest Line In View.
        /// </summary>
        /// <param name="str"> The String to Output. </param>
        public void AppendLine(string str = "", bool scroll = true)
        {
            AppendText(str + '\n');
            Update();

            if (scroll)
            {
                ScrollToCaret();
            }
        }



        public void UpdateLine(string newMsg, int line, bool scroll = true)
        {
            while (line >= Lines.Length)
            {
                AppendText("\n");
            }

            var lines = Lines;
            lines[line] = newMsg ?? " ";

            Lines = lines;
            Update();

            if (scroll)
            {
                ScrollToCaret();
            }
        }
    }



    public class GroupBox : System.Windows.Forms.GroupBox
    {
        public GroupBox() : base()
        {
            Paint += RemoveGroupBoxBorderAndText;
        }

        private void RemoveGroupBoxBorderAndText(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.Clear(((GroupBox) sender).BackColor);
        }

        public static readonly int GroupBoxContentsOffset = 7;
    }



    /// <summary>
    /// Custom TextBox Class to Better Handle Default TextBox Contents.
    /// </summary>
    public class TextBox : System.Windows.Forms.TextBox
    {
        /// <summary> Create a new winforms TextBox control. </summary>
        public TextBox()
        {

        }


        public override string Text
        {
            get => base.Text;

            set => base.Text = value?.Replace("\"", string.Empty);
        }
    }





    public class Label : System.Windows.Forms.Label
    {
        public bool IsSeparatorLine { get; set; } = false;


        public bool StretchToFitForm
        {
            get => _stretchToFitForm & IsSeparatorLine;
            set => _stretchToFitForm = value;
        }
        private bool _stretchToFitForm = false;
    }


    /// <summary>
    /// Custom Button class extension for use of additional PropertyWindow-Specific features.<br/>
    /// (DCProperty property for better readablity, rather than using the "Tag" property)
    /// </summary>
    public class PropertyButton : System.Windows.Forms.Button
    {
        public PropertyButton()
        {
            SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
        }


        /// <summary>
        /// The property associated with the current PropertyWindow button;
        /// </summary>
        public object DCProperty
        {
            get => _dcProperty;

            set {
                _dcProperty = value;
            }
        }

        private object _dcProperty;
    }





    





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
                throw new FileNotFoundException("The file at the provided path does not exist, please ensure that you're not a complete moron.");
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
#endregion
}
