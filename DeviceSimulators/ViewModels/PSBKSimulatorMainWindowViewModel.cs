
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
using DeviceCommunicators.Models;

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

			ParametersList.Add(new PowerSupplayBK_ParamData()
			{
				Command = "*IDN",
				Name = "Identification"
			});

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

				if(deviceData.Command == "MEASure:SCALar:VOLTage:ALL:DC" ||
					deviceData.Command == "VAPPLY:VOLTage:LEVel" ||
					deviceData.Command == "APPLY:CURRent:LEVel" ||
					deviceData.Command == "APPLY:OUTput")
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
					deviceData.Value = (value++).ToString();




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




					

					message = message.Trim('\n');

					if (message.EndsWith("?"))
					{
						HandleGetValue(message);
					}
					else
					{
						HandleSetValue(message);
					}



					System.Threading.Thread.Sleep(1);

				}

			}, _cancellationToken);
		}

		private void HandleGetValue(string message)
		{
			string msg = message.Trim('?');
			PowerSupplayBK_ParamData data = ParametersList.ToList().Find((p) => (p as PowerSupplayBK_ParamData).Command == msg)
				as PowerSupplayBK_ParamData;
			if (data == null)
				return;

			_commService.Send(data.Value as string);
		}



		private void HandleSetValue(
			string message)
		{
			string[] splitMessage = message.Split(" ");

			string command = splitMessage[0];
			PowerSupplayBK_ParamData data = ParametersList.ToList().Find((p) => (p as PowerSupplayBK_ParamData).Command.Trim() == command)
								as PowerSupplayBK_ParamData;
			if (data == null)
				return;

			data.Value = splitMessage[1];
		}


		#endregion Receive

		#endregion Methods

		#region Commands
		#endregion Commands
	}
}
