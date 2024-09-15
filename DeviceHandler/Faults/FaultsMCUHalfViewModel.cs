
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Enums;
using DeviceCommunicators.MCU;
using Entities.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using DeviceCommunicators.Models;
using Services.Services;
using System.Threading;
using System.Threading.Tasks;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;

namespace DeviceHandler.Faults
{
	public class FaultsMCUHalfViewModel : ObservableObject
	{
		

		#region Properties

		public ObservableCollection<RegisterBitData> FaultsList { get; set; }
		public bool IsShowFaultsOnly { get; set; }

		#endregion Properties

		#region Fields



		private System.Timers.Timer _setFaultsTimer;

		private MCU_ParamData _faultsParameter;



		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private AutoResetEvent _receivedEvent;

		private string _halfIdentifier;

		public bool IsLoaded;

		private DevicesContainer _devicesContainer;

		#endregion Fields

		#region Constructor

		public FaultsMCUHalfViewModel(
			string halfIdentifier,
			DevicesContainer devicesContainer)
		{
			_halfIdentifier = halfIdentifier;
			_devicesContainer = devicesContainer;

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;
			_receivedEvent = new AutoResetEvent(false);

			IsLoaded = false;


			
			_setFaultsTimer = new System.Timers.Timer(1000);
			_setFaultsTimer.Elapsed += SetFaultsTimerElapsedEventHandler;

			LoadFaults();

			HandleParameters();
		}

		#endregion Constructor

		#region Methods

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_setFaultsTimer.Stop();

			IsLoaded = false;
		}


		public void Start()
		{
			_setFaultsTimer.Start();
		}

		public void Stop()
		{
			_setFaultsTimer.Stop();
			FaultEvent.Invoke(null);

			if (FaultsList == null)
				return;

			for (int i = 0; i < FaultsList.Count; i++)
			{
				FaultsList[i].IsOn = null;
			}
		}

		private void LoadFaults()
		{
			try
			{
				if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
					return;
				
				DeviceFullData mcuFullData = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];

				
				ParamGroup paramGroup =
					((MCU_DeviceData)mcuFullData.Device).MCU_GroupList.ToList().Find((g) =>
					 g != null &&
					 g.GroupName != null &&
					 g.GroupName == "Monitor");

				if (paramGroup == null)
					return;

				_faultsParameter =
					paramGroup.ParamList.ToList().Find((p) => p.Name == "Faults Vector " + _halfIdentifier);
				if(_faultsParameter == null)					
				{
					LoggerService.Error(this, "Failed to find the faults parameter in the params list");
					return; 
				}



				FaultsList = new ObservableCollection<RegisterBitData>();
				int counter = 0;

				foreach (DropDownParamData ddParam in _faultsParameter.DropDown)
				{
					if (ddParam.Value == "0")
						continue;

					RegisterBitData faultData = new RegisterBitData()
					{
						Name = ddParam.Name,
						Index = counter++,
					};

					FaultsList.Add(faultData);
				}
			}
			catch (Exception ex) 
			{ 
				LoggerService.Error(this, "Failed to load the faults", ex);
			}
		}

		



		private void SetFaultsTimerElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
				return;

			DeviceFullData mcuFullData = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];

			mcuFullData.DeviceCommunicator.GetParamValue(_faultsParameter, ResponseCallback);
		}

		private void ResponseCallback(DeviceParameterData param, CommunicatorResultEnum result, string errDescription)
		{
			_receivedEvent.Set();
		}
	

		private void HandleParameters()
		{
			Task.Run(() =>
			{
				while (!_cancellationToken.IsCancellationRequested)
				{

					HandleSingleParameter(_faultsParameter);

					_receivedEvent.WaitOne();

					System.Threading.Thread.Sleep(1);
				}
			}, _cancellationToken);
		}

		private void HandleSingleParameter(MCU_ParamData param)
		{
			try
			{
				double dVal = 0;
				if (param == null || param.Value == null)
				{
					FaultEvent?.Invoke(null);
					return;
				}


				if (param.Value is string str)
				{

					var dropDownItem = param.DropDown.Find(item => item.Name == str);
					if (dropDownItem == null)
					{
						bool res = double.TryParse(str, out dVal);
						if (res == false)
							return;
					}
					else
						double.TryParse(dropDownItem.Value, out dVal);
				}
				else
				{
					bool res = double.TryParse(param.Value.ToString(), out dVal);
					if (!res)
					{
						FaultEvent?.Invoke(null);
						return;
					}
				}

				if (dVal != 0)
				{
					FaultEvent?.Invoke(true);
				}
				else
				{
					FaultEvent?.Invoke(false);
				}



				SetErrorData(param, FaultsList, (uint)dVal);

				//if (param?.DropDown?.Count > 0)
				//{
				//	/* Get drop-down item which matches recveived value */
				//	var dropDownItem = param.DropDown.Find(item => item.Value == dVal.ToString());

				//	if (dropDownItem != null)
				//	{
				//		param.Value = dropDownItem.Name;
				//	}
				//}
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to set the fualts", ex);
			}
		}

		private void SetErrorData(
			MCU_ParamData param,
			ObservableCollection<RegisterBitData> faultsList,
			uint faultValue)
		{
			if (param == null || param.DropDown == null || param.DropDown.Count == 0 ||
				faultsList == null || faultsList.Count == 0)
			{
				return;
			}

			for(int i = 0; i < faultsList.Count && i < param.DropDown.Count; i++) 
			{ 
				RegisterBitData faultData = faultsList[i];
				DropDownParamData dropDown = param.DropDown[i];
				if (faultData == null || dropDown == null)
					continue;

				
				faultData.IsOn = !(((faultValue >> i) & 1U) == 1U);

				if(IsShowFaultsOnly && faultData.IsOn == false) 
					faultData.IsVisible = true;
				else
					faultData.IsVisible = !IsShowFaultsOnly;
			}
			

		}

		#endregion Methods

		#region Commands

		//public RelayCommand LoadedCommand { get; private set; }
		//public RelayCommand UnLoadedCommand { get; private set; }


		#endregion Commands

		#region Events

		public event Action<bool?> FaultEvent;

		#endregion Events
	}


}
