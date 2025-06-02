using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace weapon_data
{
    public struct WeaponGameplayDef
    {
        public WeaponGameplayDef(byte[] binFile, long address)
        {
            AmmoCount = 0;
        }

        int AmmoCount;
    }
}
