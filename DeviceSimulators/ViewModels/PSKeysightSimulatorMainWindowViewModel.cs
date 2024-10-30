
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
using DeviceCommunicators.PowerSupplayKeysight;
using System.Net.Sockets;
using System.Net;

namespace DeviceSimulators.ViewModels
{
    public class PSKeysightSimulatorMainWindowViewModel : DeviceSimulatorViewModel
	{

		#region Fields

		private ITcpStaticService _commService;

		private System.Timers.Timer _timerChangeValue;
		private Random _rand;

		private List<string> _paramsNotToUpdateList;


		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private BlockingCollection<byte[]> _recievedMessagesQueue;

		private TcpConncetViewModel _tcpConncetViewModel
		{
			get => ConnectVM as TcpConncetViewModel;
		}

		private string Address { get; set; }

		#endregion Fields

		#region Constructor

		public PSKeysightSimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{
			GetIpAddress();


			ConnectVM = new TcpConncetViewModel(5025, 21350, 21353, Address);
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

			ParametersList.Add(new PowerSupplayKeysight_ParamData()
			{
				Command = "*IDN",
				Name = "Identification"
			});

			SetValuesToParams();


			HandleReceiveMessages();
		}



		#endregion Constructor

		#region Methods

		private void GetIpAddress()
		{
			Address = null;
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					Address = ip.ToString();
				}
			}
		}


		#region Set Values

		private void SetValuesToParams()
		{

			int value = _rand.Next(0, 1000);
			foreach (DeviceParameterData data in ParametersList)
			{
				if(!(data is PowerSupplayKeysight_ParamData deviceData))
					continue;

				if (_paramsNotToUpdateList.Contains(deviceData.Name))
					continue;

				
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
			if (_tcpConncetViewModel.IsUdpSimulation == false)
			{
				_commService = new TcpStaticService(Address, _tcpConncetViewModel.Port);
					//T(_tcpConncetViewModel.SelectedCOM, _tcpConncetViewModel.SelectedBaudrate);
			}
			else 
			{
				_commService = new TcpUdpSimulationService(_tcpConncetViewModel.RxPort, _tcpConncetViewModel.TxPort, _tcpConncetViewModel.Address);
			}




			_commService.Init(true);


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
			PowerSupplayKeysight_ParamData data = ParametersList.ToList().Find((p) => (p as PowerSupplayKeysight_ParamData).Command == msg)
				as PowerSupplayKeysight_ParamData;
			if (data == null)
				return;

			_commService.Send(data.Value as string);
		}



		private void HandleSetValue(
			string message)
		{
			string[] splitMessage = message.Split(" ");

			string command = splitMessage[0];
			PowerSupplayKeysight_ParamData data = ParametersList.ToList().Find((p) => (p as PowerSupplayKeysight_ParamData).Command.Trim() == command)
								as PowerSupplayKeysight_ParamData;
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
