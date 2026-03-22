using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using NaughtyDogDCReader;


namespace NaughtyDogDCReader
{
    public partial class Main
    {
        //=================================\\
        //--|   Structure Definitions   |--\\
        //=================================\\
        #region [Structure Definitions]
        #pragma warning disable IDE1006

        /// <summary>
        /// Details on the initial header array for the provided DC file, as well as an array of any present HeaderItems.
        /// </summary>
        public class DCModule
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="DCModule"></param>
            /// <param name="ModuleName"></param>
            public DCModule(byte[] DCModule, string ModuleName)
            {
                //#
                //## Run a few basic integrity checks
                //#

                // Read file magic from header
                var currentMagic = DCModule.Take(8).ToArray();
                if (!currentMagic.SequenceEqual(expectedMagic))
                {
                    echo($"ERROR; Invalid File Provided: Invalid file magic. 0x{BitConverter.ToUInt64(currentMagic, 0):X} != 0x{BitConverter.ToUInt64(expectedMagic, 0):X}");
                    return;
                }

                var integrityCheck = SID.Parse(GetSubArray(DCModule, 0x20));
                if (integrityCheck.RawID != KnownSIDs.array)
                {
                    echo($"ERROR; Unexpected SID \"{integrityCheck.RawID:X}\" at 0x20. Expected encoded id of type \"map\"; Aborting.");
                    return;
                }
                if ((unkInt0 = BitConverter.ToInt32(DCModule, 0x10)) != 1)
                {
                    echo($"ERROR; Unexpected Value \"{unkInt0}\" read at 0x10, aborting.");
                    return;
                }
                if ((HeaderTableStartPointer = BitConverter.ToInt64(DCModule, headerTableStartPointerAddr)) != 0x28)
                {
                    echo($"ERROR; Unexpected Value \"{HeaderTableStartPointer}\" read at {headerTableStartPointerAddr}, aborting.");
                    return;
                }



                //#
                //## Read remaining header info
                //#
                BinFileLength = BitConverter.ToInt64(DCModule, 0x8);
                TableLength = BitConverter.ToInt32(DCModule, 0x14);

                Entries = new DCEntry[TableLength];


                //#
                //## Parse header content table
                //#
#if false
                var pre = new[] { DateTime.Now.Minute, DateTime.Now.Second };
#endif
                echo($"Parsing DC Content Table (Length: {TableLength.ToString().PadLeft(2, '0')})\n ");
                UpdateStatusLabel(new[] { "Reading Script...", emptyStr, emptyStr });

                for (int tableIndex = 0, addr = 0x28; tableIndex < TableLength; tableIndex++, addr += 24)
                {
                    Entries[tableIndex] = new DCEntry(DCModule, addr);
                }

#if false
                echo ($"{DateTime.Now.Minute - pre[0]}:{DateTime.Now.Second - pre[1]}");
#endif
            }


            //#
            //## Variable Declarations
            //#
            private readonly byte[] expectedMagic = new byte[] { 0x30, 0x30, 0x43, 0x44, 0x01, 0x00, 0x00, 0x00 };
            private readonly int headerTableStartPointerAddr = 0x18; // 0x18

            public readonly int unkInt0;
            public readonly long HeaderTableStartPointer;

            public long BinFileLength;
            public int TableLength;


            /// <summary>
            /// An array of the DCFileHeader.HeaderItems parsed from the provided DC file.
            /// </summary>
            public DCEntry[] Entries;



            /// <summary>
            /// An individual item (module?) from the array at the beginning of the DC file.
            /// </summary>
            public struct DCEntry
            {
                /// <summary>
                /// //!
                /// </summary>
                /// <param name="DCFile"></param>
                /// <param name="Address"> The address of the b </param>
                public DCEntry(byte[] DCFile, int Address)
                {
                    this.Address = Address;
                    
                    Name = SID.Parse(GetSubArray(DCFile, this.Address));
                    Type = SID.Parse(GetSubArray(DCFile, this.Address + 8));

                    StructAddress = BitConverter.ToInt64(GetSubArray(DCFile, this.Address + 16), 0);


                    Struct = LoadMappedDCStructs(Main.DCFile, Type, StructAddress, Name);
                }


                /// <summary>
                /// The Address of this Header Item in the DC file.
                /// </summary>
                public int Address;

                /// <summary>
                /// The name of the current entry in the DC file header.
                /// </summary>
                public SID Name;

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
            }
        }





        /// <summary>
        /// A collection of whatever-the-fuck naughty dog felt like including. This may be annoying.
        /// </summary>
        public struct map
        {
            public map(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                Length = BitConverter.ToInt64(GetSubArray(binFile, (int) Address), 0);

                StructNames = new SID[Length];

                Structs = new object[Length];


                if (Length < 1)
                {
                    echo($"  # Empty Map Structures. ({Name.DecodedID})");
                    return;
                }
                var mapNamesArrayPtr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 8), 0);
                var mapStructsArrayPtr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 16), 0);


                for (var arrayIndex = 0; arrayIndex < Length; mapStructsArrayPtr += 8, mapNamesArrayPtr += 8, arrayIndex++)
                {
                    var structAddress = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)mapStructsArrayPtr), 0);

                    var structTypeID = SID.Parse(GetSubArray(binFile, structAddress - 8));
                    var structName   = SID.Parse(GetSubArray(binFile, (int)mapNamesArrayPtr));

                    StructNames[arrayIndex] = structName;

                    Structs[arrayIndex] = LoadMappedDCStructs(binFile, structTypeID, structAddress, structName);
                }
            }

            /*
             * 0x00: Map array length
             * 0x08: Pointer to array of encoded names for each entry
             * 0x10: Pointer to an array of pointers ton
            */


            /// <summary>
            /// The name of the map item.
            /// </summary>
            public SID Name;

            public long Address;

            public long Length;



            public SID[] StructNames { get; set; }

            public object[] Structs { get; set; }
        }
        






        /// <summary>
        /// 
        /// </summary>
        public struct symbol_array
        {
            public symbol_array(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                var arrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int)Address), 0);
                var arrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int)Address + 8), 0);

                Symbols = new SID[arrayLen];

                for (var i = 0; i < arrayLen; arrayAddr += 8, i++)
                {
                    var dat = GetSubArray(binFile, (int)arrayAddr);

                    Symbols[i] = SID.Parse(dat);
                }
            }


            public SID Name;
            public long Address;

            public SID[] Symbols { get; set; }
        }



        /// <summary>
        /// 
        /// </summary>
        public struct ammo_to_weapon_array
        {
            public ammo_to_weapon_array(byte[] binFile, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;

                var symbols = new List<string[]>();
                var hashes = new List<byte[][]>();

                var arrayLen = BitConverter.ToInt64(GetSubArray(binFile, (int) Address), 0);
                var arrayAddr = BitConverter.ToInt64(GetSubArray(binFile, (int) Address + 8), 0);

                echo($"\n  # Parsing Ammo-to-Weapon Structures...");
                for (var i = 0; i < arrayLen; arrayAddr += 16, i++)
                {
                    hashes.Add(new[] { GetSubArray(binFile, (int) arrayAddr + 8), GetSubArray(binFile, (int) arrayAddr) });
                    symbols.Add(new[] { SIDBase.DecodeSIDHash(hashes.Last()[0]), SIDBase.DecodeSIDHash(hashes.Last()[1]) });
                }
                echo($"  # Finished Parsing Ammo-to-Weapon Structures.");
                UpdateStatusLabel(new[] { null, null, emptyStr });

                Symbols = symbols.ToArray();
                Hashes = hashes.ToArray();
            }

            public SID Name;
            public long Address;

            /// <summary>
            /// 0: Ammo Type <br/>
            /// 1: Weapon Name
            /// </summary>
            public string[][] Symbols { get; set; }
            public byte[][][] Hashes { get; set; }
        }








        /// <summary>
        /// Initialize a new instance of the firearm-gameplay-def struct.
        /// </summary>
        public struct firearm_gameplay_def
        {
            /// <summary>
            /// Create a new instance of the firearm_gameplay_def struct.
            /// </summary>
            /// <param name="binFile"> The DC file this firearm_gameplay_def instance is being read from. </param>
            /// <param name="Address"> The start address of the structure in the DC file. </param>
            /// <param name="Name"> The name associated with the current firearm_gameplay_def instance. </param>
            public firearm_gameplay_def(byte[] binFile, long Address, SID Name)
            {
                //#
                //## Variable Initializations
                //#
                #region [variable initializations]
                this.Name = Name;
                this.Address = Address;

                RawData = GetSubArray(binFile, (int) Address, Size);


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
                BaseAmmoCount = 0;
                UnknownFloat_at0xA0 = 0;
                UnknownFloat_at0xA4 = 0;
                UnknownFloat_at0xA8 = 0;
                UnknownFloat_at0xAC = 0;

                ScopedLagSettings_Pointer = 0;

                ProneAim0SID = SID.Empty;
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
                UnknownAimSID = SID.Empty;
                HorseAimSID = SID.Empty;
                ProneAim1SID = SID.Empty;
                AimAssistSID = SID.Empty;
                UnknownFloat_at0x1C0 = 0;
                UnknownFloat_at0x1C4 = 0;
                FirearmDamageMovementDef_Pointer = 0;
                HapticSettingsSID = SID.Empty;
                RumbleSettingsSID = SID.Empty;
                CameraShakeRightSID = SID.Empty;
                CameraShakeLeftSID = SID.Empty;
                PointCurve0_Pointer = SID.Empty;

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
                FirearmStatBarDef_Pointer = 0;
                PointCurve1_Pointer = 0;
                PointCurve2_Pointer = 0;
                DamageLinksSID = 0;






                //#
                //## Offset Initializations
                //#
                Offsets = new[]
                {
                    0x00, // ammoTypes_Ptr (symbol-array containing ammo type names)
				
					0x14, // unknownFloat_at0x14 (Unknown float)
					0x18, // unknownFloat_at0x18 (Unknown float)
					0x20, // unknownInt_at0x20 (Unknown int)
					0x24, // unknownFloat_at0x24 (Unknown float)
					0x28, // unknownFloat_at0x28 (Unknown float)
					0x2C, // unknownFloat_at0x2C (Unknown float)
					0x30, // unknownFloat_at0x30 (Unknown float)
					0x48, // unknownFloat_at0x48 (Unknown float)
					0x50, // unknownFloat_at0x50 (Unknown float)
					0x54, // unknownFloat_at0x54 (Unknown float)
					0x60, // unknownFloat_at0x60 (Unknown float)
					0x68, // unknownInt_at0x68 (Unknown int)
					0x6C, // unknownFloat_at0x6C (Unknown float)
					0x70, // unknownFloat_at0x70 (Unknown float)
					0x74, // unknownFloat_at0x74 (Unknown float)
					0x78, // unknownFloat_at0x78 (Unknown float)
					0x7C, // unknownFloat_at0x7C (Unknown float)
					0x80, // unknownFloat_at0x80 (Unknown float)
					0x84, // unknownFloat_at0x84 (Unknown float)
					0x88, // unknownFloat_at0x88 (Unknown float)
					0x8C, // unknownFloat_at0x8C (Unknown float)
					0x98, // baseAmmoCount (integer (long or int?) amount of base ammo)
					0xA0, // unknownFloat_at0xA0 (Unknown float)
					0xA4, // unknownFloat_at0xA4 (Unknown float)
					0xA8, // unknownFloat_at0xA8 (Unknown float)
					0xAC, // unknownFloat_at0xAC (Unknown float)
				
					0xB0, // scopedLagSettings_Ptr (scoped-lag-settings*)
				
					0xB8, // proneAim0SID (Unknown SID)
					0xC0, // unknownFloat_at0xC0 (Unknown float)
					0xC4, // unknownFloat_at0xC4 (Unknown float)
					0xC8, // unknownFloat_at0xC8 (Unknown float)
					0xCC, // unknownFloat_at0xCC (Unknown float)
				
					0xD0, // firearmAimDeviationDef0_Ptr (firearm-aim-deviation-def*)
					0xD8, // firearmAimDeviationDef1_Ptr (firearm-aim-deviation-def*)
				
					0xE0, // unknownFloat_at0xE0 (Unknown float)
					0xE4, // unknownFloat_at0xE4 (Unknown float)
					0xE8, // unknownFloat_at0xE8 (Unknown float)
				
					0xF0, // firearmKickbackDef0_Ptr (firearm-kickback-def*)
					0xF8, // firearmKickbackDef1_Ptr (firearm-kickback-def*)
					0x108, // firearmKickbackDef2_Ptr (firearm-kickback-def*)
					0x110, // firearmKickbackDef3_Ptr (firearm-kickback-def*)
				
					0x118, // unknownFloat_at0x118 (Unknown float)
					0x120, // unknownFloat_at0x120 (Unknown float)
				
					0x128, // lerpAimSwaySettings0_Ptr (lerp-aim-sway-settings*)
					0x130, // lerpAimSwaySettings1_Ptr (lerp-aim-sway-settings*)
					0x140, // lerpAimSwaySettings2_Ptr (lerp-aim-sway-settings*)
					0x148, // swayHoldBreathSettings0_Ptr (sway-hold-breath-settings*)
					0x150, // swayHoldBreathSettings1_Ptr (sway-hold-breath-settings*)
				
					0x158, // unknownInt_at0x158 (Unknown int)
					0x15C, // unknownFloat_at0x15C (Unknown float)
					0x160, // unknownFloat_at0x160 (Unknown float)
					0x164, // unknownFloat_at0x164 (Unknown float)
					0x168, // unknownFloat_at0x168 (Unknown float)
					0x16C, // unknownFloat_at0x16C (Unknown float)
					0x194, // unknownInt_at0x194 (Unknown int)
					0x19C, // unknownFloat_at0x19C (Unknown float)
					0x1A0, // unknownAimSID (Unknown SID)
					0x1A8, // horseAimSID (Unknown SID)
					0x1B0, // proneAim1SID (Unknown SID)
					0x1B8, // aimAssistSID (Unknown SID)
					0x1C0, // unknownFloat_at0x1C0 (Unknown float)
					0x1C4, // unknownFloat_at0x1C4 (Unknown float)
					0x1C8, // firearmDamageMovementDef_Ptr (firearm-damage-movement-def*)
					0x1F8, // hapticSettingsSID (Unknown SID)
					0x200, // rumbleSettingsSID (Unknown SID)
					0x208, // cameraShakeRightSID (Unknown SID)
					0x210, // cameraShakeLeftSID (Unknown SID)
					0x228, // pointCurve0_Ptr (Unknown SID)
				
					0x230, // unknownFloat_at0x230 (Unknown float)
					0x234, // unknownFloat_at0x234 (Unknown float)
				
					0x250, // unknownFloat_at0x250 (Unknown float)
					0x254, // unknownFloat_at0x254 (Unknown float)
					0x260, // unknownFloat_at0x260 (Unknown float)
					0x264, // unknownFloat_at0x264 (Unknown float)
					0x268, // unknownFloat_at0x268 (Unknown float)
					0x26C, // unknownFloat_at0x26C (Unknown float)
					0x270, // unknownFloat_at0x270 (Unknown float)
					0x274, // unknownFloat_at0x274 (Unknown float)
				
				
					0x278, // gunmoveIkSettings_Ptr (gunmove-ik-settings*)
					0x280, // firearmStatBarDef_Ptr (firearm-stat-bar-def*)
					0x288, // pointCurve1_Ptr (Unknown ulong)
					0x290, // pointCurve2_Ptr (Unknown ulong)
					0x2A0  // damageLinksSID (Unknown ulong)
                };





                //#
                //## Assign Property Values
                //#
                var that = this.MemberwiseClone();
                var properties = that.GetType().GetProperties();

                for (var i = 0; i < properties.Length; i++)
                {
                    properties[i].SetValue(that, ReadPropertyValueByType(RawData, properties[i], Offsets[i]));
                }

                this = (firearm_gameplay_def) that;
                #endregion
            }


            //#
            //## Offset Declarations
            //#
            /// <summary> HUD2 Reticle Definition structure offset. </summary>
            private readonly int[] Offsets;




            //#
            //## Variable Declarations
            //#
            #region [Variable Declarations]

            //# #| Public Fields |#
            /// <summary> The name associated with the current firearm_gameplay_def instance. </summary>
            public SID Name;

            /// <summary> The start address of the structure in the DC file. </summary>
            public long Address;

            /// <summary> Size of the current structure type. </summary>
            public const int Size =  0x2B0; // The size of the structure;

            /// <summary> The raw binary data of the current StructureTemplate instance. </summary>
            public byte[] RawData;


            //# #| Public Properties |#
            /// <summary> symbol-array containing ammo type names <summary/>
            public long AmmoTypes_Pointer { get; set; }


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
            public long ScopedLagSettings_Pointer { get; set; }


            /// <summary> Unknown SID <summary/>
            public SID ProneAim0SID { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0xC0 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0xC4 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0xC8 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0xCC { get; set; }


            /// <summary> firearm-aim-deviation-def* <summary/>
            public long FirearmAimDeviationDef0_Pointer { get; set; }

            /// <summary> firearm-aim-deviation-def* <summary/>
            public long FirearmAimDeviationDef1_Pointer { get; set; }


            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0xE0 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0xE4 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0xE8 { get; set; }


            /// <summary> firearm-kickback-def* <summary/>
            public long FirearmKickbackDef0_Pointer { get; set; }

            /// <summary> firearm-kickback-def* <summary/>
            public long FirearmKickbackDef1_Pointer { get; set; }

            /// <summary> firearm-kickback-def* <summary/>
            public long FirearmKickbackDef2_Pointer { get; set; }

            /// <summary> firearm-kickback-def* <summary/>
            public long FirearmKickbackDef3_Pointer { get; set; }


            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0x118 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0x120 { get; set; }


            /// <summary> lerp-aim-sway-settings* <summary/>
            public long LerpAimSwaySettings0_Pointer { get; set; }

            /// <summary> lerp-aim-sway-settings* <summary/>
            public long LerpAimSwaySettings1_Pointer { get; set; }

            /// <summary> lerp-aim-sway-settings* <summary/>
            public long LerpAimSwaySettings2_Pointer { get; set; }

            /// <summary> sway-hold-breath-settings* <summary/>
            public long SwayHoldBreathSettings0_Pointer { get; set; }

            /// <summary> sway-hold-breath-settings* <summary/>
            public long SwayHoldBreathSettings1_Pointer { get; set; }


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

            /// <summary> Unknown SID <summary/>
            public SID UnknownAimSID { get; set; }

            /// <summary> Unknown SID <summary/>
            public SID HorseAimSID { get; set; }

            /// <summary> Unknown SID <summary/>
            public SID ProneAim1SID { get; set; }

            /// <summary> Unknown SID <summary/>
            public SID AimAssistSID { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0x1C0 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_at0x1C4 { get; set; }

            /// <summary> firearm-damage-movement-def* <summary/>
            public long FirearmDamageMovementDef_Pointer { get; set; }

            /// <summary> Unknown SID <summary/>
            public SID HapticSettingsSID { get; set; }

            /// <summary> Unknown SID <summary/>
            public SID RumbleSettingsSID { get; set; }

            /// <summary> Unknown SID <summary/>
            public SID CameraShakeRightSID { get; set; }

            /// <summary> Unknown SID <summary/>
            public SID CameraShakeLeftSID { get; set; }

            /// <summary> Unknown SID <summary/>
            public SID PointCurve0_Pointer { get; set; }


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
            public ulong FirearmStatBarDef_Pointer { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong PointCurve1_Pointer { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong PointCurve2_Pointer { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong DamageLinksSID { get; set; }
            #endregion [variable declarations]
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


                UnknownInt_at0x00 = 0;
                UnknownInt_at0x04 = 0;
                UnknownInt_at0x08 = 0;

                UnknownFloat_at0x0C = 0;
                FirearmGameplayDef_Pointer = 0;
                BlindfireAutoTargetDef_Pointer = 0;

                UnknownLong_at0x20 = 0;
                GrenadeGameplayDef_Pointer = 0;

                MeleeGameplayDef_Pointer = 0;

                UnknownLong_at0x38 = 0;
                UnknownLong_at0x40 = 0;
                UnknownLong_at0x48 = 0;
                UnknownByteArray_at0x50_s0x08 = null;

                Hud2ReticleDef_Pointer = 0;
                UnknownLong_at0x60 = 0;
                ZoomCameraDoFSettingsSP = 0;
                ZoomSniperCameraDoFSettingsSP = SID.Empty;
                UnknownLong_at0x78 = 0;
                ScreenEffectSettings_Pointer = 0;
                UnknownByteArray_at0x88_s0x08 = null;






                //#
                //## Offset Initializations
                //#
                Offsets = new[]
                {
                    0x00, // unknownInt_at0x00 (Unknown uint)
					0x04, // unknownInt_at0x04 (Unknown uint)
					0x08, // unknownInt_at0x08 (Unknown uint)
				
					0x0C, // unknownFloat_at0x0C (unknown, usually set to -1, but the bow has it set to zero)
					0x10, // firearmGameplayDef_Ptr (firearm-gameplay-def*)
					0x18, // blindfireAutoTargetDef_Ptr (blindfire-auto-target-def*)
				
					0x20, // unknownLong_at0x20 (Unknown ulong)
					0x28, // grenadeGameplayDef_Ptr (grenade-gameplay-def*)
				
					0x30, // meleeGameplayDef_Ptr (melee-gameplay-def*)
				
					0x38, // unknownLong_at0x38 (Unknown ulong)
					0x40, // unknownLong_at0x40 (Unknown ulong)
					0x48, // unknownLong_at0x48 (Unknown ulong)
					0x50, // unknownByteArray_at0x50_s0x08 (Unknown byte Array)
				
					0x58, // hud2ReticleDef_Ptr (hud2-reticle-def*)
					0x60, // unknownLong_at0x60 (Unknown ulong)
					0x68, // zoomCameraDoFSettingsSP (*zoom-camera-dof-settings-sp*)
					0x70, // zoomSniperCameraDoFSettingsSP (*zoom-sniper-camera-dof-settings-sp*)
					0x78, // unknownLong_at0x78 (Unknown ulong)
					0x80, // screenEffectSettings_Ptr (screen-effect-settings*)
					0x88  // unknownByteArray_at0x88_s0x08 (Unknown byte Array)
                };





                //#
                //## Assign Property Values
                //#
                var that = this.MemberwiseClone();
                var properties = that.GetType().GetProperties();

                for (var i = 0; i < properties.Length; i++)
                {
                    properties[i].SetValue(that, ReadPropertyValueByType(RawData, properties[i], Offsets[i]));
                }

                this = (look2) that;
                #endregion
            }


            //#
            //## Offset Declarations
            //#
            /// <summary> HUD2 Reticle Definition structure offset. </summary>
            private readonly int[] Offsets;




            //#
            //## Variable Declarations
            //#
            #region [Variable Declarations]

            //# #| Public Fields |#
            /// <summary> The name associated with the current look2 instance. </summary>
            public SID Name;

            /// <summary> The start address of the structure in the DC file. </summary>
            public long Address;

            /// <summary> Size of the current structure type. </summary>
            public const int Size =  0x90; // The size of the structure;

            /// <summary> The raw binary data of the current StructureTemplate instance. </summary>
            public byte[] RawData;


            //# #| Public Properties |#
            /// <summary> Unknown uint <summary/>
            public uint UnknownInt_at0x00 { get; set; }

            /// <summary> Unknown uint <summary/>
            public uint UnknownInt_at0x04 { get; set; }

            /// <summary> Unknown uint <summary/>
            public uint UnknownInt_at0x08 { get; set; }


            /// <summary> unknown, usually set to -1, but the bow has it set to zero <summary/>
            public float UnknownFloat_at0x0C { get; set; }

            /// <summary> firearm-gameplay-def* <summary/>
            public ulong FirearmGameplayDef_Pointer { get; set; }

            /// <summary> blindfire-auto-target-def* <summary/>
            public ulong BlindfireAutoTargetDef_Pointer { get; set; }


            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x20 { get; set; }

            /// <summary> grenade-gameplay-def* <summary/>
            public ulong GrenadeGameplayDef_Pointer { get; set; }


            /// <summary> melee-gameplay-def* <summary/>
            public ulong MeleeGameplayDef_Pointer { get; set; }


            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x38 { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x40 { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x48 { get; set; }

            /// <summary> Unknown byte Array <summary/>
            public byte[] UnknownByteArray_at0x50_s0x08 { get; set; }


            /// <summary> hud2-reticle-def* <summary/>
            public ulong Hud2ReticleDef_Pointer { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x60 { get; set; }

            /// <summary> *zoom-camera-dof-settings-sp* <summary/>
            public ulong ZoomCameraDoFSettingsSP { get; set; }

            /// <summary> *zoom-sniper-camera-dof-settings-sp* <summary/>
            public SID ZoomSniperCameraDoFSettingsSP { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x78 { get; set; }

            /// <summary> screen-effect-settings* <summary/>
            public ulong ScreenEffectSettings_Pointer { get; set; }

            /// <summary> Unknown byte Array <summary/>
            public byte[] UnknownByteArray_at0x88_s0x08 { get; set; }
            #endregion [variable declarations]
        }



        /// <summary>
        /// Initialize a new instance of the hud2-reticle-def struct.
        /// </summary>
        public struct hud2_reticle_def
        {
            /// <summary>
            /// Create a new instance of the hud2_reticle_def struct.
            /// </summary>
            /// <param name="binFile"> The DC file this hud2_reticle_def instance is being read from. </param>
            /// <param name="Address"> The start address of the structure in the DC file. </param>
            /// <param name="Name"> The name associated with the current hud2_reticle_def instance. </param>
            public hud2_reticle_def(byte[] binFile, long Address, SID Name)
            {
                //#
                //## Variable Initializations
                //#
                #region [variable initializations]
                this.Name = Name;
                this.Address = Address;

                RawData = GetSubArray(binFile, (int) Address, Size);


                UnknownLong_At0x00 = 0;
                UnknownLong_At0x08 = 0;
                UnknownLong_At0x10 = 0;
                UnknownLong_At0x18 = 0;
                UnknownLong_At0x20 = 0;
                UnknownFloat_At0x28 = 0;
                UnknownFloat_At0x30 = 0;






                //#
                //## Offset Initializations
                //#
                Offsets = new[]
                {
                    0x00, // unknownLong_At0x00 (Unknown long)
					0x08, // unknownLong_At0x08 (Unknown long)
					0x10, // unknownLong_At0x10 (Unknown long)
					0x18, // unknownLong_At0x18 (Unknown long)
					0x20, // unknownLong_At0x20 (Unknown long)
					0x28, // unknownFloat_At0x28 (Unknown float)
					0x2C  // unknownFloat_At0x30 (Unknown float)
                };





                //#
                //## Assign Property Values
                //#
                var that = this.MemberwiseClone();
                var properties = that.GetType().GetProperties();

                for (var i = 0; i < properties.Length; i++)
                {
                    properties[i].SetValue(that, ReadPropertyValueByType(RawData, properties[i], Offsets[i]));
                }

                this = (hud2_reticle_def) that;
                #endregion
            }


            //#
            //## Offset Declarations
            //#
            /// <summary> HUD2 Reticle Definition structure offset. </summary>
            private readonly int[] Offsets;




            //#
            //## Variable Declarations
            //#
            #region [Variable Declarations]

            //# #| Public Fields |#
            /// <summary> The name associated with the current hud2_reticle_def instance. </summary>
            public SID Name;

            /// <summary> The start address of the structure in the DC file. </summary>
            public long Address;

            /// <summary> Size of the current structure type. </summary>
            public const int Size =  0x30; // The size of the structure;

            /// <summary> The raw binary data of the current StructureTemplate instance. </summary>
            public byte[] RawData;


            //# #| Public Properties |#
            /// <summary> Unknown long <summary/>
            public long UnknownLong_At0x00 { get; set; }

            /// <summary> Unknown long <summary/>
            public long UnknownLong_At0x08 { get; set; }

            /// <summary> Unknown long <summary/>
            public long UnknownLong_At0x10 { get; set; }

            /// <summary> Unknown long <summary/>
            public long UnknownLong_At0x18 { get; set; }

            /// <summary> Unknown long <summary/>
            public long UnknownLong_At0x20 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_At0x28 { get; set; }

            /// <summary> Unknown float <summary/>
            public float UnknownFloat_At0x30 { get; set; }
            #endregion [variable declarations]
        }



        /// <summary>
        /// Initialize a new instance of the weapon-gameplay-def struct.
        /// </summary>
        public struct weapon_gameplay_def
        {
            /// <summary>
            /// Create a new instance of the weapon_gameplay_def struct.
            /// </summary>
            /// <param name="binFile"> The DC file this weapon_gameplay_def instance is being read from. </param>
            /// <param name="Address"> The start address of the structure in the DC file. </param>
            /// <param name="Name"> The name associated with the current weapon_gameplay_def instance. </param>
            public weapon_gameplay_def(byte[] binFile, long Address, SID Name)
            {
                //#
                //## Variable Initializations
                //#
                #region [variable initializations]
                this.Name = Name;
                this.Address = Address;

                RawData = GetSubArray(binFile, (int) Address, Size);


                UnknownInt_at0x00 = 0;
                UnknownInt_at0x04 = 0;
                UnknownInt_at0x08 = 0;

                UnknownFloat_at0x0C = 0;
                FirearmGameplayDef_Pointer = 0;
                BlindfireAutoTargetDef_Pointer = 0;

                UnknownLong_at0x20 = 0;
                GrenadeGameplayDef_Pointer = 0;

                MeleeGameplayDef_Pointer = 0;

                UnknownLong_at0x38 = 0;
                UnknownLong_at0x40 = 0;
                UnknownLong_at0x48 = 0;
                UnknownByteArray_at0x50_s0x08 = null;

                Hud2ReticleDef_Pointer = 0;
                UnknownLong_at0x60 = 0;
                ZoomCameraDoFSettingsSP = 0;
                ZoomSniperCameraDoFSettingsSP = SID.Empty;
                UnknownLong_at0x78 = 0;
                ScreenEffectSettings_Pointer = 0;
                UnknownByteArray_at0x88_s0x08 = null;






                //#
                //## Offset Initializations
                //#
                Offsets = new[]
                {
                    0x00, // unknownInt_at0x00 (Unknown uint)
					0x04, // unknownInt_at0x04 (Unknown uint)
					0x08, // unknownInt_at0x08 (Unknown uint)
				
					0x0C, // unknownFloat_at0x0C (unknown, usually set to -1, but the bow has it set to zero)
					0x10, // firearmGameplayDef_Ptr (firearm-gameplay-def*)
					0x18, // blindfireAutoTargetDef_Ptr (blindfire-auto-target-def*)
				
					0x20, // unknownLong_at0x20 (Unknown ulong)
					0x28, // grenadeGameplayDef_Ptr (grenade-gameplay-def*)
				
					0x30, // meleeGameplayDef_Ptr (melee-gameplay-def*)
				
					0x38, // unknownLong_at0x38 (Unknown ulong)
					0x40, // unknownLong_at0x40 (Unknown ulong)
					0x48, // unknownLong_at0x48 (Unknown ulong)
					0x50, // unknownByteArray_at0x50_s0x08 (Unknown byte Array)
				
					0x58, // hud2ReticleDef_Ptr (hud2-reticle-def*)
					0x60, // unknownLong_at0x60 (Unknown ulong)
					0x68, // zoomCameraDoFSettingsSP (*zoom-camera-dof-settings-sp*)
					0x70, // zoomSniperCameraDoFSettingsSP (*zoom-sniper-camera-dof-settings-sp*)
					0x78, // unknownLong_at0x78 (Unknown ulong)
					0x80, // screenEffectSettings_Ptr (screen-effect-settings*)
					0x88  // unknownByteArray_at0x88_s0x08 (Unknown byte Array)
                };





                //#
                //## Assign Property Values
                //#
                var that = this.MemberwiseClone();
                var properties = that.GetType().GetProperties();

                for (var i = 0; i < properties.Length; i++)
                {
                    properties[i].SetValue(that, ReadPropertyValueByType(RawData, properties[i], Offsets[i]));
                }

                this = (weapon_gameplay_def) that;
                #endregion
            }


            //#
            //## Offset Declarations
            //#
            /// <summary> HUD2 Reticle Definition structure offset. </summary>
            private readonly int[] Offsets;




            //#
            //## Variable Declarations
            //#
            #region [Variable Declarations]

            //# #| Public Fields |#
            /// <summary> The name associated with the current weapon_gameplay_def instance. </summary>
            public SID Name;

            /// <summary> The start address of the structure in the DC file. </summary>
            public long Address;

            /// <summary> Size of the current structure type. </summary>
            public const int Size =  0x90; // The size of the structure;

            /// <summary> The raw binary data of the current StructureTemplate instance. </summary>
            public byte[] RawData;


            //# #| Public Properties |#
            /// <summary> Unknown uint <summary/>
            public uint UnknownInt_at0x00 { get; set; }

            /// <summary> Unknown uint <summary/>
            public uint UnknownInt_at0x04 { get; set; }

            /// <summary> Unknown uint <summary/>
            public uint UnknownInt_at0x08 { get; set; }


            /// <summary> unknown, usually set to -1, but the bow has it set to zero <summary/>
            public float UnknownFloat_at0x0C { get; set; }

            /// <summary> firearm-gameplay-def* <summary/>
            public ulong FirearmGameplayDef_Pointer { get; set; }

            /// <summary> blindfire-auto-target-def* <summary/>
            public ulong BlindfireAutoTargetDef_Pointer { get; set; }


            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x20 { get; set; }

            /// <summary> grenade-gameplay-def* <summary/>
            public ulong GrenadeGameplayDef_Pointer { get; set; }


            /// <summary> melee-gameplay-def* <summary/>
            public ulong MeleeGameplayDef_Pointer { get; set; }


            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x38 { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x40 { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x48 { get; set; }

            /// <summary> Unknown byte Array <summary/>
            public byte[] UnknownByteArray_at0x50_s0x08 { get; set; }


            /// <summary> hud2-reticle-def* <summary/>
            public ulong Hud2ReticleDef_Pointer { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x60 { get; set; }

            /// <summary> *zoom-camera-dof-settings-sp* <summary/>
            public ulong ZoomCameraDoFSettingsSP { get; set; }

            /// <summary> *zoom-sniper-camera-dof-settings-sp* <summary/>
            public SID ZoomSniperCameraDoFSettingsSP { get; set; }

            /// <summary> Unknown ulong <summary/>
            public ulong UnknownLong_at0x78 { get; set; }

            /// <summary> screen-effect-settings* <summary/>
            public ulong ScreenEffectSettings_Pointer { get; set; }

            /// <summary> Unknown byte Array <summary/>
            public byte[] UnknownByteArray_at0x88_s0x08 { get; set; }
            #endregion [variable declarations]
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) sfx-info Structure
        /// </summary>
        public struct sfx_info
        {
            public sfx_info(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) sfx-info-light Structure
        /// </summary>
        public struct sfx_info_light
        {
            public sfx_info_light(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) vox-info Structure
        /// </summary>
        public struct vox_info
        {
            public vox_info(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) particle-effect-desc Structure
        /// </summary>
        public struct particle_effect_desc
        {
            public particle_effect_desc(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) sfx-info-full Structure
        /// </summary>
        public struct sfx_info_full
        {
            public sfx_info_full(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) effect-group-desc Structure
        /// </summary>
        public struct effect_group_desc
        {
            public effect_group_desc(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) render-settings-map Structure
        /// </summary>
        public struct render_settings_map
        {
            public render_settings_map(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) gesture-def Structure
        /// </summary>
        public struct gesture_def
        {
            public gesture_def(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) script-lambda Structure
        /// </summary>
        public struct script_lambda
        {
            public script_lambda(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) state-script Structure
        /// </summary>
        public struct state_script
        {
            public state_script(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) melee-attack Structure
        /// </summary>
        public struct melee_attack
        {
            public melee_attack(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) anim-overlay-set Structure
        /// </summary>
        public struct anim_overlay_set
        {
            public anim_overlay_set(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) d-array Structure
        /// </summary>
        public struct d_array
        {
            public d_array(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) level-set Structure
        /// </summary>
        public struct level_set
        {
            public level_set(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) phys-fx-set Structure
        /// </summary>
        public struct phys_fx_set
        {
            public phys_fx_set(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) environment-info Structure
        /// </summary>
        public struct environment_info
        {
            public environment_info(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) symbol Structure
        /// </summary>
        public struct symbol
        {
            public symbol(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) boolean Structure
        /// </summary>
        public struct boolean
        {
            public boolean(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) music-playlist Structure
        /// </summary>
        public struct music_playlist
        {
            public music_playlist(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) splasher-def Structure
        /// </summary>
        public struct splasher_def
        {
            public splasher_def(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) material-param-table-defs Structure
        /// </summary>
        public struct material_param_table_defs
        {
            public material_param_table_defs(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) lr2-convolution-effect-info Structure
        /// </summary>
        public struct lr2_convolution_effect_info
        {
            public lr2_convolution_effect_info(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) destruction-fx-set Structure
        /// </summary>
        public struct destruction_fx_set
        {
            public destruction_fx_set(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) foliage-map-info Structure
        /// </summary>
        public struct foliage_map_info
        {
            public foliage_map_info(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) feather-blend Structure
        /// </summary>
        public struct feather_blend
        {
            public feather_blend(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) motion-matching-set Structure
        /// </summary>
        public struct motion_matching_set
        {
            public motion_matching_set(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) npc-move-set-def Structure
        /// </summary>
        public struct npc_move_set_def
        {
            public npc_move_set_def(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) loadout-set Structure
        /// </summary>
        public struct loadout_set
        {
            public loadout_set(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) splasher-effect-particle Structure
        /// </summary>
        public struct splasher_effect_particle
        {
            public splasher_effect_particle(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) camera-strafe-settings Structure
        /// </summary>
        public struct camera_strafe_settings
        {
            public camera_strafe_settings(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) pause-card Structure
        /// </summary>
        public struct pause_card
        {
            public pause_card(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) ai-lead-settings Structure
        /// </summary>
        public struct ai_lead_settings
        {
            public ai_lead_settings(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) ai-weapon-loadout Structure
        /// </summary>
        public struct ai_weapon_loadout
        {
            public ai_weapon_loadout(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) game-interactable-def Structure
        /// </summary>
        public struct game_interactable_def
        {
            public game_interactable_def(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) cine-action-pack-def Structure
        /// </summary>
        public struct cine_action_pack_def
        {
            public cine_action_pack_def(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) env-filter-map-node Structure
        /// </summary>
        public struct env_filter_map_node
        {
            public env_filter_map_node(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) tap-exit-table Structure
        /// </summary>
        public struct tap_exit_table
        {
            public tap_exit_table(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) tap-enter-table Structure
        /// </summary>
        public struct tap_enter_table
        {
            public tap_enter_table(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) material-story Structure
        /// </summary>
        public struct material_story
        {
            public material_story(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) investigate-params Structure
        /// </summary>
        public struct investigate_params
        {
            public investigate_params(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) cloth-collider-proto Structure
        /// </summary>
        public struct cloth_collider_proto
        {
            public cloth_collider_proto(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) weapon-anim-overlay Structure
        /// </summary>
        public struct weapon_anim_overlay
        {
            public weapon_anim_overlay(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) explosion-settings Structure
        /// </summary>
        public struct explosion_settings
        {
            public explosion_settings(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) camera-zoom-offsets Structure
        /// </summary>
        public struct camera_zoom_offsets
        {
            public camera_zoom_offsets(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) point-curve Structure
        /// </summary>
        public struct point_curve
        {
            public point_curve(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) haptic-settings Structure
        /// </summary>
        public struct haptic_settings
        {
            public haptic_settings(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) ai-archetype Structure
        /// </summary>
        public struct ai_archetype
        {
            public ai_archetype(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) player-narrative-mode-settings Structure
        /// </summary>
        public struct player_narrative_mode_settings
        {
            public player_narrative_mode_settings(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) look2-collection Structure
        /// </summary>
        public struct look2_collection
        {
            public look2_collection(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) mix-snapshot-info Structure
        /// </summary>
        public struct mix_snapshot_info
        {
            public mix_snapshot_info(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) tap-cross-fade-table Structure
        /// </summary>
        public struct tap_cross_fade_table
        {
            public tap_cross_fade_table(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) npc-demeanor-def Structure
        /// </summary>
        public struct npc_demeanor_def
        {
            public npc_demeanor_def(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) tap-loop-table Structure
        /// </summary>
        public struct tap_loop_table
        {
            public tap_loop_table(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) combat-params Structure
        /// </summary>
        public struct combat_params
        {
            public combat_params(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) d-dict Structure
        /// </summary>
        public struct d_dict
        {
            public d_dict(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) bullet-effect-definition Structure
        /// </summary>
        public struct bullet_effect_definition
        {
            public bullet_effect_definition(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) ai-animation-config Structure
        /// </summary>
        public struct ai_animation_config
        {
            public ai_animation_config(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) simple-npc-config Structure
        /// </summary>
        public struct simple_npc_config
        {
            public simple_npc_config(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) model-viewer-model Structure
        /// </summary>
        public struct model_viewer_model
        {
            public model_viewer_model(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) player-move-to-settings Structure
        /// </summary>
        public struct player_move_to_settings
        {
            public player_move_to_settings(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) character-vox-table Structure
        /// </summary>
        public struct character_vox_table
        {
            public character_vox_table(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) fast-grenade-data Structure
        /// </summary>
        public struct fast_grenade_data
        {
            public fast_grenade_data(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) global-combat-params Structure
        /// </summary>
        public struct global_combat_params
        {
            public global_combat_params(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) character-wrinkle-sets Structure
        /// </summary>
        public struct character_wrinkle_sets
        {
            public character_wrinkle_sets(SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



        /// <summary>
        /// Initialize a new instance of the (unmapped) float Structure
        /// </summary>
        public struct @float
        {
            public @float (SID Type, long Address, SID Name)
            {
                this.Name = Name;
                this.Address = Address;
                TypeID = Type;
            }

            public SID TypeID;

            public SID Name;
            public long Address;
        }



    /// <summary>
    /// Initialize a new instance of the (unmapped) camera-follow-settings Structure
    /// </summary>
    public struct camera_follow_settings
    {
        public camera_follow_settings(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) actor-state-list Structure
    /// </summary>
    public struct actor_state_list
    {
        public actor_state_list(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) game-character-collision Structure
    /// </summary>
    public struct game_character_collision
    {
        public game_character_collision(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) lens-flare-desc Structure
    /// </summary>
    public struct lens_flare_desc
    {
        public lens_flare_desc(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) locator-chain Structure
    /// </summary>
    public struct locator_chain
    {
        public locator_chain(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) character-ambient-occluder-array Structure
    /// </summary>
    public struct character_ambient_occluder_array
    {
        public character_ambient_occluder_array(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) melee-attack-behavior-list Structure
    /// </summary>
    public struct melee_attack_behavior_list
    {
        public melee_attack_behavior_list(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) small-fast-point-curve Structure
    /// </summary>
    public struct small_fast_point_curve
    {
        public small_fast_point_curve(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) item-drop-type-table Structure
    /// </summary>
    public struct item_drop_type_table
    {
        public item_drop_type_table(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) var-tap-table Structure
    /// </summary>
    public struct var_tap_table
    {
        public var_tap_table(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) ocean-waves-parameter Structure
    /// </summary>
    public struct ocean_waves_parameter
    {
        public ocean_waves_parameter(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) ai-follow-settings Structure
    /// </summary>
    public struct ai_follow_settings
    {
        public ai_follow_settings(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) bullet-settings Structure
    /// </summary>
    public struct bullet_settings
    {
        public bullet_settings(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) env-filter-info Structure
    /// </summary>
    public struct env_filter_info
    {
        public env_filter_info(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) ai-melee-skill-settings Structure
    /// </summary>
    public struct ai_melee_skill_settings
    {
        public ai_melee_skill_settings(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) rad-map-info Structure
    /// </summary>
    public struct rad_map_info
    {
        public rad_map_info(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) ai-post-selector-set Structure
    /// </summary>
    public struct ai_post_selector_set
    {
        public ai_post_selector_set(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) question-page-array Structure
    /// </summary>
    public struct question_page_array
    {
        public question_page_array(SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



    /// <summary>
    /// Initialize a new instance of the (unmapped) array* Structure
    /// </summary>
    public struct array_
    {
        public array_ (SID Type, long Address, SID Name)
        {
            this.Name = Name;
            this.Address = Address;
            TypeID = Type;
        }

        public SID TypeID;

        public SID Name;
        public long Address;
    }



/// <summary>
/// Initialize a new instance of the (unmapped) driveable-vehicle-settings Structure
/// </summary>
public struct driveable_vehicle_settings
{
    public driveable_vehicle_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) worldspace-message Structure
/// </summary>
public struct worldspace_message
{
    public worldspace_message(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) E3FCF89CA87939CB Structure
/// </summary>
public struct E3FCF89CA87939CB
{
    public E3FCF89CA87939CB(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) decal-material-group Structure
/// </summary>
public struct decal_material_group
{
    public decal_material_group(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ocean-struct Structure
/// </summary>
public struct ocean_struct
{
    public ocean_struct(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) subdifficulty-definition Structure
/// </summary>
public struct subdifficulty_definition
{
    public subdifficulty_definition(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) custom-attach-point-spec-array Structure
/// </summary>
public struct custom_attach_point_spec_array
{
    public custom_attach_point_spec_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rad-info Structure
/// </summary>
public struct rad_info
{
    public rad_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) look2-tint-palette Structure
/// </summary>
public struct look2_tint_palette
{
    public look2_tint_palette(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) tip-collection Structure
/// </summary>
public struct tip_collection
{
    public tip_collection(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-attack-behavior Structure
/// </summary>
public struct melee_attack_behavior
{
    public melee_attack_behavior(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ragdoll-control-settings Structure
/// </summary>
public struct ragdoll_control_settings
{
    public ragdoll_control_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) move-performance-table Structure
/// </summary>
public struct move_performance_table
{
    public move_performance_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) snow-generation-table Structure
/// </summary>
public struct snow_generation_table
{
    public snow_generation_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) loadout-set-difficulty-settings Structure
/// </summary>
public struct loadout_set_difficulty_settings
{
    public loadout_set_difficulty_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) corner-check-anims Structure
/// </summary>
public struct corner_check_anims
{
    public corner_check_anims(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) stick-camera-settings Structure
/// </summary>
public struct stick_camera_settings
{
    public stick_camera_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) int32 Structure
/// </summary>
public struct int32
{
    public int32(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ap-entry-item-list Structure
/// </summary>
public struct ap_entry_item_list
{
    public ap_entry_item_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ap-exit-anim-list Structure
/// </summary>
public struct ap_exit_anim_list
{
    public ap_exit_anim_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher-setlist-info Structure
/// </summary>
public struct splasher_setlist_info
{
    public splasher_setlist_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher-skeleton Structure
/// </summary>
public struct splasher_skeleton
{
    public splasher_skeleton(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) particle-spawn-table Structure
/// </summary>
public struct particle_spawn_table
{
    public particle_spawn_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) throwable-bullet-settings Structure
/// </summary>
public struct throwable_bullet_settings
{
    public throwable_bullet_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) fx-npc-burn-pattern-info-list Structure
/// </summary>
public struct fx_npc_burn_pattern_info_list
{
    public fx_npc_burn_pattern_info_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hit-partial-array Structure
/// </summary>
public struct hit_partial_array
{
    public hit_partial_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) entry-set Structure
/// </summary>
public struct entry_set
{
    public entry_set(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flocking-vars Structure
/// </summary>
public struct flocking_vars
{
    public flocking_vars(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-strafe-set Structure
/// </summary>
public struct player_strafe_set
{
    public player_strafe_set(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vox-action-def Structure
/// </summary>
public struct vox_action_def
{
    public vox_action_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ocean-waves-parameters Structure
/// </summary>
public struct ocean_waves_parameters
{
    public ocean_waves_parameters(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) reminder Structure
/// </summary>
public struct reminder
{
    public reminder(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vox-remap-module-def Structure
/// </summary>
public struct vox_remap_module_def
{
    public vox_remap_module_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher-effect-sound Structure
/// </summary>
public struct splasher_effect_sound
{
    public splasher_effect_sound(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ik-constraint-info Structure
/// </summary>
public struct ik_constraint_info
{
    public ik_constraint_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flashlight-info Structure
/// </summary>
public struct flashlight_info
{
    public flashlight_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) item-drop-class-table Structure
/// </summary>
public struct item_drop_class_table
{
    public item_drop_class_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) breath-anim-table Structure
/// </summary>
public struct breath_anim_table
{
    public breath_anim_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-infected-params Structure
/// </summary>
public struct ai_infected_params
{
    public ai_infected_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-melee-settings Structure
/// </summary>
public struct camera_melee_settings
{
    public camera_melee_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-dof-settings Structure
/// </summary>
public struct camera_dof_settings
{
    public camera_dof_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) spline-network-spline-array Structure
/// </summary>
public struct spline_network_spline_array
{
    public spline_network_spline_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) listen-mode-designer-settings Structure
/// </summary>
public struct listen_mode_designer_settings
{
    public listen_mode_designer_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher Structure
/// </summary>
public struct splasher
{
    public splasher(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) item-drop-weapon-parts-info Structure
/// </summary>
public struct item_drop_weapon_parts_info
{
    public item_drop_weapon_parts_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-aim-motion-match-settings Structure
/// </summary>
public struct player_aim_motion_match_settings
{
    public player_aim_motion_match_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-melee-settings Structure
/// </summary>
public struct ai_melee_settings
{
    public ai_melee_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-strafe-settings Structure
/// </summary>
public struct melee_strafe_settings
{
    public melee_strafe_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) threat-indicator-data Structure
/// </summary>
public struct threat_indicator_data
{
    public threat_indicator_data(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) joypad-scheme Structure
/// </summary>
public struct joypad_scheme
{
    public joypad_scheme(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) driveable-boat-settings Structure
/// </summary>
public struct driveable_boat_settings
{
    public driveable_boat_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-focus-settings Structure
/// </summary>
public struct camera_focus_settings
{
    public camera_focus_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hash-table Structure
/// </summary>
public struct hash_table
{
    public hash_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) push-object-def Structure
/// </summary>
public struct push_object_def
{
    public push_object_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-jump-settings Structure
/// </summary>
public struct horse_jump_settings
{
    public horse_jump_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) game-character-collision-settings Structure
/// </summary>
public struct game_character_collision_settings
{
    public game_character_collision_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) anim-overlay-layer-priorities Structure
/// </summary>
public struct anim_overlay_layer_priorities
{
    public anim_overlay_layer_priorities(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-spot-fix-entry-array Structure
/// </summary>
public struct player_spot_fix_entry_array
{
    public player_spot_fix_entry_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) difficulty-detail-stealth Structure
/// </summary>
public struct difficulty_detail_stealth
{
    public difficulty_detail_stealth(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) difficulty-detail-enemies Structure
/// </summary>
public struct difficulty_detail_enemies
{
    public difficulty_detail_enemies(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) difficulty-detail-resources Structure
/// </summary>
public struct difficulty_detail_resources
{
    public difficulty_detail_resources(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) grenade-magnet-settings Structure
/// </summary>
public struct grenade_magnet_settings
{
    public grenade_magnet_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) difficulty-detail-player Structure
/// </summary>
public struct difficulty_detail_player
{
    public difficulty_detail_player(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) carry-object-settings Structure
/// </summary>
public struct carry_object_settings
{
    public carry_object_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) stick-pan-camera-settings Structure
/// </summary>
public struct stick_pan_camera_settings
{
    public stick_pan_camera_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) difficulty-detail-allies Structure
/// </summary>
public struct difficulty_detail_allies
{
    public difficulty_detail_allies(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) performance-entry-list Structure
/// </summary>
public struct performance_entry_list
{
    public performance_entry_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) gore-mask-set Structure
/// </summary>
public struct gore_mask_set
{
    public gore_mask_set(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) tap-anim-sequence Structure
/// </summary>
public struct tap_anim_sequence
{
    public tap_anim_sequence(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) treasure-timeline Structure
/// </summary>
public struct treasure_timeline
{
    public treasure_timeline(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) gui2-attachable-params Structure
/// </summary>
public struct gui2_attachable_params
{
    public gui2_attachable_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) difficulty-definition Structure
/// </summary>
public struct difficulty_definition
{
    public difficulty_definition(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) enum-names Structure
/// </summary>
public struct enum_names
{
    public enum_names(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-vault-settings Structure
/// </summary>
public struct horse_vault_settings
{
    public horse_vault_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vehicle-health-settings Structure
/// </summary>
public struct vehicle_health_settings
{
    public vehicle_health_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) swim-settings Structure
/// </summary>
public struct swim_settings
{
    public swim_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-move-settings Structure
/// </summary>
public struct horse_move_settings
{
    public horse_move_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) identifier-array Structure
/// </summary>
public struct identifier_array
{
    public identifier_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) blend-overlay-map Structure
/// </summary>
public struct blend_overlay_map
{
    public blend_overlay_map(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-performance-table Structure
/// </summary>
public struct ai_performance_table
{
    public ai_performance_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-motion-type-settings Structure
/// </summary>
public struct ai_motion_type_settings
{
    public ai_motion_type_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flocking-bird-vars Structure
/// </summary>
public struct flocking_bird_vars
{
    public flocking_bird_vars(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) beach-fight-difficulty-settings Structure
/// </summary>
public struct beach_fight_difficulty_settings
{
    public beach_fight_difficulty_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-balance-settings Structure
/// </summary>
public struct player_balance_settings
{
    public player_balance_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) heart-rate-state-list Structure
/// </summary>
public struct heart_rate_state_list
{
    public heart_rate_state_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) blink-settings Structure
/// </summary>
public struct blink_settings
{
    public blink_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) water-displacement-bridge-params Structure
/// </summary>
public struct water_displacement_bridge_params
{
    public water_displacement_bridge_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ground-hug-info Structure
/// </summary>
public struct ground_hug_info
{
    public ground_hug_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) animal-idle-anim-list Structure
/// </summary>
public struct animal_idle_anim_list
{
    public animal_idle_anim_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-traversal-skills Structure
/// </summary>
public struct ai_traversal_skills
{
    public ai_traversal_skills(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) corner-check-anim-collection Structure
/// </summary>
public struct corner_check_anim_collection
{
    public corner_check_anim_collection(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splash-data Structure
/// </summary>
public struct splash_data
{
    public splash_data(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) carry-data Structure
/// </summary>
public struct carry_data
{
    public carry_data(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) workbench-weapon-def Structure
/// </summary>
public struct workbench_weapon_def
{
    public workbench_weapon_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rumble-recorder-mapping-array Structure
/// </summary>
public struct rumble_recorder_mapping_array
{
    public rumble_recorder_mapping_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) gas-mask-setup Structure
/// </summary>
public struct gas_mask_setup
{
    public gas_mask_setup(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) prompt-stack-tutorial Structure
/// </summary>
public struct prompt_stack_tutorial
{
    public prompt_stack_tutorial(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) buoyancy-object-def Structure
/// </summary>
public struct buoyancy_object_def
{
    public buoyancy_object_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) water-forces-settings Structure
/// </summary>
public struct water_forces_settings
{
    public water_forces_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-vehicle-settings Structure
/// </summary>
public struct camera_vehicle_settings
{
    public camera_vehicle_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-sniper-params Structure
/// </summary>
public struct ai_sniper_params
{
    public ai_sniper_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) mp-emote-wheel Structure
/// </summary>
public struct mp_emote_wheel
{
    public mp_emote_wheel(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-weapon-particle Structure
/// </summary>
public struct melee_weapon_particle
{
    public melee_weapon_particle(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) sand-splash-data Structure
/// </summary>
public struct sand_splash_data
{
    public sand_splash_data(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hit-reaction-match-rating Structure
/// </summary>
public struct hit_reaction_match_rating
{
    public hit_reaction_match_rating(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) menu2-sound-array Structure
/// </summary>
public struct menu2_sound_array
{
    public menu2_sound_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dynamic-light Structure
/// </summary>
public struct dynamic_light
{
    public dynamic_light(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) arrow-bullet-settings Structure
/// </summary>
public struct arrow_bullet_settings
{
    public arrow_bullet_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-watch-params Structure
/// </summary>
public struct ai_watch_params
{
    public ai_watch_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-spline-network-settings Structure
/// </summary>
public struct ai_spline_network_settings
{
    public ai_spline_network_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) float-array Structure
/// </summary>
public struct float_array
{
    public float_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) leg-ik-foot-def Structure
/// </summary>
public struct leg_ik_foot_def
{
    public leg_ik_foot_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) baby-ik-info Structure
/// </summary>
public struct baby_ik_info
{
    public baby_ik_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-infected-pustule-cloud-params Structure
/// </summary>
public struct ai_infected_pustule_cloud_params
{
    public ai_infected_pustule_cloud_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) item-drop-weapon-parts-anim-pair-array Structure
/// </summary>
public struct item_drop_weapon_parts_anim_pair_array
{
    public item_drop_weapon_parts_anim_pair_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hud2-inv-item-slot-array Structure
/// </summary>
public struct hud2_inv_item_slot_array
{
    public hud2_inv_item_slot_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flame-damage-settings Structure
/// </summary>
public struct flame_damage_settings
{
    public flame_damage_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-archetype-skills Structure
/// </summary>
public struct ai_archetype_skills
{
    public ai_archetype_skills(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) entry-ap-table Structure
/// </summary>
public struct entry_ap_table
{
    public entry_ap_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-sprint-settings Structure
/// </summary>
public struct player_sprint_settings
{
    public player_sprint_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) swim-breath-settings Structure
/// </summary>
public struct swim_breath_settings
{
    public swim_breath_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) npc-awareness-options Structure
/// </summary>
public struct npc_awareness_options
{
    public npc_awareness_options(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) attachable-light-info Structure
/// </summary>
public struct attachable_light_info
{
    public attachable_light_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rail-vehicle-class Structure
/// </summary>
public struct rail_vehicle_class
{
    public rail_vehicle_class(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) bullet-hit-particle-spawn-set Structure
/// </summary>
public struct bullet_hit_particle_spawn_set
{
    public bullet_hit_particle_spawn_set(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) fact-reset-array Structure
/// </summary>
public struct fact_reset_array
{
    public fact_reset_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) radio-info Structure
/// </summary>
public struct radio_info
{
    public radio_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-transition-set Structure
/// </summary>
public struct player_transition_set
{
    public player_transition_set(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) carry-ik-info Structure
/// </summary>
public struct carry_ik_info
{
    public carry_ik_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) fact-map Structure
/// </summary>
public struct fact_map
{
    public fact_map(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) anim-actor Structure
/// </summary>
public struct anim_actor
{
    public anim_actor(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-hit-reaction-entry-list Structure
/// </summary>
public struct player_hit_reaction_entry_list
{
    public player_hit_reaction_entry_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-petting-info-set Structure
/// </summary>
public struct horse_petting_info_set
{
    public horse_petting_info_set(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-cover-params Structure
/// </summary>
public struct ai_cover_params
{
    public ai_cover_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-melee-strafe-params Structure
/// </summary>
public struct ai_melee_strafe_params
{
    public ai_melee_strafe_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) winch-settings Structure
/// </summary>
public struct winch_settings
{
    public winch_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-arrow-pull-settings Structure
/// </summary>
public struct player_arrow_pull_settings
{
    public player_arrow_pull_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ragdoll-keyframe-control-settings Structure
/// </summary>
public struct ragdoll_keyframe_control_settings
{
    public ragdoll_keyframe_control_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-lookaround-settings Structure
/// </summary>
public struct camera_lookaround_settings
{
    public camera_lookaround_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher-effect-listen-mode-reveal Structure
/// </summary>
public struct splasher_effect_listen_mode_reveal
{
    public splasher_effect_listen_mode_reveal(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) int64 Structure
/// </summary>
public struct int64
{
    public int64(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vector Structure
/// </summary>
public struct vector
{
    public vector(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) futz Structure
/// </summary>
public struct futz
{
    public futz(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) item-pickup-look Structure
/// </summary>
public struct item_pickup_look
{
    public item_pickup_look(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) leg-ik-constants Structure
/// </summary>
public struct leg_ik_constants
{
    public leg_ik_constants(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rail-vehicle-animations Structure
/// </summary>
public struct rail_vehicle_animations
{
    public rail_vehicle_animations(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ld1-delay-effect-info Structure
/// </summary>
public struct ld1_delay_effect_info
{
    public ld1_delay_effect_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-binding Structure
/// </summary>
public struct horse_binding
{
    public horse_binding(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher-effect-water-ripple Structure
/// </summary>
public struct splasher_effect_water_ripple
{
    public splasher_effect_water_ripple(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rumble-curve-def Structure
/// </summary>
public struct rumble_curve_def
{
    public rumble_curve_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) array Structure
/// </summary>
public struct array
{
    public array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) firearm-damage-movement-def Structure
/// </summary>
public struct firearm_damage_movement_def
{
    public firearm_damage_movement_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher-array Structure
/// </summary>
public struct splasher_array
{
    public splasher_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) jump-arc-settings Structure
/// </summary>
public struct jump_arc_settings
{
    public jump_arc_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-settings Structure
/// </summary>
public struct melee_settings
{
    public melee_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) surface-to-friction-array Structure
/// </summary>
public struct surface_to_friction_array
{
    public surface_to_friction_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) character-prone-settings Structure
/// </summary>
public struct character_prone_settings
{
    public character_prone_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-damages Structure
/// </summary>
public struct melee_damages
{
    public melee_damages(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) string-array Structure
/// </summary>
public struct string_array
{
    public string_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rumble-curve-list Structure
/// </summary>
public struct rumble_curve_list
{
    public rumble_curve_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rail-vehicle-spawn-info Structure
/// </summary>
public struct rail_vehicle_spawn_info
{
    public rail_vehicle_spawn_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-prone-ledge-aim-settings Structure
/// </summary>
public struct player_prone_ledge_aim_settings
{
    public player_prone_ledge_aim_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flocking-2d-params Structure
/// </summary>
public struct flocking_2d_params
{
    public flocking_2d_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flocking-2d-vision-params Structure
/// </summary>
public struct flocking_2d_vision_params
{
    public flocking_2d_vision_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-upgrade-collection Structure
/// </summary>
public struct player_upgrade_collection
{
    public player_upgrade_collection(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-evade-settings Structure
/// </summary>
public struct player_evade_settings
{
    public player_evade_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) shimmer-settings Structure
/// </summary>
public struct shimmer_settings
{
    public shimmer_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-jump-settings Structure
/// </summary>
public struct player_jump_settings
{
    public player_jump_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) journal-model-params Structure
/// </summary>
public struct journal_model_params
{
    public journal_model_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) swim-underwater-move-settings Structure
/// </summary>
public struct swim_underwater_move_settings
{
    public swim_underwater_move_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ik-node-config Structure
/// </summary>
public struct ik_node_config
{
    public ik_node_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vertigo-settings Structure
/// </summary>
public struct vertigo_settings
{
    public vertigo_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) human-shield-settings Structure
/// </summary>
public struct human_shield_settings
{
    public human_shield_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-surprise-params Structure
/// </summary>
public struct ai_surprise_params
{
    public ai_surprise_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) gore-region-array Structure
/// </summary>
public struct gore_region_array
{
    public gore_region_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-health-settings Structure
/// </summary>
public struct player_health_settings
{
    public player_health_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-jump-angle-ranges Structure
/// </summary>
public struct player_jump_angle_ranges
{
    public player_jump_angle_ranges(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) underwater-swim-to-settings Structure
/// </summary>
public struct underwater_swim_to_settings
{
    public underwater_swim_to_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) simple-npc-anim-params Structure
/// </summary>
public struct simple_npc_anim_params
{
    public simple_npc_anim_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-wall-shimmy-constants Structure
/// </summary>
public struct player_wall_shimmy_constants
{
    public player_wall_shimmy_constants(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hud-damage-indicator Structure
/// </summary>
public struct hud_damage_indicator
{
    public hud_damage_indicator(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) stun-explosion-settings Structure
/// </summary>
public struct stun_explosion_settings
{
    public stun_explosion_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-npc-rider-settings Structure
/// </summary>
public struct horse_npc_rider_settings
{
    public horse_npc_rider_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dc-types-info Structure
/// </summary>
public struct dc_types_info
{
    public dc_types_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) enum-symbols Structure
/// </summary>
public struct enum_symbols
{
    public enum_symbols(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) listen-mode-sounds-settings Structure
/// </summary>
public struct listen_mode_sounds_settings
{
    public listen_mode_sounds_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) game-task-graph Structure
/// </summary>
public struct game_task_graph
{
    public game_task_graph(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) chapter-def-array Structure
/// </summary>
public struct chapter_def_array
{
    public chapter_def_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) mix-list-info Structure
/// </summary>
public struct mix_list_info
{
    public mix_list_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) fatigue-settings Structure
/// </summary>
public struct fatigue_settings
{
    public fatigue_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) bow-charge-curves Structure
/// </summary>
public struct bow_charge_curves
{
    public bow_charge_curves(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vignette-definition Structure
/// </summary>
public struct vignette_definition
{
    public vignette_definition(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dog-sfx-list Structure
/// </summary>
public struct dog_sfx_list
{
    public dog_sfx_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-horse-combat-settings Structure
/// </summary>
public struct ai_horse_combat_settings
{
    public ai_horse_combat_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dog-sfx-config Structure
/// </summary>
public struct dog_sfx_config
{
    public dog_sfx_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-vegetation-react-settings Structure
/// </summary>
public struct player_vegetation_react_settings
{
    public player_vegetation_react_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-search-path-criterion-def-list Structure
/// </summary>
public struct ai_search_path_criterion_def_list
{
    public ai_search_path_criterion_def_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-maintain-target-settings Structure
/// </summary>
public struct camera_maintain_target_settings
{
    public camera_maintain_target_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) particle-dynamic-rt-table Structure
/// </summary>
public struct particle_dynamic_rt_table
{
    public particle_dynamic_rt_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) particle-effect-names Structure
/// </summary>
public struct particle_effect_names
{
    public particle_effect_names(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-featherblend-cancel-settings Structure
/// </summary>
public struct player_featherblend_cancel_settings
{
    public player_featherblend_cancel_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) prone-aim-blends Structure
/// </summary>
public struct prone_aim_blends
{
    public prone_aim_blends(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) blacklisted-particle-array Structure
/// </summary>
public struct blacklisted_particle_array
{
    public blacklisted_particle_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rope-sound-params Structure
/// </summary>
public struct rope_sound_params
{
    public rope_sound_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rope-mesh-info Structure
/// </summary>
public struct rope_mesh_info
{
    public rope_mesh_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-damage-fx-settings Structure
/// </summary>
public struct player_damage_fx_settings
{
    public player_damage_fx_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) fixed-strafe-instance-info Structure
/// </summary>
public struct fixed_strafe_instance_info
{
    public fixed_strafe_instance_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) grenade-arc-params Structure
/// </summary>
public struct grenade_arc_params
{
    public grenade_arc_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) chapter-area-def-array Structure
/// </summary>
public struct chapter_area_def_array
{
    public chapter_area_def_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) point Structure
/// </summary>
public struct point
{
    public point(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) joint-shader-driver-list Structure
/// </summary>
public struct joint_shader_driver_list
{
    public joint_shader_driver_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-put-out-fire-performance-list Structure
/// </summary>
public struct ai_put_out_fire_performance_list
{
    public ai_put_out_fire_performance_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-random-gesture-array Structure
/// </summary>
public struct horse_random_gesture_array
{
    public horse_random_gesture_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-stat-array Structure
/// </summary>
public struct player_stat_array
{
    public player_stat_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) name-set Structure
/// </summary>
public struct name_set
{
    public name_set(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-upgrade-tree Structure
/// </summary>
public struct player_upgrade_tree
{
    public player_upgrade_tree(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) menu2-sound-override-array Structure
/// </summary>
public struct menu2_sound_override_array
{
    public menu2_sound_override_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) divider-test-menu Structure
/// </summary>
public struct divider_test_menu
{
    public divider_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) motion-match-transition-table Structure
/// </summary>
public struct motion_match_transition_table
{
    public motion_match_transition_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) anim-blend-table Structure
/// </summary>
public struct anim_blend_table
{
    public anim_blend_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) joint-diff-weight-array Structure
/// </summary>
public struct joint_diff_weight_array
{
    public joint_diff_weight_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) pose-matching-precache-anims-list Structure
/// </summary>
public struct pose_matching_precache_anims_list
{
    public pose_matching_precache_anims_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) force-field-settings Structure
/// </summary>
public struct force_field_settings
{
    public force_field_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) traversal-action-pack-def Structure
/// </summary>
public struct traversal_action_pack_def
{
    public traversal_action_pack_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) leap-attack-list Structure
/// </summary>
public struct leap_attack_list
{
    public leap_attack_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) torch-offsets Structure
/// </summary>
public struct torch_offsets
{
    public torch_offsets(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) boat-magnet-settings Structure
/// </summary>
public struct boat_magnet_settings
{
    public boat_magnet_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) interest-point-settings Structure
/// </summary>
public struct interest_point_settings
{
    public interest_point_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) heart-rate-gesture-list Structure
/// </summary>
public struct heart_rate_gesture_list
{
    public heart_rate_gesture_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) mount-horse-anim-array Structure
/// </summary>
public struct mount_horse_anim_array
{
    public mount_horse_anim_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-dog-owner-settings Structure
/// </summary>
public struct ai_dog_owner_settings
{
    public ai_dog_owner_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-upgrades-menu Structure
/// </summary>
public struct player_upgrades_menu
{
    public player_upgrades_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) look2-remaps Structure
/// </summary>
public struct look2_remaps
{
    public look2_remaps(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-random-gesture-settings Structure
/// </summary>
public struct horse_random_gesture_settings
{
    public horse_random_gesture_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-barrier-spline-settings Structure
/// </summary>
public struct horse_barrier_spline_settings
{
    public horse_barrier_spline_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-dyno-jump-ranges Structure
/// </summary>
public struct player_dyno_jump_ranges
{
    public player_dyno_jump_ranges(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-shot-limiter-settings Structure
/// </summary>
public struct ai_shot_limiter_settings
{
    public ai_shot_limiter_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) projectile-arc-art-params Structure
/// </summary>
public struct projectile_arc_art_params
{
    public projectile_arc_art_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) moving-dodge-table Structure
/// </summary>
public struct moving_dodge_table
{
    public moving_dodge_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) module-info-array Structure
/// </summary>
public struct module_info_array
{
    public module_info_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-belt-attachment Structure
/// </summary>
public struct player_belt_attachment
{
    public player_belt_attachment(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rumble-output-def Structure
/// </summary>
public struct rumble_output_def
{
    public rumble_output_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-camera-light Structure
/// </summary>
public struct melee_camera_light
{
    public melee_camera_light(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) shotgun-damage-link-list Structure
/// </summary>
public struct shotgun_damage_link_list
{
    public shotgun_damage_link_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-idle-anim-array Structure
/// </summary>
public struct horse_idle_anim_array
{
    public horse_idle_anim_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) weapon-strap-joint-offsets Structure
/// </summary>
public struct weapon_strap_joint_offsets
{
    public weapon_strap_joint_offsets(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) mixer-config-info Structure
/// </summary>
public struct mixer_config_info
{
    public mixer_config_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) shoulder-blood-config Structure
/// </summary>
public struct shoulder_blood_config
{
    public shoulder_blood_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-slide-settings Structure
/// </summary>
public struct player_slide_settings
{
    public player_slide_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rope-vars Structure
/// </summary>
public struct rope_vars
{
    public rope_vars(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) battle-wave Structure
/// </summary>
public struct battle_wave
{
    public battle_wave(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) item-drop-difficulty-settings Structure
/// </summary>
public struct item_drop_difficulty_settings
{
    public item_drop_difficulty_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-visible-post-rect Structure
/// </summary>
public struct ai_visible_post_rect
{
    public ai_visible_post_rect(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) cover-share-preset Structure
/// </summary>
public struct cover_share_preset
{
    public cover_share_preset(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) workbench-player-character-def Structure
/// </summary>
public struct workbench_player_character_def
{
    public workbench_player_character_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) transition-set-bucket Structure
/// </summary>
public struct transition_set_bucket
{
    public transition_set_bucket(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) walk-besides-info Structure
/// </summary>
public struct walk_besides_info
{
    public walk_besides_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) all-weapons-state-info Structure
/// </summary>
public struct all_weapons_state_info
{
    public all_weapons_state_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) boat-enter-settings Structure
/// </summary>
public struct boat_enter_settings
{
    public boat_enter_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rain-motion-curves-def Structure
/// </summary>
public struct rain_motion_curves_def
{
    public rain_motion_curves_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dev-spawn-character-array Structure
/// </summary>
public struct dev_spawn_character_array
{
    public dev_spawn_character_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) attack-info Structure
/// </summary>
public struct attack_info
{
    public attack_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) skel-retarget-table Structure
/// </summary>
public struct skel_retarget_table
{
    public skel_retarget_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) render-settings Structure
/// </summary>
public struct render_settings
{
    public render_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ss-options Structure
/// </summary>
public struct ss_options
{
    public ss_options(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher-defines Structure
/// </summary>
public struct splasher_defines
{
    public splasher_defines(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) render-settings-menu-entries Structure
/// </summary>
public struct render_settings_menu_entries
{
    public render_settings_menu_entries(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) render-setting-level-prefix-exclude-list Structure
/// </summary>
public struct render_setting_level_prefix_exclude_list
{
    public render_setting_level_prefix_exclude_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) splasher-init-info-array Structure
/// </summary>
public struct splasher_init_info_array
{
    public splasher_init_info_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) map-32 Structure
/// </summary>
public struct map_32
{
    public map_32(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) namespace-array Structure
/// </summary>
public struct namespace_array
{
    public namespace_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) anim-state-player-info Structure
/// </summary>
public struct anim_state_player_info
{
    public anim_state_player_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) swim-underwater-hand-ik-settings Structure
/// </summary>
public struct swim_underwater_hand_ik_settings
{
    public swim_underwater_hand_ik_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) aim-sway-reduction Structure
/// </summary>
public struct aim_sway_reduction
{
    public aim_sway_reduction(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hud2-upgraded-icon-toggable-offsets Structure
/// </summary>
public struct hud2_upgraded_icon_toggable_offsets
{
    public hud2_upgraded_icon_toggable_offsets(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) workbench-state-def Structure
/// </summary>
public struct workbench_state_def
{
    public workbench_state_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) stick-drive-settings Structure
/// </summary>
public struct stick_drive_settings
{
    public stick_drive_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) targetable-spot-mask Structure
/// </summary>
public struct targetable_spot_mask
{
    public targetable_spot_mask(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-investigate-params Structure
/// </summary>
public struct ai_investigate_params
{
    public ai_investigate_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-global-info Structure
/// </summary>
public struct ai_global_info
{
    public ai_global_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vox-volume-tension-config Structure
/// </summary>
public struct vox_volume_tension_config
{
    public vox_volume_tension_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-vision-busyness-settings Structure
/// </summary>
public struct ai_vision_busyness_settings
{
    public ai_vision_busyness_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-dialog-look-settings Structure
/// </summary>
public struct ai_dialog_look_settings
{
    public ai_dialog_look_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-buddy-look-aim-settings Structure
/// </summary>
public struct ai_buddy_look_aim_settings
{
    public ai_buddy_look_aim_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) bow-string-offsets Structure
/// </summary>
public struct bow_string_offsets
{
    public bow_string_offsets(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-infected-pustule-cloud-run-through-fx-list Structure
/// </summary>
public struct ai_infected_pustule_cloud_run_through_fx_list
{
    public ai_infected_pustule_cloud_run_through_fx_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-anim-settings Structure
/// </summary>
public struct horse_anim_settings
{
    public horse_anim_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) simple-npc-threat-group Structure
/// </summary>
public struct simple_npc_threat_group
{
    public simple_npc_threat_group(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-steer-params Structure
/// </summary>
public struct melee_steer_params
{
    public melee_steer_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-behavior-condition-timing-list Structure
/// </summary>
public struct melee_behavior_condition_timing_list
{
    public melee_behavior_condition_timing_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) buddy-boost-anim-set Structure
/// </summary>
public struct buddy_boost_anim_set
{
    public buddy_boost_anim_set(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) permadeath-task-killed-by-array Structure
/// </summary>
public struct permadeath_task_killed_by_array
{
    public permadeath_task_killed_by_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-alt-horse-weapon-offset-settings Structure
/// </summary>
public struct player_alt_horse_weapon_offset_settings
{
    public player_alt_horse_weapon_offset_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-player-rider-settings Structure
/// </summary>
public struct horse_player_rider_settings
{
    public horse_player_rider_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-no-horse-gesture-anim-array Structure
/// </summary>
public struct player_no_horse_gesture_anim_array
{
    public player_no_horse_gesture_anim_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-wall-shimmy-dof-settings Structure
/// </summary>
public struct player_wall_shimmy_dof_settings
{
    public player_wall_shimmy_dof_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) anim-trans-graph Structure
/// </summary>
public struct anim_trans_graph
{
    public anim_trans_graph(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) search-params Structure
/// </summary>
public struct search_params
{
    public search_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) weapon-anim-transitions Structure
/// </summary>
public struct weapon_anim_transitions
{
    public weapon_anim_transitions(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) weapon-anim-blends Structure
/// </summary>
public struct weapon_anim_blends
{
    public weapon_anim_blends(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) swim-hand-touch-ik-settings Structure
/// </summary>
public struct swim_hand_touch_ik_settings
{
    public swim_hand_touch_ik_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) locked-actors Structure
/// </summary>
public struct locked_actors
{
    public locked_actors(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) world-levels Structure
/// </summary>
public struct world_levels
{
    public world_levels(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) id-array Structure
/// </summary>
public struct id_array
{
    public id_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) text2-sku-fonts-array Structure
/// </summary>
public struct text2_sku_fonts_array
{
    public text2_sku_fonts_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) text2-font-mapping-array Structure
/// </summary>
public struct text2_font_mapping_array
{
    public text2_font_mapping_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) texture-adjust-table Structure
/// </summary>
public struct texture_adjust_table
{
    public texture_adjust_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) threat-detection-params Structure
/// </summary>
public struct threat_detection_params
{
    public threat_detection_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) torch-dynamics Structure
/// </summary>
public struct torch_dynamics
{
    public torch_dynamics(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) treasure-data-array Structure
/// </summary>
public struct treasure_data_array
{
    public treasure_data_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) trophy-data-array Structure
/// </summary>
public struct trophy_data_array
{
    public trophy_data_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vignette-constraint-array Structure
/// </summary>
public struct vignette_constraint_array
{
    public vignette_constraint_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) faction-name-array Structure
/// </summary>
public struct faction_name_array
{
    public faction_name_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) vox-voice-array Structure
/// </summary>
public struct vox_voice_array
{
    public vox_voice_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) water-displacement-bridge-joints Structure
/// </summary>
public struct water_displacement_bridge_joints
{
    public water_displacement_bridge_joints(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) water-displacement-bridge-array Structure
/// </summary>
public struct water_displacement_bridge_array
{
    public water_displacement_bridge_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) global-water-params Structure
/// </summary>
public struct global_water_params
{
    public global_water_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) water-material-defs Structure
/// </summary>
public struct water_material_defs
{
    public water_material_defs(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ragdoll-splash-array Structure
/// </summary>
public struct ragdoll_splash_array
{
    public ragdoll_splash_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) weapon-exclusion-lists Structure
/// </summary>
public struct weapon_exclusion_lists
{
    public weapon_exclusion_lists(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) weapon-sub-menu-array Structure
/// </summary>
public struct weapon_sub_menu_array
{
    public weapon_sub_menu_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) arrows-art-def Structure
/// </summary>
public struct arrows_art_def
{
    public arrows_art_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) weapon-foley-priority-array Structure
/// </summary>
public struct weapon_foley_priority_array
{
    public weapon_foley_priority_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-weapon-pickup-settings Structure
/// </summary>
public struct player_weapon_pickup_settings
{
    public player_weapon_pickup_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) reticle-offset-def-1080p Structure
/// </summary>
public struct reticle_offset_def_1080p
{
    public reticle_offset_def_1080p(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) swim-speed-curve-settings Structure
/// </summary>
public struct swim_speed_curve_settings
{
    public swim_speed_curve_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rain-snow-material-defs Structure
/// </summary>
public struct rain_snow_material_defs
{
    public rain_snow_material_defs(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-move-mode-transition-list Structure
/// </summary>
public struct player_move_mode_transition_list
{
    public player_move_mode_transition_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rail-vehicle-behavior-follow Structure
/// </summary>
public struct rail_vehicle_behavior_follow
{
    public rail_vehicle_behavior_follow(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flashlight-offset-settings Structure
/// </summary>
public struct flashlight_offset_settings
{
    public flashlight_offset_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) smoke-explosion-settings Structure
/// </summary>
public struct smoke_explosion_settings
{
    public smoke_explosion_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) damage-over-time-settings Structure
/// </summary>
public struct damage_over_time_settings
{
    public damage_over_time_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) sound-impairing-explosion-settings Structure
/// </summary>
public struct sound_impairing_explosion_settings
{
    public sound_impairing_explosion_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) boat-performance-array Structure
/// </summary>
public struct boat_performance_array
{
    public boat_performance_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-sfx-config Structure
/// </summary>
public struct horse_sfx_config
{
    public horse_sfx_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dog-sit-look-at-config Structure
/// </summary>
public struct dog_sit_look_at_config
{
    public dog_sit_look_at_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-sfx-list Structure
/// </summary>
public struct horse_sfx_list
{
    public horse_sfx_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) mburro-test-menu Structure
/// </summary>
public struct mburro_test_menu
{
    public mburro_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) deprecated-prototypes-menu Structure
/// </summary>
public struct deprecated_prototypes_menu
{
    public deprecated_prototypes_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) thirds-grid Structure
/// </summary>
public struct thirds_grid
{
    public thirds_grid(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) jeldred-test-menu Structure
/// </summary>
public struct jeldred_test_menu
{
    public jeldred_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) mzhuravlov-test-menu Structure
/// </summary>
public struct mzhuravlov_test_menu
{
    public mzhuravlov_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) npc-prototypes-menu Structure
/// </summary>
public struct npc_prototypes_menu
{
    public npc_prototypes_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) aef-debug-settings Structure
/// </summary>
public struct aef_debug_settings
{
    public aef_debug_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) facial-expression-test-menu Structure
/// </summary>
public struct facial_expression_test_menu
{
    public facial_expression_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) tbraff-test-menu Structure
/// </summary>
public struct tbraff_test_menu
{
    public tbraff_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) badges-menu Structure
/// </summary>
public struct badges_menu
{
    public badges_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) arrow-pull-out-menu Structure
/// </summary>
public struct arrow_pull_out_menu
{
    public arrow_pull_out_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) audio-symbol-array Structure
/// </summary>
public struct audio_symbol_array
{
    public audio_symbol_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) aeinhorn-saving-kids-test-menu Structure
/// </summary>
public struct aeinhorn_saving_kids_test_menu
{
    public aeinhorn_saving_kids_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) fx-material-defs Structure
/// </summary>
public struct fx_material_defs
{
    public fx_material_defs(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) bonus-movie-array Structure
/// </summary>
public struct bonus_movie_array
{
    public bonus_movie_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-knockdown-settings Structure
/// </summary>
public struct horse_knockdown_settings
{
    public horse_knockdown_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-idle-anim-shared-settings Structure
/// </summary>
public struct horse_idle_anim_shared_settings
{
    public horse_idle_anim_shared_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hit-reaction-recovery-list Structure
/// </summary>
public struct hit_reaction_recovery_list
{
    public hit_reaction_recovery_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) gui2-subtitle-colors Structure
/// </summary>
public struct gui2_subtitle_colors
{
    public gui2_subtitle_colors(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) tutorials-page-list-collection Structure
/// </summary>
public struct tutorials_page_list_collection
{
    public tutorials_page_list_collection(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) sound-glossary-collection Structure
/// </summary>
public struct sound_glossary_collection
{
    public sound_glossary_collection(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) chapters-safe-def-array Structure
/// </summary>
public struct chapters_safe_def_array
{
    public chapters_safe_def_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) menu2-info-pane-images-array Structure
/// </summary>
public struct menu2_info_pane_images_array
{
    public menu2_info_pane_images_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) chapters-workbench-def-array Structure
/// </summary>
public struct chapters_workbench_def_array
{
    public chapters_workbench_def_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) gui2-category-game Structure
/// </summary>
public struct gui2_category_game
{
    public gui2_category_game(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) tutorial-audio-settings Structure
/// </summary>
public struct tutorial_audio_settings
{
    public tutorial_audio_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) threat-indicator-settings Structure
/// </summary>
public struct threat_indicator_settings
{
    public threat_indicator_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hud-photo-mode Structure
/// </summary>
public struct hud_photo_mode
{
    public hud_photo_mode(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) menu2-saved-page-stack Structure
/// </summary>
public struct menu2_saved_page_stack
{
    public menu2_saved_page_stack(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hud-hit-tick-delay Structure
/// </summary>
public struct hud_hit_tick_delay
{
    public hud_hit_tick_delay(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) gui2-accessibility-settings Structure
/// </summary>
public struct gui2_accessibility_settings
{
    public gui2_accessibility_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) text2-embedded-icon-settings Structure
/// </summary>
public struct text2_embedded_icon_settings
{
    public text2_embedded_icon_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) e3-gore-settings Structure
/// </summary>
public struct e3_gore_settings
{
    public e3_gore_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) string Structure
/// </summary>
public struct @string
        {
            public @string (SID Type, long Address, SID Name)
            {
    this.Name = Name;
    this.Address = Address;
    TypeID = Type;
}

public SID TypeID;

public SID Name;
public long Address;
        }
        


        /// <summary>
        /// Initialize a new instance of the (unmapped) gallery-category-array Structure
        /// </summary>
        public struct gallery_category_array
{
    public gallery_category_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) character-materials Structure
/// </summary>
public struct character_materials
{
    public character_materials(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) gui-spawner-menu Structure
/// </summary>
public struct gui_spawner_menu
{
    public gui_spawner_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) cgumacal-test-menu Structure
/// </summary>
public struct cgumacal_test_menu
{
    public cgumacal_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-prototypes-menu Structure
/// </summary>
public struct player_prototypes_menu
{
    public player_prototypes_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) idle-controllers-menu Structure
/// </summary>
public struct idle_controllers_menu
{
    public idle_controllers_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) crafting-item-array Structure
/// </summary>
public struct crafting_item_array
{
    public crafting_item_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) conversation-collectible-array Structure
/// </summary>
public struct conversation_collectible_array
{
    public conversation_collectible_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) cinematic-speakers Structure
/// </summary>
public struct cinematic_speakers
{
    public cinematic_speakers(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) cinematic-debug-info-array Structure
/// </summary>
public struct cinematic_debug_info_array
{
    public cinematic_debug_info_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) cinematic-transition-movie-force-enable-array Structure
/// </summary>
public struct cinematic_transition_movie_force_enable_array
{
    public cinematic_transition_movie_force_enable_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) cinematic-special-info-array Structure
/// </summary>
public struct cinematic_special_info_array
{
    public cinematic_special_info_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) character-fx-struct Structure
/// </summary>
public struct character_fx_struct
{
    public character_fx_struct(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-semi-fixed-settings Structure
/// </summary>
public struct camera_semi_fixed_settings
{
    public camera_semi_fixed_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-zoom-angle-pair-list Structure
/// </summary>
public struct camera_zoom_angle_pair_list
{
    public camera_zoom_angle_pair_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-fixed-look-around-settings Structure
/// </summary>
public struct camera_fixed_look_around_settings
{
    public camera_fixed_look_around_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-R3-settings Structure
/// </summary>
public struct camera_R3_settings
{
    public camera_R3_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-fixed-settings Structure
/// </summary>
public struct camera_fixed_settings
{
    public camera_fixed_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-spline-settings Structure
/// </summary>
public struct camera_spline_settings
{
    public camera_spline_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-player-fade-out-settings Structure
/// </summary>
public struct camera_player_fade_out_settings
{
    public camera_player_fade_out_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-base-settings Structure
/// </summary>
public struct camera_base_settings
{
    public camera_base_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-snorricam-settings Structure
/// </summary>
public struct camera_snorricam_settings
{
    public camera_snorricam_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) camera-blend-settings Structure
/// </summary>
public struct camera_blend_settings
{
    public camera_blend_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) accessibility-prototypes-menu Structure
/// </summary>
public struct accessibility_prototypes_menu
{
    public accessibility_prototypes_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-flashlight-debug-menu Structure
/// </summary>
public struct player_flashlight_debug_menu
{
    public player_flashlight_debug_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ghoechst-test-menu Structure
/// </summary>
public struct ghoechst_test_menu
{
    public ghoechst_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dynamic-checkpoint Structure
/// </summary>
public struct dynamic_checkpoint
{
    public dynamic_checkpoint(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) move-performance-debug-menu Structure
/// </summary>
public struct move_performance_debug_menu
{
    public move_performance_debug_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) afrost-test-menu Structure
/// </summary>
public struct afrost_test_menu
{
    public afrost_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) buddy-prototypes-menu Structure
/// </summary>
public struct buddy_prototypes_menu
{
    public buddy_prototypes_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) joypad-lightbar-debug-menu Structure
/// </summary>
public struct joypad_lightbar_debug_menu
{
    public joypad_lightbar_debug_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) fpark-test-menu Structure
/// </summary>
public struct fpark_test_menu
{
    public fpark_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) cwohlwend-test-menu Structure
/// </summary>
public struct cwohlwend_test_menu
{
    public cwohlwend_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) workbench-debug-menu Structure
/// </summary>
public struct workbench_debug_menu
{
    public workbench_debug_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) controller-menu Structure
/// </summary>
public struct controller_menu
{
    public controller_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) reminders-debug Structure
/// </summary>
public struct reminders_debug
{
    public reminders_debug(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-slide-settings Structure
/// </summary>
public struct horse_slide_settings
{
    public horse_slide_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) holster-debug-menu Structure
/// </summary>
public struct holster_debug_menu
{
    public holster_debug_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) to1-debug-settings Structure
/// </summary>
public struct to1_debug_settings
{
    public to1_debug_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) aeinhorn-save-lev-test-menu Structure
/// </summary>
public struct aeinhorn_save_lev_test_menu
{
    public aeinhorn_save_lev_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dmatts-test-menu Structure
/// </summary>
public struct dmatts_test_menu
{
    public dmatts_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-menu Structure
/// </summary>
public struct horse_menu
{
    public horse_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hud-menu-test Structure
/// </summary>
public struct hud_menu_test
{
    public hud_menu_test(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) e3-menu Structure
/// </summary>
public struct e3_menu
{
    public e3_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) bidris-test-menu Structure
/// </summary>
public struct bidris_test_menu
{
    public bidris_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-move-tester-menu Structure
/// </summary>
public struct melee_move_tester_menu
{
    public melee_move_tester_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) aeinhorn-test-menu Structure
/// </summary>
public struct aeinhorn_test_menu
{
    public aeinhorn_test_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) hud-prototypes-menu Structure
/// </summary>
public struct hud_prototypes_menu
{
    public hud_prototypes_menu(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) rail-vehicle-behavior-explicit Structure
/// </summary>
public struct rail_vehicle_behavior_explicit
{
    public rail_vehicle_behavior_explicit(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) horse-trample-settings Structure
/// </summary>
public struct horse_trample_settings
{
    public horse_trample_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) mix-stem-lod-config Structure
/// </summary>
public struct mix_stem_lod_config
{
    public mix_stem_lod_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-script-aim-settings Structure
/// </summary>
public struct player_script_aim_settings
{
    public player_script_aim_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-zipline-settings Structure
/// </summary>
public struct player_zipline_settings
{
    public player_zipline_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-reload-settings Structure
/// </summary>
public struct player_reload_settings
{
    public player_reload_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-wall-double-jump-blend-settings Structure
/// </summary>
public struct player_wall_double_jump_blend_settings
{
    public player_wall_double_jump_blend_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-rope-short-jump-settings Structure
/// </summary>
public struct player_rope_short_jump_settings
{
    public player_rope_short_jump_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-torch-settings Structure
/// </summary>
public struct player_torch_settings
{
    public player_torch_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) tension-mode-settings Structure
/// </summary>
public struct tension_mode_settings
{
    public tension_mode_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) animal-size-and-anim-mapping Structure
/// </summary>
public struct animal_size_and_anim_mapping
{
    public animal_size_and_anim_mapping(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) animal-behavior-settings Structure
/// </summary>
public struct animal_behavior_settings
{
    public animal_behavior_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-poi-look-settings Structure
/// </summary>
public struct player_poi_look_settings
{
    public player_poi_look_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-script-options Structure
/// </summary>
public struct player_script_options
{
    public player_script_options(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-land-speed-threshold Structure
/// </summary>
public struct player_land_speed_threshold
{
    public player_land_speed_threshold(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-hurt-anim-info Structure
/// </summary>
public struct player_hurt_anim_info
{
    public player_hurt_anim_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-arrow-pull-list Structure
/// </summary>
public struct player_arrow_pull_list
{
    public player_arrow_pull_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-blood-fx-settings Structure
/// </summary>
public struct player_blood_fx_settings
{
    public player_blood_fx_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-combat-fx-settings Structure
/// </summary>
public struct player_combat_fx_settings
{
    public player_combat_fx_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) climb-info Structure
/// </summary>
public struct climb_info
{
    public climb_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) piton-slide-setting Structure
/// </summary>
public struct piton_slide_setting
{
    public piton_slide_setting(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) blood-splatter-config Structure
/// </summary>
public struct blood_splatter_config
{
    public blood_splatter_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-rapids-settings Structure
/// </summary>
public struct player_rapids_settings
{
    public player_rapids_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-tension-settings Structure
/// </summary>
public struct player_tension_settings
{
    public player_tension_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-shimmy-settings Structure
/// </summary>
public struct player_shimmy_settings
{
    public player_shimmy_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-falling-grab-settings Structure
/// </summary>
public struct player_falling_grab_settings
{
    public player_falling_grab_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) push-object-tremble-settings Structure
/// </summary>
public struct push_object_tremble_settings
{
    public push_object_tremble_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) global-death-tip-settings Structure
/// </summary>
public struct global_death_tip_settings
{
    public global_death_tip_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) joint-diff-weight-type-list Structure
/// </summary>
public struct joint_diff_weight_type_list
{
    public joint_diff_weight_type_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) populator-level-array Structure
/// </summary>
public struct populator_level_array
{
    public populator_level_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-edge-hang-settings Structure
/// </summary>
public struct player_edge_hang_settings
{
    public player_edge_hang_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-mm-entry-settings Structure
/// </summary>
public struct player_mm_entry_settings
{
    public player_mm_entry_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-dyno-slip-minigame-settings Structure
/// </summary>
public struct player_dyno_slip_minigame_settings
{
    public player_dyno_slip_minigame_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-plank-rating-settings Structure
/// </summary>
public struct player_plank_rating_settings
{
    public player_plank_rating_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-swing-bar-jump-settings Structure
/// </summary>
public struct player_swing_bar_jump_settings
{
    public player_swing_bar_jump_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-mm-camera-settings Structure
/// </summary>
public struct player_mm_camera_settings
{
    public player_mm_camera_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-bipod-settings Structure
/// </summary>
public struct player_bipod_settings
{
    public player_bipod_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) crowd-bump-settings Structure
/// </summary>
public struct crowd_bump_settings
{
    public crowd_bump_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flash-grenade-settings Structure
/// </summary>
public struct flash_grenade_settings
{
    public flash_grenade_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-water-settings Structure
/// </summary>
public struct player_water_settings
{
    public player_water_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-aim-probe-settings Structure
/// </summary>
public struct player_aim_probe_settings
{
    public player_aim_probe_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-cover-settings Structure
/// </summary>
public struct player_cover_settings
{
    public player_cover_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) contextual-look-settings Structure
/// </summary>
public struct contextual_look_settings
{
    public contextual_look_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) weapon-pickup-indicator-settings Structure
/// </summary>
public struct weapon_pickup_indicator_settings
{
    public weapon_pickup_indicator_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-back-against-wall-settings Structure
/// </summary>
public struct player_back_against_wall_settings
{
    public player_back_against_wall_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-zoom-settings Structure
/// </summary>
public struct player_zoom_settings
{
    public player_zoom_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) projectile-camera-shake-list Structure
/// </summary>
public struct projectile_camera_shake_list
{
    public projectile_camera_shake_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-movement-anim-list Structure
/// </summary>
public struct player_movement_anim_list
{
    public player_movement_anim_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) player-turn-additive-settings Structure
/// </summary>
public struct player_turn_additive_settings
{
    public player_turn_additive_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) mix-stem-obs-probe-lod-config Structure
/// </summary>
public struct mix_stem_obs_probe_lod_config
{
    public mix_stem_obs_probe_lod_config(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) near-miss-nudge-settings Structure
/// </summary>
public struct near_miss_nudge_settings
{
    public near_miss_nudge_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) tank-settings-type Structure
/// </summary>
public struct tank_settings_type
{
    public tank_settings_type(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) swim-drag-settings Structure
/// </summary>
public struct swim_drag_settings
{
    public swim_drag_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) message-array Structure
/// </summary>
public struct message_array
{
    public message_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) pair-float Structure
/// </summary>
public struct pair_float
{
    public pair_float(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-script-settings Structure
/// </summary>
public struct melee_script_settings
{
    public melee_script_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) melee-camera-shake-table Structure
/// </summary>
public struct melee_camera_shake_table
{
    public melee_camera_shake_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) material-swap-def Structure
/// </summary>
public struct material_swap_def
{
    public material_swap_def(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) loading-screen-settings Structure
/// </summary>
public struct loading_screen_settings
{
    public loading_screen_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) listen-mode-factions-designer-settings Structure
/// </summary>
public struct listen_mode_factions_designer_settings
{
    public listen_mode_factions_designer_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) listen-mode-settings Structure
/// </summary>
public struct listen_mode_settings
{
    public listen_mode_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) light-evaluator Structure
/// </summary>
public struct light_evaluator
{
    public light_evaluator(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) sun-flares Structure
/// </summary>
public struct sun_flares
{
    public sun_flares(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) joypad-input-collection Structure
/// </summary>
public struct joypad_input_collection
{
    public joypad_input_collection(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) joypad-persistent-command-list Structure
/// </summary>
public struct joypad_persistent_command_list
{
    public joypad_persistent_command_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) journal-global-params Structure
/// </summary>
public struct journal_global_params
{
    public journal_global_params(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) journal-dirtiness-array Structure
/// </summary>
public struct journal_dirtiness_array
{
    public journal_dirtiness_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) map-chevron-settings Structure
/// </summary>
public struct map_chevron_settings
{
    public map_chevron_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) model-viewer-camera-settings Structure
/// </summary>
public struct model_viewer_camera_settings
{
    public model_viewer_camera_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) movie-info-array Structure
/// </summary>
public struct movie_info_array
{
    public movie_info_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) music-playlist-info Structure
/// </summary>
public struct music_playlist_info
{
    public music_playlist_info(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) music-game-parameter-array Structure
/// </summary>
public struct music_game_parameter_array
{
    public music_game_parameter_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) aiming-settings Structure
/// </summary>
public struct aiming_settings
{
    public aiming_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) phys-fx-listen-mode-settings Structure
/// </summary>
public struct phys_fx_listen_mode_settings
{
    public phys_fx_listen_mode_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) animated-camera-category-list Structure
/// </summary>
public struct animated_camera_category_list
{
    public animated_camera_category_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dog-door-list Structure
/// </summary>
public struct dog_door_list
{
    public dog_door_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) dog-unleash-list Structure
/// </summary>
public struct dog_unleash_list
{
    public dog_unleash_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) animated-camera-settings Structure
/// </summary>
public struct animated_camera_settings
{
    public animated_camera_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) particle-pools Structure
/// </summary>
public struct particle_pools
{
    public particle_pools(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) part-blood-save-location-table Structure
/// </summary>
public struct part_blood_save_location_table
{
    public part_blood_save_location_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) part-sampling-table Structure
/// </summary>
public struct part_sampling_table
{
    public part_sampling_table(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) interact-command-list Structure
/// </summary>
public struct interact_command_list
{
    public interact_command_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) whitelisted-particle-array Structure
/// </summary>
public struct whitelisted_particle_array
{
    public whitelisted_particle_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ocean-objects-deformation Structure
/// </summary>
public struct ocean_objects_deformation
{
    public ocean_objects_deformation(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) npc-weapon-anim-set-list Structure
/// </summary>
public struct npc_weapon_anim_set_list
{
    public npc_weapon_anim_set_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) search-look-aim-cost Structure
/// </summary>
public struct search_look_aim_cost
{
    public search_look_aim_cost(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) npc-search-settings Structure
/// </summary>
public struct npc_search_settings
{
    public npc_search_settings(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) search-look-aim-mode-info-array Structure
/// </summary>
public struct search_look_aim_mode_info_array
{
    public search_look_aim_mode_info_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-search-post-rough-criterion-def-list Structure
/// </summary>
public struct ai_search_post_rough_criterion_def_list
{
    public ai_search_post_rough_criterion_def_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) ai-heal-performance-list Structure
/// </summary>
public struct ai_heal_performance_list
{
    public ai_heal_performance_list(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) flame-joint-info-array Structure
/// </summary>
public struct flame_joint_info_array
{
    public flame_joint_info_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) npc-surface-multiplier-array Structure
/// </summary>
public struct npc_surface_multiplier_array
{
    public npc_surface_multiplier_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) oriented-particles-proto Structure
/// </summary>
public struct oriented_particles_proto
{
    public oriented_particles_proto(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
}



/// <summary>
/// Initialize a new instance of the (unmapped) test-search-gestures-array Structure
/// </summary>
public struct test_search_gestures_array
{
    public test_search_gestures_array(SID Type, long Address, SID Name)
    {
        this.Name = Name;
        this.Address = Address;
        TypeID = Type;
    }

    public SID TypeID;

    public SID Name;
    public long Address;
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

                LongMessage = $"Unmapped Structure [\n\tType: {Type.DecodedID}\n\tName: {Name.DecodedID}\n\tAddress: 0x{Address.ToString("X").PadLeft(8, '0')}\n]";
                ShortMessage = $"Type \"{Type.DecodedID}\" not yet mapped.";
            }

            public SID Name;
            public long Address;

            public readonly string LongMessage;
            public string ShortMessage { get; private set; }
        }
        #endregion
    }
}
