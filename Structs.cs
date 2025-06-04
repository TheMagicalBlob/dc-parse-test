using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static weapon_data.Main;


namespace weapon_data
{
    public partial class Main
    {

        /// <summary>
        /// Initialize Lists for the expected structures in a loaded script, or all if they changed the file name more than I could be bothered to account for.
        /// <br/> (I should probably just hash that data at the bottom or some shit, needs checking)
        /// </summary>
        /// <param name="scriptName"> The file name of the loaded script </param> //! TODO: find some bit you can scan to determine the script without forcing an expected file name 
        public void InitializeDcStructListsByScriptName(string scriptName)
        {
            UnknownDefinitions = new List<string>();

            switch (scriptName)
            {
                case var _ when scriptName.ToLower().Replace("_", "-").Contains("weapon-gameplay"):
                    WeaponDefinitions = new List<WeaponGameplayDef>();
                    AmmoToWeaponDefinitions = new List<AmmoToWeaponArray>();
                    break;

                

                case var _ when scriptName.ToLower().Contains("characters") && !scriptName.ToLower().Replace("_", "-").Contains("collision-settings"):



                // Unknown Script; Initialize all
                default:
                    WeaponDefinitions = new List<WeaponGameplayDef>();
                    break;
            }
        }


        public List<WeaponGameplayDef> WeaponDefinitions;
        public List<AmmoToWeaponArray> AmmoToWeaponDefinitions;
        public List<SymbolArrayDef> SymbolDefinitions;

        public List<string> UnknownDefinitions;

        
        //=================================\\
        //--|   Structure Definitions   |--\\
        //=================================\\
        #region [Structure Definitions]

        public struct DCFileHeader
        {
            void PrintNL(string str = "") => Venat.PrintNL(str);

            public DCFileHeader(string binName, byte[] binFile)
            {
                //#
                //## Variable Initializations
                //#
                unkInt0 = 0;
                unkInt1 = 0;
                BinFileLength = 0;
                TableLength = 0;

                DCStructures = null;

                //#
                //## Read file magic from header
                //#
                if (!binFile.Take(8).ToArray().SequenceEqual(new byte[] { 0x30, 0x30, 0x43, 0x44, 0x01, 0x00, 0x00, 0x00 }))
                {
                    echo($"ERROR; Invalid File Provided: Invalid file magic.");
                    return;
                }

            
                //#
                //## Run a few basic integrity checks
                //#
                var integrityCheck  = Venat.DecodeSIDHash(Venat.GetSubArray(binFile, 0x20));
                if (integrityCheck != "array")
                {
                    echo($"ERROR; Unexpected SID \"{integrityCheck}\" at 0x20, aborting.");
                    return;
                }
                if ((unkInt0 = BitConverter.ToInt32(binFile, 0x10)) != 1)
                {
                    echo($"ERROR; Unexpected Value \"{unkInt0}\" read at 0x10, aborting.");
                    return;
                }
                if ((unkInt1 = BitConverter.ToInt64(binFile, 0x18)) != 0x28)
                {
                    echo($"ERROR; Unexpected Value \"{unkInt1}\" read at 0x18, aborting.");
                    return;
                }
            
                if (Venat.abort) {
                    return;
                }

            
                //#
                //## Read remaining header info
                //#
                BinFileLength = BitConverter.ToInt64(binFile, 0x8);
                TableLength = BitConverter.ToInt32(binFile, 0x14);
                DCStructures = new DCStructEntry[TableLength];



                //#
                //## Parse header content table
                //#
                ActiveLabel = binName + "; Reading Script...";
    #if DEBUG
                var pre = new[] {DateTime.Now.Minute, DateTime.Now.Second};
    #endif


                PrintNL($"Parsing DC Content Table (Length: {TableLength.ToString().PadLeft(2, '0')})");
                Venat.CTUpdateLabel(ActiveLabel);
                Venat.InitializeDcStructListsByScriptName(binName);

                for (int tableIndex = 0, addr = 0x28; tableIndex < TableLength && !Venat.abort; tableIndex++, addr += 24)
                {
                    DCStructures[tableIndex] = new DCStructEntry(binFile, addr);
                    echo ($"Structure {tableIndex} Loaded Without Error.{(tableIndex < TableLength - 1 ? ".." : "")}");
                }
            
    #if DEBUG
                echo ($"{DateTime.Now.Minute - pre[0]}:{DateTime.Now.Second - pre[1]}");
    #endif
            }
            
            //#
            //## Variable Declarations
            //#
            private readonly int unkInt0;
            private readonly long unkInt1;

            public long BinFileLength;
            public int TableLength;

            public DCStructEntry[] DCStructures;
        }

        public struct DCStructEntry
        {
            public DCStructEntry(byte[] binFile, int address)
            {
                Address = address;

                Name = Venat.DecodeSIDHash(Venat.GetSubArray(binFile, address));
                Type = Venat.DecodeSIDHash(Venat.GetSubArray(binFile, address + 8));
                Pointer = BitConverter.ToInt64(Venat.GetSubArray(binFile, address + 16), 0);
            }

            public long Address;
            public string Name;
            public string Type;
            public long Pointer;
        }

        public struct SymbolArrayDef
        {
            public SymbolArrayDef(string name, byte[] binFile, long address)
            {
                Symbols = new List<string>();
                Hashes  = new List<long>();
                Name = name;
                
                var arrayLen = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)address), 0);
                var arrayAddr = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)address + 8), 0);

                PrintNL($"  Array Length: {arrayLen} ");
                for (int i = 0; i < arrayLen && !Venat.abort; arrayAddr += 16, i++)
                {
                    Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)arrayAddr));
                }
                PrintNL("");
            }

            public string Name;
            public List<string> Symbols;
            public List<long> Hashes;
        }

        public struct DCMapStruct
        {

        }

        public struct AmmoToWeaponArray
        {
            public AmmoToWeaponArray(byte[] binFile, long address)
            {
                Symbols = new List<string>();
                Hashes  = new List<long>();

                
                var arrayLen = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCFile.DCStructures[tableIndex].Pointer), 0);
                var arrayAddr = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)DCFile.DCStructures[tableIndex].Pointer + 8), 0);

                PrintNL($"  Array Length: {arrayLen} ");
                for (int i = 0; i < arrayLen && !Venat.abort; arrayAddr += 16, i++)
                {
                    Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)arrayAddr + 8)) Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)arrayAddr))
                    Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)arrayAddr + 8)) Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)arrayAddr))
                }
                PrintNL("");
            }

            public List<string> Symbols;
            public List<long> Hashes;
        }


    

        public struct WeaponGameplayDef
        {
            public WeaponGameplayDef(string name, byte[] binFile, long address)
            {
                //#
                //## Variable Initializations
                //#
                Name = name;
                AmmoCount = 0;
                Address = (int)address;
                IsFirearm = false;
            
                FirearmGameplayDefAddress = 0;
                MeleeGameplayDefAddress = 0;
                Hud2ReticleDefAddress = 0;
                Hud2ReticleName = string.Empty;
                Hud2SimpleReticleName = string.Empty;

            
                //#
                //## Local Variable Declarations
                //#

            

                
                //#
                //## Parse Weapon Gameplay Definition
                //#
                // Load firearm-related variables
                FirearmGameplayDefAddress = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)address + FirearmGameplayDefOffset), 0);
                if (FirearmGameplayDefAddress != 0)
                {
                    if (Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)FirearmGameplayDefAddress - 8)) != "firearm-gameplay-def")
                    {
                        echo($"ERROR Parsing Firearm Gameplay Definition at {FirearmGameplayDefAddress:X}; unexpected id. ({Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)FirearmGameplayDefAddress - 8)) != "firearm-gameplay-def"})");
                    }

                    IsFirearm = true;
                    AmmoCount = (int)BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)FirearmGameplayDefAddress + AmmoCountOffset), 0);
                }
            


                MeleeGameplayDefAddress = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)address + MeleeGameplayDefOffset), 0);
                if (MeleeGameplayDefAddress != 0)
                {
                    if (Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)MeleeGameplayDefAddress - 8)) != "melee-weapon-gameplay-def")
                    {
                        echo($"ERROR Parsing Melee Weapon Gameplay Definition at {MeleeGameplayDefAddress:X}; unexpected id. ({Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)MeleeGameplayDefAddress - 8)) != "melee-weapon-gameplay-def"})");
                    }
                }


                Hud2ReticleDefAddress = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)address + Hud2ReticleDefOffset), 0);
                if (Hud2ReticleDefAddress != 0)
                {
                    if (Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)Hud2ReticleDefAddress - 8)) != "hud2-reticle-def")
                    {
                        echo($"ERROR Parsing Hud2 Reticle Definition at {Hud2ReticleDefAddress:X}; unexpected id. ({Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)Hud2ReticleDefAddress - 8))} != hud2-reticle-def)");
                    }
                    echo($"Fuck Sake: {Hud2ReticleDefAddress:X}");


                    // Read Hud2 Reticle Name
                    for (var i = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)Hud2ReticleDefAddress + Hud2ReticleDefNameOffset), 0); binFile[i] != 0; Hud2ReticleName += (char)binFile[i++]);
                    // Read Hud2 Simple Reticle Name
                    for (var i = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)Hud2ReticleDefAddress + Hud2ReticleDefSimpleNameOffset), 0); binFile[i] != 0; Hud2SimpleReticleName += (char)binFile[i++]);
                }
            }

            public string Name;
            public int Address;
            public bool IsFirearm;

            public long FirearmGameplayDefAddress;
            public long MeleeGameplayDefAddress;
            public long Hud2ReticleDefAddress;
            public string Hud2ReticleName;
            public string Hud2SimpleReticleName;


            public int AmmoCount;
        }

        public struct Look2Def
        {
            public Look2Def(string name, byte[] binFile, long address)
            {

            }
        }
        #endregion
    }
}
