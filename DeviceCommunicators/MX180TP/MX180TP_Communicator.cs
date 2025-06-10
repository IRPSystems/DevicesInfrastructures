using Communication.Services;
using DeviceCommunicators.General;
using Services.Services;
using System;
using DeviceCommunicators.Enums;
using DeviceCommunicators.Models;
using Communication.Interfaces;
using NationalInstruments.DataInfrastructure;

namespace DeviceCommunicators.MX180TP
{
    public class MX180TP_Communicator : DeviceCommunicator, IDisposable
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

        public MX180TP_Communicator(LogLineListService logLineList) :
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
                LoggerService.Error(this, "Failed to init the ScopeKeySight", ex);
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
                if (!(param is MX180TP_ParamData mxparam))
                    return;


                string cmd = mxparam.Cmd;
                string fullCommand = $"{cmd}";


                if (mxparam.Channel.HasValue)
                {
                    int channelRef = mxparam.Channel.Value;
                    fullCommand += $"{channelRef}";
                }

                if (mxparam.HasValue)
                    fullCommand += " " + value.ToString();

                TCPCommService.Send(fullCommand + "\n");
                mxparam.UpdateSendResLog(fullCommand, DeviceParameterData.SendOrRecieve.Send);


                TCPCommService.Send("ERR?\n");
                TCPCommService.Read(out string errResponse);

                if (!string.IsNullOrEmpty(errResponse) && !errResponse.Trim().StartsWith("+0"))
                {
                    mxparam.UpdateSendResLog(fullCommand, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.Error.ToString());
                    callback?.Invoke(param, CommunicatorResultEnum.Error, null);
                    return;
                }


                callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }

            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to set Command for Switch relay" + param.Name, ex);
            }
        }


        public void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is MX180TP_ParamData mxparam))
                    return;


                string cmd = mxparam.Cmd.Trim();


                if (mxparam.Channel.HasValue)
                {
                    cmd.Replace("<N>", mxparam.Channel.ToString());
                }

                if (!cmd.EndsWith("?"))
                    cmd += "?";




                string response = null;
                DateTime startTime = DateTime.Now;
                for (int i = 0; i < 5; i++)
                {

                    TCPCommService.Send(cmd + "\n");

                    while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(50))
                    {
                        TCPCommService.Read(out response);
                        if (!string.IsNullOrEmpty(response))
                            break;

                        System.Threading.Thread.Sleep(1);
                    }

                    if (!string.IsNullOrEmpty(response))
                        break;

                    System.Threading.Thread.Sleep(1);
                }

                if (string.IsNullOrEmpty(response))
                {
                    mxparam.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.NoResponse.ToString());
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                    return;
                }

                response = response.Trim(new char[] { '\0', '\n' });

                param.Value = response;
                if (cmd == "*IDN?")
                {
                    if (response.Contains("TTI"))
                    {
                        param.Value = response;
                    }
                    else
                    {
                        mxparam.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.Error.ToString());
                        callback?.Invoke(param, CommunicatorResultEnum.Error, null);
                        return;
                    }
                }

                if( mxparam.Cmd == "V" ||
                    mxparam.Cmd == "I"  ||
                    mxparam.Cmd == "OCP")
                {
                    double d;
                    bool res = double.TryParse(response, out d);
                    if (res == false)
                    {
                        mxparam.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.NoResponse.ToString());
                        callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                        return;
                    }

                    param.Value = d;
                }
                else
                    param.Value = response;

                mxparam.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.OK.ToString());

                callback?.Invoke(param, CommunicatorResultEnum.OK, null);

            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }


        #endregion Methods
    }
}
