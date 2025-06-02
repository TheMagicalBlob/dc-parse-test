using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Policy;


namespace weapon_data
{
    public partial class Main
    {

        /// <summary>
        /// Initialize Lists for the expected structures in a loaded script, or all if they changed the file name more than I could be bothered to account for.
        /// <br/> (I should probably just hash that data at the bottom or some shit, needs checking)
        /// </summary>
        /// <param name="scriptName"></param>
        public void InitializeDcStructListsByScriptName(string scriptName)
        {
            switch (scriptName)
            {
                case var _ when scriptName.ToLower().Replace("_", "-").Contains("weapon-gameplay"):
                    weaponDefinitions = new List<WeaponGameplayDef>();
                    break;

                
                case var _ when scriptName.ToLower().Contains("characters") && !scriptName.ToLower().Replace("_", "-").Contains("collision-settings"):
                    break;



                // Unknown Script; Initialize all
                default:
                    weaponDefinitions = new List<WeaponGameplayDef>();
                    break;
            }
        }


        public List<WeaponGameplayDef> weaponDefinitions;
        public List<WeaponGameplayDef> symbolDefinitions;
    }


    public struct WeaponGameplayDef
    {
        public WeaponGameplayDef(string name, byte[] binFile, long address)
        {
            byte[] GetSubArray(byte[] array, int index, int len = 8)
            {
                var ret = new byte[8];
                Buffer.BlockCopy(array, index, ret, 0, len);

                return ret;
            }


            Address = (int)address;
            Name = name;
            AmmoCount = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)BitConverter.ToInt64(GetSubArray(binFile, (int)address + 0x10), 0) + 0x98), 0);
        }

        public int Address;
        public string Name;

        public int AmmoCount;
    }

    public struct Look2Def
    {
        byte[] GetSubArray(byte[] array, int index, int len = 8)
        {
            var ret = new byte[8];
            Buffer.BlockCopy(array, index, ret, 0, len);

            return ret;
        }

        public Look2Def(string name, byte[] binFile, long address)
        {

        }
    }
}
