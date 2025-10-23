using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
//using Modbus.Utility;
using System.Threading;
using ConvertObject;
using LiveCharts;
//using LiveCharts.Wpf;


namespace EAKompensator
{
    public delegate void SetBufferHandler(List<int> buffer);
    public delegate void SetBufferFloatHandler(List<float> buffer);

    class Control : INotifyPropertyChanged
    {
        Master _master;
        private ConvObject convObj = new ConvObject();

        public event PropertyChangedEventHandler PropertyChanged;
        public byte FlagCalibr;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

       // public event SetBufferHandler SetBufferHandler;
       //public event SetBufferFloatHandler SetBufferFloatHandler;

        private List<int> BufferU16 { get; set; }
        private List<float> BufferFloat { get; set; }

             
        private RegisterDat Register_Sens = new RegisterDat();
        public Control(Master master)
        {
            _master = master;
            BufferU16 = new List<int>();
            BufferFloat = new List<float>();
        }
        public void ReadDatStruct(string NumDat)
        {
            byte[] RXBuf = new byte[Marshal.SizeOf(_master.sensStruct.Sens_dat_all) + 6 + 2];            //8 - преамбула 2 - сrс
            byte[] TRBuffer = new byte[20];
            byte[] DataBytesWrite = new byte[2];

            if (NumDat == "") NumDat = "65278";             //fefe
            ushort[] tmp = new ushort[] { Convert.ToUInt16(NumDat,10) };
            DataBytesWrite=Converter.ConvertUshortArrayToByteArray(tmp);

            
            
                TRBuffer[0] = DataBytesWrite[0];
                TRBuffer[1] = DataBytesWrite[1];

                TRBuffer[2] = 0x01;                           // адрес генератора 
 
                TRBuffer[3] = Register_Sens.ReadFunc;         // команда - чтение 0x03
 
                tmp[0] = Register_Sens.Reg_ReadStruct;        // регистр - чтение всей структуры датчика 0x002b
                DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);                                              // 
                TRBuffer[4] = DataBytesWrite[0];
                TRBuffer[5] = DataBytesWrite[1];


                var mycrc = ModbusCRC.ModbusCRC16Calc(TRBuffer);
                TRBuffer[6] = mycrc[0];                      // 7
                TRBuffer[7] = mycrc[1];                      // 8

                //PrepareBufferToCommand(adrDevice, addrReg, ref byteSendBuffer, dataBufer);
                _master.driver.serial.Open();
                
                _master.driver.serial.WriteAsync(TRBuffer, 0, TRBuffer.Length);             // Запись 
                Thread.Sleep(200);
                _master.driver.serial.ReadAsync(RXBuf, 0, RXBuf.Length);
                Thread.Sleep(500);
                _master.driver.serial.Close();

//*********************************************************************************************
                int szStruct_Dat = Marshal.SizeOf(_master.sensStruct.Sens_dat_all);
           

                byte[] byteParser = new byte[szStruct_Dat];

                Array.Copy(RXBuf, 8, byteParser, 0, szStruct_Dat);
                _master.sensStruct.Sens_dat_all = convObj.ByteToStruct<Struct_Sens.Sens_struct_all>(byteParser);

                SetNums=Convert.ToString(_master.sensStruct.Sens_dat_all.ID_Module);
                SetGenNums=Convert.ToString(_master.sensStruct.Sens_dat_all.ID_Gen,10);

 

//*********************************************************************************************
        }
        public void ReadDatCalibrIH(string NumDat)
        {
            byte[] DataBytesWrite = new byte[2];
            byte[] TRBuffer = new byte[20];

           ushort[] tmp = new ushort[] { Convert.ToUInt16(NumDat, 10) };
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);

            TRBuffer[0] = DataBytesWrite[0];
            TRBuffer[1] = DataBytesWrite[1];

            TRBuffer[2] = 0x01;                           // адрес генератора 

            TRBuffer[3] = Register_Sens.ReadFunc;         // команда - чтение 0x03

            tmp[0] = Register_Sens.Reg_ReadCalib;        // регистр - чтение калибровочной хар-ки (ИХ) датчика 0x002e
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);                                              // 
            TRBuffer[4] = DataBytesWrite[0];
            TRBuffer[5] = DataBytesWrite[1];
            var mycrc = ModbusCRC.ModbusCRC16Calc(TRBuffer);
            TRBuffer[6] = mycrc[0];                      // 7
            TRBuffer[7] = mycrc[1];                      // 8

            //PrepareBufferToCommand(adrDevice, addrReg, ref byteSendBuffer, dataBufer);
        
            _master.driver.serial.Open();

            _master.driver.serial.WriteAsync(TRBuffer, 0, TRBuffer.Length);             // Запись 

           byte[] RXBuf = new byte[1608];
           byte[] tmpBuf = new byte[1600];

            Thread.Sleep(250);
            _master.driver.serial.ReadAsync(RXBuf, 0, RXBuf.Length);
            Thread.Sleep(600);
            _master.driver.serial.Close();
            Thread.Sleep(200);

            Array.Copy(RXBuf, 0, tmpBuf, 0, 1600 - 2);      //при чтении ИХ не приходящего заголовка!!!!

            int[] int_temp = new int[800];
               int_temp = Converter.ConvertByteArrayTointArray(tmpBuf);
            Values.Clear();
            Values.InsertRange(0, int_temp);
        }
        public void ReadDatFabNum(string NumDat)    //чтение из датчика фабричного номера датчика (строка)
        {
            byte[] RXBuf = new byte[10 + 8 + 2];            //9- длина фабричного имени+1 выравниввание, 8 - преамбула, 2 - сrс
            byte[] TRBuffer = new byte[20];
            byte[] DataBytesWrite = new byte[2];

           
            ushort[] tmp = new ushort[] { Convert.ToUInt16(NumDat, 10) };
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);



            TRBuffer[0] = DataBytesWrite[0];
            TRBuffer[1] = DataBytesWrite[1];

            TRBuffer[2] = 0x01;                           // адрес генератора 

            TRBuffer[3] = Register_Sens.ReadFunc;         // команда - чтение 0x03

            tmp[0] = Register_Sens.Reg_Ans_FabNum;        // регистр - чтение фабричного номера датчика 0x0035
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);                                              // 
            TRBuffer[4] = DataBytesWrite[0];
            TRBuffer[5] = DataBytesWrite[1];


            var mycrc = ModbusCRC.ModbusCRC16Calc(TRBuffer);
            TRBuffer[6] = mycrc[0];                      // 7
            TRBuffer[7] = mycrc[1];                      // 8

            //PrepareBufferToCommand(adrDevice, addrReg, ref byteSendBuffer, dataBufer);
            _master.driver.serial.Open();

            _master.driver.serial.WriteAsync(TRBuffer, 0, TRBuffer.Length);             // Запись 
            
            //отослали команду**************************************
            Thread.Sleep(50);
            _master.driver.serial.ReadAsync(RXBuf, 0, RXBuf.Length);
            _master.driver.serial.Close();

            byte[] tmpBuf = new byte[10];
            Array.Copy(RXBuf, 8, tmpBuf, 0, 9);

            SetFabNums = Converter.ConvertByteArrayToString(tmpBuf);


        }
        public void StartDatCalibrIH(string NumDat)
        {
            byte[] DataBytesWrite = new byte[2];
            byte[] TRBuffer = new byte[20];

            if (NumDat == "") NumDat = "FEFE";
           ushort[] tmp = new ushort[] { Convert.ToUInt16(NumDat, 16) };
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);

            TRBuffer[0] = DataBytesWrite[0];
            TRBuffer[1] = DataBytesWrite[1];

            TRBuffer[2] = 0x01;                           // адрес генератора -- пока не используется

            TRBuffer[3] = Register_Sens.WriteFunc;         // команда - запись 0x06

            tmp[0] = Register_Sens.Reg_StartCalibr;        // регистр - запуск калибровки (ИХ) датчика 0x002e
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);                                              // 
            TRBuffer[4] = DataBytesWrite[0];
            TRBuffer[5] = DataBytesWrite[1];
            var mycrc = ModbusCRC.ModbusCRC16Calc(TRBuffer);
            TRBuffer[6] = mycrc[0];                      // 7
            TRBuffer[7] = mycrc[1];                      // 8

            //PrepareBufferToCommand(adrDevice, addrReg, ref byteSendBuffer, dataBufer);
            _master.driver.serial.Open();

            _master.driver.serial.WriteAsync(TRBuffer, 0, TRBuffer.Length);             // Запись 

            _master.driver.serial.Close();
            Thread.Sleep(1000);
            Thread.Sleep(1000);

            ReadDatCalibrIH(NumDat);        //проверяем, что откалибровали, визуализируем

        }
        //************************************************************
        //string NumDat номер датчика текущий
        //string numdat2 номер датчика новый
        //string numgen номер генератора текущий
        //string fabnum фабричный номер

        public void WriteDatSys(string NumDat, string numdat2, string numgen, string fabnum)
        {
            byte[] DataBytesWrite = new byte[2];
            byte[] TRBuffer = new byte[30];


            ushort[] tmp = new ushort[] { Convert.ToUInt16(NumDat, 10) };
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);

            TRBuffer[0] = DataBytesWrite[0];
            TRBuffer[1] = DataBytesWrite[1];

            TRBuffer[2] = 0x01;                           // адрес генератора -- пока не используется

            TRBuffer[3] = Register_Sens.WriteFunc;         // команда - запись 0x06

            tmp[0] = Register_Sens.REG_System_Data_Write;        // регистр - запись системных параметров номер датчикаб номер генератора, фабричный номер
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);                                              // 
            TRBuffer[4] = DataBytesWrite[0];
            TRBuffer[5] = DataBytesWrite[1];
            //данные
            //новый номер датчика
            if (numdat2 == "") numdat2 = NumDat;    //если сетевой адрес не меняется
                                  
            tmp[0] = Convert.ToUInt16(numdat2, 10);
            DataBytesWrite = Converter.ConvertUshortArrayToByteArray(tmp);
            TRBuffer[6] = DataBytesWrite[0];
            TRBuffer[7] = DataBytesWrite[1];

            SetNumSyss = "";                    //гасим "новый" номер датчика

            //новый номер генератора
            TRBuffer[8] = Convert.ToByte(numgen, 10);
            //фабричный номер
            int i, index =9;
            
            for (i = 0; i < fabnum.Length; i++)
                    {
                         TRBuffer[index+i] = (byte)fabnum.ElementAt(i); 
                    }
            index =index+i;

            var mycrc = ModbusCRC.ModbusCRC16Calc(TRBuffer);
            TRBuffer[index] = mycrc[0];                         // index
            TRBuffer[index+1] = mycrc[1];                       // index+1

  
            _master.driver.serial.Open();

            _master.driver.serial.WriteAsync(TRBuffer, 0, TRBuffer.Length);             // Запись 
            Thread.Sleep(200);

            _master.driver.serial.Close();

           

        }

        //****************************************************************************
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        //private string SetNum { get; set; }
        //private string SetFabNum { get; set; }
        //private string SetGenNum { get; set; }


        public ChartValues<int> Values
        {
            get => _values;
            set
            {
                _values = value;
                OnPropertyChanged();
            }
        }
        public string SetNums
        {
            get => _setnums;
            set
            {
                SetField(ref _setnums, value, "SetNums");
            }
        }
        public string SetNumSyss
        {
            get => _setnumsyss;
            set
            {
                SetField(ref _setnumsyss, value, "SetNumSyss");
            }
        }
        public string SetFabNums
        {
            get => _setfabnums;
            set
            {
                SetField(ref _setfabnums, value, "SetFabNums");
            }
        }
        public string SetGenNums
        {
            get => _setgennums;
            set
            {
                SetField(ref _setgennums, value, "SetGenNums");
            }
        }
        public string SetFidNames
        {
            get => _setfidnames;
            set
            {
                SetField(ref _setfidnames, value, "SetFidNames");
            }
        }

        private ChartValues<int> _values = new ChartValues<int>();
         
        private string _setfidnames;    
        private string _setfabnums;       
        private string _setnums;
        private string _setnumsyss;
        private string _setgennums;
        //*************************************************************************************

    }
}
