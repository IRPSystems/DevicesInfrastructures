
using System.Collections.Concurrent;
using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.Models;
using Communication.Interfaces;
using Communication.Services;
using DeviceHandler.ViewModels;
using System.Windows;
using DeviceCommunicators.Models;

namespace DeviceSimulators.ViewModels
{
    public class SwitchRelaySimulatorMainWindowViewModel : DeviceSimulatorViewModel
	{

		public BitwiseNumberDisplayData SwitchesStatus { get; set; }

		#region Fields

		private ITcpStaticService _commService;

		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private BlockingCollection<byte[]> _recievedMessagesQueue;

		private TcpConncetViewModel _tcpConncetViewModel
		{
			get => ConnectVM as TcpConncetViewModel;
		}


		#endregion Fields

		#region Constructor

		public SwitchRelaySimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{

			ConnectVM = new TcpConncetViewModel(4196, "", 16323, 16320);
			ConnectVM.ConnectEvent += Connect;
			ConnectVM.DisconnectEvent += Disconnect;

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;
			_recievedMessagesQueue = new BlockingCollection<byte[]>();


			SwitchesStatus = new BitwiseNumberDisplayData(false, false);


			HandleReceiveMessages();
		}

		#endregion Constructor

		#region Methods



		#region Set Values


		#endregion Set Values

		#region Connect / Disconnect

		private void Connect()
		{
			_commService = new TcpStaticService(_tcpConncetViewModel.Address, _tcpConncetViewModel.Port);



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
					try
					{

						if (_commService == null)
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


						string[] msgPartsList = message.Split(',');
						switch (msgPartsList[0])
						{
							case "RELAY-SET-255":
								HandleSingleChannel(message, msgPartsList);
								break;

							case "RELAY-AON-255":
							case "RELAY-AOF-255":
								HandleAllChannel(message, msgPartsList);
								break;

							case "RELAY-SET_ALL-255":
								HandleRelayRegisters(message, msgPartsList);
								break;

							case "RELAY-READ-255":
								HandleSwitchStatus(msgPartsList);
								break;

							case "RELAY-STATE-255":
								HandleAllRelayStatus(msgPartsList);
								break;
						}

						System.Threading.Thread.Sleep(1);
					}
					catch(Exception ex) 
					{
						MessageBox.Show("Exception in receiving a message \r\n\r\n" + ex);
					}

				}

			}, _cancellationToken);
		}

		private void HandleSingleChannel(
			string message,
			string[] msgPartsList)
		{
			if (msgPartsList.Length < 3)
				return;

			string channel = msgPartsList[1];
			string value = msgPartsList[2];

			int channelIndex;
			int.TryParse(channel, out channelIndex);
			channelIndex--;

			if (value == "1")
				SwitchesStatus.BinaryValue[channelIndex].Value = true;
			if (value == "0")
				SwitchesStatus.BinaryValue[channelIndex].Value = false;

			string returnMessage = message + ",OK";
			_commService.Send(returnMessage);
		}

		private void HandleAllChannel(
			string message,
			string[] msgPartsList)
		{
			if (msgPartsList.Length < 3)
				return;

			if (msgPartsList[0].Contains("AON"))
				SwitchesStatus.NumericValue = 0xFFFFFFFF;
			else if (msgPartsList[0].Contains("AOF"))
				SwitchesStatus.NumericValue = 0;


			string returnMessage = message + ",OK";
			_commService.Send(returnMessage);
		}

		private void HandleRelayRegisters(
			string message,
			string[] msgPartsList)
		{
			if (msgPartsList.Length < 2)
				return;

			SwitchesStatus.NumericValue = 0;

			for (int i = 0; i < 4; i++)
			{
				byte val;
				byte.TryParse(msgPartsList[i + 1], out val);
				SwitchesStatus.NumericValue += (ulong)(val << (i * 8));
			}


			string returnMessage = message + ",OK";
			_commService.Send(returnMessage);
		}

		private void HandleSwitchStatus(string[] msgPartsList)
		{
			string channel = msgPartsList[1];
			int channelIndex;
			int.TryParse(channel, out channelIndex);
			channelIndex--;

			string value = "0";
			if (SwitchesStatus.BinaryValue[channelIndex].Value)
				value = "1";

			string message = "RELAY-READ-255," + channel + "," + value + ",OK";
			_commService.Send(message);
		}

		private void HandleAllRelayStatus(string[] msgPartsList)
		{
			byte byte1 = (byte)SwitchesStatus.NumericValue;
			byte byte2 = (byte)(SwitchesStatus.NumericValue >> 8);
			byte byte3 = (byte)(SwitchesStatus.NumericValue >> 16);
			byte byte4 = (byte)(SwitchesStatus.NumericValue >> 24);

			string message = "RELAY-STATE-255," + byte4 + "," + byte3 + "," + byte2 + "," + byte1 + ",OK";
			_commService.Send(message);
		}

		#endregion Receive

		#endregion Methods

		#region Commands
		#endregion Commands
	}
}
