using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// Static refference to the active DC binary's header struct.
        /// </summary>
        public static DCFileHeader DCScript;
        
        
        /// <summary>
        /// SIDBase Class instance for the active sidbase.bin.
        /// </summary>
        public static SIDBase SIDBase;


        /// <summary>
        /// List of SIDBase Class instances for the active sidbase.bin lookup tables.
        /// </summary>
        public static List<SIDBase> SIDBases;


        /// <summary>
        /// A collection of known id's used in hardcoded checks, in order to handle basic operation when missing an sidbase.bin file.
        /// </summary>
        public enum KnownSIDs : ulong
        {
            UNKNOWN_SID_64 = 0x910ADC74DA2A5F6Dul,
            array = 0x4F9E14B634C6B026ul,
            symbol_array = 0xDFD21E68AC12C54Bul,
            ammo_to_weapon_array = 0xEF3BE7EF6F790D34ul,

            map = 0x080F5919176D2D91ul,

            weapon_gameplay_def = 0x6E1BB1DB85CC7806ul,
            melee_weapon_gameplay_def = 0x730ADC6EDAF0A96Dul,

            look2_def = 0xBF24E1B6BADE9DCCul,

            placeholder = 0xDEADBEEFDEADBEEFul,
        }
        #endregion

        #endregion

        






        


        //===================================\\
        //---|   Function Delcarations   |---\\
        //===================================\\
        #region [Function Delcarations]

        private static void LoadBinFile(string DCFilePath)
        {
            if (File.Exists(DCFilePath))
            {
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
        /// 
        /// </summary>
        /// <param name="sidbasePath"></param>
        /// <returns> True if the file at <paramref name="sidbasePath"/> exists. </returns>
        public static bool LoadSIDBase(string sidbasePath)
        {
            if (File.Exists(sidbasePath))
            {
                SIDBases.Add(new SIDBase(sidbasePath));
                return true;
            }
            else
            {
                MessageBox.Show($"File does not exist:\n " + sidbasePath, "Invalid path provided for desired sidbase.bin!");
                return false;
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

                Venat.OptionsMenuDropdownBtn.TabIndex -= DCScript.Entries.Length;
                Venat.MinimizeBtn.TabIndex -= DCScript.Entries.Length;
                Venat.ExitBtn.TabIndex -= DCScript.Entries.Length;
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

                // TODO: make sure there's no difference betweeen path versions! //!
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

                case KnownSIDs.weapon_gameplay_def: return new WeaponGameplayDef(DCFile, Address, name);

                case KnownSIDs.melee_weapon_gameplay_def: return new MeleeWeaponGameplayDef(DCFile, Address, name);

                case KnownSIDs.symbol_array: return new SymbolArrayDef(DCFile, Address, name);

                case KnownSIDs.ammo_to_weapon_array: return new AmmoToWeaponArray(DCFile, Address, name);

                case KnownSIDs.look2_def: return new look2(DCFile, Address, name);


                //#
                //## Unmapped Structures
                //#
                default: return new UnmappedStructure(Type, Address, name);
            }
        }










        public static object GetPropertyValueByType(Type type, int Offset)
        {
            switch (type.Name)
            {
                case "Byte":
                    return DCFile[Offset];


                case "Single":
                    return BitConverter.ToSingle(DCFile, Offset);
                case "Double":
                    return BitConverter.ToDouble(DCFile, Offset);

                    
                case "Short":
                    return BitConverter.ToInt16(DCFile, Offset);
                    
                case "Long":
                    return BitConverter.ToInt64(DCFile, Offset);

                case "Int":
                default:
                    if (type.Name != "Int")
                    {
                        echo($"Unknown Type, Treating as signed int32");
                    }

                    return BitConverter.ToInt32(DCFile, Offset);
            }
        }

        #endregion
    }
}
