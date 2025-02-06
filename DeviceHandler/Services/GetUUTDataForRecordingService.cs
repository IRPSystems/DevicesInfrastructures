
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using Services.Services;
using System;
using System.Linq;
using System.Threading;

namespace DeviceHandler.Services
{
	public class GetUUTDataForRecordingService
	{
		#region Properties and Fields

		public string SerialNumber { get; set; }
		public string FirmwareVersion { get; set; }
		public string CoreVersion { get; set; }
		public string HWVersion { get; set; }

		public string ErrorMessage { get; set; }


		protected ManualResetEvent _waitForGet;

		#endregion Properties and Fields

		#region Methods

		public bool GetUUTData(DevicesContainer devicesContainer)
		{
			SerialNumber = "--";
			FirmwareVersion = "--";
			CoreVersion = "--";
			HWVersion = "--";

			if (devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
			{
				LoggerService.Error(this, "No UUT found");
				return false;
			}

			_waitForGet = new ManualResetEvent(false);

			DeviceFullData deviceFullData = 
				devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];
			if (deviceFullData == null)
			{
				LoggerService.Error(this, "No UUT found");
				return false;
			}

			if (!(deviceFullData.Device is MCU_DeviceData mcu_Device))
			{
				LoggerService.Error(this, "No MCU device was found");
				return false;
			}

			bool isOK = GetParameterValue_SerialNumber(
				deviceFullData.DeviceCommunicator,
				mcu_Device);
			if(!isOK)
				return false;

			isOK = GetParameterValue_FWVersion(
				deviceFullData.DeviceCommunicator,
				mcu_Device);
			if (!isOK)
				return false;

			isOK = GetParameterValue_COREVersion(
				deviceFullData.DeviceCommunicator,
				mcu_Device);
			if (!isOK)
				return false;

			isOK = GetParameterValue_HWVersion(
				deviceFullData.DeviceCommunicator,
				mcu_Device);
			if (!isOK)
				return false;

			return true;
		}

		private object GetParamValue(
			DeviceCommunicator deviceCommunicator,
			DeviceParameterData param)
		{
			deviceCommunicator.GetParamValue(param, GetValueCallback);

			int timeOut = 1000;
			if (param.CommunicationTimeout > 0)
			{
				timeOut = param.CommunicationTimeout;
			}

			bool isNotTimeout = _waitForGet.WaitOne(timeOut);
			_waitForGet.Reset();

			if (!isNotTimeout)
				return null;

			return param.Value;
		}

		private bool GetParameterValue_SerialNumber(
			DeviceCommunicator deviceCommunicator,
			MCU_DeviceData mcu_Device)
		{
			try
			{
				ParamGroup group = mcu_Device.MCU_GroupList.ToList().Find((g) => g.Name == "HW Version");
				if (group == null)
				{
					LoggerService.Error(this, "Failed to find the HW Version group");
					return false;
				}

				DeviceParameterData param =
					group.ParamList.ToList().Find((p) => p.Name == "Serial Number");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the Serial Number parameter");
					return false;
				}

				object value = GetParamValue(deviceCommunicator, param);
				if(value == null)
				{
					LoggerService.Error(this, "Failed to get the Serial Number from the UUT");
					return false;
				}

				SerialNumber = value.ToString();

				return true;
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to read the Serial Number", ex);
				return false;
			}
		}

		private bool GetParameterValue_FWVersion(
			DeviceCommunicator deviceCommunicator,
			MCU_DeviceData mcu_Device)
		{
			try
			{			

				ParamGroup group = mcu_Device.MCU_GroupList.ToList().Find((g) => g.Name == "FW Version");
				if (group == null)
				{
					group = mcu_Device.MCU_GroupList.ToList().Find((g) => g.Name == "SW Version");
					if (group == null)
					{
						LoggerService.Error(this, "Failed to find the FW/SW Version group");
						return false;
					}
				}

				#region FW Major

				DeviceParameterData param =
					group.ParamList.ToList().Find((p) => p.Cmd == "vermajor");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the FW Version Major parameter");
					return false;
				}

				object value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the FW Version Major from the UUT");
					return false;
				}

				int n = Convert.ToInt32(value);
				FirmwareVersion = n.ToString("D2");

				#endregion FW Major

				#region FW Middle
				param =
					group.ParamList.ToList().Find((p) => p.Cmd == "vermiddle");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the FW Version Middle parameter");
					return false;
				}

				value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the FW Version Middle from the UUT");
					return false;
				}

				n = Convert.ToInt32(value);
				FirmwareVersion += "." + n.ToString("D2");

				#endregion FW Middle

				#region FW Minor
				param =
					group.ParamList.ToList().Find((p) => p.Cmd == "verminor");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the FW Version Minor parameter");
					return false;
				}

				value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the FW Version Minor from the UUT");
					return false;
				}

				n = Convert.ToInt32(value);
				FirmwareVersion += "." + n.ToString("D2");

				#endregion FW Minor

				return true;
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to read the FW Version", ex);
				return false;
			}
		}

		private bool GetParameterValue_COREVersion(
			DeviceCommunicator deviceCommunicator,
			MCU_DeviceData mcu_Device)
		{
			try
			{

				ParamGroup group = mcu_Device.MCU_GroupList.ToList().Find((g) => g.Name == "Core Version");
				if (group == null)
				{
					group = mcu_Device.MCU_GroupList.ToList().Find((g) => g.Name == "SW Version");
					if (group == null)
					{
						LoggerService.Error(this, "Failed to find the HW Version group");
						return false;
					}
				}

				#region CORE Major

				DeviceParameterData param =
					group.ParamList.ToList().Find((p) => p.Cmd == "corevermajor");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the CORE Version Major parameter");
					return false;
				}

				object value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the CORE Version Major from the UUT");
					return false;
				}


				int n = Convert.ToInt32(value);
				CoreVersion = n.ToString("D2");

				#endregion CORE Major

				#region CORE Middle
				param =
					group.ParamList.ToList().Find((p) => p.Cmd == "corevermiddle");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the Core Version Middle parameter");
					return false;
				}

				value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the Core Version Middle from the UUT");
					return false;
				}


				n = Convert.ToInt32(value);
				CoreVersion += "." + n.ToString("D2");

				#endregion CORE Middle

				#region Core Minor
				param =
					group.ParamList.ToList().Find((p) => p.Cmd == "coreverminor");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the Core Version Minor parameter");
					return false;
				}

				value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the Core Version Minor from the UUT");
					return false;
				}

				n = Convert.ToInt32(value);
				CoreVersion += "." + n.ToString("D2");

				#endregion Core Minor

				return true;
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to read the CORE Version", ex);
				return false;
			}
		}

		private bool GetParameterValue_HWVersion(
			DeviceCommunicator deviceCommunicator,
			MCU_DeviceData mcu_Device)
		{
			try
			{

				ParamGroup group = mcu_Device.MCU_GroupList.ToList().Find((g) => g.Name == "HW Version");
				if (group == null)
				{
					group = mcu_Device.MCU_GroupList.ToList().Find((g) => g.Name == "SW Version");
					if (group == null)
					{
						LoggerService.Error(this, "Failed to find the HW Version group");
						return false;
					}
				}

				#region HW Major

				DeviceParameterData param =
					group.ParamList.ToList().Find((p) => p.Cmd == "hwvermajor");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the HW Version Major parameter");
					return false;
				}

				object value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the HW Version Major from the UUT");
					return false;
				}


				int n = Convert.ToInt32(value);
				HWVersion = n.ToString("D2");

				#endregion HW Major

				#region HW Middle
				param =
					group.ParamList.ToList().Find((p) => p.Cmd == "hwvermiddle");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the HW Version Middle parameter");
					return false;
				}

				value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the HW Version Middle from the UUT");
					return false;
				}


				n = Convert.ToInt32(value);
				HWVersion += "." + n.ToString("D2");

				#endregion HW Middle

				#region HW Minor
				param =
					group.ParamList.ToList().Find((p) => p.Cmd == "hwverminor");
				if (param == null)
				{
					LoggerService.Error(this, "Failed to find the HW Version Minor parameter");
					return false;
				}

				value = GetParamValue(deviceCommunicator, param);
				if (value == null)
				{
					LoggerService.Error(this, "Failed to get the HW Version Minor from the UUT");
					return false;
				}

				n = Convert.ToInt32(value);
				HWVersion += "." + n.ToString("D2");

				#endregion Core Minor

				return true;
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to read the HW Version", ex);
				return false;
			}
		}


		private void GetValueCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{


			_waitForGet.Set();

			switch (result)
			{
				case CommunicatorResultEnum.NoResponse:
					ErrorMessage +=
						"No response was received from the device.";
					break;

				case CommunicatorResultEnum.ValueNotSet:
					ErrorMessage +=
						"Failed to set the value.";
					break;

				case CommunicatorResultEnum.Error:
					ErrorMessage +=
						"The device returned an error:\r\n" +
						resultDescription;
					break;

				case CommunicatorResultEnum.InvalidUniqueId:
					ErrorMessage +=
						"Invalud Unique ID was received from the Dyno.";
					break;
			}

			
		}

		#endregion Methods
	}
}
