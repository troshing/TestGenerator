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
using ConvertObject;
using LiveCharts.Wpf;

namespace EAKompensator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //private RegisterGM register = new RegisterGM();
        //private RegisterDat registerDat = new RegisterDat();

        private Calibration _mCalib  = new Calibration();
        private SerializerXML _mSerialize = new SerializerXML();
        private GeneratorDevice myDevice = new GeneratorDevice();
        private XMLFile _mxmlFile = new XMLFile();

        private GeneratorType genStruct = new GeneratorType();

        private Struct_Sens snsStruct = new Struct_Sens();
        private SensorType sensor_type = new SensorType();

        private const ushort addrGM = 0xFEFE;                   //  Широковещательгый адрес для Генератора
        private const ushort adrSns = 0xFCFC;                   //  Широковещательгый адрес для Датчика
        private const ushort def_UIID = 0xFE00;                 // Адрес Генератора по умолчанию

        Master master;
        Control control_Dat;

        public MainWindow()
        {
            InitializeComponent();

            master = new Master();
            control_Dat = new Control(master);

            txtblckStatus.DataContext = master;
            Lbl_UpValue.DataContext = master;
            Lbl_UmValue.DataContext = master;
            //txtBoxRb.Text = "28.976";
            //txtBoxI_Low.Text = "2.51";
            //txtBoxI_High.Text = "5.03";

            TxtRm.DataContext = master;
            TxtRp.DataContext = master;
            TxtCs.DataContext = master;
            TxtTau.DataContext = master;
            TxtRfid.DataContext = master;

            SetNum.DataContext = control_Dat;
            SetNumSys.DataContext = control_Dat;
            GenNum.DataContext = control_Dat;
            FabNum.DataContext = control_Dat;
            MyLineSeries.DataContext = control_Dat;

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
            master.WriteRegisters(addrGM, master.register.Reg_START_IP, data);                  // [0x0047]
            
            Thread.Sleep(500);
    
            master.ReadRegisters(addrGM, master.register.Reg_IP);                // запрос [0x0010] Ip real
            master.ParseAnswer(RegisterType.Float);
            Ip = master.floatAnswer * 1000.0f;
            lbl_Ip.Content = @"I+= " + Ip.ToString("F2") + " мА";

            Thread.Sleep(200);

            master.ReadRegisters(addrGM, master.register.Reg_IP_Codes);                // запрос [0x0030] Ip Codes
            master.ParseAnswer(RegisterType.DWord);
            Ipcode = master.dwordAnswer;
            lbl_Ipcode.Content = @"I+= " + Ipcode.ToString("D") + " код";

            myDevice.Ip_izm = Ip;
            myDevice.Ipcode = Ipcode;
            master.DisplayiDialog("Откалиброван I+");        
        }

        private void Btn_CalcIm_Click(object sender, RoutedEventArgs e)
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            float Im = 0.0f;
            short Imcode = 0;
            data[0] = 0x0001;

            StartConversion();                                          // запустим Преобразование АЦП
            Thread.Sleep(500);
         
            master.WriteRegisters(addrGM, master.register.Reg_START_IM, data);                  // [0x0048]
            
            Thread.Sleep(500);
           
            master.ReadRegisters(addrGM, master.register.Reg_IM);                // запрос [0x0011] Im real
            master.ParseAnswer(RegisterType.Float);
            Im = master.floatAnswer * 1000.0f;
            lbl_Im.Content = @"I-= " + Im.ToString("F2") + " мА";

            Thread.Sleep(200);
      
            master.ReadRegisters(addrGM, master.register.Reg_IM_Codes);                // запрос [0x0032] Im Codes
            master.ParseAnswer(RegisterType.DWord);
            Imcode = master.dwordAnswer;
            lbl_Imcode.Content = @"I-= " + Imcode.ToString("D") + " код";
     
            myDevice.Im_izm = Im;           // mA
            myDevice.Imcode = Imcode;
            master.DisplayiDialog("Откалиброван I-");
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
            
            Thread.Sleep(200);
            data[0] = Convert.ToUInt16(myDevice.Ipcode);
            master.WriteRegisters(addrGM, master.register.Reg_IP_Codes, data);                  // [0x0030]  I+                
           
            Thread.Sleep(200);
            data[0] = Convert.ToUInt16(myDevice.Imcode);
            master.WriteRegisters(addrGM, master.register.Reg_IM_Codes, data);                  // [0x0032]  I-
            lbl_Ipcode.Content = @"I+= " + myDevice.Ipcode.ToString("D") + " код";
            lbl_Imcode.Content = @"I-= " + myDevice.Imcode.ToString("D") + " код";
            master.DisplayiDialog("Новые коды для ЦАП отправлены");
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)           // сохранить калибровку
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            master.WriteRegisters(addrGM, master.register.Reg_SaveKalibration, data);                  // [0x0028]

            master.DisplayiDialog("Калибровка успешно сохранена");
        }

        private void Btn_Apply_Click(object sender, RoutedEventArgs e)              // применить калибровку
        {
            // ModbusWriteCommand(addrGM,Register,Data);
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)             // отменить калибровку
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            master.WriteRegisters(addrGM, master.register.Reg_CancelKalibration, data);            // [0x002A]
            SetAllLabelsToNull(); 
        }

        private void btn_StartCalibration_Click(object sender, RoutedEventArgs e)   // начать калибровку
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0000;

            master.WriteRegisters(addrGM, master.register.Reg_StatusKalib, data);                  // [0x003A]
           
            data[0] = 0x0001;
           
            // Thread.Sleep(200);
            // master.WriteRegisters(register.Reg_Kalibrate, data);                 // [0x0026]
        }

        private void Btn_SetR_Ball_Click(object sender, RoutedEventArgs e)
        {
            ushort[] dataVal = new ushort[2];     //
            float Rballast = 0.0f;
            
           // Converter.ParseFloat(txtBoxRb.Text,ref Rballast);
            dataVal = Converter.ConvertFloatToTwoUint16(Rballast);

            // master.SendStartCommand();
            Thread.Sleep(200);
            master.WriteRegisters(adrSns, master.register.Reg_RP_Ballast, dataVal);                    // [0x0034] 
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

            Thread.Sleep(200);
            master.WriteRegisters(addrGM, master.register.Reg_ITEST_HIGH, dataVal);                    // [0x0046]  Itest 5 mA
            master.DisplayiDialog("Данные успешно отправлены!");
        }

        private void Btn_SetI_Low_Click(object sender, RoutedEventArgs e)
        {
            ushort[] dataVal = new ushort[2];     //
            float Itest = 0.0f;

            //Converter.ParseFloat(txtBoxRb.Text, ref Itest);
            dataVal = Converter.ConvertFloatToTwoUint16(Itest);

            Thread.Sleep(200);
            master.WriteRegisters(addrGM, master.register.Reg_ITEST_LOW, dataVal);                     // [0x0044] Itest 2.5 mA
            master.DisplayiDialog("Данные успешно отправлены!");
        }

        private void BtnKalibrateUp_OnClick(object sender, RoutedEventArgs e)
        {
            float u1MinusVal = 0.001f;
            float u2PlusVal = 0.0f;

            // _mCalib.Set_U1Minus(u1MinusVal);
            
            Converter.ParseFloat(txtUplus.Text,ref u2PlusVal);          // U+ [V]
            _mCalib.Set_U2Plus(u2PlusVal);
            StartConversion();                          // [0x0026]  Регистр запуск Калибровки [W]
            Thread.Sleep(500);                          // Подождем окончания Преобразования АЦП

            master.ReadRegisters(addrGM, master.register.Reg_UP_Codes);                // запрос U1 Plus Code [0x002C]
            master.ParseAnswer(RegisterType.DWord);
            _mCalib.Set_U2PlusCode(master.dwordAnswer);
            
            Thread.Sleep(200);

            // master.ReadRegisters(register.Reg_UM_Codes);                // запрос U2 Minus Code
            // master.ParseAnswer(RegisterType.DWord);
            // _mCalib.Set_U1MinusCode(master.dwordAnswer);

            code_p.Content = master.dwordAnswer.ToString("x4")+" "+ master.dwordAnswer.ToString("f0");
            master.DisplayiDialog("Откалиброван U+");
        }

        private void BtnKalibrateUm_OnClick(object sender, RoutedEventArgs e)
        {
            float u1PlusVal = 0.001f;
            float u2MinusVal = 0.0f;

            // _mCalib.Set_U1Plus(u1PlusVal);
            Converter.ParseFloat(txtUminus.Text, ref u2MinusVal);
            _mCalib.Set_U2Minus(u2MinusVal);                // U- [V]
            
            StartConversion();                              // [0x0026]  Регистр запуск Калибровки [W]
            Thread.Sleep(500);                               // Подождем окончания Преобразования АЦП
            
            master.ReadRegisters(addrGM, master.register.Reg_UM_Codes);                // запрос U1 Minus Code

            master.ParseAnswer(RegisterType.DWord);                     // запрос U1 Plus Code [0x002E]
            _mCalib.Set_U2MinusCode(master.dwordAnswer);

            Thread.Sleep(200);

            // master.ReadRegisters(register.Reg_UP_Codes);                // запрос U2 Plus Code
            // master.ParseAnswer(RegisterType.DWord);
            // _mCalib.Set_U1PlusCode(master.dwordAnswer);
            code_m.Content = master.dwordAnswer.ToString("x4") + " " + master.dwordAnswer.ToString("f0");
            master.DisplayiDialog("Откалиброван U-");

        }

        private void Btn_Calckoef_OnClick(object sender, RoutedEventArgs e)
        {
            _mCalib.Calc_Koeff_Uminus();
            _mCalib.Calc_Koeff_Uplus();
            myDevice.Bm = 0; // _mCalib.BUm;
            myDevice.Bp = 0; // _mCalib.BUp;
            myDevice.Km = _mCalib.KUm;
            myDevice.Kp = _mCalib.KUp;
            lbl_Km.Content = @"Km= " + _mCalib.KUm.ToString("F5");
           //  lbl_Bm.Content = @"Bm= " + _mCalib.BUm.ToString("F5");
            lbl_Kp.Content = @"Kp= " + _mCalib.KUp.ToString("F5");
           //  lbl_Bp.Content = @"Bp= " + _mCalib.BUp.ToString("F5");
        }

        private void StartConversion()
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            master.WriteRegisters(addrGM, master.register.Reg_Kalibrate, data);                    // [0x0026]     Регистр запуск Калибровки [W]
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
           
            /*
            btn_StartCalibration.IsEnabled = false;
            Btn_Save.IsEnabled = false;
            Btn_Apply.IsEnabled = false;
            Btn_Cancel.IsEnabled = false;
            Btn_EraseFlash.IsEnabled = false;
            BtnGetDataFlash.IsEnabled = false;
            */
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

            /*
             Koeff = Math.Abs(myDevice.Bm);
             dataVal = Converter.ConvertFloatToTwoUint16(Koeff);

             Thread.Sleep(200);
             master.WriteRegisters(register.Reg_KOEFF_BM, dataVal);                     // [0x0052] [float]
              */

            Koeff = Math.Abs(myDevice.Km);
            dataVal = Converter.ConvertFloatToTwoUint16(Koeff);
          
            Thread.Sleep(200);
            master.WriteRegisters(addrGM, master.register.Reg_KOEFF_KM, dataVal);                     // [0x0054] [float]

            /*
            Koeff = Math.Abs(myDevice.Bp);
            dataVal = Converter.ConvertFloatToTwoUint16(Koeff);
            
            Thread.Sleep(200);
            master.WriteRegisters(register.Reg_KOEFF_BP, dataVal);                     // [0x0056] [float]
            */

            Koeff = Math.Abs(myDevice.Kp);
            dataVal = Converter.ConvertFloatToTwoUint16(Koeff);
           
            Thread.Sleep(200);
            master.WriteRegisters(addrGM, master.register.Reg_KOEFF_KP, dataVal);                     // [0x0058] [float]

            master.DisplayiDialog("Коэфф по U применены");
        }

        private void Btn_EraseFlash_Click(object sender, RoutedEventArgs e)             // стереть калибровку
        {
            ushort[] data = new ushort[1];     // [ 0x0001 ]
            data[0] = 0x0001;

            Thread.Sleep(200);
            master.WriteRegisters(addrGM, master.register.Reg_EraseFlash, data);                    // [0x0042] 
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

        private void BtnGetDataFlash_OnClick(object sender, RoutedEventArgs e)      // считать калибровку
        {
            float Ip, Im;
            float Kp, Km, Bp, Bm;

            // Прочитать данные из вн. флэш памяти

            master.ReadRegisters(addrGM, master.register.Reg_IP);                // запрос [0x0010] Ip real
            master.ParseAnswer(RegisterType.Float);
            Ip = master.floatAnswer * 1000.0f;
            // LblIp.Content = @"I-= " + Ip.ToString("F5") + " мА";

            Thread.Sleep(100);
     
            master.ReadRegisters(addrGM, master.register.Reg_IM);                // запрос [0x0011] Im real
            master.ParseAnswer(RegisterType.Float);
            Im = master.floatAnswer * 1000.0f;
            // LblIm.Content = @"I-= " + Im.ToString("F5") + " мА";

            Thread.Sleep(100);
           
            master.ReadRegisters(addrGM, master.register.Reg_KOEFF_KM);                // запрос [0x0054] Km 
            master.ParseAnswer(RegisterType.Float);
            Km = master.floatAnswer;
            // LblKm.Content = @"Km = " + Km.ToString("F5");

            Thread.Sleep(100);
           
            master.ReadRegisters(addrGM, master.register.Reg_KOEFF_KP);                // запрос [0x0058] Kp 
            master.ParseAnswer(RegisterType.Float);
            Kp = master.floatAnswer;
            // LblKp.Content = @"Kp = " + Kp.ToString("F5");

            Thread.Sleep(100);
           
            master.ReadRegisters(addrGM, master.register.Reg_KOEFF_BM);                // запрос [0x0052] Bm ????
            master.ParseAnswer(RegisterType.Float);
            Bm = master.floatAnswer;
            // LblBm.Content = @"Bm = " + Bm.ToString("F5");

            Thread.Sleep(100);
           
            master.ReadRegisters(addrGM, master.register.Reg_KOEFF_BP);                // запрос [0x0056] Bp ????
            master.ParseAnswer(RegisterType.Float);
            Bp = master.floatAnswer;
            // LblBp.Content = @"Bp = " + Bp.ToString("F5");

        }

        private void BtnStartImpulse_OnClick(object sender, RoutedEventArgs e)    // одиночный режим генератора
        {
            BtnStartImpulse.IsEnabled = false;
            BtnSendDataSensor.IsEnabled = false;
            master.SendCommand(master.register.Reg_StartImpulse);              // [0x001A]

            Thread.Sleep(10000);                                        // 10 sec

            // данные надо получить через чтение регистров или структуры
            master.ReadStruct(def_UIID, master.register.Reg_ReadStruct);
            master.GetStructFromGen(ref genStruct);
            TxtRp.Text = genStruct.R_plus.ToString("F3");
            TxtRm.Text = genStruct.R_minus.ToString("F3");

            float Tau = genStruct.Tau_seti * 1000.0f;
            TxtTau.Text = Tau.ToString("F3");

            float C_set = genStruct.C_seti * 1000000.0f;
            TxtCs.Text = C_set.ToString("F3");

            master.DisplayiDialog("Данные обновлены для Генератора");
            BtnStartImpulse.IsEnabled = true;
            BtnSendDataSensor.IsEnabled = true;
        }
        private void BtnStartProc_OnClick(object sender, RoutedEventArgs e)    // циклический режим генератора
        {
            ushort adrSens = 0;
            adrSens = Convert.ToUInt16(TxtNdat.Text);

            BtnStartProc.IsEnabled = false;
            BtnStartImpulse.IsEnabled = false;
            BtnSendDataSensor.IsEnabled = false;
            BtnStopProc.IsEnabled = true;

            master.AddrSensor = adrSens;
            // Начать опрос U+,U-
            if (!master.isCyclicPoll)
            {                
                master.StartCyclic(11000);
            }

        }
         private void BtnStopProc_OnClick(object sender, RoutedEventArgs e)     // остановка циклический режим генератора
        {
          BtnStartProc.IsEnabled = true;
          BtnStartImpulse.IsEnabled = true;
          BtnSendDataSensor.IsEnabled = true;
            
          master.StopCyclic();
        }

        private void BtnSendDataSensor_OnClick(object sender, RoutedEventArgs e)
        {
            ushort adrSens = 0;
            adrSens = Convert.ToUInt16(TxtNdat.Text);

            BtnSendDataSensor.IsEnabled = false;
            // Converter.ParseFloat(TxtRp.Text, ref myDevice.Rp);
            // Converter.ParseFloat(TxtRm.Text, ref myDevice.Rm);
            // Converter.ParseFloat(TxtCs.Text, ref myDevice.Cs);
            myDevice.Rp = genStruct.R_plus;
            myDevice.Rm = genStruct.R_minus;
            myDevice.Ts = genStruct.Tau_seti;

            // myDevice.Rs = 1.0f / ((1.0f / myDevice.Rp) + (1.0f / myDevice.Rm) + (1.0f / myDevice.Rb));
            // myDevice.Ts = myDevice.Rs * myDevice.Cs*0.000001f;

            // TxtTau.Text = myDevice.Ts.ToString("F5");
            master.SendDataTransfer(myDevice.Rp, myDevice.Rm, myDevice.Ts);             //запись данных сети в датчик
            Thread.Sleep(2000);

            // Получить данные с датчика 
            master.ReadStruct(adrSens, master.registerDat.Reg_Ans_Data);
            master.GetStructFromSensor(ref sensor_type);
            
            TxtRfid.Text = sensor_type.Rf.ToString("F2");
            master.DisplayiDialog("Данные обновлены для Датчика");

            BtnSendDataSensor.IsEnabled = true;
        }
//**********************************************************************************
// обработка вкладки  Датчик
//**********************************************************************************
        private void Button_Calibrate_Sence_Click(object sender, RoutedEventArgs e)
        {
            control_Dat.SetNums = SetNum.Text;
            if ((control_Dat.SetNums != "") && (control_Dat.SetNums != "0"))
                {
                    control_Dat.StartDatCalibrIH(control_Dat.SetNums);
                }
        }

        private void Button_Read_Sence_Click(object sender, RoutedEventArgs e)
        {
            control_Dat.SetNums = SetNum.Text; 
             if (control_Dat.SetNums == "") control_Dat.SetNums = "65278";           
            control_Dat.ReadDatStruct(control_Dat.SetNums);
            control_Dat.ReadDatFabNum(control_Dat.SetNums);
            control_Dat.ReadDatCalibrIH(control_Dat.SetNums);
            if ((control_Dat.SetNums != "") && (control_Dat.SetNums != null))
                {
                    BCalibr.IsEnabled = true;
                    BWrite.IsEnabled = true;
                }                                                                                       

        }
        private void Button_Write_Sence_Click(object sender, RoutedEventArgs e)
        {
            string str = control_Dat.SetNumSyss;

            control_Dat.WriteDatSys(control_Dat.SetNums, control_Dat.SetNumSyss, control_Dat.SetGenNums, control_Dat.SetFabNums);
            Thread.Sleep(100);
 
            control_Dat.ReadDatStruct(str);                 //проверяем, что записали, визуализируем

            control_Dat.ReadDatFabNum(control_Dat.SetNums);
            control_Dat.ReadDatCalibrIH(control_Dat.SetNums);
        }
    }
//**********************************************************************************
// обработка вкладки  Датчик
//**********************************************************************************
}
