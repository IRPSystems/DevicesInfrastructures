
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;
using Entities.Models;
using Communication.Interfaces;
using Communication.Services;
using System.Linq;
using DeviceCommunicators.PowerSupplayBK;
using DeviceHandler.ViewModels;

namespace DeviceSimulators.ViewModels
{
    public class PSBKSimulatorMainWindowViewModel : DeviceSimulatorViewModel
	{

		#region Fields

		private ISerialService _commService;

		private System.Timers.Timer _timerChangeValue;
		private Random _rand;

		private List<string> _paramsNotToUpdateList;


		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private BlockingCollection<byte[]> _recievedMessagesQueue;

		private SerialConncetViewModel _serialConncetViewModel
		{
			get => ConnectVM as SerialConncetViewModel;
		}

		#endregion Fields

		#region Constructor

		public PSBKSimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{
			
			ConnectVM = new SerialConncetViewModel(115200, "COM1", 13320, 13323);
			ConnectVM.ConnectEvent += Connect;
			ConnectVM.DisconnectEvent += Disconnect;


			_paramsNotToUpdateList = new List<string>();

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;
			_recievedMessagesQueue = new BlockingCollection<byte[]>();



			_rand = new Random((int)DateTime.Now.Ticks);
			_timerChangeValue = new System.Timers.Timer(500);
			_timerChangeValue.Elapsed += TimerChangeValueElapsedEventHandler;
			//	_timerChangeValue.Start();


			SetValuesToParams();


			HandleReceiveMessages();
		}

		#endregion Constructor

		#region Methods

		

		#region Set Values

		private void SetValuesToParams()
		{

			int value = _rand.Next(0, 1000);
			foreach (DeviceParameterData data in ParametersList)
			{
				if(!(data is PowerSupplayBK_ParamData deviceData))
					continue;

				if (_paramsNotToUpdateList.Contains(deviceData.Name))
					continue;

				if(deviceData.Command == "MEASure:SCALar:VOLTage:ALL:DC?" ||
					deviceData.Command == "VAPPLY:VOLTage:LEVel?" ||
					deviceData.Command == "APPLY:CURRent:LEVel?" ||
					deviceData.Command == "APPLY:OUTput?")
				{
					string str = "";
					for(int i = 0; i < 3; i++)
					{
						str += value++ + ",";
					}

					str = str.TrimEnd(',');

					deviceData.Value = str;
				}
				else
					//deviceData.Value = value++;




				deviceData.GetSetVisibility = System.Windows.Visibility.Collapsed;

				System.Threading.Thread.Sleep(1);
			}


			
		}


		private void TimerChangeValueElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			SetValuesToParams();
		}


		#endregion Set Values

		#region Connect / Disconnect

		private void Connect()
		{
			if (_serialConncetViewModel.IsUdpSimulation == false)
			{
				_commService = new SerialService(_serialConncetViewModel.SelectedCOM, _serialConncetViewModel.SelectedBaudrate);
			}
			else 
			{
				_commService = new SerialUdpSimulationService(_serialConncetViewModel.RxPort, _serialConncetViewModel.TxPort, _serialConncetViewModel.Address);
			}




			_commService.Init(true);

			_commService.MessageReceivedEvent += MessageReceivedEventHandler;

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

		#endregion Connect / Disconnect

		#region Receive

		private void MessageReceivedEventHandler(byte[] buffer)
		{
			try
			{
				_recievedMessagesQueue.Add(buffer, _cancellationToken);
			}
			catch (OperationCanceledException)
			{

			}
		}

		private void HandleReceiveMessages()
		{
			Task.Run(() =>
			{
				while (!_cancellationToken.IsCancellationRequested)
				{
					
					if(_commService == null)
					{
						System.Threading.Thread.Sleep(1);
						continue;
					}
					
					string message = "";
					_commService.Read(out message);
					if (string.IsNullOrEmpty(message))
					{
						System.Threading.Thread.Sleep(1);
						continue;
					}




					PowerSupplayBK_ParamData data = null;

					if(message != "MEASure:SCALar:VOLTage:ALL:DC?") { }
					switch (message)
					{
						case "SYST:REM":
							SetParam("SYST", 1);
							break;

						case "SYST:LOC":
							SetParam("SYST", 0); 
							break;

						case "INST CH1":
							SetParam("INST", 1); 
							break;

						case "INST CH2":
							SetParam("INST", 2); 
							break;

						case "INST CH3":
							SetParam("INST", 3); 
							break;

						case "CHANnel:OUTPut OFF":
							SetParam("CHANnel:OUTPut", 0);
							break;

						case "CHANnel:OUTPut ON":
							SetParam("CHANnel:OUTPut", 1); 
							break;



						case "MEASure:SCALar:VOLTage:ALL:DC?":
						case "VAPPLY:VOLTage:LEVel?":
						case "APPLY:CURRent:LEVel?":
						case "APPLY:OUTput?":
							data = ParametersList.ToList().Find((p) => (p as PowerSupplayBK_ParamData).Command == message)
								as PowerSupplayBK_ParamData;
							if (data == null)
								continue;

							_commService.Send(data.Value as string);
							break;

						default:


							string command = "";
							string valueStr = "";
							if (message.StartsWith("VOLTage"))
							{
								valueStr = message.Replace("VOLTage ", string.Empty);
								command = "VOLTage";
							}
							else if (message.StartsWith("Current"))
							{
								valueStr = message.Replace("Current ", string.Empty);
								command = "Current";
							}

							data = ParametersList.ToList().Find((p) => (p as PowerSupplayBK_ParamData).Command.Trim() == command)
								as PowerSupplayBK_ParamData;
							if (data == null)
								continue;

							double value;
							bool res = double.TryParse(valueStr, out value);
							data.Value = value;

							break;

					}



					System.Threading.Thread.Sleep(1);

				}

			}, _cancellationToken);
		}

		private void SetParam(
			string command,
			double value)
		{
			PowerSupplayBK_ParamData data = ParametersList.ToList().Find((p) => (p as PowerSupplayBK_ParamData).Command.Trim() == command)
								as PowerSupplayBK_ParamData;
			if (data == null)
				return;
			data.Value = value;
		}


		#endregion Receive

		#endregion Methods

		#region Commands
		#endregion Commands
	}
}
