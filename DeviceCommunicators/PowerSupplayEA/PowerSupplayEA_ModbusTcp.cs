using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace DeviceCommunicators.PowerSupplayEA
{
    public class PowerSupplayEA_ModbusTcp : DeviceCommunicator
	{
		#region Fields

        public string Name;


		private AutoResetEvent _waitForResponse;
		private byte[] _data;

		private float _nominalVoltage;
		private float _nominalCurrent;
		private float _nominalPower;

		private const int _actualValuesFactor = 52428;

		#endregion Fields


		#region Properties


		private ModbusTCPSevice _modbusTCPSevice
		{
			get => CommService as ModbusTCPSevice;
		}


		#endregion Properties

		#region Constructor

		public PowerSupplayEA_ModbusTcp()
        {
			_waitForResponse = new AutoResetEvent(false);

		}

		#endregion Constructor

		#region Methods

		public void Init(
			bool isUdpSimulation,
			string address,
			DeviceData easpDevice)
        {
			CommService = new ModbusTCPSevice(address, 502, 1);
			CommService.MessageReceivedEvent += _modbusTCPSevice_MessageReceivedEvent;
			CommService.ErrorEvent += _modbusTCPSevice_ErrorEvent;
			CommService.Init(true);

			if (CommService.IsInitialized == false)
				return;

			GetNominals(easpDevice);

			InitBase();
		}

		public override void Dispose()
		{

			base.Dispose();
		}

		protected override CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
		{

			if (data.IsSet)
			{
				Set(data.Parameter);
			}
			else
			{
				Get(data.Parameter);
			}

			return CommunicatorResultEnum.OK;
		}

		private void GetNominals(DeviceData easpDevice)
		{

			PowerSupplayEA_ParamData nominalVoltageParam =
				easpDevice.ParemetersList.ToList().Find((p) => ((PowerSupplayEA_ParamData)p).Cmd == "SYSTem:NOMinal:VOLTage")
					as PowerSupplayEA_ParamData;
			Get(nominalVoltageParam);
			if (nominalVoltageParam.Value != null)
				_nominalVoltage = (float)nominalVoltageParam.Value;

			PowerSupplayEA_ParamData nominalCurrentParam =
				easpDevice.ParemetersList.ToList().Find((p) => ((PowerSupplayEA_ParamData)p).Cmd == "SYSTem:NOMinal:CURRent")
					as PowerSupplayEA_ParamData;
			Get(nominalCurrentParam);
			if (nominalCurrentParam.Value != null)
				_nominalCurrent = (float)nominalCurrentParam.Value;

			PowerSupplayEA_ParamData nominalPowerParam =
				easpDevice.ParemetersList.ToList().Find((p) => ((PowerSupplayEA_ParamData)p).Cmd == "SYSTem:NOMinal:POWer")
					as PowerSupplayEA_ParamData;
			Get(nominalPowerParam);
			if (nominalPowerParam.Value != null)
				_nominalPower = (float)nominalPowerParam.Value;
		}

		#region Set

		private void Set(DeviceParameterData e)
		{
			if (e.Value == null)
				return;

			if (!(e is PowerSupplayEA_ParamData eaParam))
				return;

			if (eaParam.Cmd == "SYST:LOCK" || eaParam.Cmd == "OUTP")
			{
				bool onOff = false;
				int value = Convert.ToInt32(eaParam.Value);
				if (value == 1)
					onOff = true;
				_modbusTCPSevice.WriteSingleCoils(
						ModbusTCPSevice.fctWriteSingleCoil,
						1,
						eaParam.ModbusAddress,
						onOff);
			}
			else
			{
				double d = Convert.ToDouble((string)eaParam.Value);


				ushort value = 0;
				if (eaParam.Cmd == "SOUR:VOLTAGE") // Limit Voltage
				{
					value = (ushort)((d * (double)_actualValuesFactor) / _nominalVoltage);
				}
				else if (eaParam.Cmd == "SOUR:CURRENT" || eaParam.Cmd == "SINK:CURRENT") // Limit PS/EL Current
				{
					value = (ushort)((d * (double)_actualValuesFactor) / _nominalCurrent);
				}
				else if (eaParam.Cmd == "SOUR:POW" || eaParam.Cmd == "SINK:POW") // Limit PS/EL Power
				{
					value = (ushort)((d * (double)_actualValuesFactor) / _nominalPower);
				}

				byte[] bytes = BitConverter.GetBytes(value);

				if (eaParam.NumOfRegisters > 1)
				{

					_modbusTCPSevice.WriteMultipleRegister(
							ModbusTCPSevice.fctWriteMultipleRegister,
							1,
							eaParam.ModbusAddress,
							bytes);
				}
				else
				{
					_modbusTCPSevice.WriteSingleRegister(
							ModbusTCPSevice.fctWriteSingleRegister,
							1,
							eaParam.ModbusAddress,
							bytes);
				}
			}

		}

		#endregion Set

		#region Get

		private void Get(DeviceParameterData e)
		{
			if (!(e is PowerSupplayEA_ParamData eaParam))
				return;

			if (eaParam.Cmd == "*IDN")
			{
				GetIdentification(eaParam);
				return;
			}

			if (eaParam.Cmd == "SYST:ERR" ||
				eaParam.Cmd == "SYSTEM:LOCK:OWNER" ||
				eaParam.Cmd == "OUTPut" ||
				eaParam.Cmd == "SYSTem:MS:LINK")
			{
				GetState(eaParam);
				return;
			}

			_data = null;
			_error = null;


			_modbusTCPSevice.ReadHoldingRegister(
				ModbusTCPSevice.fctReadHoldingRegister,
				1,
				eaParam.ModbusAddress,
				eaParam.NumOfRegisters);


			_waitForResponse.WaitOne(1000);

			if (_data == null)
			{
				if (_error != null)
					MessageBox.Show(_error, "Error");
				else
					MessageBox.Show("No message received", "Error");
			}
			else
			{
				HandleData(eaParam);
			}
		}

		private void HandleData(PowerSupplayEA_ParamData eaParam)
		{
			try
			{
				Array.Reverse(_data);

				object value = null;
				if (eaParam.ValueType == "ushort")
					value = BitConverter.ToUInt16(_data);
				else if (eaParam.ValueType == "float")
					value = BitConverter.ToSingle(_data);
				else if (eaParam.ValueType == "uint")
					value = BitConverter.ToUInt32(_data);
				else if (eaParam.ValueType == "byte")
					value = _data[0];
				else if (eaParam.ValueType == "string")
				{
					eaParam.Value = _data;
					return;
				}

				double d = Convert.ToDouble(value);
				if (eaParam.Cmd == "MEAS:VOLT" || eaParam.Cmd == "SOUR:VOLTAGE" || eaParam.Cmd == "SOUR:VOLTAGE:PROT") // Actual/Limit/Protection Voltage
				{
					value = (d * _nominalVoltage) / _actualValuesFactor;
				}
				else if (eaParam.Cmd == "MEAS:CURR" || eaParam.Cmd == "SOUR:CURRENT" || eaParam.Cmd == "SOUR:CURRENT:PROT") // Actual/Limit/Protection Current
				{
					value = (d * _nominalCurrent) / _actualValuesFactor;
				}
				else if (eaParam.Cmd == "MEAS:POW" || eaParam.Cmd == "SOUR:POW" || eaParam.Cmd == "SOUR:POW:PROT") // Actual/Limit/Protection Power
				{
					value = (d * _nominalPower) / _actualValuesFactor;
				}



				eaParam.Value = value;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void GetIdentification(PowerSupplayEA_ParamData idenParam)
		{
			PowerSupplayEA_ParamData eaParam = null;

			#region Get Manufacturer

			eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 21,
				NumOfRegisters = 20,
				ValueType = "string",
			};
			Get(eaParam);


			StringBuilder sb = new StringBuilder();
			byte[] data = eaParam.Value as byte[];
			if (data == null)
				return;

			Array.Reverse(data);
			foreach (byte b in data)
			{
				if (b == 0)
					continue;

				char c = (char)b;
				sb.Append(c);
			}

			#endregion Get Manufacturer

			sb.Append(", ");

			#region Get Part Number

			eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 1,
				NumOfRegisters = 20,
				ValueType = "string",
			};
			Get(eaParam);

			data = eaParam.Value as byte[];
			if (data == null)
				return;

			Array.Reverse(data);
			foreach (byte b in data)
			{
				if (b == 0)
					continue;

				char c = (char)b;
				sb.Append(c);
			}

			#endregion Get Part Number

			sb.Append(", ");

			#region Get Serial Number

			eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 151,
				NumOfRegisters = 20,
				ValueType = "string",
			};
			Get(eaParam);

			data = eaParam.Value as byte[];
			if (data == null)
				return;

			Array.Reverse(data);
			foreach (byte b in data)
			{
				if (b == 0)
					continue;

				char c = (char)b;
				sb.Append(c);
			}

			#endregion Get Serial Number

			sb.Append(", ");

			#region Get FW version HMI

			eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 211,
				NumOfRegisters = 20,
				ValueType = "string",
			};
			Get(eaParam);

			data = eaParam.Value as byte[];
			if (data == null)
				return;

			Array.Reverse(data);
			foreach (byte b in data)
			{
				if (b == 0)
					continue;

				char c = (char)b;
				sb.Append(c);
			}

			#endregion Get FW version HMI

			sb.Append(" ");

			#region Get FW version KE

			eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 191,
				NumOfRegisters = 20,
				ValueType = "string",
			};
			Get(eaParam);

			data = eaParam.Value as byte[];
			if (data == null)
				return;

			Array.Reverse(data);
			foreach (byte b in data)
			{
				if (b == 0)
					continue;

				char c = (char)b;
				sb.Append(c);
			}

			#endregion Get FW version KE

			sb.Append(" ");

			#region Get FW version DR

			eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 231,
				NumOfRegisters = 20,
				ValueType = "string",
			};
			Get(eaParam);

			data = eaParam.Value as byte[];
			if (data == null)
				return;

			Array.Reverse(data);
			foreach (byte b in data)
			{
				if (b == 0)
					continue;

				char c = (char)b;
				sb.Append(c);
			}

			#endregion Get FW version DR

			idenParam.Value = sb.ToString();
		}

		private void GetState(PowerSupplayEA_ParamData param)
		{
			PowerSupplayEA_ParamData eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 505,
				NumOfRegisters = 2,
				ValueType = "uint",
			};
			Get(eaParam);

			uint state = Convert.ToUInt32(eaParam.Value);

			if (param.Cmd == "SYSTEM:LOCK:OWNER")
			{
				bool isRemot = (state & 2048) == 2048;
				eaParam.Value = 0;
				if (isRemot)
					eaParam.Value = 1;
			}
			else if (param.Cmd == "OUTPut")
			{
				bool isOutput = (state & 128) == 128;
				eaParam.Value = 0;
				if (isOutput)
					eaParam.Value = 1;
			}
			else if (param.Cmd == "SYST:ERR")
			{
				bool isAlarm = (state & 32768) == 32768;
				eaParam.Value = 0;
				if (isAlarm)
					eaParam.Value = 1;
			}
			else if (param.Cmd == "SYSTem:MS:LINK")
			{
				bool isMS = (state & 64) == 64;
				eaParam.Value = 0;
				if (isMS)
					eaParam.Value = 1;
			}
		}

		#endregion Get

		private void _modbusTCPSevice_MessageReceivedEvent(byte[] data)
		{
			_waitForResponse.Set();
			_data = data;
		}

		private string _error;
		private void _modbusTCPSevice_ErrorEvent(string error)
		{
			_waitForResponse.Set();
			_error = error;
		}

		#endregion Methods

	}
}
