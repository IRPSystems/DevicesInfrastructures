using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Services.Services;
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
		private string _error;

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
				Set(data);
			}
			else
			{
				Get(data);
			}

			return CommunicatorResultEnum.OK;
		}

		private void GetNominals(DeviceData easpDevice)
		{
			try
			{
				CommunicatorIOData iOData = null;

				PowerSupplayEA_ParamData nominalVoltageParam =
					easpDevice.ParemetersList.ToList().Find((p) => ((PowerSupplayEA_ParamData)p).Cmd == "SYSTem:NOMinal:VOLTage")
						as PowerSupplayEA_ParamData;
				iOData = new CommunicatorIOData() { Parameter = nominalVoltageParam };
				Get(iOData);
				if (nominalVoltageParam.Value != null)
					_nominalVoltage = (float)nominalVoltageParam.Value;

				PowerSupplayEA_ParamData nominalCurrentParam =
					easpDevice.ParemetersList.ToList().Find((p) => ((PowerSupplayEA_ParamData)p).Cmd == "SYSTem:NOMinal:CURRent")
						as PowerSupplayEA_ParamData;
				iOData = new CommunicatorIOData() { Parameter = nominalCurrentParam };
				Get(iOData);
				if (nominalCurrentParam.Value != null)
					_nominalCurrent = (float)nominalCurrentParam.Value;

				PowerSupplayEA_ParamData nominalPowerParam =
					easpDevice.ParemetersList.ToList().Find((p) => ((PowerSupplayEA_ParamData)p).Cmd == "SYSTem:NOMinal:POWer")
						as PowerSupplayEA_ParamData;
				iOData = new CommunicatorIOData() { Parameter = nominalPowerParam };
				Get(iOData);
				if (nominalPowerParam.Value != null)
					_nominalPower = (float)nominalPowerParam.Value;
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to init EA PS Modbus TCP communication", ex);
			}
		}


		#region Set

		private void Set(CommunicatorIOData data)
		{
			try
			{
				

				if (!(data.Parameter is PowerSupplayEA_ParamData eaParam))
					return;

				LoggerService.Inforamtion(this, eaParam.Name + "; value=" + data.Value);

				if (eaParam.Cmd == "SYST:LOCK" || eaParam.Cmd == "OUTP")
				{
					

					bool onOff = false;
					
					if (data.Value == 1)
						onOff = true;
					_modbusTCPSevice.WriteSingleCoils(
							ModbusTCPSevice.fctWriteSingleCoil,
							1,
							eaParam.ModbusAddress,
							onOff);
				}
				else
				{
					
					
					ushort value = 0;
					if (eaParam.Cmd == "SOUR:VOLTAGE") // Limit Voltage
					{
						value = (ushort)((data.Value * (double)_actualValuesFactor) / _nominalVoltage);
					}
					else if (eaParam.Cmd == "SOUR:CURRENT" || eaParam.Cmd == "SINK:CURRENT") // Limit PS/EL Current
					{
						value = (ushort)((data.Value * (double)_actualValuesFactor) / _nominalCurrent);
					}
					else if (eaParam.Cmd == "SOUR:POW" || eaParam.Cmd == "SINK:POW") // Limit PS/EL Power
					{
						value = (ushort)((data.Value * (double)_actualValuesFactor) / _nominalPower);
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

				_waitForResponse.WaitOne(1000);
				
				if (_data == null)
				{
					if(eaParam.Cmd == "OUTPut")
					{
						data.Callback(data.Parameter, CommunicatorResultEnum.OK, null);
						return;
					}

					if (data.Callback != null)
					{
						if (_error != null)
							data.Callback(data.Parameter, CommunicatorResultEnum.Error, _error);
						else
							data.Callback(data.Parameter, CommunicatorResultEnum.NoResponse, _error);
					}
				}
				else
				{
					if (data.Callback != null)
						data.Callback(data.Parameter, CommunicatorResultEnum.OK, null);
				}
			}
			catch(Exception ex) 
			{
				LoggerService.Inforamtion(this, "8");
				LoggerService.Error(this, "Failed to set parameter \"" + data.Parameter.Name + "\"", ex);
				string error = "Failed to set parameter \"" + data.Parameter.Name + "\"; Value=" + data.Parameter.Value + "\r\n" + ex;
				data.Callback(data.Parameter, CommunicatorResultEnum.Error, error);
			}

		}

		#endregion Set

		#region Get

		private void Get(CommunicatorIOData data)
		{
			try
			{
				if (!(data.Parameter is PowerSupplayEA_ParamData eaParam))
					return;

				if (eaParam.Cmd == "*IDN")
				{
					bool isOK = GetIdentification(eaParam);
					if (isOK)
						data.Callback(data.Parameter, CommunicatorResultEnum.OK, null);
					else
						data.Callback(data.Parameter, CommunicatorResultEnum.Error, _error);
					return;
				}

				if (eaParam.Cmd == "SYST:ERR" ||
					eaParam.Cmd == "SYSTEM:LOCK:OWNER" ||
					eaParam.Cmd == "OUTPut" ||
					eaParam.Cmd == "SYSTem:MS:LINK" ||
					eaParam.Cmd == "STAT:OPER:COND")
				{
					GetState(eaParam);
					data.Callback(data.Parameter, CommunicatorResultEnum.OK, null);
					return;
				}

				if(eaParam.Cmd == "SYSTem:NOMinal:VOLTage") { }

				_data = null;
				_error = null;


				_modbusTCPSevice.ReadHoldingRegister(
					ModbusTCPSevice.fctReadHoldingRegister,
					1,
					eaParam.ModbusAddress,
					eaParam.NumOfRegisters);


				_waitForResponse.WaitOne(1000);

				if (_data == null || _data.Length == 0)
				{
					if (data.Callback != null)
					{
						if (_error != null)
							data.Callback(data.Parameter, CommunicatorResultEnum.Error, _error);
						else
							data.Callback(data.Parameter, CommunicatorResultEnum.NoResponse, _error);
					}
				}
				else
				{
					HandleData(eaParam);

					if (data.Callback != null)
						data.Callback(data.Parameter, CommunicatorResultEnum.OK, null);
				}
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to get value", ex);
				data.Callback(data.Parameter, CommunicatorResultEnum.Error, "Exception");
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

		private bool GetIdentification(PowerSupplayEA_ParamData idenParam)
		{
			PowerSupplayEA_ParamData eaParam = null;
			CommunicatorIOData iOData = null;

			#region Get Manufacturer

			eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 21,
				NumOfRegisters = 20,
				ValueType = "string",
			};
			iOData = new CommunicatorIOData() { Parameter = eaParam };
			Get(iOData);


			StringBuilder sb = new StringBuilder();
			byte[] data = eaParam.Value as byte[];
			if (data == null)
				return false;

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
			iOData = new CommunicatorIOData() { Parameter = eaParam };
			Get(iOData);

			data = eaParam.Value as byte[];
			if (data == null)
				return false;

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
			iOData = new CommunicatorIOData() { Parameter = eaParam };
			Get(iOData);

			data = eaParam.Value as byte[];
			if (data == null)
				return false;

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
			iOData = new CommunicatorIOData() { Parameter = eaParam };
			Get(iOData);

			data = eaParam.Value as byte[];
			if (data == null)
				return false;

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
			iOData = new CommunicatorIOData() { Parameter = eaParam };
			Get(iOData);

			data = eaParam.Value as byte[];
			if (data == null)
				return false;

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
			iOData = new CommunicatorIOData() { Parameter = eaParam };
			Get(iOData);

			data = eaParam.Value as byte[];
			if (data == null)
				return false;

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

			return true;
		}

		private void GetState(PowerSupplayEA_ParamData param)
		{
			PowerSupplayEA_ParamData eaParam = new PowerSupplayEA_ParamData()
			{
				ModbusAddress = 505,
				NumOfRegisters = 2,
				ValueType = "uint",
			};
			CommunicatorIOData iOData = new CommunicatorIOData() { Parameter = eaParam };
			Get(iOData);

			uint state = Convert.ToUInt32(eaParam.Value);

			if (param.Cmd == "SYSTEM:LOCK:OWNER")
			{
				bool isRemot = (state & 2048) == 2048;
				param.Value = "NONE";
				if (isRemot)
					param.Value = "REMOTE";
			}
			else if (param.Cmd == "OUTPut")
			{
				bool isOutput = (state & 128) == 128;
				param.Value = "OFF";
				if (isOutput)
					param.Value = "ON";
			}
			else if (param.Cmd == "SYST:ERR")
			{
				bool isAlarm = (state & 32768) == 32768;
				param.Value = 0;
				if (isAlarm)
					param.Value = 1;
			}
			else if (param.Cmd == "SYSTem:MS:LINK")
			{
				bool isMS = (state & 64) == 64;
				param.Value = 0;
				if (isMS)
					param.Value = 1;
			}
			else if (param.Cmd == "STAT:OPER:COND")
			{
				bool isOp = (state & 4096) == 4096;
				param.Value = 0;
				if (isOp)
					param.Value = 1;
			}
		}

		#endregion Get

		private void _modbusTCPSevice_MessageReceivedEvent(byte[] data)
		{
			_waitForResponse.Set();
			_data = data;
		}

		
		private void _modbusTCPSevice_ErrorEvent(string error)
		{
			_waitForResponse.Set();
			_error = error;
		}

		#endregion Methods

	}
}
