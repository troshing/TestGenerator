using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Runtime.InteropServices;
using ConvertObject;
using RJCP.IO.Ports;
using Parity = RJCP.IO.Ports.Parity;
using StopBits = RJCP.IO.Ports.StopBits;


namespace EAKompensator
{
    public class Master : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        Task pollTask;
        Task cyclicTask;
        public bool IsPoll { get; private set; }
        public bool isCyclicPoll { get; private set; }
        public byte[] DataBytesRead = new byte[12];
        public byte[] DataBytesWrite = new byte[2];

        private ConvObject convObj = new ConvObject();
        private GeneratorType genStruct = new GeneratorType();

        private SensorType sensor_type = new SensorType();

        public RegisterGM register = new RegisterGM();          //регистры генератора
        public RegisterDat registerDat = new RegisterDat();     //регистры датчика
        public Struct_Sens sensStruct = new Struct_Sens();      //структуры датчика


        //private RegisterGM mregsGm = new RegisterGM();
        private int stdStruct = 10;
        private const ushort def_UIID = 0xFE00;                 // Адрес Генератора по умолчанию
        public ushort AddrSensor { get; set; }

        private float Upf { get; set; }
        private float Umf { get; set; }

        private float Rpf { get; set; }
        private float Rmf { get; set; }
        private float Tauf { get; set; }
        private float Csf { get; set; }
        private float Rf { get; set; }
        private ushort Ndat { get; set; }

        public string Ups 
        { get => _ups;
            set
            {
                SetField(ref _ups, value, "Ups");
            }
        }

        public string Ums
        {
            get => _ums;
          
            set => SetField(ref _ums, value, "Ums");
        }

        public string Rps
        {
            get => _rp;
            set
            {
                SetField(ref _rp, value, "Rps");
            }
        }

        public string Rms
        {
            get => _rm;
            set
            {
                SetField(ref _rm, value, "Rms");
            }
        }

        public string Taus
        {
            get => _tau;
            set
            {
                SetField(ref _tau, value, "Taus");
            }
        }

        public string Cs
        {
            get => _cs;
            set
            {
                SetField(ref _cs, value, "Cs");
            }
        }

        public string Rfs
        {
            get => _rf;
            set
            {
                SetField(ref _rf, value, "Rfs");
            }
        }

        public string Ndats
        {
            get => _ndat;
            set
            {
                SetField(ref _ndat, value, "Ndats");
            }
        }

        private string _ndat;
        private string _ups;
        private string _ums;
        private string _rp;
        private string _rm;
        private string _tau;
        private string _cs;
        private string _rf;

        SerialPortStream ModbusMaster { get; set; }

        public SerialPortStream _SerialPort { get; set; }
        public GmSerialDriver driver = new GmSerialDriver();


        


        public ushort SlaveAddress { get; set; }
        private ushort CommAdrSens = 0xFEFE; 
        public short dwordAnswer;
        public float floatAnswer;

        private string _info;
        public string Info
        {
            get => _info;
            set
            {
                SetField(ref _info, value, "Info");
            }
        }

        public Master()
        {
            _SerialPort = new SerialPortStream(GetPortName(), 115200, 8, Parity.None,  StopBits.One);
            _SerialPort.ReadTimeout = 500;
            SlaveAddress = 0xFEFE;                        // по умолчанию
            IsPoll = false;
            isCyclicPoll = false;
            driver.SetSerialPort(_SerialPort);
            UpdateInfo(SlaveAddress);
        }

        public string GetPortName()
        {
            string[] portNames = SerialPortStream.GetPortNames(); // SerialPort.GetPortNames();
            string portName = " ";
            if (portNames.Length != 0)
                portName = portNames[0];
            
            return portName;
        }

        public bool ArePortsExist()
        {
            string[] portNames = SerialPortStream.GetPortNames();
            if (portNames.Length != 0) return true;
            else return false;
        }

        public void UpdateInfo(ushort adrDevice)
        {
            float verSoft = 0.0f;     
            ReadRegisters(adrDevice, register.Reg_VerSoftware);                // запрос [0x0036] Ver Software
            ParseAnswer(RegisterType.Float);
            verSoft = floatAnswer;

                Info = $" Адрес устройства : {SlaveAddress}       Порт: {_SerialPort.PortName}    Скорость: {_SerialPort.BaudRate}       Данные: {_SerialPort.DataBits}       " +
                $"Четность: {_SerialPort.Parity}     Стопбиты: {_SerialPort.StopBits}     Версия ПО:{verSoft}";
        }

        public void WriteRegisters(ushort adrDevice,ushort startAddress, ushort[] data)
        {
            try
            {
                DataBytesWrite = Converter.ConvertUshortArrayToByteArray(data);
                driver.WriteRegister(adrDevice,startAddress, ref DataBytesWrite);
            }
            catch (Exception e)
            {
                // _SerialPort.Close();
               //  driver.DisplayiDialog(e.Message);
                //throw;
            }
        }



        public void WriteMultipleRegister(ushort adrDevice, ushort startAddress,ushort[] data)
        {
            try
            {
                DataBytesWrite = Converter.ConvertUshortArrayToByteArray(data);
                driver.WriteAcqCommand(adrDevice, startAddress, ref DataBytesWrite);
            }
            catch (Exception e)
            {
                // _SerialPort.Close();
                //  driver.DisplayiDialog(e.Message);
                //throw;
            }
        }

        public void WriteRegisters(RegisterBase registerBase)
        {
            try
            {
                _SerialPort.Open();
                switch (registerBase)
                {
                    case IValue<float> r:
                        //ModbusMaster.WriteMultipleRegisters(SlaveAddress, registerBase.Address, Converter.ConvertFloatToTwoUint16(r.Value));
                        break;
                    case IValue<string> r:
                        string defaultString = new string(' ', 20);
                        byte[] stringBytes = Encoding.Default.GetBytes(r.Value);
                        stringBytes.CopyTo(stringBytes, 0);
                        //ModbusMaster.WriteMultipleRegisters(SlaveAddress, registerBase.Address, Converter.ConvertByteArrayToUshortArray(stringBytes));
                        break;
                    case IValue<ushort> r:
                       // ModbusMaster.WriteMultipleRegisters(SlaveAddress, registerBase.Address, new ushort[] { r.Value });
                        break;
                }

            }
            catch (Exception e)
            {
                // driver.DisplayiDialog(e.Message);
                // throw;
            }
            finally
            {
                _SerialPort.Close();
            }

        }

        public void ReadRegisters(ushort adrDevice, ushort startAddress)
        {
            try
            {

                driver.ReadRegister(adrDevice, startAddress, ref DataBytesRead);
            }
            catch (Exception e)
            {

            }
        }

        public void ReadStruct(ushort adrDevice,ushort startAddress)
        {
            byte[] byteReadBuffer = new byte[20];
            byte[] byteParser = new byte[20];
            int szStruct = 0;
            try
            {
                if (startAddress == register.Reg_ReadStruct)
                {
                    szStruct = Marshal.SizeOf(genStruct);
                    byteReadBuffer = new byte[szStruct+ stdStruct];
                    byteParser = new byte[szStruct];
                }
                else if (startAddress == register.Reg_Ans_Data)
                {
                    szStruct = Marshal.SizeOf(sensStruct);
                    byteReadBuffer = new byte[szStruct+ stdStruct];
                    byteParser = new byte[szStruct];
                }

                driver.ReadStructRegs (adrDevice,startAddress, ref byteReadBuffer);

                if (startAddress == register.Reg_ReadStruct)
                {
                    Array.Copy(byteReadBuffer, 7, byteParser, 0, szStruct);
                    genStruct = convObj.ByteToStruct<GeneratorType>(byteParser);
                }
                else if (startAddress == register.Reg_Ans_Data)
                {
                    Array.Copy(byteReadBuffer, 8, byteParser, 0, szStruct);
                    sensor_type = convObj.ByteToStruct<SensorType>(byteParser);
                }

            }
            catch (Exception e)
            {

            }
        }

        public void GetStructFromGen(ref GeneratorType genType)
        {
            genType = genStruct;  
        }

        public void GetStructFromSensor(ref SensorType sStruct)
        {
            sStruct = sensor_type;
        }

        public void ReadRegisters(RegisterBase registerBase)
        {
            try
            {
                _SerialPort.Open();
                // ushort[] response = ModbusMaster.ReadHoldingRegisters(SlaveAddress, registerBase.Address, registerBase.Length);
                // EAChargeMonitor.ParseResponse(registerBase, response);
            }
            catch (Exception e)
            {
                // driver.DisplayiDialog(e.Message);
                //throw;
            }
            finally
            {
                _SerialPort.Close();
            }
        }

        public void ParseAnswer(RegisterType typeRegs)
        {
            ushort[] uArray = new ushort[2];
            switch (typeRegs)
            {
                case RegisterType.DWord:
                {
                    dwordAnswer = 0;
                    dwordAnswer = (short)((DataBytesRead[6] << 8) | DataBytesRead[7]);
                    break;
                }
                case RegisterType.Float:
                {
                    floatAnswer = 0.0f;
                    uArray = Converter.ConvertByteArrayToUshortArray(DataBytesRead, 6, 2);  // buffer, index, length
                    floatAnswer = Converter.ConvertTwoUInt16ToFloat(uArray);
                    break;
                }
            }
        }

        public void Reset()
        {
            // WriteRegisters(SlaveAddress,8003, new ushort[] { 0x7273 });
        }

        public void FactorySetting()
        {
            // WriteRegisters(SlaveAddress,8003, new ushort[] { 0x6661 });
        }

        private void ClearBuffers(ref byte[] byteBuffer)
        {
            for (int j = 0; j < byteBuffer.Length; j++)
            {
                byteBuffer[j] = 0;
            }
        }

        public void DisplayiDialog(string data)
        {
            // TODO сделать аналог MessageBox
            MessageBox.Show(data);
        }

        public void SendStartCommand()
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;
            ClearBuffers(ref DataBytesRead);
            WriteRegisters(SlaveAddress,register.EmptyReg, data);                         // [0x0000]
        }

        public void SendCommand(UInt16 regsGM)
        {
            ushort[] data = new ushort[1];
            data[0] = 0x0001;
            WriteRegisters(SlaveAddress,regsGM,data);
        }
 //***************************************************************************************
        //запись данных сети в датчик
        public void SendDataTransfer(float Rp, float Rm, float Tau)           
        {
            ushort[] datatemp = new ushort[2];
            ushort[] data = new ushort[6];
            datatemp = Converter.ConvertFloatToTwoUint16(Rp);
            datatemp.CopyTo(data,0);

            datatemp = Converter.ConvertFloatToTwoUint16(Rm);
            datatemp.CopyTo(data, 2);
            datatemp = Converter.ConvertFloatToTwoUint16(Tau);
            datatemp.CopyTo(data, 4);
            WriteMultipleRegister(CommAdrSens, registerDat.Reg_UpdateData,data);                   // 0x002C запись данных в датчик
        }

        public void StartPoll(int interval)
        {
            IsPoll = true;
            pollTask = new Task(() => Poll(interval));
            pollTask.Start();
        }

        public void StopPoll()
        {
            IsPoll = false;
        }

        public void StartCyclic(int interval)
        {
            isCyclicPoll = true;
            cyclicTask = new Task(() => CyclicPoll(interval));
            cyclicTask.Start();
        }
        public void StopCyclic()
        {
            isCyclicPoll = false;
        }

        private void Poll(int interval)
        {
            while (IsPoll)
            {
                for (int i = 0; i < 7 && IsPoll; i++)
                {
                    ReadMeasureRegisters();
                    Thread.Sleep(interval);
                }
            }
        }

        private void CyclicPoll(int interval)
        {
            while(isCyclicPoll)
            {
                SendCommand(register.Reg_StartImpulse);              // [0x001A]
                
                Thread.Sleep(interval);                             //  11 sec
                ReadStruct(def_UIID, register.Reg_ReadStruct);
                GetStructFromGen(ref genStruct);
                
                Rpf = genStruct.R_plus;
                Rps = Rpf.ToString("F0");
                
                Rmf = genStruct.R_minus;
                Rms = Rmf.ToString("F0");

                Tauf = genStruct.Tau_seti * 1000.0f;
                Taus = Tauf.ToString("F0");

                Csf = genStruct.C_seti * 1000000.0f;
                Cs = Csf.ToString("F0");

                SendDataTransfer(Rpf, Rmf, genStruct.Tau_seti);     //пересылка данных сети --> в датчик
                Thread.Sleep(1000);

                //************************************************************************************
                ReadStruct(AddrSensor, registerDat.Reg_Ans_Data);     //чтение данных из датчика
                GetStructFromSensor(ref sensor_type);
                //************************************************************************************

                Rf = sensor_type.Rf;
                Rfs = Rf.ToString("F1");

                Ndat = sensor_type.ID_Module;
                _ndat = Ndat.ToString("D3");
                Thread.Sleep(3000);
            }
            
        }

        private void ReadMeasureRegisters()
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;
            
            WriteRegisters(SlaveAddress,register.Reg_Kalibrate, data);                    // [0x0026] Старт измерения

            Thread.Sleep(500);
            ReadRegisters(SlaveAddress,register.Reg_UP);                                  // [0x0002] Регистр U+
            ParseAnswer(RegisterType.Float);
            Upf = floatAnswer;
            Ups = "U+="+Upf.ToString("F2");

            Thread.Sleep(100);
            ReadRegisters(SlaveAddress,register.Reg_UM);                                  // [0x0004] Регистр U-
            ParseAnswer(RegisterType.Float);
            Umf = floatAnswer;
            Ums = "U-="+Umf.ToString("F2");
     
        }

    }
}
