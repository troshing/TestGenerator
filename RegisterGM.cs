using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace EAKompensator
{
    public class RegisterGM : INotifyPropertyChanged
    {
        ushort _address;

        public ushort Reg_UP = 0x0002;                      // float 4 bytes U+ [V]
        public ushort Reg_UM = 0x0004;                      // float 4 bytes U- [V];

        public ushort Reg_IP = 0x0010;                      // float 4 bytes Ig+ [mA]
        public ushort Reg_IM = 0x0011;                      // float 4 bytes Ig- [mA]

        public ushort Reg_UP_Codes = 0x002C;                // 2 bytes
        public ushort Reg_UM_Codes = 0x002E;                // 2 bytes 

        public ushort Reg_IP_Codes = 0x0030;
        public ushort Reg_IM_Codes = 0x0032;

        public ushort Reg_RP_Ballast = 0x0034;
        public ushort Reg_RM_Ballast = 0x0035;

        public ushort Reg_Kalibrate = 0x0026;                   // вход в режим Калибровки
        public ushort Reg_StartMeasure = 0x0027;                // запуск фазы измерения 

        public ushort Reg_CancelKalibration = 0x002A;
        public ushort Reg_SaveKalibration = 0x0028;
        public ushort Reg_StopKalibration = 0x0029;
        public ushort Reg_Reboot = 0x0020;

        public ushort Reg_StartImpulse = 0x001A;                    // запуск импульса одиночный режим
        public ushort Reg_StartCycle = 0x007A;                      // запуск циклический режим

        public ushort Reg_UpdateData = 0x002C;
        public ushort Reg_ReadStruct = 0x002B;                      // Чтение структуры из генератора
        public ushort Reg_Ans_Data = 0x0030;                        // Чтение структуры данных из датчика     //(совпадает с Reg_IP_Codes)

        public ushort Reg_StartConv = 0x001A;                       // Регистр запуск Измерения [W]
        public ushort Reg_StatusKalib = 0x003A;
        public ushort Reg_Reset_DACA = 0x003C;
        public ushort Reg_Reset_DACB = 0x003D;

        public ushort Reg_RP_Utechka;
        public ushort Reg_RM_Utechka;

        public ushort Reg_RDPlus = 0x0040;
        public ushort Reg_RDMinus = 0x0041;
        public ushort Reg_EraseFlash = 0x0042;
        public ushort Reg_ITEST_LOW = 0x0044;
        public ushort Reg_ITEST_HIGH = 0x0046;

        public ushort Reg_START_IP = 0x0047;
        public ushort Reg_START_IM = 0x0048;

        public ushort Reg_KOEFF_BM = 0x0052;
        public ushort Reg_KOEFF_KM = 0x0054;
        public ushort Reg_KOEFF_BP = 0x0056;
        public ushort Reg_KOEFF_KP = 0x0058;

        public ushort EmptyReg = 0x0000;
        public byte ReadFunc = 0x03;
        public byte WriteFunc = 0x06;
        public byte WriteAcq = 0x2B;
        public ushort byteCount = 0x0001;

        public ushort Reg_VerSoftware = 0x0036;

        public ushort Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }
        RegisterType _type;
        public RegisterType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }
        string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }        
        string _value;
        
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
         
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

    }
}
