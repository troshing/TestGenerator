using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using RJCP.IO.Ports;

namespace EAKompensator
{
    public class GmSerialDriver
    {
        private SerialPortStream serial = new SerialPortStream();
        private RegisterGM mregsGm = new RegisterGM();
        private const byte addrPM = 0xFE;                       // 254
        private const byte stdAdrLen = 2;
        private const byte stdPrefix = 7;
        private const byte stdPostfix = 2;          // CRC

        public GmSerialDriver()
        {
            
        }

        public void SetSerialPort(SerialPortStream streamPort)
        {
            serial = streamPort;
            serial.ReadTimeout = 2000;
            serial.WriteTimeout = 2000;
        }

        public void ReadRegister(ushort addrReg, ref byte[] byteParser)
        {
            byte[] byteSendBuffer = new byte[10];                          // [ 00 FA 00 03 XX XX 00 01 ] [ CRC_H CRC_L ]

            byte[] byteReadBuffer = new byte[stdAdrLen + stdPrefix + stdPostfix];
            try
            {
                PrepareBufferToRead(addrReg, ref byteSendBuffer);
                serial.Open();
                serial.Write(byteSendBuffer, 0, byteSendBuffer.Length);             // Чтение Конфигурации 1 
                // Thread.Sleep(200);
                serial.Read(byteReadBuffer, 0, byteReadBuffer.Length);
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

        private void PrepareBufferToRead(ushort addrReg, ref byte[] bufferOut)
        {
            int i = 0;

            bufferOut[i++] = 0x00;                          // 0
            bufferOut[i++] = addrPM;                        // 1
            bufferOut[i++] = 0x00;                          // 2
            bufferOut[i++] = mregsGm.ReadFunc;              // 3

            ConvertUshortToBuffer(addrReg, ref bufferOut, ref i);
            ConvertUshortToBuffer(mregsGm.byteCount, ref bufferOut, ref i);     // 4,5 # Кол-во Регистров

            var mycrc = ModbusCRC.ModbusCRC16Calc(bufferOut);

            bufferOut[i++] = mycrc[0];
            bufferOut[i] = mycrc[1];
        }

        public void WriteRegister(ushort addrReg, ref byte[] dataBufer)
        {
            byte[] byteSendBuffer = new byte[dataBufer.Length + stdPrefix + stdPostfix];    // [ 00 FA 00 06 XX XX , dataBufer] [ CRC_H CRC_L ]
            byte[] byteReadBuffer = new byte[10];
            try
            {
                PrepareBufferToWrite(addrReg, ref byteSendBuffer, dataBufer);
                serial.Open();
                serial.WriteAsync(byteSendBuffer, 0, byteSendBuffer.Length);             // Чтение Конфигурации 1 
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

        private void PrepareBufferToWrite(ushort addrReg, ref byte[] bufferOut, byte[] dataBufer)
        {
            int i = 0;
            bufferOut[i++] = 0xFE;                          // 0
            bufferOut[i++] = addrPM;                        // 1
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
