using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using NationalInstruments.DataInfrastructure;
using Services.Services;
using System;
using System.Linq;

namespace DeviceCommunicators.IT_M3100
{
    public class IT_M3100_Communicator : DeviceCommunicator, IDisposable
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

        public IT_M3100_Communicator(LogLineListService logLineList) :
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
                if (!(param is IT_M3100_ParamData m3100param))
                    return;


                string cmd = m3100param.Cmd;
                string fullCommand = $"{cmd}";



                TCPCommService.Send(fullCommand + "\n");
                m3100param.UpdateSendResLog(fullCommand, DeviceParameterData.SendOrRecieve.Send);


                TCPCommService.Send("SYST:ERR?\n");
                DateTime startTime = DateTime.Now;
                string errResponse = null;
                while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(200))
                {
                    TCPCommService.Read(out errResponse);
                    if (!string.IsNullOrEmpty(errResponse))
                        break;

                    System.Threading.Thread.Sleep(1);
                }

                if (string.IsNullOrEmpty(errResponse))
                {
                    m3100param.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.NoResponse.ToString());
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                    return;
                }

                errResponse = errResponse.Trim(new char[] { '\0' });
                string[] parts = errResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string errValue = parts.Last();
                parts = errValue.Split(',');
                

                if (!string.IsNullOrEmpty(errValue) && (int.TryParse(parts[0], out int errCode) && errCode !=0))
                {
                    m3100param.UpdateSendResLog(fullCommand, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.Error.ToString());
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
                if (!(param is IT_M3100_ParamData m3100param))
                    return;


                string cmd = m3100param.Cmd.Trim();

                if (!cmd.EndsWith("?"))
                    cmd += "?";


                string response = null;
                DateTime startTime = DateTime.Now;
                for (int i = 0; i < 5; i++)
                {

                    TCPCommService.Send(cmd + "\n");

                    while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(100))
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
                    m3100param.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.NoResponse.ToString());
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                    return;
                }

                response = response.Trim(new char[] { '\0', '\n' });

                param.Value = response;
                if (cmd == "*IDN?")
                {
                    if (response.Contains("ITECH"))
                    {
                        param.Value = response;
                    }
                    else
                    {
                        m3100param.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.Error.ToString());
                        callback?.Invoke(param, CommunicatorResultEnum.Error, null);
                        return;
                    }
                }

                if (m3100param.Cmd == "V" ||
                    m3100param.Cmd == "I" ||
                    m3100param.Cmd == "OCP")
                {
                    double d;
                    bool res = double.TryParse(response, out d);
                    if (res == false)
                    {
                        m3100param.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.NoResponse.ToString());
                        callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                        return;
                    }

                    param.Value = d;
                }
                else
                    param.Value = response;

                m3100param.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.OK.ToString());

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
