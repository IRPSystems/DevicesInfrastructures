using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;

namespace DeviceCommunicators.PowerSupplayGK
{
    public class PowerSupplayGK_Communicator: DeviceCommunicator
	{
		#region Fields


        public string Name;

		private string _ipAdderss;
		private ushort _port;

		#endregion Fields


		#region Properties


		private ModbusTCPSevice ModbusTCP
		{
			get => CommService as ModbusTCPSevice;
		}


		#endregion Properties

		#region Constructor

		public PowerSupplayGK_Communicator()
        {
		}

		#endregion Constructor

		#region Methods

		public void Init(
			bool isUdpSimulation,
			string ipAdderss,
			ushort port)
        {
			_ipAdderss = ipAdderss;
			_port = port;


			if (isUdpSimulation)
			{

			}
            else
			    CommService = new ModbusTCPSevice(_ipAdderss, _port, 255);

			CommService.Init(false);

			InitBase();
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

		private void SetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				if (!(param is PowerSupplayGK_ParamData gk_ParamData))
					return;

				double val = Convert.ToUInt16(gk_ParamData.Value);
				val /= gk_ParamData.Scale;
				byte[] buffer = BitConverter.GetBytes((ushort)val);



				ModbusTCP.WriteSingleRegister(7, 255, gk_ParamData.WriteAddress, buffer);
				ModbusTCP.WriteSingleRegister(7, 255, gk_ParamData.WriteTriggerAddress, buffer); // trigger

				callback?.Invoke(param, CommunicatorResultEnum.OK, "");


			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set value for parameter: " + param.Name, ex);
				callback?.Invoke(param, CommunicatorResultEnum.Error, "Exception when sending");
			}
		}


        private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
            try
            {
				if (!(param is PowerSupplayGK_ParamData gk_ParamData))
					return;

				byte[] buffer = null;
				ModbusTCP.ReadInputRegister(4, 255, gk_ParamData.ReadAddress, 1, ref buffer);

				if(buffer == null || buffer.Length < 2)
				{
					callback?.Invoke(param, CommunicatorResultEnum.Error, "Exception when sending");
					return;
				}

				double val = BitConverter.ToUInt16(buffer, 0);
				param.Value = val * gk_ParamData.Scale;

				callback?.Invoke(param, CommunicatorResultEnum.OK, "");

			}
            catch(Exception ex) 
            { 
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
				callback?.Invoke(param, CommunicatorResultEnum.Error, "Exception when sending");
			}
		}

		#endregion Methods

	}
}
