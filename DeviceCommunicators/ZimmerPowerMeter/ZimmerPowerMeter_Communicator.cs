using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Entities.Models;
using NationalInstruments.DataInfrastructure;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Xml.Linq;

namespace DeviceCommunicators.ZimmerPowerMeter
{
    public class ZimmerPowerMeter_Communicator: DeviceCommunicator
	{

		#region Properties

		private ISerialService SerialService
        {
            get => CommService as ISerialService;
        }


		#endregion Properties

		#region Constructor

		public ZimmerPowerMeter_Communicator()
		{
			
		}

		#endregion Constructor

		#region Methods

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
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to init", ex);
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


		private void SetParamValue_Do(
			DeviceParameterData param,
			double value,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{ 
            try
            {
                
				callback?.Invoke(param, CommunicatorResultEnum.OK, null);
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set value for parameter: " + param.Name, ex);
			}
		}

			

		private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{ 
            try
            {
                if (!(param is ZimmerPowerMeter_ParamData powerMeter))
                    return;

				if (powerMeter.Channel == 0)
					powerMeter.Channel = 1;

				SerialService.ClearBuffer();

				string cmd = powerMeter.Command;
				if (powerMeter.Command != "*IDN")
					cmd += powerMeter.Channel;
				cmd += "?\r\n";
				SerialService.Send(cmd);

				Thread.Sleep(500);


				string received;
				SerialService.Read(out received);
				if (string.IsNullOrEmpty(received))
				{
					callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
					return;
				}

				if (powerMeter.Command == "*IDN")
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
            catch(Exception ex) 
            { 
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
		}

		



		

		#endregion Methods

	}
}
