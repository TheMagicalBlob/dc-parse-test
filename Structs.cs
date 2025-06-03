using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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
            switch (scriptName)
            {
                case var _ when scriptName.ToLower().Replace("_", "-").Contains("weapon-gameplay"):
                    weaponDefinitions = new List<WeaponGameplayDef>();
                    break;

                

                case var _ when scriptName.ToLower().Contains("characters") && !scriptName.ToLower().Replace("_", "-").Contains("collision-settings"):



                // Unknown Script; Initialize all
                default:
                    weaponDefinitions = new List<WeaponGameplayDef>();
                    break;
            }
        }


        public List<WeaponGameplayDef> weaponDefinitions;
        public List<WeaponGameplayDef> symbolDefinitions;
    }



    //==========================\\
    //--|   MAP STRUCTURES   |--\\
    //==========================\\
    #region [MAP STRUCTURES]

    public struct DCFileHeader
    {
        public DCFileHeader(string name, byte[] binFile)
        {
            // Read additional header info
            BinFileLength = BitConverter.ToInt64(binFile, 0x8);
            
            unkInt0 = BitConverter.ToInt32(binFile, 0x10);
            unkInt1 = BitConverter.ToInt64(binFile, 0x18);

            TableLength = BitConverter.ToInt32(binFile, 0x14);

            Structures = new List<DCStructArray>(TableLength);
        }

        
        private int unkInt0;
        private long unkInt1;

        public long BinFileLength;
        public int TableLength;

        public List<DCStructArray> Structures;
    }

    public struct DCStructArray
    {
        public DCStructArray(byte[] binFile, int address)
        {
            Address = address;

            Name = Venat.DecodeSIDHash(Venat.GetSubArray(binFile, address));
            Label = Venat.DecodeSIDHash(Venat.GetSubArray(binFile, address + 8));
            Pointer = (int)BitConverter.ToInt64(Venat.GetSubArray(binFile, address + 16), 0);
        }

        public int Address;
        public string Name;
        public string Label;
        public int Pointer;
    }

    public struct SymbolArrayDef
    {
        public SymbolArrayDef(long arrayPointer, byte[] binFile, long address)
        {
            Symbols = new List<string>();
            Hashes  = new List<long  >();



        }

        public List<string> Symbols;
        public List<long> Hashes;
    }

    public struct WeaponGameplayDef
    {
        public WeaponGameplayDef(string name, byte[] binFile, long address)
        {
            Name = name;
            AmmoCount = 0;
            Address = (int)address;

            // Load firearm-related variables
            FirearmGameplayDefAddress = BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)address + 0x10), 0);
            if (FirearmGameplayDefAddress != 0)
            {
                if (Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)FirearmGameplayDefAddress - 8)) != "firearm-gameplay-def")
                {
                    Venat.PrintNL($"ERROR Parsing Firearm Gameplay Definition at {FirearmGameplayDefAddress:X}; unexpected id. ({Venat.DecodeSIDHash(Venat.GetSubArray(binFile, (int)FirearmGameplayDefAddress - 8)) != "firearm-gameplay-def"})");
                }

                IsFirearm = true;
                AmmoCount = (int)BitConverter.ToInt64(Venat.GetSubArray(binFile, (int)FirearmGameplayDefAddress + 0x98), 0);
            }

            else {
                IsFirearm = false;
            }
            
        }

        public string Name;
        public int Address;
        public bool IsFirearm;

        public long FirearmGameplayDefAddress;

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
