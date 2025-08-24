using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using DeviceCommunicators.RigolM300;
using NationalInstruments.DataInfrastructure;
using Services.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace DeviceCommunicators.RigolM300
{
    public class RigolM300_Communicator : DeviceCommunicator, IDisposable
    {

        #region Fields

        private string _ipAddres;
        private int _port;


        #endregion Fields

        #region Properties

        private ITcpStaticService TCPCommService
        {
            get => CommService as TcpStaticService;
        }

        #endregion Properties

        #region Constructor

        public RigolM300_Communicator(LogLineListService logLineList) :
            base(logLineList)
        {

        }

        #endregion Constructor

        #region Methods

        public void Init(   
            string ipAddres,
            int port,
            int rxPort,
            int txPort)
        {

            try
            {
                _ipAddres = ipAddres;
                _port = port;

                CommService = new TcpStaticService(_ipAddres, _port);

                CommService.Init(false);



                InitBase();
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to init the RigolM300", ex);
            }


        }


        protected override CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
        {

            if (data.IsSet)
            {
                SetParamValue_Do(
                    data.Parameter,
                    data.Value,
                    data.Callback);
            }
            else
            {
                GetParamValue_Do(
                    data.Parameter,
                    data.Callback);
            }

            return CommunicatorResultEnum.OK;

        }


        public void SetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is RigolM300_ParamData rigolparam))
                    return;


                string cmd = rigolparam.Cmd;
                string fullCommand = $"{cmd}";

                if (rigolparam.Cmd == "ROUT:")
                    if (value == 1)
                        fullCommand += "OPEN ";
                    else
                        fullCommand += "CLOS ";
                else if (rigolparam.Cmd.Contains("CONF") && value == 0)
                {
                    fullCommand += " " + rigolparam.Range.ToString();
                    fullCommand += ",DEF,";
                }
                else if (!rigolparam.Cmd.Contains("SCAN"))
                    fullCommand += " " + value.ToString();

                if (rigolparam.Slot.HasValue && rigolparam.Channel.HasValue)
                {
                    int channelRef = rigolparam.Slot.Value * 100 + rigolparam.Channel.Value;
                    //if (cmd.Contains("CONF"))
                        //fullCommand += ",DEF,";
                    fullCommand += $"(@{channelRef})";
                }
                TCPCommService.Send(fullCommand + "\n");
                rigolparam.UpdateSendResLog(fullCommand, DeviceParameterData.SendOrRecieve.Send);


                //TCPCommService.Send("SYST:ERR?\n");
                //DateTime startTime = DateTime.Now;
                //string errResponse = null;
                //while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(200))
                //{
                //    TCPCommService.Read(out errResponse);
                //    if (!string.IsNullOrEmpty(errResponse))
                //        break;

                //    System.Threading.Thread.Sleep(1);
                //}

                //if (!string.IsNullOrEmpty(errResponse) && !errResponse.Trim().StartsWith("+0"))
                //{
                //    rigolparam.UpdateSendResLog(fullCommand, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.Error.ToString());
                //    callback?.Invoke(param, CommunicatorResultEnum.Error, null);
                //    return;
                //}
                if (rigolparam.Cmd == "ROUT:")
                {
                    var started = DateTime.UtcNow;
                    string resp;
                    TCPCommService.Send("ROUT:DONE?");  // returns "1" when done

                    resp = ReadUntilComplete(800);

                    if (resp?.Trim(new char[] { '\0','\n' }) != "1")
                    {
                        callback?.Invoke(param, CommunicatorResultEnum.Error, null);
                        return;
                    }
                }

                callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }

            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to set Command for RigolM300" + param.Name, ex);
            }
        }


        public void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is RigolM300_ParamData rigolparam))
                    return;

                string cmd = rigolparam.Cmd.Trim();
                if (!cmd.EndsWith("?"))
                    cmd += "?";

                string queryCmd = $"{cmd}";

                if (queryCmd != "READ?")
                {
                    if (rigolparam.Range.HasValue && rigolparam.Range != 0)
                        queryCmd += $" {rigolparam.Range},DEF,";

                    if (rigolparam.Slot.HasValue && rigolparam.Channel.HasValue)
                    {
                        int channelRef = rigolparam.Slot.Value * 100 + rigolparam.Channel.Value;
                        if (cmd.Contains("FREQ"))
                            queryCmd += " ";
                        queryCmd += $"(@{channelRef})";
                    }
                }
                

                FlushRemaining();
                string response = null;
                DateTime startTime = DateTime.Now;
                for (int i = 0; i < 5; i++)
                {

                    TCPCommService.Send(queryCmd + "\n");

                    if (queryCmd.Contains("FREQ"))
                        response = ReadUntilComplete(10000);
                    else if(queryCmd.Contains("RES"))
                        response = ReadUntilComplete(2000);
                    else if(queryCmd.Contains("AC"))
                        response = ReadUntilComplete(2000);
                    else
                        response = ReadUntilComplete(3000);

                    //while ((DateTime.Now - startTime) < TimeSpan.FromMilliseconds(1000))
                    //{
                    //    TCPCommService.Read(out response);
                    //    if (!string.IsNullOrEmpty(response))
                    //        break;

                    //    System.Threading.Thread.Sleep(1);
                    //}

                    if (!string.IsNullOrEmpty(response))
                        break;

                    System.Threading.Thread.Sleep(1);
                }

                
                if (string.IsNullOrEmpty(response))
                {
                    rigolparam.UpdateSendResLog(queryCmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.NoResponse.ToString());
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                    return;
                }

                response = response.Trim(new char[] { '\0' });
                string[] parts = response.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string value = parts.Last();

                if (cmd == "*IDN?")
                {
                    if (response.Contains("RIGOL"))
                    {
                        param.Value = response;
                    }
                    else
                    {
                        rigolparam.UpdateSendResLog(queryCmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.Error.ToString());
                        callback?.Invoke(param, CommunicatorResultEnum.Error, "Device is not Rigol M300");
                        return;
                    }
                }

                if (queryCmd.Contains("VOLT") ||
                    queryCmd.Contains("CURR") ||
                    queryCmd.Contains("RES")  ||
                    queryCmd.Contains("FREQ") ||
                    queryCmd.Equals("READ?"))
                {
                    double d;
                    bool res = double.TryParse(value, out d);
                    if (res == false)
                    {
                        rigolparam.UpdateSendResLog(queryCmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.NoResponse.ToString());
                        callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                        return;
                    }

                    param.Value = d;
                }
                else
                    param.Value = response;

                rigolparam.UpdateSendResLog(queryCmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.OK.ToString());

                callback?.Invoke(param, CommunicatorResultEnum.OK, null);

            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }

        string ReadUntilComplete(int timeoutMs = 1000)
        {
            StringBuilder sb = new StringBuilder();
            DateTime start = DateTime.Now;
            string chunk;

            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                TCPCommService.Read(out chunk);

                if (!string.IsNullOrEmpty(chunk))
                {
                    sb.Append(chunk);

                    //// If the device ends with newline, this tells us it's done
                    if (chunk.Contains("\n"))
                        break;
                }

                Thread.Sleep(1); // Don’t hog CPU
            }

            TimeSpan end = DateTime.Now - start;
            return sb.ToString();
        }

        void FlushRemaining()
        {
            //DateTime start = DateTime.Now;

            //while ((DateTime.Now - start).TotalMilliseconds < 100)
            //{
            //    TCPCommService.Read(out string chunk);
            //    if (string.IsNullOrEmpty(chunk))
            //        break;
            //}
            TCPCommService.Read(out _);
        }

        #endregion Methods
    }
}
