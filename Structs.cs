using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
    #if false
                var pre = new[] { DateTime.Now.Minute, DateTime.Now.Second };
    #endif


                echo($"Parsing DC Content Table (Length: {TableLength.ToString().PadLeft(2, '0')})\n ");


                Venat?.CTUpdateStatusLabel("Reading Script...");
                

                Venat?.InitializeDcStructListsByScriptName(binName);

                for (int tableIndex = 0, addr = 0x28; tableIndex < TableLength; tableIndex++, addr += 24)
                {
                    HeaderItems[tableIndex] = new DCHeaderItem(binFile, addr);
                    echo($"{(tableIndex < TableLength - 1 ? $"  # Structure {tableIndex} Loaded Without Error..." : "All DC Entries Loaded Successfully.")}");
                }
                echo();
            
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
        public struct DCMapDef
        {
            public DCMapDef(byte[] binFile, long Address, string Name)
            {
                long mapLength;
                this.Name = Name;
                Items = new object[mapLength = BitConverter.ToInt64(GetSubArray(binFile, (int)Address), 0)][];
                var mapNamesArrayPtr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 8), 0);
                long mapStructsArrayPtr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 16), 0);


                if (mapLength < 1)
                {
                    echo($"  # Empty Map Structures.");
                    return;
                }
                Items[0] = new object[2];

                    

                echo();

                for (int arrayIndex = 0; arrayIndex < mapLength; mapStructsArrayPtr += 8, mapNamesArrayPtr += 8, arrayIndex++)
                {
                    echo($"  # Parsing Map Structures... {arrayIndex + 1} / {mapLength}");
                    
                    var structAddr = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructsArrayPtr), 0);
                    var structType = DecodeSIDHash(GetSubArray(binFile, structAddr - 8));
                    var structName = DecodeSIDHash(GetSubArray(binFile, (int)mapNamesArrayPtr));

                    echo($"    - 0x{structAddr.ToString("X").PadLeft(6, '0')} Type: {structType} Name: {structName}" + 1);


                    Items[arrayIndex] = new object[2];

                    Items[arrayIndex][0] = structType;
                    Items[arrayIndex][1] = LoadDCStructByType(binFile, structType, structAddr, structName);
                }
                echo($"  # Finished Parsing All Map Structures.");
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
            public SymbolArrayDef(byte[] binFile, long Address, string Name)
            {
                this.Name = Name;
                this.Address = Address;

                Symbols = new List<string>();
                Hashes  = new List<long>();


                var arrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)Address), 0);
                var arrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 8), 0);

                for (int i = 0; i < arrayLen; arrayAddr += 8, i++)
                {
                    var dat = GetSubArray(binFile, (int)arrayAddr);

                    Hashes.Add(BitConverter.ToInt64(dat, 0));
                    Symbols.Add(DecodeSIDHash(dat));
                }
            }


            public string Name;
            public long Address;

            public List<string> Symbols;
            public List<long> Hashes;
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

                echo("\n");
                for (int i = 0; i < arrayLen; arrayAddr += 16, i++)
                {
                    echo($"  # Parsing Ammo-to-Weapon Structures... {i} / {arrayLen - 1}");

                    hashes.Add(new[] { GetSubArray(binFile, (int)arrayAddr + 8), GetSubArray(binFile, (int)arrayAddr) });
                    symbols.Add(new[] { DecodeSIDHash(hashes.Last()[0]), DecodeSIDHash(hashes.Last()[1]) });
                }
                echo($"  # Finished Parsing Ammo-to-Weapon Structures.");

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
                //## Variable Initializations
                //#
                this.Name = Name;
                this.Address = Address;

                # region [variable initializations]
				UnknownInt_at0x00 = 0;
				UnknownInt_at0x04 = 0;
				UnknownInt_at0x08 = 0;
				UnknownFloat_at0x0C = 0;
				UnknownLong_at0x20 = 0;
				UnknownLong_at0x38 = 0;
				UnknownLong_at0x40 = 0;
				UnknownLong_at0x48 = 0;
				UnknownByteArray_at0x50 = null;
				UnknownLong_at0x60 = 0;
				UnknownLong_at0x78 = 0;
				UnknownByteArray_at0x88 = null;
			
			
				ZoomCameraDoFSettingsSP = 0;
				ZoomSniperCameraDoFSettingsSP = 0;
				ScreenEffectSettings = 0;
				#endregion [variable initializations]


                

                //#
                //## Parse Weapon Gameplay Definition
                //#
                
                // Load firearm-related variables
                FirearmGameplayDefinition = new FirearmGameplayDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int)Address + firearmGameplayDef_Ptr), 0), Name);
                
                MeleeWeaponGameplayDefinition = new MeleeWeaponGameplayDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int)Address + meleeGameplayDef_Ptr), 0), Name);

                BlindfireAutoTargetDefinition = new BlindfireAutoTargetDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int)Address + blindfireAutoTargetDef_Ptr), 0), Name);

                GrenadeGameplayDefinition = new GrenadeGameplayDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int)Address + grenadeGameplayDef_Ptr), 0), Name);

                Hud2ReticleDefinition = new Hud2ReticleDef(binFile,  BitConverter.ToInt64(GetSubArray(binFile, (int)Address + hud2ReticleDef_Ptr), 0), Name);
                
            }


            //#
            //## Variable Declarations
            //#
            //# Private Members
            /// <summary> Weapon Gameplay Definition structure offset. </summary>
			private const int
				# region [offsets]
				unknownInt_at0x00 = 0x00, // unknown uint
				unknownInt_at0x04 = 0x04, // unknown uint
				unknownInt_at0x08 = 0x08, // unknown uint
				
				unknownFloat_at0x0C = 0x0C, // unknown, usually set to -1, but the bow has it set to zero
				firearmGameplayDef_Ptr = 0x10, // firearm-gameplay-def*
				blindfireAutoTargetDef_Ptr = 0x18, // blindfire-auto-target-def*
				
				unknownLong_at0x20 = 0x20, // unknown ulong
				grenadeGameplayDef_Ptr = 0x28, // grenade-gameplay-def*
				
				meleeGameplayDef_Ptr = 0x30, // melee-gameplay-def*
				
				unknownLong_at0x38 = 0x38, // unknown ulong
				unknownLong_at0x40 = 0x40, // unknown ulong
				unknownLong_at0x48 = 0x48, // unknown ulong
				unknownByteArray_at0x50 = 0x50, // unknown byte[]
				
				hud2ReticleDef_Ptr = 0x58, // hud2-reticle-def*
				unknownLong_at0x60 = 0x60, // unknown ulong
				zoomCameraDoFSettingsSP = 0x68, // *zoom-camera-dof-settings-sp*
				zoomSniperCameraDoFSettingsSP = 0x70, // *zoom-sniper-camera-dof-settings-sp*
				unknownLong_at0x78 = 0x78, // unknown ulong
				screenEffectSettings_Ptr = 0x80, // screen-effect-settings*
				unknownByteArray_at0x88 = 0x88 // unknown byte[]
				#endregion [offsets]
			;

            
            //# Public Members (heh)
            public string Name;
            public long Address;

            
			#region [variable declarations]
			/// <summary> unknown uint <summary/>
			public uint UnknownInt_at0x00;

			/// <summary> unknown uint <summary/>
			public uint UnknownInt_at0x04;

			/// <summary> unknown uint <summary/>
			public uint UnknownInt_at0x08;

			
			/// <summary> unknown, usually set to -1, but the bow has it set to zero <summary/>
			public float UnknownFloat_at0x0C;

			/// <summary> firearm-gameplay-def* <summary/>
			public FirearmGameplayDef FirearmGameplayDefinition;

			/// <summary> blindfire-auto-target-def* <summary/>
			public BlindfireAutoTargetDef BlindfireAutoTargetDefinition;

			
			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x20;

			/// <summary> grenade-gameplay-def* <summary/>
			public GrenadeGameplayDef GrenadeGameplayDefinition;

			
			/// <summary> melee-gameplay-def* <summary/>
			public MeleeWeaponGameplayDef MeleeWeaponGameplayDefinition;

			
			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x38;

			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x40;

			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x48;

			/// <summary> unknown byte[] <summary/>
			public byte[] UnknownByteArray_at0x50;

			
			/// <summary> hud2-reticle-def* <summary/>
			public Hud2ReticleDef Hud2ReticleDefinition;

			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x60;

			/// <summary> *zoom-camera-dof-settings-sp* <summary/>
			public ulong ZoomCameraDoFSettingsSP;

			/// <summary> *zoom-sniper-camera-dof-settings-sp* <summary/>
			public ulong ZoomSniperCameraDoFSettingsSP;

			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x78;

			/// <summary> screen-effect-settings* <summary/>
			public ulong ScreenEffectSettings;

			/// <summary> unknown byte[] <summary/>
			public byte[] UnknownByteArray_at0x88;
			#endregion [varaible declarations]
        }



        /// <summary>
        /// 
        /// </summary>
        public struct FirearmGameplayDef
        {
            public FirearmGameplayDef(byte[] binFile, long Address, string Name)
            {
                //#
                //## Variable Initializations
                //#
                // TODO:
                // - Remove the initializations for variables once code to read said variable has been added
                this.Name = Name;
                this.Address = Address;

                # region [variable initializations]
				AmmoTypes = 0;
			
				UnknownFloat_at0x14 = 0;
				UnknownFloat_at0x18 = 0;
				UnknownInt_at0x20 = 0;
				UnknownFloat_at0x24 = 0;
				UnknownFloat_at0x28 = 0;
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
				BaseAmmoCount = 0;
				UnknownFloat_at0xA0 = 0;
				UnknownFloat_at0xA4 = 0;
				UnknownFloat_at0xA8 = 0;
				UnknownFloat_at0xAC = 0;
			
				ScopedLagSettings = 0;
			
				ProneAim0SID = 0;
				UnknownFloat_at0xC0 = 0;
				UnknownFloat_at0xC4 = 0;
				UnknownFloat_at0xC8 = 0;
				UnknownFloat_at0xCC = 0;
			
				FirearmAimDeviationDef0 = 0;
				FirearmAimDeviationDef1 = 0;
			
				UnknownFloat_at0xE0 = 0;
				UnknownFloat_at0xE4 = 0;
				UnknownFloat_at0xE8 = 0;
			
				FirearmKickbackDef0 = 0;
				FirearmKickbackDef1 = 0;
				FirearmKickbackDef2 = 0;
				FirearmKickbackDef3 = 0;
			
				UnknownFloat_at0x118 = 0;
				UnknownFloat_at0x120 = 0;
			
				LerpAimSwaySettings0 = 0;
				LerpAimSwaySettings1 = 0;
				LerpAimSwaySettings2 = 0;
				SwayHoldBreathSettings0 = 0;
				SwayHoldBreathSettings1 = 0;
			
				UnknownInt_at0x158 = 0;
				UnknownFloat_at0x15C = 0;
				UnknownFloat_at0x160 = 0;
				UnknownFloat_at0x164 = 0;
				UnknownFloat_at0x168 = 0;
				UnknownFloat_at0x16C = 0;
				UnknownInt_at0x194 = 0;
				UnknownFloat_at0x19C = 0;
			
				UnknownAimSID = 0;
				HorseAimSID = 0;
				ProneAim1SID = 0;
				AimAssistSID = 0;
				HapticSettingsSID = 0;
				RumbleSettingsSID = 0;
				CameraShakeRightSID = 0;
				CameraShakeLeftSID = 0;
				PointCurve0 = 0;
				GunmoveIkSettings = 0;
				PointCurve1 = 0;
				PointCurve2 = 0;
				DamageLinksSID = 0;
				#endregion [variable initializations]




                BaseAmmoCount = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)Address + baseAmmoCount), 0);

                
                FirearmDamageMovementDefinition = new FirearmDamageMovementDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int)Address + firearmDamageMovementDef_Ptr), 0), Name);
                
                FirearmStatBarDefinition = new FirearmStatBarDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int)Address + firearmStatBarDef_Ptr), 0), Name);
            }

            

            //#
            //## Variable Declarations
            //#
            
            //# Private Members
            /// <summary>  Firearm Gameplay Definition structure offset. </summary>
            
			private const int
				# region [offsets]
				ammoTypes_Ptr = 0x00, // symbol-array containing ammo type names
				
				unknownFloat_at0x14 = 0x14, // unknown float
				unknownFloat_at0x18 = 0x18, // unknown float
				unknownInt_at0x20 = 0x20, // unknown int
				unknownFloat_at0x24 = 0x24, // unknown float
				unknownFloat_at0x28 = 0x28, // unknown float
				unknownFloat_at0x2C = 0x2C, // unknown float
				unknownFloat_at0x30 = 0x30, // unknown float
				unknownFloat_at0x48 = 0x48, // unknown float
				unknownFloat_at0x50 = 0x50, // unknown float
				unknownFloat_at0x54 = 0x54, // unknown float
				unknownFloat_at0x60 = 0x60, // unknown float
				unknownInt_at0x68 = 0x68, // unknown int
				unknownFloat_at0x6C = 0x6C, // unknown float
				unknownFloat_at0x70 = 0x70, // unknown float
				unknownFloat_at0x74 = 0x74, // unknown float
				unknownFloat_at0x78 = 0x78, // unknown float
				unknownFloat_at0x7C = 0x7C, // unknown float
				unknownFloat_at0x80 = 0x80, // unknown float
				unknownFloat_at0x84 = 0x84, // unknown float
				unknownFloat_at0x88 = 0x88, // unknown float
				unknownFloat_at0x8C = 0x8C, // unknown float
				baseAmmoCount = 0x98, // integer (long or int?) amount of base ammo
				unknownFloat_at0xA0 = 0xA0, // unknown float
				unknownFloat_at0xA4 = 0xA4, // unknown float
				unknownFloat_at0xA8 = 0xA8, // unknown float
				unknownFloat_at0xAC = 0xAC, // unknown float
				
				scopedLagSettings_Ptr = 0xB0, // scoped-lag-settings*
				
				proneAim0SID = 0xB8, // unknown ulong
				unknownFloat_at0xC0 = 0xC0, // unknown float
				unknownFloat_at0xC4 = 0xC4, // unknown float
				unknownFloat_at0xC8 = 0xC8, // unknown float
				unknownFloat_at0xCC = 0xCC, // unknown float
				
				firearmAimDeviationDef0_Ptr = 0xD0, // firearm-aim-deviation-def*
				firearmAimDeviationDef1_Ptr = 0xD8, // firearm-aim-deviation-def*
				
				unknownFloat_at0xE0 = 0xE0, // unknown float
				unknownFloat_at0xE4 = 0xE4, // unknown float
				unknownFloat_at0xE8 = 0xE8, // unknown float
				
				firearmKickbackDef0_Ptr = 0xF0, // firearm-kickback-def*
				firearmKickbackDef1_Ptr = 0xF8, // firearm-kickback-def*
				firearmKickbackDef2_Ptr = 0x108, // firearm-kickback-def*
				firearmKickbackDef3_Ptr = 0x110, // firearm-kickback-def*
				
				unknownFloat_at0x118 = 0x118, // unknown float
				unknownFloat_at0x120 = 0x120, // unknown float
				
				lerpAimSwaySettings0_Ptr = 0x128, // lerp-aim-sway-settings*
				lerpAimSwaySettings1_Ptr = 0x130, // lerp-aim-sway-settings*
				lerpAimSwaySettings2_Ptr = 0x140, // lerp-aim-sway-settings*
				swayHoldBreathSettings0_Ptr = 0x148, // sway-hold-breath-settings*
				swayHoldBreathSettings1_Ptr = 0x150, // sway-hold-breath-settings*
				
				unknownInt_at0x158 = 0x158, // unknown int
				unknownFloat_at0x15C = 0x15C, // unknown float
				unknownFloat_at0x160 = 0x160, // unknown float
				unknownFloat_at0x164 = 0x164, // unknown float
				unknownFloat_at0x168 = 0x168, // unknown float
				unknownFloat_at0x16C = 0x16C, // unknown float
				unknownInt_at0x194 = 0x194, // unknown int
				unknownFloat_at0x19C = 0x19C, // unknown float
				
				unknownAimSID = 0x1A0, // unknown ulong
				horseAimSID = 0x1A8, // unknown ulong
				proneAim1SID = 0x1B0, // unknown ulong
				aimAssistSID = 0x1B8, // unknown ulong
				firearmDamageMovementDef_Ptr = 0x1C8, // firearm-damage-movement-def*
				hapticSettingsSID = 0x1F8, // unknown ulong
				rumbleSettingsSID = 0x200, // unknown ulong
				cameraShakeRightSID = 0x208, // unknown ulong
				cameraShakeLeftSID = 0x210, // unknown ulong
				pointCurve0_Ptr = 0x228, // unknown ulong
				gunmoveIkSettings_Ptr = 0x278, // gunmove-ik-settings*
				firearmStatBarDef_Ptr = 0x280, // firearm-stat-bar-def*
				pointCurve1_Ptr = 0x288, // unknown ulong
				pointCurve2_Ptr = 0x290, // unknown ulong
				damageLinksSID = 0x2A0  // unknown ulong
				#endregion [offsets]
			;


            //# Public Members
            public string Name;
            public long Address;

            #region [variable declarations]
			/// <summary> symbol-array containing ammo type names <summary/>
			public ulong AmmoTypes;

			
			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x14;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x18;

			/// <summary> unknown int <summary/>
			public int UnknownInt_at0x20;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x24;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x28;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x2C;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x30;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x48;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x50;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x54;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x60;

			/// <summary> unknown int <summary/>
			public int UnknownInt_at0x68;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x6C;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x70;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x74;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x78;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x7C;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x80;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x84;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x88;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x8C;

			/// <summary> integer (long or int?) amount of base ammo <summary/>
			public long BaseAmmoCount;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xA0;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xA4;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xA8;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xAC;

			
			/// <summary> scoped-lag-settings* <summary/>
			public ulong ScopedLagSettings;

			
			/// <summary> unknown ulong <summary/>
			public ulong ProneAim0SID;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xC0;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xC4;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xC8;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xCC;

			
			/// <summary> firearm-aim-deviation-def* <summary/>
			public ulong FirearmAimDeviationDef0;

			/// <summary> firearm-aim-deviation-def* <summary/>
			public ulong FirearmAimDeviationDef1;

			
			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xE0;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xE4;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xE8;

			
			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef0;

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef1;

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef2;

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef3;

			
			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x118;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x120;

			
			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings0;

			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings1;

			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings2;

			/// <summary> sway-hold-breath-settings* <summary/>
			public ulong SwayHoldBreathSettings0;

			/// <summary> sway-hold-breath-settings* <summary/>
			public ulong SwayHoldBreathSettings1;

			
			/// <summary> unknown int <summary/>
			public int UnknownInt_at0x158;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x15C;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x160;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x164;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x168;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x16C;

			/// <summary> unknown int <summary/>
			public int UnknownInt_at0x194;

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x19C;

			
			/// <summary> unknown ulong <summary/>
			public ulong UnknownAimSID;

			/// <summary> unknown ulong <summary/>
			public ulong HorseAimSID;

			/// <summary> unknown ulong <summary/>
			public ulong ProneAim1SID;

			/// <summary> unknown ulong <summary/>
			public ulong AimAssistSID;

			/// <summary> firearm-damage-movement-def* <summary/>
			public FirearmDamageMovementDef FirearmDamageMovementDefinition;

			/// <summary> unknown ulong <summary/>
			public ulong HapticSettingsSID;

			/// <summary> unknown ulong <summary/>
			public ulong RumbleSettingsSID;

			/// <summary> unknown ulong <summary/>
			public ulong CameraShakeRightSID;

			/// <summary> unknown ulong <summary/>
			public ulong CameraShakeLeftSID;

			/// <summary> unknown ulong <summary/>
			public ulong PointCurve0;

			/// <summary> gunmove-ik-settings* <summary/>
			public ulong GunmoveIkSettings;

			/// <summary> firearm-stat-bar-def* <summary/>
			public FirearmStatBarDef FirearmStatBarDefinition;

			/// <summary> unknown ulong <summary/>
			public ulong PointCurve1;

			/// <summary> unknown ulong <summary/>
			public ulong PointCurve2;

			/// <summary> unknown ulong <summary/>
			public ulong DamageLinksSID;
			#endregion [varaible declarations]
        }



        /// <summary>
        /// 
        /// </summary>
        public struct MeleeWeaponGameplayDef
        {
            public MeleeWeaponGameplayDef(byte[] binFile, long Address, string Name)
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
        public struct GrenadeGameplayDef
        {
            public GrenadeGameplayDef(byte[] binFile, long Address, string Name)
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
        public struct BlindfireAutoTargetDef
        {
            public BlindfireAutoTargetDef(byte[] binFile, long Address, string Name)
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
        public struct FirearmDamageMovementDef
        {
            public FirearmDamageMovementDef(byte[] binFile, long Address, string Name)
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
        public struct FirearmStatBarDef
        {
            public FirearmStatBarDef(byte[] binFile, long Address, string Name)
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
        public struct Hud2ReticleDef
        {
            public Hud2ReticleDef(byte[] binFile, long Address, string Name = "unnamed")
            {
                this.Name = Name;
                this.Address = Address;
                
                ReticleName = 0;
                ReticleSimpleName = 0;

                
                    // Read Hud2 Reticle Name
                for (var i = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + reticleDefNameOffset), 0); binFile[i] != 0;)
                {
                    ReticleName += (char)binFile[i++];
                }
                // Read Hud2 Simple Reticle Name
                for (var i = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + reticleDefSimpleNameOffset), 0); binFile[i] != 0;)
                {
                    ReticleSimpleName += (char)binFile[i++];
                }
            }

            public string Name;
            public long Address;

            
            /// <summary>
            /// HUD2 Reticle Definition structure offset.
            /// </summary>
            private const byte
                reticleDefNameOffset = 0x8,
                reticleDefSimpleNameOffset = 0x18
            ;

            
            public long ReticleName;
            public long ReticleSimpleName;
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



        /// <summary>
        /// 
        /// </summary>
        private struct UnknownStruct
        {
            public UnknownStruct(string Type, long Address, string Name)
            {
                this.Name = Name;
                this.Address = Address;

                Message = $"Unknown Structure: {Type}\n    Struct Addr: 0x{Address.ToString("X").PadLeft(8, '0')}\n    Struct Name: {Name}";
            }

            public string Name;
            public string Message;
            public long Address;
        }



        /// <summary>
        /// 
        /// </summary>
        private struct StructTemplate
        {
            public StructTemplate(byte[] binFile, long Address, string Name)
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
