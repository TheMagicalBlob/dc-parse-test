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

                headerTableStartPointerAddr = 0x18;
                HeaderTableStartPointer = 0;

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
                var integrityCheck = new SID(GetSubArray(binFile, 0x20));
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
                if ((HeaderTableStartPointer = BitConverter.ToInt64(binFile, headerTableStartPointerAddr)) != 0x28)
                {
                    echo($"ERROR; Unexpected Value \"{HeaderTableStartPointer}\" read at {headerTableStartPointerAddr}, aborting.");
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
            private readonly int headerTableStartPointerAddr; // 0x18

            public readonly int unkInt0;
            public readonly long HeaderTableStartPointer;

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
        public struct Map
        {
            public Map(byte[] binFile, long Address, SID Name)
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
                for (var arrayIndex = 0; arrayIndex < mapLength; mapStructsArrayPtr += 8, mapNamesArrayPtr += 8, arrayIndex++)
                {
                    var structAddress = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructsArrayPtr), 0);

                    var structTypeID = new SID(GetSubArray(binFile, structAddress - 8));
                    var structName = new SID(GetSubArray(binFile, (int)mapNamesArrayPtr));

                    echo($"    - 0x{structAddress.ToString("X").PadLeft(6, '0')} Type: {structTypeID.DecodedID} Name: {structName.DecodedID}" + 1);

                    Items[arrayIndex] = new object[2]
                    {
                        structTypeID,
                        LoadMappedDCStructs(binFile, structTypeID, structAddress, structName)
                    };
                }
                echo($"  # Finished Parsing All Map Structures.");
            }


            /// <summary>
            /// The name of the map item.
            /// </summary>
            public SID Name { get; set; }

            public long Address { get; set; }


            /// <summary>
            /// A jagged object array of header items and their type id (not in that order...) <br/>
            /// 
            /// <br/> 0: the map item's struct type.
            /// <br/> 1: the struct itself.
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
                Hashes = new List<long>();


                var arrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)Address), 0);
                var arrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 8), 0);

                for (var i = 0; i < arrayLen; arrayAddr += 8, i++)
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
                var hashes = new List<byte[][]>();
                this.Name = Name;

                var arrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)address), 0);
                var arrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)address + 8), 0);

                echo($"\n  # Parsing Ammo-to-Weapon Structures...");
                for (var i = 0; i < arrayLen; arrayAddr += 16, i++)
                {
                    StatusLabelMammet(new[] { null, null, $"Ammo-to-Weapon Entry: {i} / {arrayLen - 1}" });

                    hashes.Add(new[] { GetSubArray(binFile, (int) arrayAddr + 8), GetSubArray(binFile, (int) arrayAddr) });
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

                #region [variable initializations]
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
                FirearmGameplayDefinition = new FirearmGameplayDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int) Address + firearmGameplayDef_Ptr), 0), Name);

                MeleeWeaponGameplayDefinition = new MeleeWeaponGameplayDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int) Address + meleeGameplayDef_Ptr), 0), Name);

                BlindfireAutoTargetDefinition = new BlindfireAutoTargetDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int) Address + blindfireAutoTargetDef_Ptr), 0), Name);

                GrenadeGameplayDefinition = new GrenadeGameplayDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int) Address + grenadeGameplayDef_Ptr), 0), Name);

                Hud2ReticleDefinition = new Hud2ReticleDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int) Address + hud2ReticleDef_Ptr), 0), Name);

            }


            //#
            //## Variable Declarations
            //#
            //# Private Members
            /// <summary> Weapon Gameplay Definition structure offset. </summary>
			private const int
            #region [offsets]
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
        /// Initialize a new instance of the firearmGameplayDef struct.
        /// </summary>
        public struct FirearmGameplayDef
        {
            /// <summary>
            /// Create a new instance of the firearmGameplayDef struct.
            /// </summary>
            /// <param name="binFile"> The DC file this firearmGameplayDef instance is being read from. </param>
            /// <param name="Address"> The start address of the structure in the DC file. </param>
            /// <param name="Name"> The name associated with the current firearmGameplayDef instance. </param>
            public FirearmGameplayDef(byte[] binFile, long Address, SID Name)
            {
                //#
                //## Variable Initializations
                //#
                #region [variable initializations]
                this.Name = Name;
                this.Address = Address;

                AmmoTypes_Pointer = 0;
			
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
				UnknownFloat_at0xA0 = 0;
				UnknownFloat_at0xA4 = 0;
				UnknownFloat_at0xA8 = 0;
				UnknownFloat_at0xAC = 0;
			
				ScopedLagSettings_Pointer = 0;
			
				ProneAim0SID = 0;
				UnknownFloat_at0xC0 = 0;
				UnknownFloat_at0xC4 = 0;
				UnknownFloat_at0xC8 = 0;
				UnknownFloat_at0xCC = 0;
			
				FirearmAimDeviationDef0_Pointer = 0;
				FirearmAimDeviationDef1_Pointer = 0;
			
				UnknownFloat_at0xE0 = 0;
				UnknownFloat_at0xE4 = 0;
				UnknownFloat_at0xE8 = 0;
			
				FirearmKickbackDef0_Pointer = 0;
				FirearmKickbackDef1_Pointer = 0;
				FirearmKickbackDef2_Pointer = 0;
				FirearmKickbackDef3_Pointer = 0;
			
				UnknownFloat_at0x118 = 0;
				UnknownFloat_at0x120 = 0;
			
				LerpAimSwaySettings0_Pointer = 0;
				LerpAimSwaySettings1_Pointer = 0;
				LerpAimSwaySettings2_Pointer = 0;
				SwayHoldBreathSettings0_Pointer = 0;
				SwayHoldBreathSettings1_Pointer = 0;
			
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
				UnknownFloat_at0x1C0 = 0;
				UnknownFloat_at0x1C4 = 0;
				HapticSettingsSID = 0;
				RumbleSettingsSID = 0;
				CameraShakeRightSID = 0;
				CameraShakeLeftSID = 0;
				PointCurve0_Pointer = 0;
			
				UnknownFloat_at0x230 = 0;
				UnknownFloat_at0x234 = 0;
			
				UnknownFloat_at0x250 = 0;
				UnknownFloat_at0x254 = 0;
				UnknownFloat_at0x260 = 0;
				UnknownFloat_at0x264 = 0;
				UnknownFloat_at0x268 = 0;
				UnknownFloat_at0x26C = 0;
				UnknownFloat_at0x270 = 0;
				UnknownFloat_at0x274 = 0;
			
			
				GunmoveIkSettings_Pointer = 0;
				PointCurve1_Pointer = 0;
				PointCurve2_Pointer = 0;
				DamageLinksSID = 0;
				GrenadeGameplayDef1Start = 0;
				GrenadeGameplayDef2Start = 0;


                
                BaseAmmoCount = (int) BitConverter.ToInt64(GetSubArray(binFile, (int) Address + baseAmmoCount), 0);

                FirearmDamageMovementDefinition = new FirearmDamageMovementDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int) Address + firearmDamageMovementDef_Ptr), 0), Name);

                FirearmStatBarDefinition = new FirearmStatBarDef(binFile, BitConverter.ToInt64(GetSubArray(binFile, (int) Address + firearmStatBarDef_Ptr), 0), Name);
                #endregion
            }


            //#
            //## Offset Declarations
            //#
            #region [Offset Declarations]
            private const int
				ammoTypes_Ptr = 0x00, // symbol-array containing ammo type names
				
				unknownFloat_at0x14 = 0x14, // Unknown float
				unknownFloat_at0x18 = 0x18, // Unknown float
				unknownInt_at0x20 = 0x20, // Unknown int
				unknownFloat_at0x24 = 0x24, // Unknown float
				unknownFloat_at0x28 = 0x28, // Unknown float
				unknownFloat_at0x2C = 0x2C, // Unknown float
				unknownFloat_at0x30 = 0x30, // Unknown float
				unknownFloat_at0x48 = 0x48, // Unknown float
				unknownFloat_at0x50 = 0x50, // Unknown float
				unknownFloat_at0x54 = 0x54, // Unknown float
				unknownFloat_at0x60 = 0x60, // Unknown float
				unknownInt_at0x68 = 0x68, // Unknown int
				unknownFloat_at0x6C = 0x6C, // Unknown float
				unknownFloat_at0x70 = 0x70, // Unknown float
				unknownFloat_at0x74 = 0x74, // Unknown float
				unknownFloat_at0x78 = 0x78, // Unknown float
				unknownFloat_at0x7C = 0x7C, // Unknown float
				unknownFloat_at0x80 = 0x80, // Unknown float
				unknownFloat_at0x84 = 0x84, // Unknown float
				unknownFloat_at0x88 = 0x88, // Unknown float
				unknownFloat_at0x8C = 0x8C, // Unknown float
				baseAmmoCount = 0x98, // integer (long or int?) amount of base ammo
				unknownFloat_at0xA0 = 0xA0, // Unknown float
				unknownFloat_at0xA4 = 0xA4, // Unknown float
				unknownFloat_at0xA8 = 0xA8, // Unknown float
				unknownFloat_at0xAC = 0xAC, // Unknown float
				
				scopedLagSettings_Ptr = 0xB0, // scoped-lag-settings*
				
				proneAim0SID = 0xB8, // Unknown ulong
				unknownFloat_at0xC0 = 0xC0, // Unknown float
				unknownFloat_at0xC4 = 0xC4, // Unknown float
				unknownFloat_at0xC8 = 0xC8, // Unknown float
				unknownFloat_at0xCC = 0xCC, // Unknown float
				
				firearmAimDeviationDef0_Ptr = 0xD0, // firearm-aim-deviation-def*
				firearmAimDeviationDef1_Ptr = 0xD8, // firearm-aim-deviation-def*
				
				unknownFloat_at0xE0 = 0xE0, // Unknown float
				unknownFloat_at0xE4 = 0xE4, // Unknown float
				unknownFloat_at0xE8 = 0xE8, // Unknown float
				
				firearmKickbackDef0_Ptr = 0xF0, // firearm-kickback-def*
				firearmKickbackDef1_Ptr = 0xF8, // firearm-kickback-def*
				firearmKickbackDef2_Ptr = 0x108, // firearm-kickback-def*
				firearmKickbackDef3_Ptr = 0x110, // firearm-kickback-def*
				
				unknownFloat_at0x118 = 0x118, // Unknown float
				unknownFloat_at0x120 = 0x120, // Unknown float
				
				lerpAimSwaySettings0_Ptr = 0x128, // lerp-aim-sway-settings*
				lerpAimSwaySettings1_Ptr = 0x130, // lerp-aim-sway-settings*
				lerpAimSwaySettings2_Ptr = 0x140, // lerp-aim-sway-settings*
				swayHoldBreathSettings0_Ptr = 0x148, // sway-hold-breath-settings*
				swayHoldBreathSettings1_Ptr = 0x150, // sway-hold-breath-settings*
				
				unknownInt_at0x158 = 0x158, // Unknown int
				unknownFloat_at0x15C = 0x15C, // Unknown float
				unknownFloat_at0x160 = 0x160, // Unknown float
				unknownFloat_at0x164 = 0x164, // Unknown float
				unknownFloat_at0x168 = 0x168, // Unknown float
				unknownFloat_at0x16C = 0x16C, // Unknown float
				unknownInt_at0x194 = 0x194, // Unknown int
				unknownFloat_at0x19C = 0x19C, // Unknown float
				unknownAimSID = 0x1A0, // Unknown ulong
				horseAimSID = 0x1A8, // Unknown ulong
				proneAim1SID = 0x1B0, // Unknown ulong
				aimAssistSID = 0x1B8, // Unknown ulong
				unknownFloat_at0x1C0 = 0x1C0, // Unknown float
				unknownFloat_at0x1C4 = 0x1C4, // Unknown float
				firearmDamageMovementDef_Ptr = 0x1C8, // firearm-damage-movement-def*
				hapticSettingsSID = 0x1F8, // Unknown ulong
				rumbleSettingsSID = 0x200, // Unknown ulong
				cameraShakeRightSID = 0x208, // Unknown ulong
				cameraShakeLeftSID = 0x210, // Unknown ulong
				pointCurve0_Ptr = 0x228, // Unknown ulong
				
				unknownFloat_at0x230 = 0x230, // Unknown float
				unknownFloat_at0x234 = 0x234, // Unknown float
				
				unknownFloat_at0x250 = 0x250, // Unknown float
				unknownFloat_at0x254 = 0x254, // Unknown float
				unknownFloat_at0x260 = 0x260, // Unknown float
				unknownFloat_at0x264 = 0x264, // Unknown float
				unknownFloat_at0x268 = 0x268, // Unknown float
				unknownFloat_at0x26C = 0x26C, // Unknown float
				unknownFloat_at0x270 = 0x270, // Unknown float
				unknownFloat_at0x274 = 0x274, // Unknown float
				
				
				gunmoveIkSettings_Ptr = 0x278, // gunmove-ik-settings*
				firearmStatBarDef_Ptr = 0x280, // firearm-stat-bar-def*
				pointCurve1_Ptr = 0x288, // Unknown ulong
				pointCurve2_Ptr = 0x290, // Unknown ulong
				damageLinksSID = 0x2A0, // Unknown ulong
				grenadeGameplayDef1Start = 0x2B0, // Unknown ulong
				grenadeGameplayDef2Start = 0x348 // Unknown ulong
			;
            #endregion [offset declarations]



            //#
            //## Variable Declarations
            //#
            #region [Variable Declarations]

            //# #|Private Members|#
            // [private members here]

            //# #|Public Members|#
            /// <summary> The name associated with the current firearmGameplayDef instance. </summary>
            public SID Name { get; set; }

            /// <summary> The start address of the structure in the DC file. </summary>
            public long Address { get; set; }
            


            /// <summary> symbol-array containing ammo type names <summary/>
			public ulong AmmoTypes_Pointer { get; set; }

			
			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x14 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x18 { get; set; }

			/// <summary> Unknown int <summary/>
			public int UnknownInt_at0x20 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x24 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x28 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x2C { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x30 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x48 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x50 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x54 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x60 { get; set; }

			/// <summary> Unknown int <summary/>
			public int UnknownInt_at0x68 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x6C { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x70 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x74 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x78 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x7C { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x80 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x84 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x88 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x8C { get; set; }

			/// <summary> integer (long or int?) amount of base ammo <summary/>
			public long BaseAmmoCount { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xA0 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xA4 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xA8 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xAC { get; set; }

			
			/// <summary> scoped-lag-settings* <summary/>
			public ulong ScopedLagSettings_Pointer { get; set; }

			
			/// <summary> Unknown ulong <summary/>
			public ulong ProneAim0SID { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xC0 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xC4 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xC8 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xCC { get; set; }

			
			/// <summary> firearm-aim-deviation-def* <summary/>
			public ulong FirearmAimDeviationDef0_Pointer { get; set; }

			/// <summary> firearm-aim-deviation-def* <summary/>
			public ulong FirearmAimDeviationDef1_Pointer { get; set; }

			
			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xE0 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xE4 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0xE8 { get; set; }

			
			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef0_Pointer { get; set; }

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef1_Pointer { get; set; }

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef2_Pointer { get; set; }

			/// <summary> firearm-kickback-def* <summary/>
			public ulong FirearmKickbackDef3_Pointer { get; set; }

			
			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x118 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x120 { get; set; }

			
			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings0_Pointer { get; set; }

			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings1_Pointer { get; set; }

			/// <summary> lerp-aim-sway-settings* <summary/>
			public ulong LerpAimSwaySettings2_Pointer { get; set; }

			/// <summary> sway-hold-breath-settings* <summary/>
			public ulong SwayHoldBreathSettings0_Pointer { get; set; }

			/// <summary> sway-hold-breath-settings* <summary/>
			public ulong SwayHoldBreathSettings1_Pointer { get; set; }

			
			/// <summary> Unknown int <summary/>
			public int UnknownInt_at0x158 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x15C { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x160 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x164 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x168 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x16C { get; set; }

			/// <summary> Unknown int <summary/>
			public int UnknownInt_at0x194 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x19C { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong UnknownAimSID { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong HorseAimSID { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong ProneAim1SID { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong AimAssistSID { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x1C0 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x1C4 { get; set; }

			/// <summary> firearm-damage-movement-def* <summary/>
			public FirearmDamageMovementDef FirearmDamageMovementDefinition { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong HapticSettingsSID { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong RumbleSettingsSID { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong CameraShakeRightSID { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong CameraShakeLeftSID { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong PointCurve0_Pointer { get; set; }

			
			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x230 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x234 { get; set; }

			
			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x250 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x254 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x260 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x264 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x268 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x26C { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x270 { get; set; }

			/// <summary> Unknown float <summary/>
			public float UnknownFloat_at0x274 { get; set; }

			
			
			/// <summary> gunmove-ik-settings* <summary/>
			public ulong GunmoveIkSettings_Pointer { get; set; }

			/// <summary> firearm-stat-bar-def* <summary/>
			public FirearmStatBarDef FirearmStatBarDefinition { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong PointCurve1_Pointer { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong PointCurve2_Pointer { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong DamageLinksSID { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong GrenadeGameplayDef1Start { get; set; }

			/// <summary> Unknown ulong <summary/>
			public ulong GrenadeGameplayDef2Start { get; set; }
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

                //Size = 0;
                //RawData = GetSubArray(binFile, (int) Address, Size);
            }

            public SID Name;
            public long Address;

            //public int Size { get; set; }
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

                ReticleNamePointer = 0;
                ReticleSimpleNamePointer = 0;


                Offsets = new[]
                {
                    //[offset initializations here]
                    0x8, // reticleDefNameOffset
                    0x18 // reticleDefSimpleNameOffset
                };

                var properties = GetType().GetProperties();
                echo("Members: {");
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    var propertyType = property.GetType();
                    var propertyValue = GetPropertyValueByType(propertyType, Offsets[i]);
                    


                    echo($"{property.Name} [");
                    echo($"\toffset: {Offsets[i]:X}");
                    echo($"\tproperty type: {property.PropertyType.Name}");
                    echo($"\tproperty value: {propertyValue}");
                    echo("]");

                    echo($"Attempting to set property value...");

                    property.SetValue(this, propertyValue);
                    
                    echo($"{property.Name}.Value: {property.GetValue(this)}");
/*
                    properties[i].SetValue(this, getPropertyValueByType(properties[i].GetType(), Offsets[i]));
*/
                }
                echo("}\n\n");
            }

            /// <summary> HUD2 Reticle Definition structure offset. </summary>
            private readonly int[] Offsets;



            public SID Name;

            public long Address;

            
            public long ReticleNamePointer { get; set; }
            public long ReticleSimpleNamePointer { get; set; }
        }


        
        /// <summary>
        /// Initialize a new instance of the look2 struct.
        /// </summary>
        public struct look2
        {
            /// <summary>
            /// Create a new instance of the look2 struct.
            /// </summary>
            /// <param name="binFile"> The DC file this look2 instance is being read from. </param>
            /// <param name="Address"> The start address of the structure in the DC file. </param>
            /// <param name="Name"> The name associated with the current look2 instance. </param>
            public look2(byte[] binFile, long Address, SID Name)
            {
                //#
                //## Variable Initializations
                //#
                #region [variable initializations]
                this.Name = Name;
                this.Address = Address;

                
                RawData = GetSubArray(binFile, (int) Address, Size);


				EncodedNameID = 0;
				DecodedName_Pointer = 0;
				DecodedSkeletonName_Pointer = 0;
				EncodedSkeletonNameID = 0;
			
				DecodedMovers1Name_Pointer = 0;
				EncodedMovers1NameID = 0;
				DecodedMovers2Name_Pointer = 0;
				EncodedMovers2NameID = 0;
			
				HeadName_Pointer = 0;
				UnkHeadPartInt1 = 0;
				UnkHeadPartInt2 = 0;
			
				HairClothName_Pointer = 0;
				UnkHairClothInt1 = 0;
				UnkSkelNameInt1 = 0;
				UnkHairClothInt2 = 0;
			
				UnkSkelNameInt2 = 0;
				UnkSkelName_Pointer = 0;
				NextCharacterLook2_0_Pointer = 0;
				NextCharacterLook2_1_Pointer = 0;
			
				AmbientOccludersID = 0;
				ModelScale_1 = 0;
				ModelScale_2 = 0;
				IsPlayerFlag = 0;
				Vec4_Pointer = 0;
                #endregion
            }


            //#
            //## Offset Declarations
            //#
            #region [Offset Declarations]
            private const int
				encodedNameID = 0x00, // The encoded character symbol/name id
				decodedName_Ptr = 0x08, // The decoded character symbol/name string*
				decodedSkeletonName_Ptr = 0x10, // The decoded skeleton name string*
				encodedSkeletonNameID = 0x18, // The encoded skeleton name id
				
				decodedMovers1Name_Ptr = 0x20, // Haven't looked in to this one //!
				encodedMovers1NameID = 0x28, // Haven't looked in to this one //!
				decodedMovers2Name_Ptr = 0x30, // Haven't looked in to this one //!
				encodedMovers2NameID = 0x38, // Haven't looked in to this one //!
				
				headName_Ptr = 0x90, // The decoded head part name string*
				unkHeadPartInt1 = 0x98, // Unknown head part int 1
				unkHeadPartInt2 = 0x9C, // Unknown head part int 2
				
				hairClothName_Ptr = 0xA0, // The decoded hair cloth name string*
				unkHairClothInt1 = 0xA8, // Unknown hair cloth int 1
				unkSkelNameInt1 = 0xA8, // Unknown skel name int 1
				unkHairClothInt2 = 0xAC, // Unknown hair cloth int 2
				
				unkSkelNameInt2 = 0xAC, // Unknown skel name int 2
				unkSkelName_Ptr = 0xB0, // The decoded head part name string*
				nextCharacterLook2_0_Ptr = 0xD0, // What? Why?
				nextCharacterLook2_1_Ptr = 0xE0, // And why two of 'em??
				
				ambientOccludersID = 0x100, // Unknown ulong
				modelScale_1 = 0x150, // The scale of the character model
				modelScale_2 = 0x154, // The scale of the character model (but slightly different?)
				isPlayerFlag = 0x160, // A flag for whether the character is a player character (I think)
				vec4_Ptr = 0x1A8 // vec4* (vector?)
			;
            #endregion [offset declarations]



            //#
            //## Variable Declarations
            //#
            #region [Variable Declarations]

            //# #|Private Members|#
            // [private members here]

            //# #|Public Members|#
            /// <summary> The name associated with the current look2 instance. </summary>
            public SID Name { get; set; }

            /// <summary> The start address of the structure in the DC file. </summary>
            public long Address { get; set; }
            


            /// <summary> The encoded character symbol/name id <summary/>
			public ulong EncodedNameID { get; set; }

			/// <summary> The decoded character symbol/name string* <summary/>
			public long DecodedName_Pointer { get; set; }

			/// <summary> The decoded skeleton name string* <summary/>
			public ulong DecodedSkeletonName_Pointer { get; set; }

			/// <summary> The encoded skeleton name id <summary/>
			public ulong EncodedSkeletonNameID { get; set; }

			
			/// <summary> Haven't looked in to this one //! <summary/>
			public ulong DecodedMovers1Name_Pointer { get; set; }

			/// <summary> Haven't looked in to this one //! <summary/>
			public ulong EncodedMovers1NameID { get; set; }

			/// <summary> Haven't looked in to this one //! <summary/>
			public ulong DecodedMovers2Name_Pointer { get; set; }

			/// <summary> Haven't looked in to this one //! <summary/>
			public ulong EncodedMovers2NameID { get; set; }

			
			/// <summary> The decoded head part name string* <summary/>
			public long HeadName_Pointer { get; set; }

			/// <summary> Unknown head part int 1 <summary/>
			public uint UnkHeadPartInt1 { get; set; }

			/// <summary> Unknown head part int 2 <summary/>
			public uint UnkHeadPartInt2 { get; set; }

			
			/// <summary> The decoded hair cloth name string* <summary/>
			public long HairClothName_Pointer { get; set; }

			/// <summary> Unknown hair cloth int 1 <summary/>
			public uint UnkHairClothInt1 { get; set; }

			/// <summary> Unknown skel name int 1 <summary/>
			public uint UnkSkelNameInt1 { get; set; }

			/// <summary> Unknown hair cloth int 2 <summary/>
			public uint UnkHairClothInt2 { get; set; }

			
			/// <summary> Unknown skel name int 2 <summary/>
			public uint UnkSkelNameInt2 { get; set; }

			/// <summary> The decoded head part name string* <summary/>
			public long UnkSkelName_Pointer { get; set; }

			/// <summary> What? Why? <summary/>
			public ulong NextCharacterLook2_0_Pointer { get; set; }

			/// <summary> And why two of 'em?? <summary/>
			public ulong NextCharacterLook2_1_Pointer { get; set; }

			
			/// <summary> Unknown ulong <summary/>
			public ulong AmbientOccludersID { get; set; }

			/// <summary> The scale of the character model <summary/>
			public float ModelScale_1 { get; set; }

			/// <summary> The scale of the character model (but slightly different?) <summary/>
			public float ModelScale_2 { get; set; }

			/// <summary> A flag for whether the character is a player character (I think) <summary/>
			public ulong IsPlayerFlag { get; set; }

			/// <summary> vec4* (vector?) <summary/>
			public long Vec4_Pointer { get; set; }


            /// <summary> Size of the current structure type. </summary>
            public const int Size =  0x3B0; // The size of the structure

            /// <summary> The raw binary data of the current StructureTemplate instance. </summary>
            public byte[] RawData { get; set; }
            #endregion [variable declarations]
        }




        /// <summary>
        /// 
        /// </summary>
        public struct UnmappedStructure
        {
            public UnmappedStructure(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                Message = $"Unknown Structure [\n\tType: {Type.DecodedID}\n\tName: {Name.DecodedID}\n\tAddress: 0x{Address.ToString("X").PadLeft(8, '0')}\n]";
            }

            public SID Name { get; set; }
            public long Address { get; set; }

            public string Message { get; set; }
        }

        public struct BaseStruct
        {
            public BaseStruct(long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
            }

            public SID Name { get; set; }
            public long Address { get; set; }
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
            public SID(byte[] EncodedSIDArray)
            {
                DecodedID = SIDBase.DecodeSIDHash(EncodedSIDArray);
                EncodedID = BitConverter.ToString(EncodedSIDArray).Replace("-", emptyStr);
                RawID = (KnownSIDs) BitConverter.ToUInt64(EncodedSIDArray, 0);
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
        /// The amount of items in the sidbase.bin's lookup table.<br/>
        /// No reason to read it as a long integer, as if it's that big; we've already fucked off by now.
        /// </summary>
        public readonly int HashTableCount;
        
        
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
