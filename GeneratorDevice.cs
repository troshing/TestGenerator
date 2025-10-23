using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAKompensator
{
    [Serializable]
    public class GeneratorDevice
    {
        public ushort ID_Module { get; set; }      // id платы 

        public float Km;                            // коэфф Усиления по входу АЦП U-
        public float Bm;

        public float Kp;                            // коэфф Усиления по входу АЦП U+
        public float Bp;

        public float Rbal_p;                        // R+ Балластн = 28 000 [Om]
        public float Rbal_m;                        // R- Балластн = 28 000 [Om]
        public float Rb;                            // Rобщ балластн 

        public float Rp;                            // R+  [Om]
        public float Rm;                            // R-  [Om]

        public float Cs;                            // Емкость сети
        public float Ts;                            // Т сети 
        public float Rs;                            // Общее R сети 
        public float Ip;                            // I + [A] Тестовый ток при R > 40 kOm
        public float Im;                            // I - [A] Тестовый ток при R > 40 kOm

        public float Ipreal_1mA;                    // Фактич после Калибровки I+
        public float Imreal_1mA;                    // Фактич после Калибровки I-
        public float Ip_izm;                        // Измеренные значения I+
        public float Im_izm;                        // Измеренные значения I-

        public short Ipcode;                       // ток Компенсатора I+ [codes] ЦАП
        public short Imcode;                       // ток Компенсатора I- [codes] ЦАП

        public byte Status;                         // Статус Компенсатора


        public void SetDefaultData()
        {
            Km = 0.06924829157f;
            Bm = 4.5906605922f;

            Kp = 0.0686230248f;
            Bp = 279.693002257f;
           
            Rbal_m = 27960.0f;           //  28 000 [Om]
            Rbal_p = 27942.0f;           //  28 000 [Om]

            Ip = 0.0f;
            Im = 0.0f;
            
            Ipcode = 135;
            Imcode = 135;
            ID_Module = 0xFEFE;
            Rb = Rbal_m * Rbal_p / (Rbal_p + Rbal_m);
            
            Rs = 0.0f;                   //   [Om]
            Cs = 0.0f;                   //   [uF]
            Ts = 0.0000001f;             //   [sec]
            Rm = 0.0f;                   //   [Om]
            Rp = 0.0f;                   //   [Om]
        }
    }
}
