using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using Services.Services;
using System;
using System.Threading;
using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;

namespace DeviceCommunicators.NI_6002
{
	public class NI6002_Communicator : DeviceCommunicator, IDisposable
	{

		#region Fields and Properties



		public INiCommands _commmand_to_device;



		private bool _isInitialized;
		public override bool IsInitialized => _isInitialized;
		//private double Minimum_voltage_throttle = 1.05;
		//private double Maximum_voltage_throttle = 4.45;

		#endregion Fields and Properties

		#region Constructor

		public NI6002_Communicator()
		{
			_isInitialized = false;
		}

		#endregion Constructor

		#region Methods

		public void Init(
			string device_name,
			bool simulation = false)
		{

			try
			{
				if (!simulation)
					_commmand_to_device = new NI6002_Command(device_name);
				else
					_commmand_to_device = new NI6002_CommandSimulation();


				InitBase();
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to init the NI", ex);
			}


			_isInitialized = true;
		}

		public override void Dispose()
		{
			base.Dispose();

			_isInitialized = false;
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
				if (!(param is NI6002_ParamData niParamData))
					return;
				//need add to send data
				bool res = (bool)Send_command(niParamData);

				if (res)
					callback?.Invoke(param, CommunicatorResultEnum.OK, null);
				else
					callback?.Invoke(param, CommunicatorResultEnum.Error, null);
			}

			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set Command for Ni" + param.Name, ex);
			}
		}


		public void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				if (!(param is NI6002_ParamData niParamData))
					return;

				// need change to send data
				string message = (string)Send_command(niParamData);
				if (string.IsNullOrEmpty(message) || message == "Error")
				{
					callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
					return;
				}


				Thread.Sleep(10);


				param.Value = message;
				callback?.Invoke(param, CommunicatorResultEnum.OK, null);

			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
			}
		}



		public object Send_command(NI6002_ParamData niParamData)
		{
			if (niParamData == null)
				return null;

			int port = niParamData.Io_port;
			int line = niParamData.portLine;

            object data;

			bool isPass = HandleVandVCommands(niParamData);


			string cmd = niParamData.command_to_device.ToLower();


			double value = 0;
			switch (cmd)
            {
                case "digital input":
                    data = _commmand_to_device.DigitalIO_input(port , line);
                    break;
                case  "digital output":
                    _commmand_to_device.DigitalIO_output(port,line, Convert.ToBoolean(niParamData.Value) ? Convert.ToInt32(Math.Pow(2, line)) : 0 );
                    data = true;
                    break;
                case "analog input":
                    data = _commmand_to_device.Anolog_input(port);
                    break;
                case "analog output":
                    value = Convert.ToDouble(niParamData.Value);
                    _commmand_to_device.Anolog_output(port, value);
                    data = true;
                    break;
                case "analog input current":
                    data = _commmand_to_device.Anolog_input_current(port, niParamData.shunt_resistor);
                    break;
                case "digital counter":
                    data = _commmand_to_device.Digital_Counter();
                    break;


				default:
					return null;
			}

			return data;
		}

		private bool HandleVandVCommands(NI6002_ParamData niParamData)
		{
			if (niParamData.command_to_device.Contains("Digital Port") &&
				niParamData.command_to_device.Contains("port output"))
			{
				string[] split = niParamData.command_to_device.Split(' ');

				int port;
				bool res = int.TryParse(split[2], out port);
				if (!res)
					return false;
				niParamData.Io_port = port;

				int line;
				res = int.TryParse(split[5], out line);
				if (!res)
					return false;
				niParamData.portLine = line;

				niParamData.command_to_device = "digital output";
			}

			else if (niParamData.command_to_device.Contains("Port") &&
				niParamData.command_to_device.Contains("digital input"))
			{
				string[] split = niParamData.command_to_device.Split(' ');

				int port;
				bool res = int.TryParse(split[1], out port);
				if (!res)
					return false;
				niParamData.Io_port = port;

				int line;
				res = int.TryParse(split[4], out line);
				if (!res)
					return false;
				niParamData.portLine = line;

				niParamData.command_to_device = "digital input";
			}

			else if (niParamData.command_to_device.Contains("Analog port output"))
			{
				string[] split = niParamData.command_to_device.Split(' ');

				int port;
				bool res = int.TryParse(split[3], out port);
				if (!res)
					return false;
				niParamData.Io_port = port;

				niParamData.command_to_device = "analog output";
			}

			else if (niParamData.command_to_device.Contains("Read Anolog input"))
			{
				string[] split = niParamData.command_to_device.Split(' ');

				int port;
				bool res = int.TryParse(split[3], out port);
				if (!res)
					return false;
				niParamData.Io_port = port;

				niParamData.command_to_device = "analog input";
			}

			return true;
		}

		public string Read_command(NI6002_ParamData niParamData)
		{
			if (niParamData == null)
				return "";

			string data = "";


			//try
			//{

			//    if (niParamData.command_to_device.ToLower() == ("Port 0 digital input 0").ToLower())
			//    {
			//        data=port_0_to_digitalal_out(0,niParamData);

			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 0 digital input 1").ToLower())
			//    {
			//        data = port_0_to_digitalal_out(1, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 0 digital input 2").ToLower())
			//    {
			//        data = port_0_to_digitalal_out(2, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 0 digital input 3").ToLower())
			//    {
			//        data = port_0_to_digitalal_out(3, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 0 digital input 4").ToLower())
			//    {
			//        data =port_0_to_digitalal_out(4, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 0 digital input 5").ToLower())
			//    {
			//        _commmand_to_device._Port_Io = "port0";
			//        data = port_0_to_digitalal_out(5, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 0 digital input 6").ToLower())
			//    {
			//        data = port_0_to_digitalal_out(6, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 0 digital input 7").ToLower())
			//    {
			//        data = port_0_to_digitalal_out(7, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 1 digital input 0").ToLower())
			//    {
			//        data = port_1_to_digitalal_out(0, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 1 digital input 1").ToLower())
			//    {
			//        data = port_1_to_digitalal_out(1, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 1 digital input 2").ToLower())
			//    {
			//        data = port_1_to_digitalal_out(2, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 1 digital input 3").ToLower())
			//    {
			//        data = port_1_to_digitalal_out(3, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Port 2 digital input 0").ToLower())
			//    {
			//        data = port_2_to_digitalal_out(0, niParamData);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input 0").ToLower())
			//    {
			//        niParamData.Io_port = 0;
			//        data = _commmand_to_device.Anolog_input((IO_Pin)niParamData.Io_port);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input 1").ToLower())
			//    {
			//        niParamData.Io_port = 1;
			//        data = _commmand_to_device.Anolog_input((IO_Pin)niParamData.Io_port);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input 2").ToLower())
			//    {
			//        niParamData.Io_port = 2;
			//        data = _commmand_to_device.Anolog_input((IO_Pin)niParamData.Io_port);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input 3").ToLower())
			//    {
			//        niParamData.Io_port = 3;
			//        data = _commmand_to_device.Anolog_input((IO_Pin)niParamData.Io_port);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input 4").ToLower())
			//    {
			//        niParamData.Io_port = 4;
			//        data = _commmand_to_device.Anolog_input((IO_Pin)niParamData.Io_port);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input 5").ToLower())
			//    {
			//        niParamData.Io_port = 5;
			//        data = _commmand_to_device.Anolog_input((IO_Pin)niParamData.Io_port);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input 6").ToLower())
			//    {
			//        niParamData.Io_port = 6;
			//        data = _commmand_to_device.Anolog_input((IO_Pin)niParamData.Io_port);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input 7").ToLower())
			//    {
			//        niParamData.Io_port = 7;
			//        data = _commmand_to_device.Anolog_input((IO_Pin)niParamData.Io_port);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Anolog input current").ToLower())
			//    {
			//        data = _commmand_to_device.Anolog_input_current((IO_Pin)niParamData.Io_port, niParamData.shunt_resistor);
			//    }
			//    else if (niParamData.command_to_device.ToLower() == ("Read Digital counter").ToLower())
			//    {
			//        data = _commmand_to_device.Digital_Counter();
			//    }

			//}
			//catch(Exception ex)
			//{
			//    LoggerService.Error(this, "Failed to get data", ex);
			//}

			return data;
		}


		//private string port_0_to_digitalal_out(int number_port, NI6002_ParamData niParamData)
		//{
		//    string data = "";
		//    string curent_port = "";
		//    curent_port = _commmand_to_device._Port_Io;
		//    _commmand_to_device._Port_Io = "port0";
		//    niParamData.Io_port = number_port;
		//     data= _commmand_to_device.DigitalIO_input((IO_Pin)niParamData.Io_port);
		//    _commmand_to_device._Port_Io = curent_port;
		//    return data;
		//}

		//private string  port_1_to_digitalal_out(int number_port, NI6002_ParamData niParamData)
		//{
		//    string data = "";
		//    string curent_port = "";
		//    curent_port = _commmand_to_device._Port_Io;
		//    _commmand_to_device._Port_Io = "port1";
		//    niParamData.Io_port = number_port;
		//    data = _commmand_to_device.DigitalIO_input((IO_Pin)niParamData.Io_port);
		//    _commmand_to_device._Port_Io = curent_port;
		//    return data;
		//}

		//private string port_2_to_digitalal_out(int number_port, NI6002_ParamData niParamData)
		//{
		//    string data = "";
		//    string curent_port = "";
		//    curent_port = _commmand_to_device._Port_Io;
		//    _commmand_to_device._Port_Io = "port2";
		//    niParamData.Io_port = number_port;
		//    data = _commmand_to_device.DigitalIO_input((IO_Pin)niParamData.Io_port);
		//    _commmand_to_device._Port_Io = curent_port;
		//    return data;
		//}




		public override bool Equals(object obj)
		{
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion Methods
	}
}

