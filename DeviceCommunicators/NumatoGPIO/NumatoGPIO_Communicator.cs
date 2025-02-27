using Communication.Interfaces;
using Communication.Services;
using Services.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using System;
using System.Windows;
using System.Runtime.ConstrainedExecution;
using DeviceCommunicators.ZimmerPowerMeter;
using System.Threading;
using System.Windows.Shapes;

namespace DeviceCommunicators.NumatoGPIO
{
    public class NumatoGPIO_Communicator: DeviceCommunicator
    {

        #region Properties


        private ISerialService SerialService
        {
            get => CommService as ISerialService;
        }


        #endregion Properties

        public NumatoGPIO_Communicator(LogLineListService logLineList) :
			base(logLineList)
		{

        }

        public void Init(
            bool isUdpSimulation,
            string comName,
            int baudtate,
            int rxPort = 0,
            int txPort = 0,
            string address = "")
        {
            try
            {
                if (isUdpSimulation)
                    CommService = new SerialUdpSimulationService(rxPort, txPort, address);
                else
                    CommService = new SerialService(comName, baudtate);

                CommService.Init(false);


                InitBase();
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to init", ex);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
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
                    data.Value,
                    data.Callback);
            }

            return CommunicatorResultEnum.OK;
        }

        private void SetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is NumatoGPIO_ParamData paramData))
                    return;

                if (value > 1)
                {
                    MessageBox.Show("Gpio set value must be 0 or 1");
                    return;
                }
                if (value == 0)
                {
                    paramData.Cmd = "gpio clear";
                }

                string cmd = paramData.Cmd + " " + paramData.Io_port;
                SerialService.Send(cmd, true);
                paramData.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Send);


                callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }
            catch (Exception ex)
            {
                param.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Send, "Failed to set value for parameter: " + ex);
                LoggerService.Error(this, "Failed to set value for parameter: " + param.Name, ex);
            }
        }

        private void GetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is NumatoGPIO_ParamData paramData))
                    return;

                //SerialService.ClearBuffer();

                string cmd;
                if(paramData.Cmd != "ver")
                {
                    cmd = paramData.Cmd + " " + paramData.Io_port;
                }
                else
                {
                    cmd = paramData.Cmd;
                }

                SerialService.Send(cmd, true);
                paramData.UpdateSendResLog(cmd, DeviceParameterData.SendOrRecieve.Send);

                Thread.Sleep(500);

                //SerialService.ClearBuffer();

                string received;
                SerialService.Read(out received);
                if (string.IsNullOrEmpty(received))
                {
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                    paramData.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.NoResponse.ToString());
                    return;
                }

                if (paramData.Cmd == "ver")
                {
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                    return;
                }

                string response = received.Trim();

                double d;
                bool isParseOk = double.TryParse(response, out d);

                if (isParseOk)
                {
                    if (paramData.Cmd.Contains("adc"))
                    {
                        double convertedAnalog = ConvertAdcToVoltage((int)d);
                        response = convertedAnalog.ToString();
                    }
                }

                param.Value = response;

                if (!isParseOk)
                {
                    callback?.Invoke(param, CommunicatorResultEnum.Error, "Received: " + received);
                    paramData.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Recieve, CommunicatorResultEnum.ReceiveParsingError.ToString());
                }
                    
                else
                {
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                }
                    
            }
            catch (Exception ex)
            {
                param.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Recieve, "Failed to get value for parameter: " + ex);
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }

        private double ConvertAdcToVoltage(int adcValue)
        {
            const double maxAdcValue = 1023.0;
            const double referenceVoltage = 5.0;
            double voltage = (adcValue / maxAdcValue) * referenceVoltage;
            //double deviation = GetDeviationFromLut(voltage);
            //voltage = voltage - deviation;
            return voltage;
        }
    }
}
