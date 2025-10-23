using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using RJCP.IO.Ports;
using System.Runtime.InteropServices;

namespace EAKompensator
{
    public class GmSerialDriver
    {
        public RJCP.IO.Ports.SerialPortStream serial = new RJCP.IO.Ports.SerialPortStream();
        // public SerialPortStream serial = new SerialPortStream();
        private RegisterGM mregsGm = new RegisterGM();
        private GeneratorType genStruct = new GeneratorType();
        private Struct_Sens snsStruct = new Struct_Sens();

        private const byte stdAdrLen = 2;
        private const byte stdPrefix = 6;
        private const byte stdPostfix = 2;                      // CRC
        private int stdStruct = 10;


        public GmSerialDriver()
        {
            
        }

        public void SetSerialPort(RJCP.IO.Ports.SerialPortStream streamPort)
        {
            serial = streamPort;
            serial.ReadTimeout = 2000;
            serial.WriteTimeout = 2000;
        }
        
        public void ReadRegister(ushort adrDevice, ushort addrReg, ref byte[] byteParser)
        {
            byte[] byteSendBuffer = new byte[10];                          // [ FE FE 00 03 XX XX 00 01 ] [ CRC_H CRC_L ]

            byte[] byteReadBuffer = new byte[stdAdrLen + stdPrefix + stdPostfix];
            try
            {
                PrepareBufferToRead(adrDevice,addrReg, ref byteSendBuffer);
                serial.Open();
                //serial.FlushAsync();
                serial.Write(byteSendBuffer, 0, byteSendBuffer.Length);             // Чтение 
                // Thread.Sleep(100);
                serial.ReadAsync(byteReadBuffer, 0, byteReadBuffer.Length);
                Array.Copy(byteReadBuffer, 0, byteParser, 0, byteReadBuffer.Length);
                serial.Close();
            }
            catch (Exception e)
            {
                serial.Close();
                DisplayiDialog(e.Message);
               // throw;
            }
        }

        public void ReadStructRegs(ushort adrDevice,ushort addrReg, ref byte[] byteParser)
        {
            byte[] byteSendBuffer = new byte[10];                          // [ FE FE 00 03 XX XX 00 01 ] [ CRC_H CRC_L ]
            byte[] byteReadBuffer = new byte[stdAdrLen + stdPrefix + stdPostfix];
            
            try
            {
                if (addrReg == mregsGm.Reg_ReadStruct)
                {
                    int szStruct = Marshal.SizeOf(genStruct);
                    byteReadBuffer = new byte[szStruct + stdStruct];
                }
                else if (addrReg == mregsGm.Reg_Ans_Data)
                {
                    int szStruct = Marshal.SizeOf(snsStruct);
                    byteReadBuffer = new byte[szStruct+ stdStruct];
                }     
                PrepareBufferToRead(adrDevice,addrReg, ref byteSendBuffer);
                serial.Open();
                //serial.FlushAsync();
                serial.Write(byteSendBuffer, 0, byteSendBuffer.Length);             // Чтение 
                // Thread.Sleep(100);
                serial.ReadAsync(byteReadBuffer, 0, byteReadBuffer.Length);
                Array.Copy(byteReadBuffer, 0, byteParser, 0, byteReadBuffer.Length);
                serial.Close();
            }
            catch (Exception e)
            {
                serial.Close();
                DisplayiDialog(e.Message);
                // throw;
            }
        }

        private void PrepareBufferToRead(ushort adrDevice,ushort addrReg, ref byte[] bufferOut)
        {
            int i = 0;
            byte[] HiLo = new byte[2];
            HiLo = BitConverter.GetBytes(adrDevice);

            bufferOut[i++] = HiLo[1];             // 0
            bufferOut[i++] = HiLo[0];             // 1
            bufferOut[i++] = 0x00;                          // 2
            bufferOut[i++] = mregsGm.ReadFunc;              // 3

            ConvertUshortToBuffer(addrReg, ref bufferOut, ref i);
            ConvertUshortToBuffer(mregsGm.byteCount, ref bufferOut, ref i);     // 4,5 # Кол-во Регистров

            var mycrc = ModbusCRC.ModbusCRC16Calc(bufferOut);

            bufferOut[i++] = mycrc[0];
            bufferOut[i] = mycrc[1];
        }

        public void WriteRegister(ushort adrDevice,ushort addrReg, ref byte[] dataBufer)
        {
            byte[] byteSendBuffer = new byte[dataBufer.Length + stdPrefix + stdPostfix];    // [ FE FE 00 06 XX XX , dataBufer] [ CRC_H CRC_L ]
            byte[] byteReadBuffer = new byte[10];
            try
            {
                PrepareBufferToWrite(adrDevice,addrReg, ref byteSendBuffer, dataBufer);
                serial.Open();
                // serial.FlushAsync();
                serial.WriteAsync(byteSendBuffer, 0, byteSendBuffer.Length);             // Запись 
                Thread.Sleep(100);
                serial.ReadAsync(byteReadBuffer, 0, byteReadBuffer.Length);
                // Array.Copy(byteReadBuffer, 3, dataBufer, 0, dataBufer.Length);
                serial.Close();
            }
            catch (Exception e)
            {
                serial.Close();
                DisplayiDialog(e.Message);
                //throw;
            }
        }

        public void WriteAcqCommand(ushort adrDevice, ushort addrReg, ref byte[] dataBufer)
        {
            byte[] byteSendBuffer = new byte[dataBufer.Length + stdPrefix + stdPostfix];    // [ FE FE 00 2B XX XX , dataBufer] [ CRC_H CRC_L ]
            byte[] byteReadBuffer = new byte[10];
            try
            {
                PrepareBufferToCommand(adrDevice, addrReg, ref byteSendBuffer, dataBufer);
                serial.Open();
                // serial.FlushAsync();
                serial.WriteAsync(byteSendBuffer, 0, byteSendBuffer.Length);             // Запись 
                Thread.Sleep(100);
                serial.ReadAsync(byteReadBuffer, 0, byteReadBuffer.Length);
                // Array.Copy(byteReadBuffer, 3, dataBufer, 0, dataBufer.Length);
                serial.Close();
            }
            catch (Exception e)
            {
                serial.Close();
                DisplayiDialog(e.Message);
                //throw;
            }
        }

        private void PrepareBufferToWrite(ushort adrDevice,ushort addrReg, ref byte[] bufferOut, byte[] dataBufer)
        {
            int i = 0;
            byte[] HiLo = new byte[2];
            HiLo = BitConverter.GetBytes(adrDevice);
            bufferOut[i++] = HiLo[0];             // 0
            bufferOut[i++] = HiLo[1];             // 1
            bufferOut[i++] = 0x00;                          // 2
            bufferOut[i++] = mregsGm.WriteFunc;             // 3

            ConvertUshortToBuffer(addrReg, ref bufferOut, ref i);   // 4,5 
            foreach (var t in dataBufer)
            {
                bufferOut[i++] = t;                         // 6,7
            }

            var mycrc = ModbusCRC.ModbusCRC16Calc(bufferOut);

            bufferOut[i++] = mycrc[0];                      // 8
            bufferOut[i] = mycrc[1];                        // 9
        }

        private void PrepareBufferToCommand(ushort adrDevice, ushort addrReg, ref byte[] bufferOut, byte[] dataBufer)
        {
            int i = 0;
            byte[] HiLo = new byte[2];
            HiLo = BitConverter.GetBytes(adrDevice);
            bufferOut[i++] = HiLo[0];               // 0
            bufferOut[i++] = HiLo[1];               // 1
            bufferOut[i++] = 0x00;                  // 2
            bufferOut[i++] = mregsGm.WriteAcq;      // 3 - 0x2B

            ConvertUshortToBuffer(addrReg, ref bufferOut, ref i);   // 4,5 
            foreach (var t in dataBufer)
            {
                bufferOut[i++] = t;                         // 6,7
            }

            var mycrc = ModbusCRC.ModbusCRC16Calc(bufferOut);

            bufferOut[i++] = mycrc[0];                      // 8
            bufferOut[i] = mycrc[1];                        // 9
        }

        public void ConvertUshortToBuffer(ushort data, ref byte[] buffer, ref int offset)
        {
            var byteWord = BitConverter.GetBytes(data);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteWord);
            }

            buffer[offset++] = byteWord[0];
            buffer[offset++] = byteWord[1];
        }

        public void DisplayiDialog(string data)
        {
            // TODO сделать аналог MessageBox
            MessageBox.Show(data);
        }
       
    }
}
