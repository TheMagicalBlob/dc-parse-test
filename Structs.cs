using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //=================================\\
        //--|   Structure Definitions   |--\\
        //=================================\\
        #region [Structure Definitions]

        /// <summary>
        /// Details on the initial header array for the provided DC file, as well as an array of any present HeaderItems.
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

                Entries = null;

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
                var integrityCheck  = new SID(GetSubArray(binFile, 0x20));
                if (integrityCheck.RawID != KnownSIDs.array)
                {
                    echo($"ERROR; Unexpected SID \"{integrityCheck.RawID:X}\" at 0x20, aborting.");
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

                Entries = new DCHeaderItem[TableLength];


                //#
                //## Parse header content table
                //#
    #if false
                var pre = new[] { DateTime.Now.Minute, DateTime.Now.Second };
    #endif
                echo($"Parsing DC Content Table (Length: {TableLength.ToString().PadLeft(2, '0')})\n ");
                StatusLabelMammet(new[] { "Reading Script...", emptyStr, emptyStr });
                
                for (int tableIndex = 0, addr = 0x28; tableIndex < TableLength; tableIndex++, addr += 24)
                {
                    Entries[tableIndex] = new DCHeaderItem(binFile, addr);
                }
            
    #if false
                echo ($"{DateTime.Now.Minute - pre[0]}:{DateTime.Now.Second - pre[1]}");
    #endif
            }
            
            
            //#
            //## Variable Declarations
            //#
            public readonly int unkInt0;
            public readonly long unkInt1;

            public long BinFileLength;
            public int TableLength;

            /// <summary> An array of the DCHeaderItems parsed from the provided DC file. </summary>
            public DCHeaderItem[] Entries;
        }


        /// <summary>
        /// An individual item (module?) from the array at the beginning of the DC file.
        /// </summary>
        public struct DCHeaderItem
        {
            public DCHeaderItem(byte[] binFile, int address)
            {
                Address = address;

                Name = new SID(GetSubArray(binFile, Address));
                Type = new SID(GetSubArray(binFile, Address + 8));
                
                StructAddress = BitConverter.ToInt64(GetSubArray(binFile, Address + 16), 0);

                
                Struct = LoadMappedDCStructs(DCFile, Type, StructAddress, Name);
            }


            /// <summary>
            /// The Address of this Header Item in the DC file.
            /// </summary>
            public int Address { get; set; }

            /// <summary>
            /// The name of the current entry in the DC file header.
            /// </summary>
            public SID Name { get; set; }
            
            /// <summary>
            /// The struct type of the current entry in the DC file header.
            /// </summary>
            public SID Type { get; set; }
            
            /// <summary>
            /// The address of the struct pointed to by tbe current dc header entry.
            /// </summary>
            public long StructAddress { get; set; }

            /// <summary>
            /// The actual mapped structure object.
            /// </summary>
            public object Struct { get; private set; }



            /// <summary>
            /// Begin loading the header item's structure. <br/>
            /// //! write something more verbose lmao      <br/>
            /// //! Scratch that; I don't think this is really necessary anymore, woo. Keeping it for now just-in-case. (not that it'd be difficult to rewrite...)
            /// </summary>
            public void LoadItemStruct()
            {
                echo("Loading Item Struct...");
                Struct = LoadMappedDCStructs(DCFile, Type, StructAddress, Name);
            }
        }




        /// <summary>
        /// A collection of whatever-the-fuck naughty dog felt like including. This may be annoying.
        /// </summary>
        public struct DCMapDef
        {
            public DCMapDef(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                
                var mapLength = BitConverter.ToInt64(GetSubArray(binFile, (int)Address), 0);
                if (mapLength < 1)
                {
                    echo($"  # Empty Map Structures.");
                    Items = Array.Empty<object[]>();
                    return;
                }
                var mapNamesArrayPtr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 8), 0);
                var mapStructsArrayPtr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 16), 0);

                Items = new object[mapLength][];
                Items[0] = new object[2];



                echo($"\n  # Parsing {mapLength} Map Structures...");
                for (int arrayIndex = 0; arrayIndex < mapLength; mapStructsArrayPtr += 8, mapNamesArrayPtr += 8, arrayIndex++)
                {
                    var structAddress = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructsArrayPtr), 0);

                    var structTypeID = new SID(GetSubArray(binFile, structAddress - 8));
                    var structName = new SID(GetSubArray(binFile, (int)mapNamesArrayPtr));

                    echo($"    - 0x{structAddress.ToString("X").PadLeft(6, '0')} Type: {structTypeID} Name: {structName.DecodedID}" + 1);

                    Items[arrayIndex] = new object[2];

                    Items[arrayIndex][0] = structTypeID;
                    Items[arrayIndex][1] = LoadMappedDCStructs(binFile, structTypeID, structAddress, structName);
                }
                echo($"  # Finished Parsing All Map Structures.");
            }

            
            /// <summary>
            /// The name of the map item.
            /// </summary>
            public SID Name { get; set; }

            public long Address { get; set; }

            /// <summary>
            /// An array of object arrays with the first element being the map item's struct type, and the other being the struct itself
            /// </summary>
            public object[][] Items { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct SymbolArrayDef
        {
            public SymbolArrayDef(byte[] binFile, long Address, SID Name)
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
                    Symbols.Add(SIDBase.DecodeSIDHash(dat));
                }
            }


            public SID Name { get; set; }
            public long Address { get; set; }

            public List<string> Symbols { get; set; }
            public List<long> Hashes { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct AmmoToWeaponArray
        {
            public AmmoToWeaponArray(byte[] binFile, long address, SID Name)
            {
                var symbols = new List<string[]>();
                var hashes  = new List<byte[][]>();
                this.Name = Name;
                
                var arrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                var arrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);

                echo($"\n  # Parsing Ammo-to-Weapon Structures...");
                for (int i = 0; i < arrayLen; arrayAddr += 16, i++)
                {
                    StatusLabelMammet(new[] { null, null, $"Ammo-to-Weapon Entry: {i} / {arrayLen - 1}" });

                    hashes.Add(new[] { GetSubArray(binFile, (int)arrayAddr + 8), GetSubArray(binFile, (int)arrayAddr) });
                    symbols.Add(new[] { SIDBase.DecodeSIDHash(hashes.Last()[0]), SIDBase.DecodeSIDHash(hashes.Last()[1]) });
                }
                echo($"  # Finished Parsing Ammo-to-Weapon Structures.");
                StatusLabelMammet(new[] { null, null, emptyStr });

                Symbols = symbols.ToArray();
                Hashes = hashes.ToArray();
            }

            public SID Name { get; set; }


            public string[][] Symbols { get; set; }
            public byte[][][] Hashes { get; set; }
        }

    

        /// <summary>
        /// 
        /// </summary>
        public struct WeaponGameplayDef
        {
            public WeaponGameplayDef(byte[] binFile, long Address, SID Name)
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

                echo("bingus");

                

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


            
            // ## Public Members (heh)
            /// <summary> The name for the weapon this WeaponGameplayDefinition belongs to. </summary>
            public SID Name { get; set; }
            /// <summary> The address of the WeaponGameplayDefinition struct in the provided DC file. </summary>
            public long Address { get; set; }

            
			#region [variable declarations]
			/// <summary> unknown uint <summary/>
			public uint UnknownInt_at0x00 { get; set; }

			/// <summary> unknown uint <summary/>
			public uint UnknownInt_at0x04 { get; set; }

			/// <summary> unknown uint <summary/>
			public uint UnknownInt_at0x08 { get; set; }

			
			/// <summary> unknown, usually set to -1, but the bow has it set to zero <summary/>
			public float UnknownFloat_at0x0C { get; set; }

			/// <summary> firearm-gameplay-def* <summary/>
			public FirearmGameplayDef FirearmGameplayDefinition { get; set; }

			/// <summary> blindfire-auto-target-def* <summary/>
			public BlindfireAutoTargetDef BlindfireAutoTargetDefinition { get; set; }

			
			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x20 { get; set; }

			/// <summary> grenade-gameplay-def* <summary/>
			public GrenadeGameplayDef GrenadeGameplayDefinition { get; set; }

			
			/// <summary> melee-gameplay-def* <summary/>
			public MeleeWeaponGameplayDef MeleeWeaponGameplayDefinition { get; set; }

			
			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x38 { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x40 { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x48 { get; set; }

			/// <summary> unknown byte[] <summary/>
			public byte[] UnknownByteArray_at0x50 { get; set; }

			
			/// <summary> hud2-reticle-def* <summary/>
			public Hud2ReticleDef Hud2ReticleDefinition { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x60 { get; set; }

			/// <summary> *zoom-camera-dof-settings-sp* <summary/>
			public ulong ZoomCameraDoFSettingsSP { get; set; }

			/// <summary> *zoom-sniper-camera-dof-settings-sp* <summary/>
			public ulong ZoomSniperCameraDoFSettingsSP { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong UnknownLong_at0x78 { get; set; }

			/// <summary> screen-effect-settings* <summary/>
			public ulong ScreenEffectSettings { get; set; }

			/// <summary> unknown byte[] <summary/>
			public byte[] UnknownByteArray_at0x88 { get; set; }
			#endregion [variable declarations]
        }



        /// <summary>
        /// 
        /// </summary>
        public struct FirearmGameplayDef
        {
            public FirearmGameplayDef(byte[] binFile, long Address, SID Name)
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
            /// <summary> The name for the weapon this FirearmGameplayDefinition belongs to. </summary>
            public SID Name { get; set; }
            /// <summary> The address of the FirearmGameplayDefinition struct in the provided DC file. </summary>
            public long Address { get; set; }

            #region [variable declarations]
			/// <summary> symbol-array containing ammo type names <summary/>
			public ulong AmmoTypes { get; set; }

			
			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x14 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x18 { get; set; }

			/// <summary> unknown int <summary/>
			public int UnknownInt_at0x20 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x24 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x28 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x2C { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x30 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x48 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x50 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x54 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x60 { get; set; }

			/// <summary> unknown int <summary/>
			public int UnknownInt_at0x68 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x6C { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x70 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x74 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x78 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x7C { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x80 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x84 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x88 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x8C { get; set; }

			/// <summary> integer (long or int?) amount of base ammo <summary/>
			public long BaseAmmoCount { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xA0 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xA4 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xA8 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xAC { get; set; }

			
			/// <summary> scoped-lag-settings* <summary/>
			public ulong ScopedLagSettings { get; set; }

			
			/// <summary> unknown ulong <summary/>
			public ulong ProneAim0SID { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xC0 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xC4 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xC8 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xCC { get; set; }

			
			/// <summary> firearm-aim-deviation-def* <summary/>
			public ulong FirearmAimDeviationDef0 { get; set; }

			/// <summary> firearm-aim-deviation-def* <summary/>
			public ulong FirearmAimDeviationDef1 { get; set; }

			
			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xE0 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xE4 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0xE8 { get; set; }

			
			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef0 { get; set; }

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef1 { get; set; }

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef2 { get; set; }

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef3 { get; set; }

			
			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x118 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x120 { get; set; }

			
			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings0 { get; set; }

			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings1 { get; set; }

			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings2 { get; set; }

			/// <summary> sway-hold-breath-settings* <summary/>
			public ulong SwayHoldBreathSettings0 { get; set; }

			/// <summary> sway-hold-breath-settings* <summary/>
			public ulong SwayHoldBreathSettings1 { get; set; }

			
			/// <summary> unknown int <summary/>
			public int UnknownInt_at0x158 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x15C { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x160 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x164 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x168 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x16C { get; set; }

			/// <summary> unknown int <summary/>
			public int UnknownInt_at0x194 { get; set; }

			/// <summary> unknown float <summary/>
			public float UnknownFloat_at0x19C { get; set; }

			
			/// <summary> unknown ulong <summary/>
			public ulong UnknownAimSID { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong HorseAimSID { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong ProneAim1SID { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong AimAssistSID { get; set; }

			/// <summary> firearm-damage-movement-def* <summary/>
			public FirearmDamageMovementDef FirearmDamageMovementDefinition { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong HapticSettingsSID { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong RumbleSettingsSID { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong CameraShakeRightSID { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong CameraShakeLeftSID { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong PointCurve0 { get; set; }

			/// <summary> gunmove-ik-settings* <summary/>
			public ulong GunmoveIkSettings { get; set; }

			/// <summary> firearm-stat-bar-def* <summary/>
			public FirearmStatBarDef FirearmStatBarDefinition { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong PointCurve1 { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong PointCurve2 { get; set; }

			/// <summary> unknown ulong <summary/>
			public ulong DamageLinksSID { get; set; }
			#endregion [variable declarations]
        }



        /// <summary>
        /// 
        /// </summary>
        public struct MeleeWeaponGameplayDef
        {
            public MeleeWeaponGameplayDef(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                //Size = ?
                //RawData = GetSubArray(binFile, Address, Size);
            }

            public SID Name { get; set; }
            public long Address { get; set; }

            //public uint Size { get; set; }
            //public byte[] RawData { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct GrenadeGameplayDef
        {
            public GrenadeGameplayDef(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                //Size = ?
                //RawData = GetSubArray(binFile, Address, Size);
            }

            public SID Name { get; set; }
            public long Address { get; set; }

            //public uint Size { get; set; }
            //public byte[] RawData { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct BlindfireAutoTargetDef
        {
            public BlindfireAutoTargetDef(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                //Size = ?
                //RawData = GetSubArray(binFile, Address, Size);
            }

            public SID Name;
            public long Address;

            //public uint Size { get; set; }
            //public byte[] RawData { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct FirearmDamageMovementDef
        {
            public FirearmDamageMovementDef(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                //Size = ?
                //RawData = GetSubArray(binFile, Address, Size);
            }

            public SID Name;
            public long Address;

            //public uint Size { get; set; }
            //public byte[] RawData { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct FirearmStatBarDef
        {
            public FirearmStatBarDef(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                //Size = ?
                //RawData = GetSubArray(binFile, Address, Size);
            }

            public SID Name;
            public long Address;

            //public uint Size { get; set; }
            //public byte[] RawData { get; set; }
        }


        /// <summary>
        /// 
        /// </summary>
        public struct Hud2ReticleDef
        {
            public Hud2ReticleDef(byte[] binFile, long Address, SID Name)
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
            
            /// <summary> HUD2 Reticle Definition structure offset. </summary>
            private const byte
                reticleDefNameOffset = 0x8,
                reticleDefSimpleNameOffset = 0x18
            ;



            public SID Name { get; set; }
            public long Address { get; set; }
            
            public long ReticleName { get; set; }
            public long ReticleSimpleName { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct Look2Def
        {
            public Look2Def(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
            }

            public SID Name { get; set; }
            public long Address { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct UnknownStruct
        {
            public UnknownStruct(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                Message = $"Unknown Structure [\n\tType: {Type.DecodedID}\n\tName: {Name.DecodedID}\n\tAddress: 0x{Address.ToString("X").PadLeft(8, '0')}\n]";
            }

            public SID Name { get; set; }
            public long Address { get; set; }

            public string Message { get; set; }
        }



        /// <summary>
        /// N/A
        /// </summary>
        public struct StructTemplate
        {
            /// <summary>
            /// Create a new instance of the StructTemplate struct.
            /// </summary>
            /// <param name="binFile"> The DC file this StructTemplate instance is being read from. </param>
            /// <param name="Address"> The start address of the structure in the DC file. </param>
            /// <param name="Name"> The name associated with the current StructTemplate instance. </param>
            public StructTemplate(byte[] binFile, long Address, SID Name)
            {
                //#
                //## Variable Initializations
                //#
                // TODO:
                // - Remove the initializations for variables once code to read said variable has been added
                #region [variable initializations]
                this.Name = Name;
                this.Address = Address;
                //RawData = GetSubArray(binFile, Address, Size);

                // _VARIABLE_DECLARATIONS_HERE_
                #endregion
            }


            //#
            //## Offset Declarations
            //#
            #region [Offset Declarations]
            // _OFFSET_DECLARATIONS_HERE_
            #endregion [offset declarations]



            //#
            //## Variable Declarations
            //#
            #region [Variable Declarations]

            //# #|Private Members|#
            // _PRIVATE_MEMBERS_HERE_

            //# #|Public Members|#
            /// <summary> The name associated with the current StructTemplate instance. </summary>
            public SID Name { get; set; }
            /// <summary> The start address of the structure in the DC file. </summary>
            public long Address { get; set; }
            /// <summary> Size of the current structure type. </summary>
            //public const uint Size = 0xDEADBEEF;
            /// <summary> The raw binary data of the current StructureTemplate instance. </summary>
            //public byte[] RawData { get; set; }

            // _PRIVATE_MEMBERS_HERE_

            #endregion [variable declarations]
        }
        




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
            public SID(byte[] EncodedSIDArray)
            {
                DecodedID = SIDBase.DecodeSIDHash(EncodedSIDArray);
                EncodedID = BitConverter.ToString(EncodedSIDArray).Replace("-", emptyStr);
                RawID = (KnownSIDs) BitConverter.ToUInt64(EncodedSIDArray, 0);

                Venat?.DecodedSIDs.Add(this);
            }
            
            /// <summary>
            /// Create a new SID instance from a provided ulong hash, and attempt to decode the id.
            /// </summary>
            /// <param name="EncodedSID"> The encoded ulong string id. </param>
            public SID(ulong EncodedSID)
            {
                var EncodedSIDArray = BitConverter.GetBytes(EncodedSID);

                DecodedID = SIDBase.DecodeSIDHash(EncodedSIDArray);
                EncodedID = BitConverter.ToString(EncodedSIDArray).Replace("-", emptyStr);
                RawID = (KnownSIDs) EncodedSID;

                Venat?.DecodedSIDs.Add(this);
            }


            /// <summary>
            /// I didn't know how else to make that SID.Empty thing
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
        }
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
        /// Initialize a new sidbase instance with the path provided. <br/>
        ///
        /// TODO:
        /// Implement functionality for parsing multiple sidbases.
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

            for (/* muahahahahhahahaaa */; length > 0; ret[length - 1] = array[index + (length-- - 1)]);
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
