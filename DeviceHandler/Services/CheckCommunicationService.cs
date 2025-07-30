
using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;
using DeviceCommunicators.Enums;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using Entities.Models;
using Services.Services;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace DeviceHandler.Services
{
    public class CheckCommunicationService: ObservableObject, IDisposable
	{
		#region Fields

		private DeviceFullData _deviceFullData;
		private DeviceParameterData _parameter;

		private bool _isFirstMessageReceived;
		private bool _isCommunicatorInitiated;

		private int _noReplyCounter;

        private System.Timers.Timer _timerMessageSending;
		private System.Timers.Timer _timerTimeout;

		private CommunicationStateEnum prevStatus;

		public string Name;

		private bool _isInitialized;
		private bool _isReconnect;

		public CommunicationStateEnum Status;

		#endregion Fields

		#region Constructor

		public CheckCommunicationService(
			DeviceFullData deviceFullData,
			DeviceParameterData parameter,
			string name) 
		{
			_deviceFullData = deviceFullData;
			_parameter = parameter;
			Name = name;

			if(_parameter == null) 
			{
				LoggerService.Error(this, "Parameter is null");
			}

			prevStatus = CommunicationStateEnum.None;

			_isFirstMessageReceived = false;

			_isCommunicatorInitiated = _deviceFullData.DeviceCommunicator.IsInitialized;

			if(deviceFullData is DeviceFullData_RigolM300)
                _timerMessageSending = new System.Timers.Timer(1000);
			else
				_timerMessageSending = new System.Timers.Timer(1000);
			_timerMessageSending.Elapsed += _timerMessageSending_Elapsed;

			if(deviceFullData is DeviceFullData_RigolM300)
                _timerTimeout = new System.Timers.Timer(3000);
			else
				_timerTimeout = new System.Timers.Timer(3000);
			_timerTimeout.Elapsed += TimoutElapsedEventHandler;

			_isReconnect = false;
		}

		#endregion Constructor

		#region Methods

		public void Init()
		{
			_noReplyCounter = 0;

			if (_deviceFullData.DeviceCommunicator.IsInitialized)
			{
				if(!_isReconnect)
					NotifyStatus(CommunicationStateEnum.Initiated, null);
				else
					NotifyStatus(CommunicationStateEnum.Disconnected, null);
			}
			else
				NotifyStatus(CommunicationStateEnum.None, null);

			_timerMessageSending.Start();

			_isInitialized = true;
			_isReconnect = true;
		}



		private void _timerMessageSending_Elapsed(object sender, ElapsedEventArgs e)
		{
			_deviceFullData.DeviceCommunicator.GetParamValue(_parameter, GetValueCallback);
		}

		public void Dispose()
		{
			_timerTimeout.Stop();
			_timerMessageSending.Stop();
			NotifyStatus(CommunicationStateEnum.Disconnected, null);

			_isInitialized = false;
		}

		protected void GetValueCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{
			_timerTimeout.Stop();

			if (!_isInitialized)
				return;

			try
			{
				if (result == CommunicatorResultEnum.OK)
				{
					NotifyStatus(CommunicationStateEnum.Connected, null);
					_isFirstMessageReceived = true;
					_noReplyCounter = 0;

					if (param is MCU_ParamData)
					{
						uint uval = (uint)Convert.ToDouble(param.Value);
						uint errorState = (uval >> 8) & 0xF;
						FaultEvent?.Invoke((ActiveErrorLevelEnum)errorState);
					}
				}
				else
				{
					if (result == CommunicatorResultEnum.Error)
					{
						NotifyStatus(CommunicationStateEnum.Disconnected, resultDescription);
						_isFirstMessageReceived = true;
						_noReplyCounter = 0;
						return;
					}

					if (result == CommunicatorResultEnum.NoResponse)
					{
						_noReplyCounter++;
						//	LoggerService.Inforamtion(this, "_noReplyCounter=" + _noReplyCounter);

						//if (_noReplyCounter >= 10)
						//{
						//	Task.Factory.StartNew(() =>
						//	{
						//		_deviceFullData.Disconnect();
						//		System.Threading.Thread.Sleep(1000);
						//		_deviceFullData.Connect();							
						//	});

						//	_noReplyCounter = 0;
						//	LoggerService.Inforamtion(this, "Disconnect/connect");
						//}

						if (_noReplyCounter % 2 != 0)
						{
							return;
						}
					}
					else
					{
						LoggerService.Inforamtion(this, "_noReplyCounter=0");
						_noReplyCounter = 0;
					}



					if (_isFirstMessageReceived)
					{
						NotifyStatus(CommunicationStateEnum.Disconnected, null);
					}
					else
					{
						if (_deviceFullData.DeviceCommunicator.IsInitialized)
							NotifyStatus(CommunicationStateEnum.Initiated, null);
						else
						{
							if (_isCommunicatorInitiated)
								NotifyStatus(CommunicationStateEnum.Disconnected, null);
							else
								NotifyStatus(CommunicationStateEnum.None, null);
						}
					}
				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, $"Failed to handle the response. Parameter: {param}", ex);
			}

			if(_isFirstMessageReceived)
				_timerTimeout.Start();
		}

		private void TimoutElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			NotifyStatus(CommunicationStateEnum.Disconnected, null);
		}

		private void NotifyStatus(CommunicationStateEnum status, string errDescription)
		{
			Status = status;
			CommunicationStateReprotEvent?.Invoke(status, errDescription);

			if (status != prevStatus)
			{
				LoggerService.Inforamtion(
					this,
					"Name: " + Name +
					" - Status: " + status +
					" - First msg rcvd: " + _isFirstMessageReceived +
					" - Error msg: " + errDescription);
			}

			prevStatus = status;
		}

		#endregion Methods

		#region Events

		public event Action<CommunicationStateEnum, string> CommunicationStateReprotEvent;
		public event Action<ActiveErrorLevelEnum> FaultEvent;

		#endregion Events
	}
}
