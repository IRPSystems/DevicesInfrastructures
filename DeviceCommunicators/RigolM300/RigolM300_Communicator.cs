using Communication.Services;
using DeviceCommunicators.General;
using Services.Services;
using System;
using DeviceCommunicators.Enums;
using DeviceCommunicators.Models;
using Communication.Interfaces;
using DeviceCommunicators.RigolM300;
using NationalInstruments.DataInfrastructure;

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


        public void SetParamValue_Do(DeviceParameterData param, double? value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is RigolM300_ParamData rigolparam))
                    return;


                string cmd = rigolparam.Cmd;
                string fullCommand = string.Empty;

                if (cmd == "SYSTem:COMMunicate:RLSTate")
                {
                    if (value == 0)
                        fullCommand = $"{cmd} LOCal";
                    else if (value == 1)
                        fullCommand = $"{cmd} REMote";
                }
                else
                    fullCommand = $"{cmd} {value.ToString()}";

                if (rigolparam.Slot.HasValue && rigolparam.Channel.HasValue )
                {
                    int channelRef = rigolparam.Slot.Value * 100 + rigolparam.Channel.Value;
                    fullCommand += $"{value},DEF,(@{channelRef})";
                }
                else
                {
                    callback?.Invoke(param, CommunicatorResultEnum.Error, null);
                }
                TCPCommService.Send(cmd + "\n");

                rigolparam.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Send);

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
                if (!(param is RigolM300_ParamData rigolparam))
                    return;

                string cmd = $"{rigolparam.Cmd}?";

                string queryCmd;
                if (rigolparam.Slot.HasValue && rigolparam.Channel.HasValue)
                {
                    int channelRef = rigolparam.Slot.Value * 100 + rigolparam.Channel.Value;
                    queryCmd = $"{cmd}? (@{channelRef})";
                }
                else
                {
                    queryCmd = $"{cmd}?";
                }

                string response = null;
                DateTime startTime = DateTime.Now;
                for (int i = 0; i < 5; i++)
                {
                    
                    TCPCommService.Send(queryCmd + "\n");

                    while ((DateTime.Now - startTime) < TimeSpan.FromMilliseconds(50))
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
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                    return;
                }

                response = response.Trim(new char[] { '\0', '\n' });

                param.Value = response;
                if (cmd == "*IDN?")
                {
                    if (response.Contains("RIGOL"))
                    {
                        param.Value = response;
                    }
                    else
                    {
                        callback?.Invoke(param, CommunicatorResultEnum.Error, "Device is not Rigol M300");
                        return;
                    }
                }

                if (queryCmd.Contains("VOLT") ||
                    queryCmd.Contains("CURR"))
                {
                    double d;
                    bool res = double.TryParse(response, out d);
                    if (res == false)
                    {
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


        #endregion Methods
    }
}
