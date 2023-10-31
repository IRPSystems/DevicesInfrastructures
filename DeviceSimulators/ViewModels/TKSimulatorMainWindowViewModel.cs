
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;
using Entities.Models;
using Communication.Interfaces;
using Communication.Services;
using DeviceHandler.ViewModels;

namespace DeviceSimulators.ViewModels
{
    public class TKSimulatorMainWindowViewModel : DeviceSimulatorViewModel
	{

		#region Fields

		private ISerialService _commService;

		private System.Timers.Timer _timerChangeValue;
		private Random _rand;

		private Dictionary<string, DeviceParameterData> _nameToParameter;

		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private BlockingCollection<byte[]> _recievedMessagesQueue;

		private SerialConncetViewModel _serialConncetViewModel
		{
			get => ConnectVM as SerialConncetViewModel;
		}

		#endregion Fields

		#region Constructor

		public TKSimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{
			
			ConnectVM = new SerialConncetViewModel(57600, "COM1", 17320, 17323);
			ConnectVM.ConnectEvent += Connect;
			ConnectVM.DisconnectEvent += Disconnect;


			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;
			_recievedMessagesQueue = new BlockingCollection<byte[]>();



			_rand = new Random((int)DateTime.Now.Ticks);
			_timerChangeValue = new System.Timers.Timer(500);
			_timerChangeValue.Elapsed += TimerChangeValueElapsedEventHandler;
			//	_timerChangeValue.Start();

			_nameToParameter = new Dictionary<string, DeviceParameterData>();
			foreach(DeviceParameterData data in ParametersList) 
			{
				_nameToParameter.Add(data.Name, data);
			}

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
				data.Value = value++;
				data.GetSetVisibility = System.Windows.Visibility.Collapsed;

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

					message = message.Replace("\r\n", string.Empty);

					if(message == "MEAS:ALL?")
					{
						string response = "1234";

						DeviceParameterData parameter = GetParemeterFromMessage("Torque");
						response += "|" + parameter.Value;
						parameter = GetParemeterFromMessage("Speed");
						response += "|" + parameter.Value;

						response += "|4321\r";

						_commService.Send(response);
					}
					else if (message == "OUTP:TARE:AUTO")
					{
						_commService.Send("0\r");
					}


					System.Threading.Thread.Sleep(1);

				}

			}, _cancellationToken);
		}

		private DeviceParameterData GetParemeterFromMessage(string message)
		{
			message = message.Replace("?", string.Empty);
			int index = message.IndexOf(" ");
			if(index >= 0) 
			{
				message = message.Substring(0, index);				
			}

			if (_nameToParameter.ContainsKey(message))
				return _nameToParameter[message];

			return null;
		}

		#endregion Receive

		#endregion Methods

		#region Commands
		#endregion Commands
	}
}
