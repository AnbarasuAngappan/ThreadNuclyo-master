using ModbusUber;
using System;
using System.Collections.Generic;
using System.Data;
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
using ThreadNuclyo.Model;

namespace ThreadNuclyo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static private string logFile = "E:\\Log\\meterreader.log";// Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\meterreader.log";
        static private UberDLMX.DLMX dLMX = new UberDLMX.DLMX();

        static private System.Threading.Thread syncDLMXThread = new System.Threading.Thread(DLMXsync);
        static private System.Threading.Thread syncSolarThread = new System.Threading.Thread(SolarSync);
        static private System.Threading.Thread syncWaterThread = new System.Threading.Thread(WaterSync);
        static private System.Threading.Thread syncLPGThread = new System.Threading.Thread(LPGSync);
        static private System.Threading.Thread syncDVThread = new System.Threading.Thread(DGSync);
        static private System.Threading.Thread syncPVThread = new System.Threading.Thread(PVSync);

        static private int _sleepTime = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["SleepKey"]);
        static private int syncInterval = Convert.ToInt32(TimeSpan.FromMinutes(_sleepTime).TotalMilliseconds);
        static private string _apiKey = null;
        static private string _societyIDKey = null;

        static private string _txtAPIKey = null;
        static private string _txtSocietyID = null;
        static private ModBus modbusReader = null;

        public MainWindow()
        {
            InitializeComponent();
            txtSocietyID.Focus();
        }

        #region UserDefinedMethods

        static private void DLMXsync()
        {
            var readDataClient = new ReadingServiceReference.UberServiceClient("BasicHttpBinding_IUberService");
            do
            {
                try
                {
                    _apiKey = _txtAPIKey.ToString(); //System.Configuration.ConfigurationManager.AppSettings["APIKey"];
                    _societyIDKey = _txtSocietyID.ToString(); //System.Configuration.ConfigurationManager.AppSettings["SocietyKey"];

                    if (_apiKey != null && _apiKey.Length > 0 && _societyIDKey != null && _societyIDKey.Length > 0)
                    {
                        if (readDataClient == null)
                        {
                            readDataClient = new ReadingServiceReference.UberServiceClient("BasicHttpBinding_IUberService");
                            WriteToLog(DateTime.Now + "New Object Initialized For GetHouseDetails");
                        }
                        if (readDataClient != null)
                        {
                            DataTable objHouseDetailsdataTable = new DataTable();
                            if (readDataClient == null)
                            {
                                readDataClient = new ReadingServiceReference.UberServiceClient("BasicHttpBinding_IUberService");
                                WriteToLog(DateTime.Now + "New Object Initialized For GetHouseDetails");
                            }
                            objHouseDetailsdataTable = readDataClient.GetHouseDetails(_societyIDKey, _apiKey);

                            if (objHouseDetailsdataTable != null && objHouseDetailsdataTable.Rows.Count > 0)
                            {
                                foreach (DataRow datarowItem in objHouseDetailsdataTable.Rows)
                                {
                                    var _soceityID = datarowItem.Field<string>("SiD");
                                    var _houseID = datarowItem.Field<string>("HiD");
                                    var _houseNo = datarowItem.Field<string>("House No");
                                    var _meterID = datarowItem.Field<string>("MiD");
                                    var _meterType = Convert.ToInt16(datarowItem.Field<Int16>("PiD"));
                                    var _meterSettings = datarowItem.Field<string>("metersetting");
                                    var _ipAddress = datarowItem.Field<string>("IPAddress");
                                    var _port = Convert.ToInt32(datarowItem.Field<string>("Port"));
                                    if (_meterType == 3)
                                    {
                                        //Modbus itembus = Newtonsoft.Json.JsonConvert.DeserializeObject<Modbus>(_meterSettings);
                                        //if (itembus != null && itembus.RiD.Length > 0 && itembus.Address.Length > 0)
                                        //{
                                        //    var _regType = Convert.ToInt32(itembus.RiD);
                                        //    var _startAddress = Convert.ToInt32(itembus.Address);
                                        //    var _qty = itembus.Quantity;
                                        //    var _deviceID = itembus.DeviceID;

                                        //    int[] readHoldingRegisters = null; //ModbusReading.ReadingRegister(_ipAddress, _port, _startAddress, _regType, _qty);
                                        //    //ReadModbus(_ipAddress, _port, _startAddress, _regType);

                                        //    if (readHoldingRegisters != null && readHoldingRegisters.Length > 0)
                                        //    {
                                        //        //objdataTableWriteDLMX.Rows.Add(_soceityID, _houseID, _meterID, _ipAddress, _port, readHoldingRegisters);
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        Model.DLMX itemdLMX = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.DLMX>(_meterSettings);

                                        if (itemdLMX != null && itemdLMX.Manufacturer.Length > 0 && itemdLMX.Model.Length > 0)
                                        {
                                            var manufacture = itemdLMX.Manufacturer;
                                            var model = itemdLMX.Model;
                                            var importExport = Convert.ToInt16(itemdLMX.ImportExport);
                                            string[] val = dLMX.DLMXRead(_ipAddress, _port, importExport);
                                            if (val != null && val.Length > 0)
                                            {
                                                try
                                                {
                                                    if (readDataClient == null)
                                                    {
                                                        readDataClient = new ReadingServiceReference.UberServiceClient("BasicHttpBinding_IUberService");
                                                        WriteToLog(DateTime.Now + "New Object Initialized");
                                                    }

                                                    readDataClient.WriteDLMXDetails(_soceityID, _houseNo, _meterID, _ipAddress, Convert.ToString(_port), val[0], val[1], importExport);

                                                    if (val[2] == "1" || val[2] == "2" || val[2] == "0")
                                                    {
                                                        readDataClient.WriteErrorLog(_soceityID, _houseID, _meterID, _ipAddress, Convert.ToString(_port), "FAULT", val[2], val[3], DateTime.Now);
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    WriteToLog(DateTime.Now + e.InnerException.ToString());
                                                }
                                            }
                                            else
                                            {
                                                readDataClient.WriteDLMXDetails(_soceityID, _houseID, _meterID, _ipAddress, Convert.ToString(_port), null, null, 0);//asdsadfg
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                    }

                }
                #region
                //catch (CommunicationException _communicationException)
                //{
                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Console.WriteLine(_communicationException.InnerException.ToString());
                //    Console.WriteLine("Got {0}", _communicationException.GetType());
                //    WriteToLog(DateTime.Now + _communicationException.InnerException.ToString() + " : Communication Out Exception");
                //    Console.ResetColor();
                //    //readDataClient.Abort();
                //}
                //catch (TimeoutException _timeoutException)
                //{
                //    Console.ForegroundColor = ConsoleColor.Red;
                //    Console.WriteLine(_timeoutException.InnerException.ToString());
                //    Console.WriteLine("Time Out Exception {0}", _timeoutException.GetType());
                //    WriteToLog(DateTime.Now + _timeoutException.InnerException.ToString() + " : Time Out Exception");
                //    Console.ResetColor();
                //    //readDataClient.Abort();
                //}
                #endregion
                catch (Exception _exception)
                {
                    if (_exception.InnerException != null)
                        WriteToLog(_exception.InnerException.ToString());
                    else
                        WriteToLog(_exception.Message.ToString());
                }

                System.Threading.Thread.Sleep(syncInterval);
            }
            while (true);
        }

        static private void SolarSync()
        {
            var readDataClient = new ReadingServiceReference.UberServiceClient("BasicHttpBinding_IUberService");
            if(modbusReader == null)
            {
                modbusReader = new ModBus();
            }
            do
            {
                try
                {
                    _apiKey = "35e63cb9-d1b8-49cd-b5c1-39212d33376g";//System.Configuration.ConfigurationManager.AppSettings["APIKey"];
                    _societyIDKey = "8c634931-122a-4318-b5f7-f63a0d3135b3";//System.Configuration.ConfigurationManager.AppSettings["SocietyKey"];

                    if (_apiKey != null && _apiKey.Length > 0 && _societyIDKey != null && _societyIDKey.Length > 0)
                    {
                        if (readDataClient == null)
                        {
                            readDataClient = new ReadingServiceReference.UberServiceClient("BasicHttpBinding_IUberService");
                            WriteToLog(DateTime.Now + "New Object Initialized For GetHouseDetails");
                        }
                        if (readDataClient != null)
                        {
                            DataTable objHouseDetailsdataTable = new DataTable();
                            if (readDataClient == null)
                            {
                                readDataClient = new ReadingServiceReference.UberServiceClient("BasicHttpBinding_IUberService");
                                WriteToLog(DateTime.Now + "New Object Initialized For GetHouseDetails");
                            }
                            objHouseDetailsdataTable = readDataClient.GetModbusDetails(_societyIDKey, _apiKey);

                            if (objHouseDetailsdataTable != null && objHouseDetailsdataTable.Rows.Count > 0)
                            {
                                foreach (DataRow datarowItem in objHouseDetailsdataTable.Rows)
                                {
                                    var _soceityID = datarowItem.Field<string>("SiD");
                                    var _houseID = datarowItem.Field<string>("HiD");
                                    var _houseNo = datarowItem.Field<string>("House No");
                                    var _meterID = datarowItem.Field<string>("MiD");
                                    var _meterType = Convert.ToInt16(datarowItem.Field<Int16>("PiD"));
                                    var _meterSettings = datarowItem.Field<string>("metersetting");
                                    var _ipAddress = datarowItem.Field<string>("IPAddress");
                                    var _port = Convert.ToInt32(datarowItem.Field<string>("Port"));
                                    if (_meterType == 3)
                                    {
                                        Modbus itembus = Newtonsoft.Json.JsonConvert.DeserializeObject<Modbus>(_meterSettings);
                                        if (itembus != null && itembus.RiD.Length > 0 && itembus.Address.Length > 0)
                                        {
                                            var _regType = Convert.ToInt32(itembus.RiD);
                                            var _startAddress = Convert.ToInt32(itembus.DEAddress);
                                            var _qty = itembus.DEQuantity;
                                            var _deviceID = itembus.DeviceID;
                                            //int[] readHoldingRegisters = ModbusReading.ReadRegisterWithDeviceIDs(_ipAddress, _port, _startAddress, _regType, _qty, _deviceID);
                                            //var byteresult = GetMSB(readHoldingRegisters);
                                            bool _response = modbusReader.OpenProtocol(_ipAddress, _port);
                                            if(_response == true)
                                            {
                                                var _reading = modbusReader.ReadHoldingregister(Convert.ToString(_deviceID), Convert.ToString(_startAddress), Convert.ToString(_qty));//("1", "3204", "5");
                                               
                                                if (_reading != null && _reading.Length > 0)
                                                {

                                                    var id = _reading[0];//4655;
                                                    var hexid = $"{id:X}";
                                                    var id1 = _reading[1];//31213;
                                                    var hexid1 = $"{id1:X}";
                                                    var resulthex = hexid + hexid1;
                                                    int value = Convert.ToInt32(resulthex, 16);

                                                    WriteToLog(Convert.ToString(value));

                                                    //for (int i = 0; (i <= (_qty - 1)); i++)
                                                    //{
                                                    //   //_txtSocietyID = ((_reading[i] + "  "));
                                                    //    WriteToLog(Convert.ToString(_reading[i]));
                                                    //}
                                                }
                                            }
                                          
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                    }

                }
                catch (Exception _exception)
                {
                    if (_exception.InnerException != null)
                        WriteToLog(_exception.InnerException.ToString());
                    else
                        WriteToLog(_exception.Message.ToString());
                }

                System.Threading.Thread.Sleep(syncInterval);
            }
            while (true);
        }

        static private void WaterSync()
        {
            //Implementation----
        }

        static private void LPGSync()
        {
            //Implementation----
        }

        static private void DGSync()
        {
            //Implementation----
        }

        static private void PVSync()
        {
            //Implementation----
        }

        static private void WriteToLog(string _text)
        {
            try
            {
                if (_text != null && _text.Length > 0)
                {
                    System.IO.File.AppendAllText(logFile, _text + DateTime.Now + "\r\n");
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static int GetMSB(int[] intValue)
        {
            try
            {
                if (intValue != null && intValue.Length > 0)
                {
                    var id = intValue[3];//4655;
                    var hexid = $"{id:X}";
                    var id1 = intValue[4];//31213;
                    var hexid1 = $"{id1:X}";
                    var resulthex = hexid + hexid1;
                    int value = Convert.ToInt32(resulthex, 16);//Convert the Hex value to Integer(MSB)
                    return value;
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        public static byte[] GetMSBs(int[] intValue)
        {

            byte[] baArray1 = new byte[2];
            short nOffset = 0x55BB;

            baArray1[0] = (byte)(nOffset & 0x00FF);
            nOffset = (short)(nOffset >> 8);
            baArray1[1] = (byte)(nOffset & 0x00FF);


            byte[] baArray = new byte[2];
            int value = 0;//intValue;//...; // put anything you want

            baArray[0] = (byte)(value & 0x000000FF);
            value = value >> 24;
            baArray[1] = (byte)(value & 0x000000FF);
            return baArray;
        }

        #endregion

        #region ClickEvent

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAPIKey != null && txtSocietyID != null && txtAPIKey.Text.Length > 0 && txtSocietyID.Text.Length > 0)
                {
                    _txtAPIKey = txtAPIKey.Text.ToString();
                    _txtSocietyID = txtSocietyID.Text.ToString();
                    if (chkboxDLMX.IsChecked == true)
                    {
                        syncDLMXThread.Start();
                    }
                    else if (chkboxSolar.IsChecked == true)
                    {
                        syncSolarThread.Start();
                    }
                }
            }
            catch (Exception _exception)
            {
                MessageBox.Show(_exception.Message.ToString(), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(chkboxDLMX.IsChecked == true)
                {
                    syncDLMXThread.Abort();
                }
                this.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Event
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
