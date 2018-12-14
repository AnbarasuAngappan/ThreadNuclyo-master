using FieldTalk.Modbus.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadNuclyo
{
    class ModBus
    {
        private static MbusMasterFunctions myProtocol;
        private int retryCnt;
        private int pollDelay;
        private int timeOut;
        private int tcpPort;
        private int res;

        public ModBus()
        {
            
        }

        public bool OpenProtocol(string _ipAddress, int _port, string _protocalType)
        {
            try
            {
                ///First we must instantiate class if we haven't done so already
                if (myProtocol == null)
                {
                    try
                    {
                        if (_protocalType == "TCP")
                            myProtocol = new MbusTcpMasterProtocol();
                        else
                            myProtocol = new MbusRtuOverTcpMasterProtocol();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else// already instantiated, close protocol and reinstantiate
                {
                    if (myProtocol.isOpen())
                        myProtocol.closeProtocol();
                    myProtocol = null;

                    try
                    {
                        if (_protocalType == "")
                            myProtocol = new MbusTcpMasterProtocol();
                        else
                            myProtocol = new MbusRtuOverTcpMasterProtocol();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }


                try
                {
                    retryCnt = int.Parse("");
                }
                catch (Exception)
                {
                    retryCnt = 0;
                }
                try
                {
                    pollDelay = int.Parse("");
                }
                catch (Exception)
                {
                    pollDelay = 0;
                }
                try
                {
                    timeOut = int.Parse("1000");
                }
                catch (Exception)
                {
                    timeOut = 1000;
                }

                myProtocol.timeout = timeOut;
                myProtocol.retryCnt = retryCnt;
                myProtocol.pollDelay = pollDelay;

                try
                {
                    tcpPort = _port;
                    if (_protocalType == "TCP")
                    {
                        ((MbusTcpMasterProtocol)myProtocol).port = (short)tcpPort;
                        res = ((MbusTcpMasterProtocol)myProtocol).openProtocol(_ipAddress);
                        if ((res == BusProtocolErrors.FTALK_SUCCESS))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        ((MbusRtuOverTcpMasterProtocol)myProtocol).port = (short)tcpPort;
                        res = ((MbusRtuOverTcpMasterProtocol)myProtocol).openProtocol(_ipAddress);
                        if ((res == BusProtocolErrors.FTALK_SUCCESS))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public short[] ReadHoldingregister(string _slaveAddress, string _strAddress, string _noReadRegister)
        {
            int slave;
            int startRdReg;
            short[] readVals = new short[125];
            int numRdRegs;
            try
            {
                slave = int.Parse(_slaveAddress);
            }
            catch (Exception)
            {
                slave = 1;
            }

            try
            {
                startRdReg = int.Parse(_strAddress);
            }
            catch (Exception)
            {
                startRdReg = 1;
            }

            try
            {
                numRdRegs = int.Parse(_noReadRegister);
            }
            catch (Exception)
            {
                numRdRegs = 1;
            }

            try
            {
                res = myProtocol.readMultipleRegisters(slave, startRdReg, readVals, numRdRegs);
                if (readVals != null)
                {
                    return readVals;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
