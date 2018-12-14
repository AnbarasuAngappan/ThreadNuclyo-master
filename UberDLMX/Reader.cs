using Gurux.Common;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Reader;
using Gurux.DLMS.Secure;
using Gurux.Net;
using Gurux.Serial;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UberDLMX.Model;

namespace UberDLMX
{
    public class Settings
    {
        public IGXMedia media = null;
        public TraceLevel trace = TraceLevel.Info;
        public bool iec = false;
        public GXDLMSSecureClient client = new GXDLMSSecureClient(true);
        //Objects to read.
        public List<KeyValuePair<string, int>> readObjects = new List<KeyValuePair<string, int>>();
    }

    public class Reader
    {
        Settings settings = new Settings();
        //Gurux.DLMS.Reader.GXDLMSReader reader = null;
        GXDLMSReader reader = null;
        Configuration configuration = null;
        public bool _boolGetParamAtFirst = false;

        public Reader()
        {

        }

        public Reader(string _ipAddress, int _port, string _clientAdd, string _name, string _authentication, string _password, string _sObisValue, string _rObisValue)
        {
            try
            {
                configuration = new Configuration();
                configuration.IpAddress = _ipAddress;
                configuration.Port = _port;
                configuration.ClientAdd = _clientAdd;
                configuration.Name = _name;
                configuration.Authentication = _authentication;
                configuration.Password = _password;
                configuration.SObisValue = _sObisValue;
                configuration.RObisValue = _rObisValue;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string[] DLMSImport()
        {
            string[] _result = new string[4];
            try
            {
                _result = Read();
                if (_result[2] == "1" || _result[2] == "2")
                {
                    System.Threading.Thread.Sleep(2000);
                    _result = Read();
                    return _result;
                }
                else
                {
                    return _result;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public int GetParameters(Settings settings)
        {
            string[] tmp;
            List<GxCommand> parameters = new List<GxCommand>();//List<GXCmdParameter> parameters = GXCommon.GetParameters(args, "h:p:c:s:r:it:a:wP:g:S:C:");
            parameters.Add(new GxCommand() { Tag = 'c', Value = configuration.ClientAdd });//"32"
            parameters.Add(new GxCommand() { Tag = 'r', Value = configuration.Name });//"ln"
            parameters.Add(new GxCommand() { Tag = 'h', Value = configuration.IpAddress });//"192.168.5.104"
            parameters.Add(new GxCommand() { Tag = 'p', Value = Convert.ToString(configuration.Port) });//"502"
            parameters.Add(new GxCommand() { Tag = 'a', Value = configuration.Authentication });//"Low"
            parameters.Add(new GxCommand() { Tag = 'P', Value = configuration.Password });//"1111111111111111"
            parameters.Add(new GxCommand() { Tag = 'g', Value = configuration.SObisValue });//"1.0.1.8.0.255:2"
            parameters.Add(new GxCommand() { Tag = 'g', Value = configuration.RObisValue });
            GXNet net = null;            
            foreach (GxCommand it in parameters)
            {
                switch (it.Tag)
                {
                    case 'w':
                        settings.client.InterfaceType = InterfaceType.WRAPPER;
                        break;
                    case 'r':
                        if (string.Compare(it.Value, "sn", true) == 0)
                        {
                            settings.client.UseLogicalNameReferencing = false;
                        }
                        else if (string.Compare(it.Value, "ln", true) == 0)
                        {
                            settings.client.UseLogicalNameReferencing = true;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid reference option.");
                        }
                        break;
                    case 'h':
                        //Host address.
                        if (settings.media == null)
                        {
                            settings.media = new GXNet();
                        }
                        net = settings.media as GXNet;
                        net.HostName = it.Value;
                        break;
                    case 't':
                        //Trace.
                        try
                        {
                            settings.trace = (TraceLevel)Enum.Parse(typeof(TraceLevel), it.Value);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("Invalid trace level option. (Error, Warning, Info, Verbose, Off)");
                        }
                        break;
                    case 'p':
                        //Port.
                        if (settings.media == null)
                        {
                            settings.media = new GXNet();
                        }
                        net = settings.media as GXNet;
                        net.Port = int.Parse(it.Value);
                        break;
                    case 'P'://Password
                        settings.client.Password = ASCIIEncoding.ASCII.GetBytes(it.Value);
                        break;
                    case 'i':
                        //IEC.
                        settings.iec = true;
                        break;
                    case 'g':
                        //Get (read) selected objects.
                        foreach (string o in it.Value.Split(new char[] { ';', ',' }))
                        {
                            tmp = o.Split(new char[] { ':' });
                            if (tmp.Length != 2)
                            {
                                throw new ArgumentOutOfRangeException("Invalid Logical name or attribute index.");
                            }
                            settings.readObjects.Add(new KeyValuePair<string, int>(tmp[0].Trim(), int.Parse(tmp[1].Trim())));
                        }
                        break;
                    case 'S'://Serial Port
                        settings.media = new GXSerial();
                        GXSerial serial = settings.media as GXSerial;
                        tmp = it.Value.Split(':');
                        serial.PortName = tmp[0];
                        if (tmp.Length > 1)
                        {
                            serial.BaudRate = int.Parse(tmp[1]);
                            serial.DataBits = int.Parse(tmp[2].Substring(0, 1));
                            serial.Parity = (Parity)Enum.Parse(typeof(Parity), tmp[2].Substring(1, tmp[2].Length - 2));
                            serial.StopBits = (StopBits)int.Parse(tmp[2].Substring(tmp[2].Length - 1, 1));
                        }
                        else
                        {
                            serial.BaudRate = 9600;
                            serial.DataBits = 8;
                            serial.Parity = Parity.None;
                            serial.StopBits = StopBits.One;
                        }
                        break;
                    case 'a':
                        try
                        {
                            if (string.Compare("None", it.Value, true) == 0)
                            {
                                settings.client.Authentication = Authentication.None;
                            }
                            else if (string.Compare("Low", it.Value, true) == 0)
                            {
                                settings.client.Authentication = Authentication.Low;
                            }
                            else if (string.Compare("High", it.Value, true) == 0)
                            {
                                settings.client.Authentication = Authentication.High;
                            }
                            else if (string.Compare("HighMd5", it.Value, true) == 0)
                            {
                                settings.client.Authentication = Authentication.HighMD5;
                            }
                            else if (string.Compare("HighSha1", it.Value, true) == 0)
                            {
                                settings.client.Authentication = Authentication.HighSHA1;
                            }
                            else if (string.Compare("HighSha256", it.Value, true) == 0)
                            {
                                settings.client.Authentication = Authentication.HighSHA256;
                            }
                            else if (string.Compare("HighGMac", it.Value, true) == 0)
                            {
                                settings.client.Authentication = Authentication.HighGMAC;
                            }
                            else
                            {
                                throw new ArgumentException("Invalid Authentication option: '" + it.Value + "'. (None, Low, High, HighMd5, HighSha1, HighGMac, HighSha256)");
                            }
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("Invalid Authentication option: '" + it.Value + "'. (None, Low, High, HighMd5, HighSha1, HighGMac, HighSha256)");
                        }
                        break;
                    case 'C':
                        try
                        {
                            settings.client.Ciphering.Security = (Security)Enum.Parse(typeof(Security), it.Value);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("Invalid Ciphering option. (None, Authentication, Encrypted, AuthenticationEncryption)");
                        }
                        break;
                    case 'o':
                        break;
                    case 'c':
                        settings.client.ClientAddress = int.Parse(it.Value);
                        break;
                    case 's':
                        settings.client.ServerAddress = int.Parse(it.Value);
                        break;
                    case '?':
                        switch (it.Tag)
                        {
                            case 'c':
                                throw new ArgumentException("Missing mandatory client option.");
                            case 's':
                                throw new ArgumentException("Missing mandatory server option.");
                            case 'h':
                                throw new ArgumentException("Missing mandatory host name option.");
                            case 'p':
                                throw new ArgumentException("Missing mandatory port option.");
                            case 'r':
                                throw new ArgumentException("Missing mandatory reference option.");
                            case 'a':
                                throw new ArgumentException("Missing mandatory authentication option.");
                            case 'S':
                                throw new ArgumentException("Missing mandatory Serial port option.");
                            case 't':
                                throw new ArgumentException("Missing mandatory trace option.");
                            case 'g':
                                throw new ArgumentException("Missing mandatory OBIS code option.");
                            case 'C':
                                throw new ArgumentException("Missing mandatory Ciphering option.");
                            default:
                                //ShowHelp();
                                return 1;
                        }
                    default:
                        //ShowHelp();
                        return 1;
                }
            }
            if (settings.media == null)
            {
                // ShowHelp();
                return 1;
            }
            return 0;
        }

        public string[] Read()
        {
            object values = null;
            int ret = 0;
            string[] _result = new string[4];
            try
            {
                if(_boolGetParamAtFirst == false)
                {
                    ret = GetParameters(settings);
                    _boolGetParamAtFirst = true;
                }
                
                if (ret != 0)
                {
                    //return ret;
                }
                ////////////////////////////////////////
                //Initialize connection settings.
                if (settings.media is GXSerial)
                {
                    GXSerial serial = settings.media as GXSerial;
                    if (settings.iec)
                    {
                        serial.BaudRate = 300;
                        serial.DataBits = 7;
                        serial.Parity = System.IO.Ports.Parity.Even;
                        serial.StopBits = System.IO.Ports.StopBits.One;
                    }
                    else
                    {
                        serial.BaudRate = 9600;
                        serial.DataBits = 8;
                        serial.Parity = System.IO.Ports.Parity.None;
                        serial.StopBits = System.IO.Ports.StopBits.One;
                    }
                }
                else if (settings.media is GXNet)
                {
                }
                else
                {
                    throw new Exception("Unknown media type.");
                }

                reader = new GXDLMSReader(settings.client, settings.media, settings.trace);
                //reader = new Gurux.DLMS.Reader.GXDLMSReader.GXDLMSReader(settings.client, settings.media, settings.trace);
                settings.media.Open();
                //Some meters need a break here.
                Thread.Sleep(1000);
                if (settings.media.IsOpen)
                {
                    if (settings.readObjects.Count != 0)
                    {
                        reader.InitializeConnection();
                        reader.GetAssociationView(false);
                        foreach (KeyValuePair<string, int> it in settings.readObjects)
                        {
                            object val = reader.Read(settings.client.Objects.FindByLN(ObjectType.None, it.Key), it.Value);
                            //reader.ShowValue(val, it.Value);
                            values = reader.ShowValues(val, it.Value);
                            if (_result != null && _result[0] == null)
                            {
                                _result[0] = values.ToString();
                            }
                            else
                            {
                                _result[1] = values.ToString();
                            }
                        }
                        _result[2] = "0";
                        _result[3] = "Success";
                        return _result;
                    }
                    else
                    {
                        reader.ReadAll(false);
                    }
                }
                else
                {
                    _result[2] = "1";
                    _result[3] = "IP not reachable";
                    return _result;
                }
                return _result;

            }
            catch (Exception)
            {
                _result[2] = "2";
                _result[3] = "Error in Initialize TCP Connection / Meter Not reachable";
                return _result;
                throw;
            }
        }

    }
}
