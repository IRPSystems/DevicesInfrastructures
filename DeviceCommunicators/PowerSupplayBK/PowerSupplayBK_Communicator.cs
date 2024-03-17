using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Entities.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
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

		private ISerialService SerialService
        {
            get => CommService as ISerialService;
        }


		#endregion Properties

		#region Constructor

		public PowerSupplayBK_Communicator()
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

            if(isUdpSimulation)
				CommService = new SerialUdpSimulationService(rxPort, txPort, address);
            else
				CommService = new SerialService(comName, baudtate);
            
            SerialService.Init(false);

            if (SerialService.IsInitialized)
            {
                lock (_lockObj)
                {
                    SerialService.Send("SYST:REM\n");
                }
            }



			InitBase();

		}

		public override void Dispose()
		{
            if (SerialService != null)
            {
                if (SerialService.IsInitialized)
                {
                    lock (_lockObj)
                    {
                        SerialService.Send("SYST:LOC\n");
                    }
                }
            }

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

                send_comand_to_supply(supplay_Parameter.Name, value);
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

                request_commad_from_supply(supplay_Parameter.Name);

                Thread.Sleep(10);


                param.Value = receive_value_from_supply(supplay_Parameter.Name);

                if(param.Value == null)
                	callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
				else
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }
            catch(Exception ex) 
            { 
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
		}

		



		private void send_comand_to_supply(string command, double value)
        {
            if (SerialService == null)
                return;

            lock (_lockObj)
            {

                if (command == "Remote command")
                {
                    if (value == 1)
                    {
                        SerialService.Send("SYST:REM");
                    }
                    else if (value == 0)
                    {
                        SerialService.Send("SYST:LOC");
                    }
                }
                else if (command == "Choose channel")
                {
                    if (value == 1)
                    {
                        SerialService.Send("INST CH1");
                    }
                    else if (value == 2)
                    {
                        SerialService.Send("INST CH2");
                    }
                    else if (value == 3)
                    {
                        SerialService.Send("INST CH3");
                    }
                }
                else if (command == "Turn ON channel")
                {
                    if (value == 0)
                    {
                        SerialService.Send("CHANnel:OUTPut OFF");
                    }
                    else if (value == 1)
                    {
                        SerialService.Send("CHANnel:OUTPut ON");
                    }
                }
                else if (command == "Voltage")
                {
                    SerialService.Send("VOLTage " + value);
                }
                else if (command == "Current")
                {
                    SerialService.Send("Current " + value);
                }
            }
        }

		private void request_commad_from_supply(string name)
        {
            if (SerialService == null)
                return;

            lock (_lockObj)
            {

                if (name == "MEASure voltage in supply")
                {
                    SerialService.Send("MEASure:SCALar:VOLTage:ALL:DC?");
                }
                if (name == "Voltage in supply")
                {
                    SerialService.Send("VAPPLY:VOLTage:LEVel?");
                }
                if (name == "Current status")
                {
                    SerialService.Send("APPLY:OUTput?");
                }
                else if (name == "Current value")
                {
                    SerialService.Send("APPLY:CURRent:LEVel?");
                }
            }
        }

		private string receive_value_from_supply(string name)
		{
            if(SerialService == null)
                return null;

			string internal_buffer;

            lock (_lockObj)
            {
                SerialService.Read(out internal_buffer);
            }

            if (string.IsNullOrEmpty(internal_buffer))
                return null;


			return internal_buffer;
		}

		#endregion Methods

	}
}
