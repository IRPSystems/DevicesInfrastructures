
using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.Models;
using Communication.Interfaces;
using Communication.Services;
using DeviceHandler.ViewModels;
using DeviceCommunicators.Models;

namespace DeviceSimulators.ViewModels
{
    public class BTMTempLoggerSimulatorMainWindowViewModel : DeviceSimulatorViewModel
	{

		#region Fields

		private ISerialService _commService;

		private SerialConncetViewModel _serialConncetViewModel
		{
			get => ConnectVM as SerialConncetViewModel;
		}


		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private Random _rand;

		#endregion Fields

		#region Constructor

		public BTMTempLoggerSimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;


			ConnectVM = new SerialConncetViewModel(
				9600, 
				string.Empty, 
				15320, 
				15323,
				"", 
				"", 
				"");
			ConnectVM.ConnectEvent += Connect;
			ConnectVM.DisconnectEvent += Disconnect;

			_rand = new Random((int)DateTime.Now.ToFileTimeUtc());
		}

		#endregion Constructor





		#region Methods



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

			ConnectVM.IsConnectButtonEnabled = false;
			ConnectVM.IsDisconnectButtonEnabled = true;

			SendLoop();
		}

		public override void Disconnect()
		{
			_cancellationTokenSource.Cancel();

			if (_commService != null)
				_commService.Dispose();

			ConnectVM.IsConnectButtonEnabled = true;
			ConnectVM.IsDisconnectButtonEnabled = false;
		}

		#endregion Connect / Disconnect

		private void SendLoop()
		{
			Task.Run(() =>
			{
				while (!_cancellationToken.IsCancellationRequested)
				{
					int val = _rand.Next(-100, 100);
					string message = "";
					for (int i = 1; i <= 12; i++)
					{
						string hexi = i.ToString("X");
						message += "\u00024" + hexi + "0101" + val.ToString("00000000") + "\r";
						val++;
						System.Threading.Thread.Sleep(1);
					}

					_commService.Send(message);

					System.Threading.Thread.Sleep(1);

				}
			}, _cancellationToken);
		}

		#endregion Methods

		#region Commands


		#endregion Commands
	}
}
