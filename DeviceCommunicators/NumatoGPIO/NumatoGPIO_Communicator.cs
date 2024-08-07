﻿using Communication.Interfaces;
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

                string cmd = paramData.Cmd + " " + value;
                SerialService.Send(cmd);
            

                callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to set value for parameter: " + param.Name, ex);
            }
        }

        private void GetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is NumatoGPIO_ParamData paramData))
                    return;

                SerialService.ClearBuffer();

                string cmd = paramData.Cmd + " " + value;

                SerialService.Send(cmd);

                Thread.Sleep(500);


                string received;
                SerialService.Read(out received);
                if (string.IsNullOrEmpty(received))
                {
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                    return;
                }

                if (paramData.Cmd == "ver")
                {
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                    return;
                }

                double d;
                bool res = double.TryParse(received, out d);
                param.Value = d;

                if (!res)
                    callback?.Invoke(param, CommunicatorResultEnum.Error, "Received: " + received);
                else
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }
    }
}
