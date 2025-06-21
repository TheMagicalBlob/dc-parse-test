using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


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
                    SymbolDefinitions = new List<SymbolArrayDef>();
                    break;

                

                case var _ when scriptName.ToLower().Contains("characters") && !scriptName.ToLower().Replace("_", "-").Contains("collision-settings"):



                // Unknown Script; Initialize all
                default:
                    WeaponDefinitions = new List<WeaponGameplayDef>();
                    AmmoToWeaponDefinitions = new List<AmmoToWeaponArray>();
                    SymbolDefinitions = new List<SymbolArrayDef>();
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

        /// <summary>
        /// 
        /// </summary>
        public struct DCFileHeader
        {
            public DCFileHeader(byte[] binFile, string binName)
            {
                //#
                //## Variable Initializations
                //#
                unkInt0 = 0;
                unkInt1 = 0;
                BinFileLength = 0;
                TableLength = 0;

                HeaderItems = null;

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
                var integrityCheck  = DecodeSIDHash(GetSubArray(binFile, 0x20));
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
            
            

                //#
                //## Read remaining header info
                //#
                BinFileLength = BitConverter.ToInt64(binFile, 0x8);
                TableLength = BitConverter.ToInt32(binFile, 0x14);
                HeaderItems = new DCHeaderItem[TableLength];


                //#
                //## Parse header content table
                //#
                ActiveLabel = binName + "; Reading Script...";
    #if ass
                var pre = new[] { DateTime.Now.Minute, DateTime.Now.Second };
    #endif


                PrintNL($"Parsing DC Content Table (Length: {TableLength.ToString().PadLeft(2, '0')})");
                Venat?.CTUpdateLabel(ActiveLabel);
                Venat?.InitializeDcStructListsByScriptName(binName);

                var outputLine = Venat?.GetOutputWindowLines().Length - 1 ?? 0;
                for (int tableIndex = 0, addr = 0x28; tableIndex < TableLength; tableIndex++, addr += 24)
                {
                    HeaderItems[tableIndex] = new DCHeaderItem(binFile, addr);
                    Venat?.PrintLL($"{(tableIndex < TableLength - 1 ? $"  # Structure {tableIndex} Loaded Without Error..." : "All DC Entries Loaded Successfully.")}", outputLine);
                }
                PrintNL();
            
    #if false
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

            public DCHeaderItem[] HeaderItems;
        }


        /// <summary>
        /// An individual item (module?) from the array at the beginning of the DC file.
        /// </summary>
        public struct DCHeaderItem
        {
            public DCHeaderItem(byte[] binFile, int address)
            {
                Address = address;
                Name = DecodeSIDHash(GetSubArray(binFile, address));
                Type = DecodeSIDHash(GetSubArray(binFile, address + 8));

                StructAddress = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 16), 0);
            }


            /// <summary>
            /// The Address of this Header Item in the DC file.
            /// </summary>
            public long Address;

            /// <summary>
            /// The name of the current entry in the DC file header.
            /// </summary>
            public string Name;
            
            /// <summary>
            /// The struct type of the current entry in the DC file header.
            /// </summary>
            public string Type;
            
            /// <summary>
            /// The address of the struct pointed to by tbe current dc header entry.
            /// </summary>
            public long StructAddress;
        }



        /// <summary>
        /// A collection of whatever-the-fuck naughty dog felt like including. This may be annoying.
        /// </summary>
        public struct MapDefinition
        {
            public MapDefinition(byte[] binFile, long Address, string Name)
            {
                this.Name = Name;
                this.Address = Address;

                Type = DecodeSIDHash(GetSubArray(binFile, (int)Address + 8));
                Pointer = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 16), 0);

                
                var mapLength = BitConverter.ToInt64(GetSubArray(binFile, (int)this.Address), 0);
                var mapSymbolArray = BitConverter.ToInt64(GetSubArray(binFile, (int)this.Address + 8), 0);
                long mapStructArray = BitConverter.ToInt64(GetSubArray(binFile, (int)this.Address + 16), 0);


                PrintNL($"  Found Map: [ Length: {mapLength}; Symbol Array Address: 0x{mapSymbolArray:X}; Struct Array Address: 0x{mapStructArray:X} ]\n");
                var outputLine = Venat?.GetOutputWindowLines().Length - 1 ?? 0;
                    
                for (int arrayIndex = 0; arrayIndex < mapLength; mapStructArray += 8, mapSymbolArray += 8, arrayIndex++)
                {
                    Venat?.PrintLL($"  Parsing Map Structures... {arrayIndex} / {mapLength - 1}", outputLine);


                    var structAddr = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructArray), 0);
                    var structType = DecodeSIDHash(GetSubArray(binFile, structAddr - 8));

                    LoadDCStructByType(binFile, structType, structAddr, DecodeSIDHash(GetSubArray(binFile, (int)mapSymbolArray)));
                }
                PrintNL();
            }

            public long Address;
            public string Name;
            public string Type;
            public long Pointer;


        }



        /// <summary>
        /// 
        /// </summary>
        public struct DCMapDef
        {
            public DCMapDef(byte[] binFile, string Type, long Address, string Name = "")
            {
                this.Name = Name;

                var mapLength = BitConverter.ToInt64(GetSubArray(binFile, (int)Address), 0);
                var mapNamesArrayPtr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 8), 0);
                long mapStructsArrayPtr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 16), 0);

                Items = new object[mapLength][];
                Items[0] = new object[2];

                var outputLine = Venat?.GetOutputWindowLines().Length - 1 ?? 0;
                    

                PrintNL();

                for (int arrayIndex = 0; arrayIndex < mapLength; mapStructsArrayPtr += 8, mapNamesArrayPtr += 8, arrayIndex++)
                {
                    Venat?.PrintLL($"  # Parsing Map Structures... {arrayIndex + 1} / {mapLength}", outputLine);
                    
                    var structAddr = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructsArrayPtr), 0);
                    var structType = DecodeSIDHash(GetSubArray(binFile, structAddr - 8));
                    var structName = DecodeSIDHash(GetSubArray(binFile, (int)mapNamesArrayPtr));

                    Venat?.PrintLL($"    - 0x{structAddr.ToString("X").PadLeft(6, '0')} Type: {structType} Name: {structName}", outputLine + 1);


                    Items[arrayIndex] = new object[2];

                    Items[arrayIndex][0] = structType;
                    Items[arrayIndex][1] = LoadDCStructByType(binFile, structType, structAddr, structName);
                }
                Venat?.PrintLL($"  # Finished Parsing All Map Structures.", outputLine);
            }

            
            /// <summary>
            /// The name of the map item.
            /// </summary>
            public string Name;

            /// <summary>
            /// An array of object arrays with the first element being the map item's struct type, and the other being the struct itself
            /// </summary>
            public object[][] Items;
        }



        /// <summary>
        /// 
        /// </summary>
        public struct SymbolArrayDef
        {
            public SymbolArrayDef(string name, byte[] binFile, long address)
            {
                Symbols = new List<string>();
                Hashes  = new List<byte[]>();
                Name = name;
                
                var arrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                var arrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);

                for (int i = 0; i < arrayLen; arrayAddr += 8, i++)
                {
                    Hashes.Add(GetSubArray(binFile, (int)arrayAddr));
                    Symbols.Add(DecodeSIDHash(Hashes.Last()));
                }
            }

            public string Name;
            public List<string> Symbols;
            public List<byte[]> Hashes;
        }



        /// <summary>
        /// 
        /// </summary>
        public struct AmmoToWeaponArray
        {
            public AmmoToWeaponArray(byte[] binFile, long address, string Name)
            {
                var symbols = new List<string[]>();
                var hashes  = new List<byte[][]>();
                this.Name = Name;
                
                var arrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                var arrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);

                var outputLine = Venat?.GetOutputWindowLines().Length ?? 0;

                PrintNL("\n");
                for (int i = 0; i < arrayLen; arrayAddr += 16, i++)
                {
                    Venat?.PrintLL($"  # Parsing Ammo-to-Weapon Structures... {i} / {arrayLen - 1}", outputLine);

                    hashes.Add(new[] { GetSubArray(binFile, (int)arrayAddr + 8), GetSubArray(binFile, (int)arrayAddr) });
                    symbols.Add(new[] { DecodeSIDHash(hashes.Last()[0]), DecodeSIDHash(hashes.Last()[1]) });
                }
                Venat?.PrintLL($"  # Finished Parsing Ammo-to-Weapon Structures.", outputLine);

                Symbols = symbols.ToArray();
                Hashes = hashes.ToArray();
            }

            public string Name;


            public string[][] Symbols;
            public byte[][][] Hashes;
        }

    

        /// <summary>
        /// 
        /// </summary>
        public struct WeaponGameplayDef
        {
            public WeaponGameplayDef(byte[] binFile, long Address, string Name)
            {
                //#
                //## Variable Declarations
                //#
                this.Name = Name;
                this.Address = Address;

            
                UnknownInt_0_at0x00 = 0;
                UnknownInt_1_at0x04 = 0;
                UnknownInt_2_at0x08 = 0;
                UnknownFloat_0_at0x0C = 0;

                FirearmGameplayDefAddress = 0;
                    AmmoCount = 0;

                MeleeGameplayDefAddress = 0;
                
                BlindfireAutoTargetDef = 0;
                
                Hud2ReticleDefAddress = 0;
                    Hud2ReticleName = string.Empty;
                    Hud2SimpleReticleName = string.Empty;
                
                UnknownByteArray_0_at0x50 = null;
                ZoomCameraDoFSettingsSP = "none";
                ScreenEffectSettings = 0;
                UnknownByteArray_1_at0x88 = null;


                

                //#
                //## Parse Weapon Gameplay Definition
                //#

                // Load firearm-related variables
                FirearmGameplayDefAddress = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + firearmGameplayDef), 0);
                
                if (FirearmGameplayDefAddress != 0)
                {
                    AmmoCount = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)FirearmGameplayDefAddress + ammoCount), 0);
                }
            

                MeleeGameplayDefAddress = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + meleeGameplayDef), 0);


                Hud2ReticleDefAddress = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + hud2ReticleDef), 0);
                
                if (Hud2ReticleDefAddress != 0)
                {
                    // Read Hud2 Reticle Name
                    for (var i = BitConverter.ToInt64(GetSubArray(binFile, (int)Hud2ReticleDefAddress + hud2ReticleDefNameOffset), 0); binFile[i] != 0;)
                    {
                        Hud2ReticleName += (char)binFile[i++];
                    }
                    // Read Hud2 Simple Reticle Name
                    for (var i = BitConverter.ToInt64(GetSubArray(binFile, (int)Hud2ReticleDefAddress + hud2ReticleDefSimpleNameOffset), 0); binFile[i] != 0;)
                    {
                        Hud2SimpleReticleName += (char)binFile[i++];
                    }
                }
            }


            //#
            //## Private Members
            //#
            /// <summary>
            /// Weapon Gameplay Definition structure offset.
            /// </summary>
            private const byte
                unknownInt_0_at0x00 = 0x00, // unknown
                unknownInt_1_at0x04 = 0x04, // unknown
                unknownInt_2_at0x08 = 0x08, // unknown
                unknownFloat_0_at0x0C = 0x0C, // unknown, usually set to -1, but the bow has it set to zero

                firearmGameplayDef = 0x10, // firearm-gameplay-def*
                    ammoCount = 0x98,
                
                blindfireAutoTargetDef = 0x18, // blindfire-auto-target-def

                meleeGameplayDef = 0x30, // melee-gameplay-def*

                hud2ReticleDef = 0x58, // hud2-reticle-def*
                    hud2ReticleDefNameOffset = 0x8,
                    hud2ReticleDefSimpleNameOffset = 0x18,

                unknownByteArray_0_at0x50 = 0x50, // unknown
                zoomCameraDoFSettingsSP = 0x68, // *zoom-camera-dof-settings-sp*
                screenEffectSettings = 0x80, // screen-effect-settings*
                unknownByteArray_1_at0x88 = 0x88 // unknown
            ;



            
            //#
            //## Public Members (heh)
            //#
            public string Name;
            public long Address;


            public int UnknownInt_0_at0x00;
            public int UnknownInt_1_at0x04;
            public int UnknownInt_2_at0x08;
            public float UnknownFloat_0_at0x0C;

            public long FirearmGameplayDefAddress;
                public int AmmoCount;

            public long BlindfireAutoTargetDef;

            public long MeleeGameplayDefAddress;

            public long Hud2ReticleDefAddress;
                public string Hud2ReticleName;
                public string Hud2SimpleReticleName;
            
            public byte[] UnknownByteArray_0_at0x50;
            public string ZoomCameraDoFSettingsSP;
            public long ScreenEffectSettings;
            public byte[] UnknownByteArray_1_at0x88;
        }



        /// <summary>
        /// 
        /// </summary>
        public struct FirearmGameplayDef
        {
            public FirearmGameplayDef(byte[] binFile, long Address, string Name = "unnamed")
            {
                //#
                //## Variable Declarations
                //#
                // TODO:
                // - Remove the initializations for variables once code to read said variable has been added
                this.Name = Name;
                this.Address = Address;

                AmmoTypes_Ptr = 0;
                UnknownFloat_at0x14 = 0;
                UnknownFloat_at0x18 = 0;
                UnknownFloat_at0x24 = 0;
                UnknownFloat_at0x28 = 0;
                UnknownInt_at0x20 = 0;
                UnknownFloat_at0x2C = 0;
                UnknownFloat_at0x30 = 0;
                UnknownFloat_at0x48 = 0;
                UnknownFloat_at0x50 = 0;
                UnknownFloat_at0x54 = 0;
                UnknownFloat_at0x60 = 0;
                UnknownInt_at0x68 = 0;
                UnknownFloat_at0x6C = 0;
                UnknownFloat_at0x70 = 0;
                UnknownFloat_at0x74 = 0;
                UnknownFloat_at0x78 = 0;
                UnknownFloat_at0x7C = 0;
                UnknownFloat_at0x80 = 0;
                UnknownFloat_at0x84 = 0;
                UnknownFloat_at0x88 = 0;
                UnknownFloat_at0x8C = 0;
                UnknownFloat_at0xA0 = 0;
                UnknownFloat_at0xA4 = 0;
                UnknownFloat_at0xA8 = 0;
                UnknownFloat_at0xAC = 0;
                ProneAimSID = 0;
                ScopedLagSettings_Ptr = 0;
                UnknownFloat_at0xC0 = 0;
                UnknownFloat_at0xC4 = 0;
                UnknownFloat_at0xC8 = 0;
                UnknownFloat_at0xCC = 0;
                AmmoCount = 0;
                FirearmAimDeviationDef0_Ptr = 0;
                FirearmAimDeviationDef1_Ptr = 0;
                UnknownFloat_at0xE0 = 0;
                UnknownFloat_at0xE4 = 0;
                UnknownFloat_at0xE8 = 0;
                FirearmKickbackDef0_Ptr = 0;
                FirearmKickbackDef1_Ptr = 0;
                FirearmKickbackDef2_Ptr = 0;
                FirearmKickbackDef3_Ptr = 0;
                UnknownFloat_at0x118 = 0;
                UnknownFloat_at0x120 = 0;
                LerpAimSwaySettings0_Ptr = 0;
                LerpAimSwaySettings1_Ptr = 0;
                LerpAimSwaySettings2_Ptr = 0;
                SwayHoldBreathSettings0_Ptr = 0;
                SwayHoldBreathSettings1_Ptr = 0;
                UnknownInt_at0x158 = 0;
                UnknownFloat_at0x15C = 0;
                UnknownFloat_at0x160 = 0;
                UnknownFloat_at0x164 = 0;
                UnknownFloat_at0x168 = 0;
                UnknownFloat_at0x16C = 0;
                UnknownInt_at0x194 = 0;
                UnknownFloat_at0x19C = 0;
            }


            /// <summary>
            /// Firearm Gameplay Definition structure offset.
            /// </summary>
            private const int
                ammoTypes_Ptr = 0x00, // symbol-array containing ammo type names
                unknownFloat_at0x14 = 0x14,
                unknownFloat_at0x18 = 0x18,
                unknownFloat_at0x24 = 0x24,
                unknownFloat_at0x28 = 0x28,
                unknownInt_at0x20 = 0x20,
                unknownFloat_at0x2C = 0x2C,
                unknownFloat_at0x30 = 0x30,
                unknownFloat_at0x48 = 0x48,
                unknownFloat_at0x50 = 0x50,
                unknownFloat_at0x54 = 0x54,
                unknownFloat_at0x60 = 0x60,
                unknownInt_at0x68 = 0x68,
                unknownFloat_at0x6C = 0x6C,
                unknownFloat_at0x70 = 0x70,
                unknownFloat_at0x74 = 0x74,
                unknownFloat_at0x78 = 0x78,
                unknownFloat_at0x7C = 0x7C,
                unknownFloat_at0x80 = 0x80,
                unknownFloat_at0x84 = 0x84,
                unknownFloat_at0x88 = 0x88,
                unknownFloat_at0x8C = 0x8C,
                unknownFloat_at0xA0 = 0xA0,
                unknownFloat_at0xA4 = 0xA4,
                unknownFloat_at0xA8 = 0xA8,
                unknownFloat_at0xAC = 0xAC,
                proneAimSID = 0xB8,
                scopedLagSettings_Ptr = 0xB0, // scoped-lag-settings*
                unknownFloat_at0xC0 = 0xC0,
                unknownFloat_at0xC4 = 0xC4,
                unknownFloat_at0xC8 = 0xC8,
                unknownFloat_at0xCC = 0xCC,
                ammoCount = 0x98, // integer (long or int?) amount of base ammo
                firearmAimDeviationDef0_Ptr = 0xD0, // firearm-aim-deviation-def
                firearmAimDeviationDef1_Ptr = 0xD8, // firearm-aim-deviation-def
                unknownFloat_at0xE0 = 0xE0, // unknown float
                unknownFloat_at0xE4 = 0xE4, // unknown float
                unknownFloat_at0xE8 = 0xE8, // unknown float
                firearmKickbackDef0_Ptr = 0xF0, // firearm-kickback-def*
                firearmKickbackDef1_Ptr = 0xF8, // firearm-kickback-def*
                firearmKickbackDef2_Ptr = 0x108, // firearm-kickback-def*
                firearmKickbackDef3_Ptr = 0x110, // firearm-kickback-def*
                unknownFloat_at0x118 = 0x118, // unknown float
                unknownFloat_at0x120 = 0x120, // unknown float
                lerpAimSwaySettings0_Ptr = 0x128, // lerp-aim-sway-settings
                lerpAimSwaySettings1_Ptr = 0x130, // lerp-aim-sway-settings
                lerpAimSwaySettings2_Ptr = 0x140, // lerp-aim-sway-settings
                swayHoldBreathSettings0_Ptr = 0x148, // sway-hold-breath-settings*
                swayHoldBreathSettings1_Ptr = 0x150, // sway-hold-breath-settings*
                unknownInt_at0x158 = 0x158,
                unknownFloat_at0x15C = 0x15C,
                unknownFloat_at0x160 = 0x160, // unknown float
                unknownFloat_at0x164 = 0x164, // unknown float
                unknownFloat_at0x168 = 0x168, // unknown float
                unknownFloat_at0x16C = 0x16C, // unknown float
                unknownInt_at0x194 = 0x194,
                unknownFloat_at0x19C = 0x19C    
            ;

            //#
            //## Private Members
            //#




            //#
            //## Public Members
            //#
            public string Name;
            public long Address;

            public ulong AmmoTypes_Ptr; // symbol-array containing ammo type names
            public float UnknownFloat_at0x14;
            public float UnknownFloat_at0x18;
            public float UnknownFloat_at0x24;
            public float UnknownFloat_at0x28;
            public int   UnknownInt_at0x20;
            public float UnknownFloat_at0x2C;
            public float UnknownFloat_at0x30;
            public float UnknownFloat_at0x48;
            public float UnknownFloat_at0x50;
            public float UnknownFloat_at0x54;
            public float UnknownFloat_at0x60;
            public int   UnknownInt_at0x68;
            public float UnknownFloat_at0x6C;
            public float UnknownFloat_at0x70;
            public float UnknownFloat_at0x74;
            public float UnknownFloat_at0x78;
            public float UnknownFloat_at0x7C;
            public float UnknownFloat_at0x80;
            public float UnknownFloat_at0x84;
            public float UnknownFloat_at0x88;
            public float UnknownFloat_at0x8C;
            public float UnknownFloat_at0xA0;
            public float UnknownFloat_at0xA4;
            public float UnknownFloat_at0xA8;
            public float UnknownFloat_at0xAC;
            public ulong ProneAimSID;
            public ulong ScopedLagSettings_Ptr; // scoped-lag-settings*
            public float UnknownFloat_at0xC0;
            public float UnknownFloat_at0xC4;
            public float UnknownFloat_at0xC8;
            public float UnknownFloat_at0xCC;
            public long AmmoCount; // integer (long or int?) amount of base ammo
            public ulong FirearmAimDeviationDef0_Ptr; // firearm-aim-deviation-def
            public ulong FirearmAimDeviationDef1_Ptr; // firearm-aim-deviation-def
            public float UnknownFloat_at0xE0; // Unknown float
            public float UnknownFloat_at0xE4; // Unknown float
            public float UnknownFloat_at0xE8; // Unknown float
            public ulong FirearmKickbackDef0_Ptr; // firearm-kickback-def*
            public ulong FirearmKickbackDef1_Ptr; // firearm-kickback-def*
            public ulong FirearmKickbackDef2_Ptr; // firearm-kickback-def*
            public ulong FirearmKickbackDef3_Ptr; // firearm-kickback-def*
            public float UnknownFloat_at0x118; // Unknown float
            public float UnknownFloat_at0x120; // Unknown float
            public ulong LerpAimSwaySettings0_Ptr; // lerp-aim-sway-settings
            public ulong LerpAimSwaySettings1_Ptr; // lerp-aim-sway-settings
            public ulong LerpAimSwaySettings2_Ptr; // lerp-aim-sway-settings
            public ulong SwayHoldBreathSettings0_Ptr; // sway-hold-breath-settings*
            public ulong SwayHoldBreathSettings1_Ptr; // sway-hold-breath-settings*
            public int UnknownInt_at0x158;
            public float UnknownFloat_at0x15C;
            public float UnknownFloat_at0x160; // Unknown float
            public float UnknownFloat_at0x164; // Unknown float
            public float UnknownFloat_at0x168; // Unknown float
            public float UnknownFloat_at0x16C; // Unknown float
            public int UnknownInt_at0x194;
            public float UnknownFloat_at0x19C;
        }


        public struct Hud2ReticleDef
        {
            public Hud2ReticleDef(byte[] binFile, long Address, string Name = "unnamed")
            {
                this.Name = Name;
                this.Address = Address;
            }

            public string Name;
            public long Address;
        }


        /// <summary>
        /// 
        /// </summary>
        public struct MeleeWeaponGameplayDef
        {
            public MeleeWeaponGameplayDef(byte[] binFile, long Address, string Name = "unnamed")
            {
                this.Name = Name;
                this.Address = Address;
            }

            public string Name;
            public long Address;
        }



        /// <summary>
        /// 
        /// </summary>
        public struct Look2Def
        {
            public Look2Def(byte[] binFile, long Address, string Name = "unnamed")
            {
                this.Name = Name;
                this.Address = Address;
            }

            public string Name;
            public long Address;
        }
        #endregion
    }
}
