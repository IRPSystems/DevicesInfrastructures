using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Services.Services;
using System;
using System.Threading;

namespace DeviceCommunicators.PowerSupplayBK
{
    public class PowerSupplayBK_Communicator: DeviceCommunicator
	{
		#region Fields

		
		//private string _name_comport;
		//private int _boud_rate;

		#endregion Fields


		#region Properties

		private ISerialService _serial_port
        {
            get => CommService as ISerialService;
        }


		#endregion Properties

		#region Constructor

		public PowerSupplayBK_Communicator(LogLineListService logLineList) :
			base(logLineList)
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
			//_name_comport = comName;
			//_boud_rate = baudtate;

			try
			{
				if (isUdpSimulation)
					CommService = new SerialUdpSimulationService(rxPort, txPort, address);
				else
					CommService = new SerialService(comName, baudtate);

				_serial_port.Init(false);

				if (_serial_port.IsInitialized)
					_serial_port.Send("SYST:REM\n");



				InitBase();
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to init", ex);
			}

		}

		public override void Dispose()
		{
			try
			{
				if (_serial_port != null)
				{
					if (_serial_port.IsInitialized)
						_serial_port.Send("SYST:LOC\n");
				}


				base.Dispose();

			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to dispose", ex);
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
                if (!(param is PowerSupplayBK_ParamData supplay_Parameter))
                    return;

                send_comand_to_supply(supplay_Parameter, value);
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
                if (!(param is PowerSupplayBK_ParamData supplay_Parameter))
                    return;

				request_commad_from_supply(supplay_Parameter);

                Thread.Sleep(100);


                param.Value = receive_value_from_supply(supplay_Parameter.Name);
				LoggerService.Inforamtion(this, $"{supplay_Parameter.Name} - {param.Value}");

				if (param.Value == null)
                	callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
				else
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }
            catch(Exception ex) 
            { 
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
		}

		



		private void send_comand_to_supply(
			PowerSupplayBK_ParamData supplay_Parameter, 
			double value)
        {
            if (_serial_port == null)
                return;

            if (supplay_Parameter.Name == "Remote command")
            {
                if (value == 1)
                {
					_serial_port.Send("SYST:REM");
                }
                else if (value == 0)
                {
					_serial_port.Send("SYST:LOC");
                }
            }
            else if (supplay_Parameter.Name == "Choose channel")
            {
				string cmd = $"{supplay_Parameter.Command} CH{value}";
				_serial_port.Send(cmd);
			}
            else if (supplay_Parameter.Name == "Turn ON channel")
            {
				string cmd = supplay_Parameter.Command;
				
				if (value == 0)
                {
					cmd += " OFF";
                }
                else if (value == 1)
                {
					cmd += " ON";
				}

				_serial_port.Send(cmd);
			}
            else
            {
				string cmd = $"{supplay_Parameter.Command} {value}";
				_serial_port.Send(cmd);
            }
        }

		private void request_commad_from_supply(PowerSupplayBK_ParamData supplay_Parameter)
        {
            if (_serial_port == null)
                return;

            LoggerService.Inforamtion(this, supplay_Parameter.Name);

			string cmd = $"{supplay_Parameter.Command}?";

            _serial_port.Send(cmd);
        }

		private string receive_value_from_supply(string name)
		{
            if(_serial_port == null)
                return null;

			string internal_buffer;

			_serial_port.Read(out internal_buffer);

            if (string.IsNullOrEmpty(internal_buffer))
                return null;


			return internal_buffer;
		}

		#endregion Methods

	}
}
