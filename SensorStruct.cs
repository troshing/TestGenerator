

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using System.ComponentModel;



namespace EAKompensator
{
    public struct SensorType
    {
        public ushort ID_Module;            // номер датчика
        public float If;                    // ток датчика
        public float Rf;                    // Сопротивление утечки фидера
    }

    public class RegisterDat : INotifyPropertyChanged
    {
        ushort _address;

        public ushort Reg_Kalibrate = 0x0026;                   // вход в режим Калибровки
        public ushort Reg_StartMeasure = 0x0027;                // запуск фазы измерения 

        public ushort Reg_CancelKalibration = 0x002A;
        public ushort Reg_SaveKalibration = 0x0028;
        public ushort Reg_StopKalibration = 0x0029;
        public ushort Reg_Reboot = 0x0020;

        public ushort Reg_UpdateData = 0x002C;
        public ushort Reg_ReadStruct = 0x002B;                      // Чтение всей структуры из датчика
        public ushort Reg_Ans_Data = 0x0030;                        // Чтение структуры данных из датчика
        public ushort Reg_Ans_FabNum = 0x0035;                      // Чтение фабричного номера (строки) из датчика 
        // 
        public ushort Reg_StartConv = 0x001A;                       // Регистр запуск Измерения [W]
        public ushort Reg_StartCalibr = 0x001B;                       // Регистр запуск Калибровки [W]
        public ushort Reg_StatusKalib = 0x003A;
        public ushort Reg_ReadCalib = 0x002E;                       // Чтение калибровочного сигнала из датчика 
        public ushort REG_System_Data_Write  = 0x0037;              // Регистр записи                
                                                                                    //  номера датчика (uint_16t), 
                                                                                    //  номера генератора (uint8_t)
                                                                                    //  Фабричного номера (строка 9 символов), 

        public ushort Reg_EraseFlash = 0x0042;

        public ushort EmptyReg = 0x0000;
        public byte ReadFunc = 0x03;
        public byte WriteFunc = 0x06;
        public byte WriteAcq = 0x2B;
        public ushort byteCount = 0x0001;

        public ushort Reg_VerSoftware = 0x0036;

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

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
        //**************************************************************
        private uint FabNum { get; set; } = 1;
        private uint SetNum { get; set; } = 10;
        private ushort GenNum { get; set; } = 1;
        private string _FNum;
        private string _SNum;
        private string _GNum;
        public string FabNums
        {
            get => _FNum;
            set
            {
                SetField(ref _FNum, value, "FabNums");
            }
        }

        public string SetNums
        {
            get => _SNum;
            set
            {
                SetField(ref _SNum, value, "SetNums");
            }
        }

        public string GenNums
        {
            get => _GNum;
            set
            {
                SetField(ref _GNum, value, "GenNums");
            }
        }

        //**************************************************************

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class Struct_Sens
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
     

        public struct Sens_struct_all
        {
            public byte ID_Gen;                            // Адрес Генератора = 10
            public byte Oversampling;                      // = 1  коэф. прореживания "ИХ" характеристики 
            public byte Kalibrated;                        // Проверочное значение для Калибровки
            public byte Upr;                               // U присоед
            public byte Status_ID;                         // статус ИД (флаги текущего состояния ИД)

            // public byte pad_buffer_1;                       // Буфер для выравнивания кратно 4 
            // public byte pad_buffer_2;                       // Буфер для выравнивания кратно 4
            // public byte pad_buffer_3;                       // Буфер для выравнивания кратно 4

            public ushort ID_Module;                          // Адрес Датчика                 ***************
            public ushort Null_ADC;                           // Нуль АЦП при Калибровке
            public ushort Zero_ADC;                           // Нуль АЦП при Измерениях
            public ushort Data_DAC;

            public float Kdif;                                  // Коэфф перекоса  = R+ / R-
            public float If;                                    // Ток в исследуемом фидере [mA] ***************
            public float Rf;                                    // Сопротивление фидера [kOm]                                   
            public float R_plus;                                // Сопротивление фидера на + сети
            public float R_minus;                               // Сопротивление фидера на - сети
            public float Rcomm;                                 // Сопротивление R общ [kOm]     *************** полное

            public float Tau_seti;                              // Постоянная времени сети - RC  ***************
            public float I_test;                                // Тестовый ток в сети (2,5 мА или 5 мА)
            public float I_kalibr;                              // Калибровочный ток = 2 мА
            public float R_seti;                                // Общее R сети [kOm]  без учета сопротивлений комплекса
            public float C_seti;                                // Емкость сети [uF]

            public float Up;                                    // U+ сети [V]
            public float Um;                                    // U- сети [V]
                                                             
        }
            private const ushort REC_BUF = 800;
            private const ushort FabNum_LEN = 9;
            private const ushort Name_FID_LEN = 20;

        public byte[] Name_FID;                             // Пока не используется
        public byte[] Num_Fab;                              // Номер датчика заводской ИД (9 байт)                   
        public ushort[] buff_Kalibrovki;                    // Буфер содержит оцифр калибровочный сигнал 
        public short[] buff_ImpulseSignal;                  // Буфер для импульсной характеристики сигнала
        public float[] buffer_Raschet_IH;                   // Буфер для расчетной ИХ

        public Sens_struct_all Sens_dat_all;
        
        public Struct_Sens()                //      конструктор
        {
            Sens_dat_all = new Sens_struct_all();

            Name_FID = new  byte[Name_FID_LEN];
            Num_Fab = new  byte[FabNum_LEN];

            buff_Kalibrovki = new ushort[REC_BUF];
            buff_ImpulseSignal = new short[REC_BUF];
            buffer_Raschet_IH = new float[REC_BUF];
    }
        
    }



}
