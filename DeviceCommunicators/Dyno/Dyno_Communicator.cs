using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Model;
using DeviceCommunicators.Models;
using Entities.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace DeviceCommunicators.Dyno
{
    public class Dyno_Communicator: DeviceCommunicator
    {
		#region Fields

		public ConcurrentDictionary<int, string> _dynoErrorToDescription;



		private const int _maxNumOfMessages = 500;

		private BlockingCollection<byte[]> _buffersPool;

		private System.Timers.Timer _poolBuildTimer;

		private bool _isTimeout;
		private System.Timers.Timer _timeoutTimer;


		private List<TimeSpan> _commTimeList;

		private uint _nodeId;



		private ConcurrentDictionary<(int,byte), byte[]> _messagesDict;

		#endregion Fields

		#region Properties

		protected CanService CanService
		{
			get => CommService as CanService;
		}

		#endregion Properties

		#region Constructor

		public Dyno_Communicator()
		{

			InitErrorsDictionary();

			_buffersPool = new BlockingCollection<byte[]>();

			_poolBuildTimer = new System.Timers.Timer(500);
			_poolBuildTimer.Elapsed += PoolBuildTimerElapsed;

			_commTimeList = new List<TimeSpan>();
		}

		#endregion Constructor

		#region Methods

		protected override void InitErrorsDictionary()
		{
			_dynoErrorToDescription = new ConcurrentDictionary<int, string>();
			_dynoErrorToDescription[0x05030000] = "Toggle bit not changed";
			_dynoErrorToDescription[0x05040000] = "SDO protocol expired";
			_dynoErrorToDescription[0x05040001] = "Invalid or unknown client/server command specifier";
			_dynoErrorToDescription[0x05040002] = "Invalid block size (only block mode)";
			_dynoErrorToDescription[0x05040003] = "Invalid sequence number (only block mode)";
			_dynoErrorToDescription[0x05040004] = "CRC error (only block mode)";
			_dynoErrorToDescription[0x05040005] = "Not sufficient memory";
			_dynoErrorToDescription[0x06010000] = "Object access not supported";
			_dynoErrorToDescription[0x06010001] = "Attempt to read a write-only object";
			_dynoErrorToDescription[0x06010002] = "Attempt to write to a read-only object";
			_dynoErrorToDescription[0x06020000] = "Object not listed in object directory";
			_dynoErrorToDescription[0x06040041] = "Object not mapped to PDO";
			_dynoErrorToDescription[0x06040042] = "Number and length of objects to be transferred longer than PDO length.";
			_dynoErrorToDescription[0x06040043] = "General parameter incompatibility";
			_dynoErrorToDescription[0x06040047] = "General internal device incompatibility";
			_dynoErrorToDescription[0x06060000] = "Access denied because of hardware error";
			_dynoErrorToDescription[0x06070010] = "Unsuitable data type, unsuitable service parameter length";
			_dynoErrorToDescription[0x06070012] = "Unsuitable data type, service parameter length exceeded";
			_dynoErrorToDescription[0x06070013] = "Unsuitable data type, service parameter length not long enough";
			_dynoErrorToDescription[0x06090011] = "Subindex does not exist";
			_dynoErrorToDescription[0x06090030] = "Parameter value range exceeded";
			_dynoErrorToDescription[0x06090031] = "Parameter values too high";
			_dynoErrorToDescription[0x06090032] = "Parameter values too low";
			_dynoErrorToDescription[0x06090036] = "Maximum value falls below minimum value";
			_dynoErrorToDescription[0x08000000] = "General error";
			_dynoErrorToDescription[0x08000020] = "Data cannot be transferred/saved for application.";
			_dynoErrorToDescription[0x08000021] = "Data cannot be transferred/saved for application due to local control.";
			_dynoErrorToDescription[0x08000022] = "Data cannot be transferred/saved for application due to current device status.";
			_dynoErrorToDescription[0x08000023] = "Dynamic generation of object directory failed or no object directory available (e.g. object ";
		}


		public void Init(
			string canAdapterType,
			int baudrate,
			uint nodeId,
			ushort hwId = 0,
			int rxPort = 0,
			int txPort = 0,
			string address = "")
        {
			LoggerService.Inforamtion(
				this, 
				"Initiating communication - Adapter: " + canAdapterType);

			_nodeId = nodeId;


			if (canAdapterType == "PCAN")
			{
				CommService = new CanPCanService(baudrate, 0x600 + _nodeId, hwId, 0x580 + _nodeId, 0x600 + _nodeId);
			}
			else if (canAdapterType == "UDP Simulator")
			{
				CommService = new CanUdpSimulationService(baudrate, 0x600 + _nodeId, rxPort, txPort, address, 0x580 + _nodeId, 0x600 + _nodeId);
			}


			CanService.RegisterId(0x580 + _nodeId, MessageReceivedEventHandler);

			CommService.Init(false);
			CommService.Name = "Dyno_Communicator";

			_timeoutTimer = new System.Timers.Timer(2000);
			_timeoutTimer.Elapsed += TimoutElapsedEventHandler;

			_poolBuildTimer.Start();


			FireConnectionEvent();

			InitBase();
		}

		public void InitMessageDict(DeviceData device)
		{
			_messagesDict = new ConcurrentDictionary<(int, byte), byte[]>();

			foreach (DeviceParameterData param in device.ParemetersList)
			{
				if (!(param is Dyno_ParamData dynoParam))
					continue;

				_messagesDict[(dynoParam.Index, dynoParam.SubIndex)] = null;
			}
		}


		public override void Dispose()
		{
			LoggerService.Inforamtion(this, "Disposing");

			_isTimeout = true;

			if(_cancellationTokenSource != null) 
				_cancellationTokenSource.Cancel();

			if (_poolBuildTimer != null)
				_poolBuildTimer.Stop();

			if (_timeoutTimer != null)
				_timeoutTimer.Stop();

			FireConnectionEvent();

			base.Dispose();


			//try
			//{

			//	LoggerService.Inforamtion(this, "Dyno time");
			//	foreach (TimeSpan time in _commTimeList)
			//	{
			//		LoggerService.Debug(this, time.TotalMilliseconds.ToString());
			//	}
			//}
			//catch { }

		}



		private void HandleSetParams(CommunicatorIOData data)
		{


			if (!(data.Parameter is Dyno_ParamData dynoParam))
				return;

			LoggerService.Debug(this, 
				"Setting parameter - Nane: " + dynoParam +
				" - Value: " + data.Value);

			int uniqueID = Dyno_ParamData.BaseUniqueParamID - dynoParam.Index;
			int val = uniqueID << 8;
			int uniqueParamID =
				Dyno_ParamData.SetFirstByte +
				val;


			double value = data.Value;
			value = value * (1 / dynoParam.Coefficient);

			if (value < 0)
			{
				value = Math.Pow(2, 32) + value;
			}


			byte[] buffer = _buffersPool.Take(_cancellationToken);

			int index = 0;

			SetDataToBuffer(uniqueParamID, buffer, index, 3);
			index += 3;

			buffer[index] = dynoParam.SubIndex;
			index++;


			buffer[index++] = Convert.ToByte(value % 256);
			value = (value - value % 256) / 256;
			buffer[index++] = Convert.ToByte(value % 256);
			value = (value - value % 256) / 256;
			buffer[index++] = Convert.ToByte(value % 256);
			value = (value - value % 256) / 256;
			buffer[index++] = Convert.ToByte(value);

			CanService.Send(buffer);

			
			
			WaitForResponse(
				dynoParam,
				data.Callback,
				true);
			


		}

       

		private void HandleGetParams(CommunicatorIOData data)
		{
			

			if (!(data.Parameter is Dyno_ParamData dynoParam))
				return;

			LoggerService.Debug(this,
				"Getting parameter - Nane: " + dynoParam);

			int uniqueID = Dyno_ParamData.BaseUniqueParamID - dynoParam.Index;
			int val = uniqueID << 8;
			int uniqueParamID =
				Dyno_ParamData.GetFirstByte +
				val;

			byte[] buffer = _buffersPool.Take(_cancellationToken);

			int index = 0;

			SetDataToBuffer(uniqueParamID, buffer, index, 3);
			index += 3;

			buffer[index] = dynoParam.SubIndex;
			index++;


			CommService.Send(buffer);

			

			
			WaitForResponse(
				dynoParam,
				data.Callback,
				false);
			

		}
		protected override CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
		{

			if (data.IsSet)
			{
				HandleSetParams(data);
			}
			else
			{
				HandleGetParams(data);
			}

			return CommunicatorResultEnum.OK;

		}

		#region Response

		private object _lockObj = new object();
		private void WaitForResponse(
			Dyno_ParamData dynoParam,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback,
			bool isSet,
			int setValue = 0)
		{
			lock (_lockObj)
			{
				
				_isTimeout = false;
				_timeoutTimer.Start();

				byte[] readBuffer = null;
				
				while (readBuffer == null)
				{
					if (_isTimeout)
						break;


					if (_messagesDict.ContainsKey((dynoParam.Index, dynoParam.SubIndex)) == false)
						continue;

					readBuffer = _messagesDict[(dynoParam.Index, dynoParam.SubIndex)];

					System.Threading.Thread.Sleep(1);

				}
				


				if (readBuffer != null)
				{
					byte commandByte = readBuffer[0];
					commandByte = (byte)(commandByte >> 4);
					switch(commandByte)
					{
						case 0x4:
							GetResponse(
								dynoParam,
								readBuffer,
								callback);
							break;

						case 0x6:
							SetResponse(
								dynoParam,
								readBuffer,
								callback);
							break;

						case 0x8:
							ErrorResponse(
								dynoParam,
								readBuffer,
								callback);
							break;
					}

				}

				if (readBuffer == null || _isTimeout)
				{
					callback?.Invoke(dynoParam, CommunicatorResultEnum.NoResponse, null);
				}

				_timeoutTimer.Stop();
				_isTimeout = false;

				
			}
		}

		

		private void ErrorResponse(
			Dyno_ParamData dynoParam,
			byte[] readBuffer,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			

			int errCode = (int)GetDataFromBuffer(readBuffer, 4, 4);

			string errDescription = "Unknown error code";
			if (_dynoErrorToDescription.ContainsKey(errCode)) 
				errDescription = _dynoErrorToDescription[errCode];

			LoggerService.Debug(this,
				"Error response - Nane: " + dynoParam +
				" - " + errDescription);

			callback?.Invoke(dynoParam, CommunicatorResultEnum.Error, errDescription);
		}

		private void SetResponse(
			Dyno_ParamData dynoParam,
			byte[] readBuffer,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			LoggerService.Debug(this,
				"Response for set - Nane: " + dynoParam);

			
						
			callback?.Invoke(dynoParam, CommunicatorResultEnum.OK, null);
		}

		private void GetResponse(
			Dyno_ParamData dynoParam,
			byte[] readBuffer,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			LoggerService.Debug(this,
				"Response for get - Nane: " + dynoParam);

			

			double value = (int)GetDataFromBuffer(readBuffer, 4, 4);

			

			
			byte commandByte = readBuffer[0];
			switch(commandByte)
			{
				case 0x43: commandByte = 32; break;
				case 0x4B: commandByte = 16; break;
				case 0x4F: commandByte = 1; break;
			}

			if (commandByte > 1)
			{

				if (value >= Math.Pow(2, (commandByte - 1)))
				{
					value -= Math.Pow(2, commandByte);
				}
			}

			dynoParam.Value = value / (1 / dynoParam.Coefficient);
			callback?.Invoke(dynoParam, CommunicatorResultEnum.OK, null);
		}

		#endregion Response


		public static void SetDataToBuffer(
			long val,
			byte[] buffer,
			int startIndex,
			int length)
		{
			for (int i = 0; i < length; i++)
			{
				buffer[startIndex + i] = (byte)(val >> (i * 8));
			}
		}



		public static long GetDataFromBuffer(byte[] buffer, int startIndex, int length)
		{
			long l = 0;
			for (int i = 0; i < length; i++)
			{
				l += buffer[startIndex + i] << (i * 8);
			}

			return l;
		}

		private void TimoutElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			_isTimeout = true;
		}


		public static async Task WaitWhile(Func<bool> condition, int frequency = 25, int timeout = -1)
		{
			var waitTask = Task.Run(async () =>
			{
				while (condition()) await Task.Delay(frequency);
			});

			if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
				throw new TimeoutException();
		}

		private void PoolBuildTimerElapsed(object sender, ElapsedEventArgs e)
		{
			while (_buffersPool.Count < _maxNumOfMessages)
			{
				_buffersPool.Add(new byte[8], _cancellationToken);
                System.Threading.Thread.Sleep(1);
            }
		}




		private void MessageReceivedEventHandler(byte[] buffer)
		{
			if (_messagesDict == null)
			{
				return;
			}

			DateTime start = DateTime.Now;

			int index = 1;
			int uniqueParamID = (int)GetDataFromBuffer(buffer, index, 2);
			index += 2;

			int subIndex = (byte)GetDataFromBuffer(buffer, index, 1);


			int uniqueId = Dyno_ParamData.BaseUniqueParamID - uniqueParamID;

			byte commandByte = buffer[0];
			commandByte = (byte)(commandByte >> 4);
			if (commandByte == 0x8)
			{
				uniqueId = Math.Abs(uniqueId);
			}

			_messagesDict[(uniqueId, (byte)subIndex)] = buffer;

			TimeSpan diff = DateTime.Now - start;
		}


		#endregion Methods

	}
}
