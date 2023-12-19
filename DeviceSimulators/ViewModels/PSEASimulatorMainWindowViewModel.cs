
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;
using Entities.Models;
using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.PowerSupplayBK;
using DeviceHandler.ViewModels;
using DeviceCommunicators.PowerSupplayEA;
using System.Text.RegularExpressions;
using DeviceCommunicators.Models;

namespace DeviceSimulators.ViewModels
{
    public class PSEASimulatorMainWindowViewModel : DeviceSimulatorViewModel
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

		public PSEASimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{
			
			ConnectVM = new SerialConncetViewModel(115200, "COM1", 14320, 14323);
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
				if(data is PowerSupplayEA_ParamData ea_ParamData)
					_nameToParameter.Add(ea_ParamData.Cmd, data);
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
				if(!(data is PowerSupplayEA_ParamData deviceData))
					continue;


				deviceData.Value = value++;


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

					if(message == "*IDN?")
					{
						_commService.Send("EA");
						continue;
					}

					DeviceParameterData parameter = GetParemeterFromMessage(message);
					if (!(parameter is PowerSupplayEA_ParamData ea_ParamData))
						continue;

					if (message.EndsWith("?"))
					{
						string cmd = ea_ParamData.Value + " " + ea_ParamData.Units;
						_commService.Send(cmd);
					
					}
					else
					{
						int index = message.IndexOf(" ");
						if (index < 0)
							continue;
						
						message = message.Substring(index + 1);

						double dVal;
						bool res = double.TryParse(message, out dVal);
						if (res) 
							parameter.Value = dVal;
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
