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
        public bool IsPoll { get; private set; }
        public byte[] DataBytesRead = new byte[12];
        public byte[] DataBytesWrite = new byte[2];

        public float Upf { get; set; }
        public float Umf { get; set; }

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
            set
            {
                SetField(ref _ums, value, "Ums");
            }
        }

        private string _ups;
        private string _ums;

        SerialPortStream ModbusMaster { get; set; }

        public SerialPortStream _SerialPort { get; set; }
        private GmSerialDriver driver = new GmSerialDriver();
        private RegisterGM register = new RegisterGM();

        public byte SlaveAddress { get; set; }
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
            SlaveAddress = 0x00FE;                        // по умолчанию
            IsPoll = false;
            driver.SetSerialPort(_SerialPort);
            UpdateInfo();
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

        public void UpdateInfo()
        {
            float verSoft = 0.0f;
            // SendStartCommand();
            ReadRegisters(register.Reg_VerSoftware);                // запрос [0x0036] Ver Software
            ParseAnswer(RegisterType.Float);
            verSoft = floatAnswer;

                Info = $" Адрес устройства : {SlaveAddress}       Порт: {_SerialPort.PortName}    Скорость: {_SerialPort.BaudRate}       Данные: {_SerialPort.DataBits}       " +
                $"Четность: {_SerialPort.Parity}     Стопбиты: {_SerialPort.StopBits}     Версия ПО:{verSoft}";
        }

        public void WriteRegisters(ushort startAddress, ushort[] data)
        {
            try
            {
                DataBytesWrite = Converter.ConvertUshortArrayToByteArray(data);
                driver.WriteRegister(startAddress, ref DataBytesWrite);
            }
            catch (Exception e)
            {
                // _SerialPort.Close();
               //  driver.DisplayiDialog(e.Message);
                //throw;
            }
        }

        public void WriteMultipleRegister(ushort startAddress,ushort[] dataBufer)
        {

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

        public void ReadRegisters(ushort startAddress)
        {
            try
            {
                driver.ReadRegister(startAddress, ref DataBytesRead);
            }
            catch (Exception e)
            {

            }
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
                    dwordAnswer = (short)((DataBytesRead[7] << 8) | DataBytesRead[8]);
                    break;
                }
                case RegisterType.Float:
                {
                    floatAnswer = 0.0f;
                    uArray = Converter.ConvertByteArrayToUshortArray(DataBytesRead, 7, 2);
                    floatAnswer = Converter.ConvertTwoUInt16ToFloat(uArray);
                    break;
                }
            }
        }

        public void Reset()
        {
            WriteRegisters(8003, new ushort[] { 0x7273 });
        }

        public void FactorySetting()
        {
            WriteRegisters(8003, new ushort[] { 0x6661 });
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
            WriteRegisters(register.EmptyReg, data);                         // [0x0000]
        }

        public void SendCommand(UInt16 regsGM)
        {
            ushort[] data  =new ushort[1];
            data[0] = 0x0001;
            WriteRegisters(regsGM,data);
        }

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
            WriteRegisters(register.Reg_UpdateData,data);                   // 0x002C
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

        private void ReadMeasureRegisters()
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            SendStartCommand();
            Thread.Sleep(200);
            WriteRegisters(register.Reg_StartMeasure, data);                    // [0x0027] Старт измерения

            SendStartCommand();
            Thread.Sleep(200);
            ReadRegisters(register.Reg_UP);                                     // [0x0002] Регистр U+
            ParseAnswer(RegisterType.Float);
            Upf = floatAnswer;
            Ups = Upf.ToString("F2");

            SendStartCommand();
            Thread.Sleep(200);
            ReadRegisters(register.Reg_UM);                                     // [0x0004] Регистр U-
            ParseAnswer(RegisterType.Float);
            Umf = floatAnswer;
            Ums = Umf.ToString("F2");

        }

    }
}
