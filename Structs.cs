using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        #pragma warning disable IDE1006

        /// <summary>
        /// Details on the initial header array for the provided DC file, as well as an array of any present HeaderItems.
        /// </summary>
        public struct DCScript
        {
            public DCScript(byte[] DCFile, string ScriptName)
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
                if (!DCFile.Take(8).ToArray().SequenceEqual(new byte[] { 0x30, 0x30, 0x43, 0x44, 0x01, 0x00, 0x00, 0x00 }))
                {
                    echo($"ERROR; Invalid File Provided: Invalid file magic.");
                    return;
                }


                //#
                //## Run a few basic integrity checks
                //#
                var integrityCheck = SID.Parse(GetSubArray(DCFile, 0x20));
                if (integrityCheck.RawID != KnownSIDs.array)
                {
                    echo($"ERROR; Unexpected SID \"{integrityCheck.RawID:X}\" at 0x20, aborting.");
                    return;
                }
                if ((unkInt0 = BitConverter.ToInt32(DCFile, 0x10)) != 1)
                {
                    echo($"ERROR; Unexpected Value \"{unkInt0}\" read at 0x10, aborting.");
                    return;
                }
                if ((HeaderTableStartPointer = BitConverter.ToInt64(DCFile, headerTableStartPointerAddr)) != 0x28)
                {
                    echo($"ERROR; Unexpected Value \"{HeaderTableStartPointer}\" read at {headerTableStartPointerAddr}, aborting.");
                    return;
                }



                //#
                //## Read remaining header info
                //#
                BinFileLength = BitConverter.ToInt64(DCFile, 0x8);
                TableLength = BitConverter.ToInt32(DCFile, 0x14);

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
                    Entries[tableIndex] = new DCEntry(DCFile, addr);
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


            /// <summary>
            /// An array of the DCFileHeader.HeaderItems parsed from the provided DC file.
            /// </summary>
            public DCEntry[] Entries;


            /// <summary>
            /// An individual item (module?) from the array at the beginning of the DC file.
            /// </summary>
            public struct DCEntry
            {
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

        private struct map_entry
        {

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
            public const int Size = 0x90; // The size of the structure;

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
				UnknownLong_At0x28 = 0;






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
					0x28  // unknownLong_At0x28 (Unknown long)
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

			/// <summary> Unknown long <summary/>
			public long UnknownLong_At0x28 { get; set; }
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
            }

            public SID Name;
            public long Address;
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
            }

            public SID Name;
            public long Address;
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
            }

            public SID Name;
            public long Address;
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
            }

            public SID Name;
            public long Address;
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
            }

            public SID Name;
            public long Address;
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






                //#
                //## Offset Initializations
                //#
                Offsets = new[]
                {
                    0x00, // encodedNameID (The encoded character symbol/name id)
					0x08, // decodedName_Ptr (The decoded character symbol/name string*)
					0x10, // decodedSkeletonName_Ptr (The decoded skeleton name string*)
					0x18, // encodedSkeletonNameID (The encoded skeleton name id)
				
					0x20, // decodedMovers1Name_Ptr (Haven't looked in to this one //!)
					0x28, // encodedMovers1NameID (Haven't looked in to this one //!)
					0x30, // decodedMovers2Name_Ptr (Haven't looked in to this one //!)
					0x38, // encodedMovers2NameID (Haven't looked in to this one //!)
				
					0x90, // headName_Ptr (The decoded head part name string*)
					0x98, // unkHeadPartInt1 (Unknown head part int 1)
					0x9C, // unkHeadPartInt2 (Unknown head part int 2)
				
					0xA0, // hairClothName_Ptr (The decoded hair cloth name string*)
					0xA8, // unkHairClothInt1 (Unknown hair cloth int 1)
					0xA8, // unkSkelNameInt1 (Unknown skel name int 1)
					0xAC, // unkHairClothInt2 (Unknown hair cloth int 2)
				
					0xAC, // unkSkelNameInt2 (Unknown skel name int 2)
					0xB0, // unkSkelName_Ptr (The decoded head part name string*)
					0xD0, // nextCharacterLook2_0_Ptr (What? Why?)
					0xE0, // nextCharacterLook2_1_Ptr (And why two of 'em??)
				
					0x100, // ambientOccludersID (Unknown ulong)
					0x150, // modelScale_1 (The scale of the character model)
					0x154, // modelScale_2 (The scale of the character model (but slightly different?))
					0x160, // isPlayerFlag (A flag for whether the character is a player character (I think))
					0x1A8  // vec4_Ptr (vec4* (vector?))
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
            public const int Size =  0x3B0; // The size of the structure;

            /// <summary> The raw binary data of the current StructureTemplate instance. </summary>
            public byte[] RawData;

            
            //# #| Public Properties |#
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
