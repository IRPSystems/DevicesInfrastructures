
using Communication.Services;
using System;
using System.Windows;
using Entities.Models;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using DeviceCommunicators.Dyno;
using System.Linq;
using System.Timers;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DeviceCommunicators.MCU;
using DeviceHandler.ViewModels;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceSimulators.ViewModels
{
	public class MCUSimulatorMainWindowViewModel: DeviceSimulatorViewModel
	{
		public class CanMessageData: ObservableObject
		{
			public uint Id { get; set; }
			public long Message { get; set; }
		}

		#region Properties

		public ObservableCollection<CanMessageData> CanMessagesList { get; set; }

		#endregion Properties

		#region Fields

		private const uint _func1ID = 0x0C3D69;
		private const uint _func2ID = 0x5FE4C3;
		private const uint _func3ID = 0x0738A6;
		private const uint _func4ID = 0x6B89DD;

		private CanService _commService;

		private Dictionary<byte[], MCU_ParamData> _md5ToParam;

		private System.Timers.Timer _timerChangeValue;
		private Random _rand;

		private List<string> _paramsNotToUpdateList;




		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private BlockingCollection<(uint,byte[])> _recievedMessagesQueue;

		//private MCU_DeviceData _mcu_Device;

		private CanConnectViewModel _canConnectViewModel
		{
			get => ConnectVM as CanConnectViewModel;
		}

		private List<uint> _functionsIdList;

		#endregion Fields

		#region Constructor

		public MCUSimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{
			if (deviceData.DeviceType == Entities.Enums.DeviceTypesEnum.MCU)
				ConnectVM = new CanConnectViewModel(250000, 171, 12220, 12223);
			else if (deviceData.DeviceType == Entities.Enums.DeviceTypesEnum.MCU_B2B)
				ConnectVM = new CanConnectViewModel(250000, 171, 19220, 19223);
			ConnectVM.ConnectEvent += Connect;
			ConnectVM.DisconnectEvent += Disconnect;

			

			_paramsNotToUpdateList = new List<string>();

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;
			_recievedMessagesQueue = new BlockingCollection<(uint, byte[])>();



			_rand = new Random((int)DateTime.Now.Ticks);
			_timerChangeValue = new System.Timers.Timer(500);
			_timerChangeValue.Elapsed += TimerChangeValueElapsedEventHandler;
			_timerChangeValue.Start();


			CanMessagesList = new ObservableCollection<CanMessageData>();

			_functionsIdList = new List<uint>()
			{
				_func1ID, _func2ID, _func3ID, _func4ID,
			};

			BuildLookupTable();
			HandleReceiveMessages();
		}



		#endregion Constructor

		#region Methods

		public void Dispose()
		{
			Disconnect();
			_cancellationTokenSource.Cancel();
			_timerChangeValue.Stop();
		}

		private void BuildLookupTable()
		{
			if (ParametersList == null || ParametersList.Count == 0)
				return;

			try
			{
				_md5ToParam = new Dictionary<byte[], MCU_ParamData>();
				using (var md5 = MD5.Create())
				{
					foreach (DeviceParameterData data in ParametersList)
					{
						if (!(data is MCU_ParamData mcu_ParamData))
							continue;

						if (mcu_ParamData.Cmd == null)
							continue;

						try
						{
							byte[] id = new byte[3];
							Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(mcu_ParamData.Cmd)), 0, id, 0, 3);
							_md5ToParam.Add(id, mcu_ParamData);
						}
						catch{ }
					}

					byte[] idEmpty = new byte[3];
					Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes("")), 0, idEmpty, 0, 3);
					_md5ToParam.Add(idEmpty, new MCU_ParamData() { Cmd = "", Name = "Empty command" });

					idEmpty = new byte[3];
					Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes("save_param")), 0, idEmpty, 0, 3);
					_md5ToParam.Add(idEmpty, new MCU_ParamData() { Cmd = "save_param", Name = "save param" });
				}
			}
			catch { }
		}

		#region Set Values

		private void SetValuesToParams()
		{
			if (ParametersList == null)
				return;

			int value = _rand.Next(0, 1000);
			foreach (DeviceParameterData data in ParametersList)
			{
				if (_paramsNotToUpdateList.Contains(data.Name))
					continue;

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
			if (_canConnectViewModel.SelectedAdapter == "PCAN")
			{
				_commService = new CanPCanService(_canConnectViewModel.SelectedBaudrate, _canConnectViewModel.NodeID, 
					_canConnectViewModel.GetSelectedHWId(_canConnectViewModel.SelectedHwId));
			}
			else if (_canConnectViewModel.SelectedAdapter == "UDP Simulator")
			{
				_commService = new CanUdpSimulationService(_canConnectViewModel.SelectedBaudrate, _canConnectViewModel.NodeID, _canConnectViewModel.RxPort, _canConnectViewModel.TxPort, _canConnectViewModel.Address);
			}




			_commService.Init(true);
			_commService.Name = "MCUSimulator";

			_commService.CanMessageReceivedEvent += MessageReceivedEventHandler;
			_commService.ErrorEvent += ErrorEventHendler;

			ConnectVM.IsConnectButtonEnabled = false;
			ConnectVM.IsDisconnectButtonEnabled = true;
		}

		public override void Disconnect()
		{
			if (_commService == null)
				return;

			_commService.Dispose();

			_commService.CanMessageReceivedEvent -= MessageReceivedEventHandler;
			_commService.ErrorEvent -= ErrorEventHendler;

			ConnectVM.IsConnectButtonEnabled = true;
			ConnectVM.IsDisconnectButtonEnabled = false;
		}

		#endregion Connect / Disconnect

		#region Receive

		private void MessageReceivedEventHandler(uint node, byte[] buffer)
		{
			try
			{
				_recievedMessagesQueue.Add((node,buffer), _cancellationToken);
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
					(uint,byte[]) data;

					try
					{

						try
						{
							data = _recievedMessagesQueue.Take(_cancellationToken);
						}
						catch (OperationCanceledException)
						{
							continue;
						}

						byte[] buffer = data.Item2;
						uint id = data.Item1;

						if (buffer == null)
							continue;

						if (id != 0xAB)
						{
							HandleCanMessage(id, buffer);
							continue;
						}

						byte[] idBuffer = new byte[3];
						Array.Copy(buffer, idBuffer, 3);






						byte[] idBufferForNumber = new byte[4];
						byte[] idBuffer1= new byte[3];
						Array.Copy(idBuffer, idBuffer1, 3);
						Array.Reverse(idBuffer1);
						Array.Copy(idBuffer1, idBufferForNumber, 3);

						uint parameterId = BitConverter.ToUInt32(idBufferForNumber, 0);
						if (_functionsIdList.IndexOf(parameterId) >= 0)
						{
							HandleRecording(parameterId);
							continue;
						}


						if (_md5ToParam == null)
							continue;



						MCU_ParamData param = null;
						foreach (KeyValuePair<byte[], MCU_ParamData> keyValuePair in _md5ToParam)
						{
							if (Enumerable.SequenceEqual(keyValuePair.Key, idBuffer))
							{
								param = keyValuePair.Value;
								break;
							}
						}


						if (param == null)
							continue;

						


						byte info = buffer[3];


						if ((info & 0x02) == 0x02)
						{

							if (Application.Current == null)
								continue;

							byte[] valueBuff = new byte[4];
							Array.Copy(buffer, 4, valueBuff, 0, 4);
							Array.Reverse(valueBuff);

							int val = (int)Dyno_Communicator.GetDataFromBuffer(valueBuff, 0, 4);

							double value = Convert.ToDouble(val);
							value = value / param.Scale;

							Application.Current.Dispatcher.Invoke(() =>
							{
								param.Value = Math.Round(value);
							});

							ReturnResponse(
								param,
								buffer);

							_paramsNotToUpdateList.Add(param.Name);
						}
						else
						{
							ReturnResponse(
								param,
								buffer);
						}



						System.Threading.Thread.Sleep(1);
					}
					catch(Exception) { }

				}

			}, _cancellationToken);
		}

		private void HandleCanMessage(uint id, byte[] buffer)
		{
			try
			{
				long message = 0;
				for (int i = 0; i < buffer.Length; i++)
				{
					message += ((long)buffer[i] << (i * 8));
				}

				CanMessageData canMessage = new CanMessageData()
				{ Id = id, Message = message };

				Application.Current.Dispatcher.Invoke(() =>
				{
					CanMessagesList.Add(canMessage);
				});

			}
			catch { }
		}

		private void ReturnResponse(
			MCU_ParamData param,
			byte[] buffer)
		{
			byte[] bvalue = null;
			if (param.Value is string)
				param.Value = 0;

			double value = Convert.ToInt32(param.Value);
			value *= param.Scale;

			if (!string.IsNullOrEmpty(param.Cmd))
				bvalue = BitConverter.GetBytes(Convert.ToInt32(value));
			else
				bvalue = new byte[4];

			Array.Reverse(bvalue);
			Array.Copy(bvalue, 0, buffer, 4, 4);


			_commService.Send(buffer);
		}


		private void HandleRecording(uint id)
		{
			if (id != _func4ID)
				return;

			byte[] buffer = new byte[8];



			uint header = 0xABCD;
			Array.Copy(BitConverter.GetBytes(header), buffer, 4);
			_commService.Send(buffer, 0xAA, false);
			System.Threading.Thread.Sleep(1000);



			Random rand = new Random((int)DateTime.Now.Ticks);
			for(int i = 0; i < (512 / 2); i++) 
			{
				uint val = (uint)rand.Next(0, 1000);
				Array.Copy(BitConverter.GetBytes(val), buffer, 4);

				val = (uint)rand.Next(0, 1000);
				Array.Copy(BitConverter.GetBytes(val), 0, buffer, 4, 4);

				_commService.Send(buffer, 0xAA, false);

				System.Threading.Thread.Sleep(1);
			}
		}

		#endregion Receive

		private void ErrorEventHendler(string errorDescription)
		{
			
		}


		#endregion Methods

	}
}
