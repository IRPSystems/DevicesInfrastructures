

using Communication.Services;
using System;
using System.Collections.Concurrent;
using System.Windows;
using DeviceCommunicators.Models;
using DeviceCommunicators.Dyno;
using DeviceHandler.ViewModels;
using Entities.Models;
using System.Timers;
using Communication.Interfaces;

namespace DeviceSimulators.ViewModels
{
	public class DynoSimulatorMainWindowViewModel : DeviceSimulatorViewModel
	{

		#region Fields

		private CanService _commService;

		private ConcurrentDictionary<int, Dyno_ParamData> _uniqueIdToParam;
		private System.Timers.Timer _timerChangeValue;

		private CanConnectViewModel _canConnectViewModel
		{
			get => ConnectVM as CanConnectViewModel;
		}

		#endregion Fields

		#region Constructor

		public DynoSimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{

			ConnectVM = new CanConnectViewModel(500000, 1, 1, 11220, 11223);
			ConnectVM.ConnectEvent += Connect;
			ConnectVM.DisconnectEvent += Disconnect;

			_timerChangeValue = new System.Timers.Timer(500);
			_timerChangeValue.Elapsed += TimerChangeValueElapsedEventHandler;
			_timerChangeValue.Start();

			BuildUniqueIdDict();
			SetValuesToParams();
		}

		#endregion Constructor

		#region Methods

		private void TimerChangeValueElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			SetValuesToParams();
		}


		private void BuildUniqueIdDict()
		{
			_uniqueIdToParam = new ConcurrentDictionary<int, Dyno_ParamData>();
			foreach (DeviceParameterData parameterData in ParametersList)
			{
				if (!(parameterData is Dyno_ParamData param))
					continue;

				int id = (param.Index << 8) + param.SubIndex;
				_uniqueIdToParam[id] = param;
			}
		}

		private void SetValuesToParams()
		{
			Random rand = new Random((int)DateTime.Now.Ticks);

			int value = rand.Next(0, 1000);
			foreach (DeviceParameterData parameterData in ParametersList)
			{
				if (!(parameterData is Dyno_ParamData param))
					continue;

				param.Value = value++;
				param.GetSetVisibility = System.Windows.Visibility.Collapsed;
			}
		}

		private void Connect()
		{
			uint canID = 0x580 + _canConnectViewModel.SyncNodeID;
			if (_canConnectViewModel.SelectedAdapter == "PCAN")
			{
				_commService = new CanPCanService(
					_canConnectViewModel.SelectedBaudrate,
					CanPCanService.GetHWId(_canConnectViewModel.SelectedHwId),
					0x600 + _canConnectViewModel.SyncNodeID,
					0x580 + _canConnectViewModel.SyncNodeID);
			}
			else if (_canConnectViewModel.SelectedAdapter == "UDP Simulator")
			{
				_commService = new CanUdpSimulationService(_canConnectViewModel.SelectedBaudrate,
					0x600 + _canConnectViewModel.SyncNodeID, 0x580 + _canConnectViewModel.SyncNodeID,
					_canConnectViewModel.RxPort,
					_canConnectViewModel.TxPort, _canConnectViewModel.Address);
			}



			_commService.Name = "DynoSimulator";

			_commService.Init(true);

			_commService.MessageReceivedEvent += MessageReceivedEventHandler;
			_commService.ErrorEvent += ErrorEventHendler;

			ConnectVM.IsConnectButtonEnabled = false;
			ConnectVM.IsDisconnectButtonEnabled = true;


		}

		public override void Disconnect()
		{
			if (_commService == null)
				return;



			_commService.Dispose();

			ConnectVM.IsConnectButtonEnabled = true;
			ConnectVM.IsDisconnectButtonEnabled = false;
		}

		private void MessageReceivedEventHandler(byte[] buffer)
		{
			int uniqueId = (int)Dyno_Communicator.GetDataFromBuffer(buffer, 1, 2);

			//int uniqueId = uniqueParamId >> 8;
			byte messageType = buffer[0];

			uniqueId = Dyno_ParamData.BaseUniqueParamID - uniqueId;
			uniqueId = uniqueId << 8;
			uniqueId += buffer[3];

			//if (uniqueId == 0)
			//{
			//             SendResponse(
			//                 new Dyno_ParamData(),
			//                 uniqueParamId,
			//                 buffer);
			//         }


			if (_uniqueIdToParam.ContainsKey(uniqueId) == false)
				return;

			Dyno_ParamData param = _uniqueIdToParam[uniqueId];

			if (messageType == Dyno_ParamData.SetFirstByte)
			{

				if (Application.Current == null)
					return;


				int value = (int)Dyno_Communicator.GetDataFromBuffer(buffer, 4, 4);

				double dvalue = Convert.ToDouble(value);
				dvalue = dvalue / (1 / param.Coefficient);

				Application.Current.Dispatcher.Invoke(() =>
				{
					param.Value = (int)dvalue;
				});

				SendResponse(param);
			}
			else if (messageType == Dyno_ParamData.GetFirstByte)
			{
				SendResponse(param);

			}

			System.Threading.Thread.Sleep(1);
		}

		private void SendResponse(
			Dyno_ParamData param)
		{
			// int uniqueParamID = uniqueParamId & 0xFFFF00;
			// uniqueParamID += Dyno_ParamData.ResponseGetFirstByte;

			int uniqueParamId = Dyno_ParamData.BaseUniqueParamID - param.Index;


			double value = Convert.ToDouble(param.Value);
			value = value * (1 / param.Coefficient);

			if (value < 0)
			{
				value = Math.Pow(2, 32) + value;
			}

			byte[] sendBuffer = new byte[8];
			int index = 0;

			sendBuffer[index] = Dyno_ParamData.ResponseGetFirstByte;
			index++;

			Dyno_Communicator.SetDataToBuffer(uniqueParamId, sendBuffer, index, 2);
			index += 2;

			sendBuffer[index] = param.SubIndex;
			index++;

			Dyno_Communicator.SetDataToBuffer((long)value, sendBuffer, index, 4);


			_commService.Send(sendBuffer, 0x580 + _canConnectViewModel.SyncNodeID, false);
		}

		private void ErrorEventHendler(string errorDescription)
		{

		}

		#endregion Methods
	}
}