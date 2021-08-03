using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAKompensator
{
    public enum RegisterType
    {
        Uint = 2,                   // для Регисторв типа UINT16
        DWord = 3,                  // для Регисторв типа UINT8
        Float = 4,                  // для Регисторв типа Float
        Double = 6,
        Long = 8,
        ShortString = 15,           // для Регисторв типа UINT8[4]
        String = 20                 // для Регисторв типа char[]
    }
}
