using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace EAKompensator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Master master;
        private RegisterGM register = new RegisterGM();
        private Calibration _mCalib  = new Calibration();
        private SerializerXML _mSerialize = new SerializerXML();
        private KompensatorDevice myDevice = new KompensatorDevice();
        private XMLFile _mxmlFile = new XMLFile();

        public MainWindow()
        {
            InitializeComponent();
            master = new Master();
            
            txtblckStatus.DataContext = master;
            //txtBoxRb.Text = "28.976";
            //txtBoxI_Low.Text = "2.51";
            //txtBoxI_High.Text = "5.03";
            _mCalib.InitValues();
            myDevice.SetDefaultData();
            SetAllLabelsToNull();
            AllButtonsOff();
        }

        private void MenuConnection_OnClick(object sender, RoutedEventArgs e)
        {
            ConnectionSettingsWindow connectionSettingsWindow = new ConnectionSettingsWindow(master);
            connectionSettingsWindow.ShowDialog();
        }

        private void MenuExit_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_CalcIp_Click(object sender, RoutedEventArgs e)
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            float Ip = 0.0f;
            short Ipcode = 0;
            data[0] = 0x0001;
            
            StartConversion();                                         // запустим Преобразование АЦП
            Thread.Sleep(500);
            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_START_IP, data);                  // [0x0047]
            
            Thread.Sleep(500);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_IP);                // запрос [0x0010] Ip real
            master.ParseAnswer(RegisterType.Float);
            Ip = master.floatAnswer * 1000.0f;
            lbl_Ip.Content = @"I+= " + Ip.ToString("F5") + " мА";

            Thread.Sleep(200);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_IP_Codes);                // запрос [0x0030] Ip Codes
            master.ParseAnswer(RegisterType.DWord);
            Ipcode = master.dwordAnswer;
            lbl_Ipcode.Content = @"I+= " + Ipcode.ToString("D") + " код";
            myDevice.Ip_izm = Ip;
            myDevice.Ipcode = Ipcode;
            master.DisplayiDialog("Откалибровано I+");
        }

        private void Btn_CalcIm_Click(object sender, RoutedEventArgs e)
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            float Im = 0.0f;
            short Imcode = 0;
            data[0] = 0x0001;

            StartConversion();                                          // запустим Преобразование АЦП
            Thread.Sleep(500);
            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_START_IM, data);                  // [0x0048]
            
            Thread.Sleep(500);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_IM);                // запрос [0x0011] Im real
            master.ParseAnswer(RegisterType.Float);
            Im = master.floatAnswer * 1000.0f;
            lbl_Im.Content = @"I-= " + Im.ToString("F5") + " мА";

            Thread.Sleep(200);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_IM_Codes);                // запрос [0x0032] Im Codes
            master.ParseAnswer(RegisterType.DWord);
            Imcode = master.dwordAnswer;
            lbl_Imcode.Content = @"I-= " + Imcode.ToString("D") + " код";
            myDevice.Im_izm = Im;
            myDevice.Imcode = Imcode;
            master.DisplayiDialog("Откалибровано I-");
        }

        private void Btn_ApplyICode_OnClick(object sender, RoutedEventArgs e)
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            // применить коды для Ip,Im
            myDevice.Ipreal_1mA = myDevice.Ipcode / myDevice.Ip_izm;
            myDevice.Imreal_1mA = myDevice.Imcode / myDevice.Im_izm;
            myDevice.Ipcode = (short)Math.Round(myDevice.Ipreal_1mA);
            myDevice.Imcode = (short) Math.Round(myDevice.Imreal_1mA);

            // записываем новый код в ЦАП
            master.SendStartCommand();
            Thread.Sleep(200);
            data[0] = Convert.ToUInt16(myDevice.Ipcode);
            master.WriteRegisters(register.Reg_IP_Codes, data);                  // [0x0030]
            
            Thread.Sleep(200);
            master.SendStartCommand();
            Thread.Sleep(200);
            data[0] = Convert.ToUInt16(myDevice.Imcode);
            master.WriteRegisters(register.Reg_IM_Codes, data);                  // [0x0032]
            lbl_Ipcode.Content = @"I+= " + myDevice.Ipcode.ToString("D") + " код";
            lbl_Imcode.Content = @"I-= " + myDevice.Imcode.ToString("D") + " код";
            master.DisplayiDialog("Новые коды для ЦАП отправлены");
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_SaveKalibration, data);                  // [0x0028]

            master.DisplayiDialog("Калибровка успешно сохранена");

        }

        private void Btn_Apply_Click(object sender, RoutedEventArgs e)
        {
            // ModbusWriteCommand(Register,Data);
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_CancelKalibration, data);                // [0x002A]
            SetAllLabelsToNull(); 
        }


        private void btn_StartCalibration_Click(object sender, RoutedEventArgs e)
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0000;

            master.WriteRegisters(register.Reg_StatusKalib, data);                      // [0x003A]
            master.WriteRegisters(register.Reg_StatusKalib, data);                      // [0x003A]

            data[0] = 0x0001;
            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_Kalibrate, data);                        // [0x0026]
        }

        private void Btn_SetR_Ball_Click(object sender, RoutedEventArgs e)
        {
            ushort[] dataVal = new ushort[2];     //
            float Rballast = 0.0f;
            
           // Converter.ParseFloat(txtBoxRb.Text,ref Rballast);
            dataVal = Converter.ConvertFloatToTwoUint16(Rballast);

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_RP_Ballast, dataVal);                    // [0x0034] 
            myDevice.Rbal_m = Rballast;
            myDevice.Rbal_p = Rballast;
            master.DisplayiDialog("Данные успешно отправлены!");
        }

        private void Btn_SetI_High_Click(object sender, RoutedEventArgs e)
        {
            ushort[] dataVal = new ushort[2];     //
            float Itest = 0.0f;

           // Converter.ParseFloat(txtBoxRb.Text, ref Itest);
            dataVal = Converter.ConvertFloatToTwoUint16(Itest);

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_ITEST_HIGH, dataVal);                    // [0x0046]  Itest 5 mA
            master.DisplayiDialog("Данные успешно отправлены!");
        }

        private void Btn_SetI_Low_Click(object sender, RoutedEventArgs e)
        {
            ushort[] dataVal = new ushort[2];     //
            float Itest = 0.0f;

            //Converter.ParseFloat(txtBoxRb.Text, ref Itest);
            dataVal = Converter.ConvertFloatToTwoUint16(Itest);

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_ITEST_LOW, dataVal);                     // [0x0044] Itest 2.5 mA
            master.DisplayiDialog("Данные успешно отправлены!");
        }

        private void BtnKalibrateUp_OnClick(object sender, RoutedEventArgs e)
        {
            float u1MinusVal = 0.001f;
            float u2PlusVal = 0.0f;

            _mCalib.Set_U1Minus(u1MinusVal);
            Converter.ParseFloat(txtUplus.Text,ref u2PlusVal);
            _mCalib.Set_U2Plus(u2PlusVal);
            StartConversion();
            Thread.Sleep(500);                          // Подождем окончания Преобразования АЦП

            master.SendStartCommand();
            master.ReadRegisters(register.Reg_UP_Codes);                // запрос U1 Plus Code
            master.ParseAnswer(RegisterType.DWord);
            _mCalib.Set_U2PlusCode(master.dwordAnswer);
            
            Thread.Sleep(200);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_UM_Codes);                // запрос U2 Minus Code
            master.ParseAnswer(RegisterType.DWord);
            _mCalib.Set_U1MinusCode(master.dwordAnswer);
            master.DisplayiDialog("Откалибровано U+");
        }

        private void BtnKalibrateUm_OnClick(object sender, RoutedEventArgs e)
        {
            float u1PlusVal = 0.001f;
            float u2MinusVal = 0.0f;

            _mCalib.Set_U1Plus(u1PlusVal);
            Converter.ParseFloat(txtUminus.Text, ref u2MinusVal);
            _mCalib.Set_U2Minus(u2MinusVal);
            
            StartConversion();
            Thread.Sleep(500);                          // Подождем окончания Преобразования АЦП
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_UM_Codes);                // запрос U1 Minus Code
            master.ParseAnswer(RegisterType.DWord);
            _mCalib.Set_U2MinusCode(master.dwordAnswer);

            Thread.Sleep(200);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_UP_Codes);                // запрос U2 Plus Code
            master.ParseAnswer(RegisterType.DWord);
            _mCalib.Set_U1PlusCode(master.dwordAnswer);
            master.DisplayiDialog("Откалибровано U-");

        }

        private void Btn_Calckoef_OnClick(object sender, RoutedEventArgs e)
        {
            _mCalib.Calc_Koeff_Uminus();
            _mCalib.Calc_Koeff_Uplus();
            myDevice.Bm = _mCalib.BUm;
            myDevice.Bp = _mCalib.BUp;
            myDevice.Km = _mCalib.KUm;
            myDevice.Kp = _mCalib.KUp;
            lbl_Km.Content = @"Km= " + _mCalib.KUm.ToString("F5");
            lbl_Bm.Content = @"Bm= " + _mCalib.BUm.ToString("F5");
            lbl_Kp.Content = @"Kp= " + _mCalib.KUp.ToString("F5");
            lbl_Bp.Content = @"Bp= " + _mCalib.BUp.ToString("F5");
        }

        private void StartConversion()
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_StartConv, data);                    // [0x0038]
        }


        private void MenuOpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            _mxmlFile.OpenFileXML(ref fileName);                  // сделать открытие файла настроек из опред папки

            _mxmlFile.OpenStreamXml(fileName, _mSerialize);
        }

        private void MenuSaveFile_OnClick(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            _mxmlFile.SaveFileXML(ref fileName);
            _mSerialize.LstsClass1.Add(myDevice);
            _mxmlFile.SaveStreamXml(fileName, _mSerialize);
        }

        private void MenuAboutProg_OnClick(object sender, RoutedEventArgs e)
        {
            // О Программе
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void SetAllLabelsToNull()
        {
            lbl_Km.Content = @"Km= 0.0";
            lbl_Bm.Content = @"Bm= 0.0";
            lbl_Kp.Content = @"Kp= 0.0";
            lbl_Bp.Content = @"Bp= 0.0";

            lbl_Ip.Content = @"I+= 0.0";
            lbl_Im.Content = @"I-= 0.0";

            lbl_Ipcode.Content = @"I+= 0.0";
            lbl_Imcode.Content = @"I-= 0.0";

            //LblKm.Content = @"Km= 0.0";
            //LblBm.Content = @"Bm= 0.0";
            //LblKp.Content = @"Kp= 0.0";
            //LblBp.Content = @"Bp= 0.0";

            //LblIp.Content = @"I+= 0.0";
            //LblIm.Content = @"I-= 0.0";

            myDevice.SetDefaultData();
        }

        private void AllButtonsOff()
        {
            btn_StartCalibration.IsEnabled = false;
            Btn_Save.IsEnabled = false;
            Btn_Apply.IsEnabled = false;
            Btn_Cancel.IsEnabled = false;
            Btn_EraseFlash.IsEnabled = false;
            BtnGetDataFlash.IsEnabled = false;
        }

        private void BtnApplyKoeff_OnClick(object sender, RoutedEventArgs e)
        {
            // Применить Коэфф по Напряжению
            /*
            myDevice.Bm;
            myDevice.Bp;
            myDevice.Km;
            myDevice.Kp;
            */
            ushort[] dataVal = new ushort[2];     //
            float Koeff = 0.0f;
            myDevice.Bm = -2.88633f;
            myDevice.Bp = 279.56735f;
            myDevice.Km = 0.06875f;
            myDevice.Kp = 0.06881f;

            Koeff = Math.Abs(myDevice.Bm);
            dataVal = Converter.ConvertFloatToTwoUint16(Koeff);

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_KOEFF_BM, dataVal);                     // [0x0052] [float]

            Koeff = Math.Abs(myDevice.Km);
            dataVal = Converter.ConvertFloatToTwoUint16(Koeff);

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_KOEFF_KM, dataVal);                     // [0x0054] [float]

            Koeff = Math.Abs(myDevice.Bp);
            dataVal = Converter.ConvertFloatToTwoUint16(Koeff);

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_KOEFF_BP, dataVal);                     // [0x0056] [float]

            Koeff = Math.Abs(myDevice.Kp);
            dataVal = Converter.ConvertFloatToTwoUint16(Koeff);

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_KOEFF_KP, dataVal);                     // [0x0058] [float]

            master.DisplayiDialog("Коэфф по U применены");
        }

        private void Btn_EraseFlash_Click(object sender, RoutedEventArgs e)
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_EraseFlash, data);                    // [0x0042]
        }

        private void Btn_StartPolling_OnClick(object sender, RoutedEventArgs e)
        {
            // Начать опрос U+,U-
            if (!master.IsPoll)
            {
                TxtPoll.Text = "Остановить опрос";
                master.StartPoll(1000);
            }
            else
            {
                TxtPoll.Text = "Начать опрос";
                master.StopPoll();
            }
           
        }

        private void BtnGetDataFlash_OnClick(object sender, RoutedEventArgs e)
        {
            float Ip, Im;
            float Kp, Km, Bp, Bm;

            // Прочитать данные из вн. флэш памяти

            master.SendStartCommand();
            master.ReadRegisters(register.Reg_IP);                // запрос [0x0010] Ip real
            master.ParseAnswer(RegisterType.Float);
            Ip = master.floatAnswer * 1000.0f;
            // LblIp.Content = @"I-= " + Ip.ToString("F5") + " мА";

            Thread.Sleep(100);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_IM);                // запрос [0x0011] Im real
            master.ParseAnswer(RegisterType.Float);
            Im = master.floatAnswer * 1000.0f;
            // LblIm.Content = @"I-= " + Im.ToString("F5") + " мА";

            Thread.Sleep(100);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_KOEFF_KM);                // запрос [0x0054] Km 
            master.ParseAnswer(RegisterType.Float);
            Km = master.floatAnswer;
            // LblKm.Content = @"Km = " + Km.ToString("F5");

            Thread.Sleep(100);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_KOEFF_KP);                // запрос [0x0058] Kp 
            master.ParseAnswer(RegisterType.Float);
            Kp = master.floatAnswer;
            // LblKp.Content = @"Kp = " + Kp.ToString("F5");

            Thread.Sleep(100);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_KOEFF_BM);                // запрос [0x0052] Bm
            master.ParseAnswer(RegisterType.Float);
            Bm = master.floatAnswer;
            // LblBm.Content = @"Bm = " + Bm.ToString("F5");

            Thread.Sleep(100);
            master.SendStartCommand();
            master.ReadRegisters(register.Reg_KOEFF_BP);                // запрос [0x0056] Bp
            master.ParseAnswer(RegisterType.Float);
            Bp = master.floatAnswer;
            // LblBp.Content = @"Bp = " + Bp.ToString("F5");

        }

        private void BtnStartImpulse_OnClick(object sender, RoutedEventArgs e)
        {
            BtnStartImpulse.IsEnabled = false;
            master.SendCommand(register.Reg_StartImpulse);
  
           Thread.Sleep(10000);
           master.DisplayiDialog("Команда отправлена");
           BtnStartImpulse.IsEnabled = true;
        }

        private void BtnSendDataSensor_OnClick(object sender, RoutedEventArgs e)
        {

            BtnSendDataSensor.IsEnabled = false;
            Converter.ParseFloat(TxtRp.Text, ref myDevice.Rp);
            Converter.ParseFloat(TxtRm.Text, ref myDevice.Rm);
            Converter.ParseFloat(TxtCs.Text, ref myDevice.Cs);

            myDevice.Rs = 1.0f / ((1.0f / myDevice.Rp) + (1.0f / myDevice.Rm) + (1.0f / myDevice.Rb));
            myDevice.Ts = myDevice.Rs * myDevice.Cs*0.000001f;

            TxtTau.Text = myDevice.Ts.ToString("F5");
            master.SendDataTransfer(myDevice.Rp, myDevice.Rm, myDevice.Ts);
            Thread.Sleep(2000);
            master.DisplayiDialog("Данные отправлены");
            BtnSendDataSensor.IsEnabled = true;
        }
    }
}
