using System;
using System.Collections.Generic;


namespace weapon_data
{
    public partial class Main
    {
        public List<WeaponGameplayDef> weaponDefinitions = new List<WeaponGameplayDef>();
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



            Name = name;
            AmmoCount = (int)BitConverter.ToInt64(GetSubArray(binFile, (int)BitConverter.ToInt64(GetSubArray(binFile, (int)address + 0x10), 0) + 0x98), 0);
        }

        public string Name;

        public int AmmoCount;
    }
}
