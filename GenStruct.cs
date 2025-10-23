using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EAKompensator
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GeneratorType
    {
        public ushort ID_Module;                         // id Генератора

        public float U_o;                                // U0
        public float U_plus;                             // U + [V]
        public float U_minus;                            // U - [V]  

        public float U_ab;                               // U АКБ [V]
        public float U_n;                                // U нейтрали [V]                                          

        public float Rcomm;                              // Сопротивление R общ [kOm]
        public float R_plus;                             // R + [kOm]
        public float R_minus;                            // R - [kOm]
        public float Rseti;                              // Сопротивление всей сети, без балластных резисторов 
        public float C_seti;                             // Емкость сети [uF]
        public float T_seti;                             // 
        public float Tau_seti;                           // Постоянная времени сети - RC [sec]

        public byte T_dischrg;                           // Время разряда Генератора
        public byte T_chrg;                              // Время заряда Генератора
        public byte Total_Chrg;                          // Суммарное время работы Генератора
        public byte Status;                              // Статус Генератора
        public byte Activity;                            // Активность ИД (0-не активен; 1- активен)
    }
}
