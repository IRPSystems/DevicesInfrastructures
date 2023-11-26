//#define _SAVE_TIME
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Model;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using Entities.Enums;
using Entities.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if _SAVE_TIME
using System.IO;
#endif
using System.Timers;
using TmctlAPINet;

namespace DeviceCommunicators.MCU
{
	public class MCU_Communicator : DeviceCommunicator, IDisposable
	{
#region Fields

		private const int _maxNumOfBuffers = 3000;
		private const int _maxNumOfIds = 5000;
		private const byte _negativeShift = 0;
		private const byte _negativeMask = 0x01;
		private const byte _writeMask = 0x02;
		private const byte _writeShift = 1;
		private const byte _errMask = 0xF0;
		private const byte _errShift = 4;

		private const int _getResponseRepeats = 5;

		public ConcurrentDictionary<int, string> _mcuErrorToDescription;

		private BlockingCollection<byte[]> _buffersPool;
		private BlockingCollection<byte[]> _idBuffersPool;

		private System.Timers.Timer _poolBuildTimer;

		private bool _isTimeout;
		private System.Timers.Timer _timeoutTimer;

		private  ConcurrentDictionary<uint, byte[]> _messagesDict;


#if _SAVE_TIME
		private List<(TimeSpan, string)> _commTimeList;
#endif

		#endregion Fields

		#region Properties

		public CanService CanService
		{
			get => CommService as CanService;
		}

#endregion Properties

#region Constructor

		public MCU_Communicator()
		{

			_buffersPool = new BlockingCollection<byte[]>();
			_idBuffersPool = new BlockingCollection<byte[]>();

			_poolBuildTimer = new System.Timers.Timer(500);
			_poolBuildTimer.Elapsed += PoolBuildTimerElapsed;


#if _SAVE_TIME
			_commTimeList = new List<(TimeSpan, string)>();
#endif
		}

#endregion Constructor

#region Methods

		protected override void InitErrorsDictionary()
		{
			_mcuErrorToDescription = new ConcurrentDictionary<int, string>();
			_mcuErrorToDescription[0] = "OK";
			_mcuErrorToDescription[1] = "Value out of range";
			_mcuErrorToDescription[2] = "Command is not legal";
			_mcuErrorToDescription[3] = "Write permision error";
			_mcuErrorToDescription[4] = "Invalid message ID received";
		}

		public void Init(
			string canAdapterType,
			int baudrate,
			uint nodeId,
			string hwId = "",
			uint syncId = 0xAB, 
			uint asyncId = 0xAA,
			int rxPort = 0,
			int txPort = 0,
			string address = "")
		{
			LoggerService.Inforamtion(
				this,
				"Initiating communication - Adapter: " + canAdapterType);

			if (canAdapterType == "PCAN")
			{
				CommService = new CanPCanService(baudrate, nodeId, CanPCanService.GetHWId(hwId), asyncId, syncId);
			}
			if (canAdapterType.ToUpper() == "IXXAT")
			{
				CommService = new CanIxxatService(baudrate, nodeId, CanIxxatService.GetDeviceId(hwId), asyncId, syncId);
			}
			else if (canAdapterType == "UDP Simulator")
			{
				CommService = new CanUdpSimulationService(baudrate, nodeId, rxPort, txPort, address, asyncId, syncId);
			}


			CanService.RegisterId(syncId, SyncMessageHandler);
			CanService.RegisterId(asyncId, AsyncMessageHandler);


			CommService.Init(false);
			CommService.Name = "MCU_Communicator";


			_timeoutTimer = new System.Timers.Timer(50);
			_timeoutTimer.Elapsed += TimoutElapsedEventHandler;

			_poolBuildTimer.Start();

			FireConnectionEvent();

			InitBase();
		}

		public void InitMessageDict(DeviceData device)
		{
			_messagesDict = new ConcurrentDictionary<uint, byte[]>();

			foreach(DeviceParameterData param in device.ParemetersList)
			{
				if (!(param is MCU_ParamData mcuParam))
					continue;

				if (mcuParam.Cmd == null)
					continue;

				byte[] idBuff = new byte[3];
				mcuParam.GetMessageID(ref idBuff);
				
				uint id = GetIdFromBuffer(idBuff);

				_messagesDict[id] = null;
			}
		}

		public override void Dispose()
		{
			LoggerService.Inforamtion(this, "Disposing");

			_isTimeout = true;

			if (_timeoutTimer != null)
				_timeoutTimer.Stop();

			_poolBuildTimer.Stop();



			FireConnectionEvent();

			base.Dispose();

#if _SAVE_TIME
			try
			{
				//LoggerService.Inforamtion(this, "MCU time");
				using (StreamWriter sw = new StreamWriter("MCU Time.txt"))
				{
					foreach ((TimeSpan,string) time in _commTimeList)
					{
						string name = time.Item2.Replace("\n", "-");
						sw.WriteLine(time.Item1.TotalMilliseconds.ToString() + "\t\t\t" + name);
						//LoggerService.Debug(this, time.TotalMilliseconds.ToString());
					}
				}
			}
			catch { }
#endif
		}




		protected override CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
		{
			if (data is CommunicatorIOData_SendMessage sendMessageData)
			{
				SendCANMessage(sendMessageData);
				return CommunicatorResultEnum.OK;
			}

			if (!(data?.Parameter is MCU_ParamData mcuParam))
				return CommunicatorResultEnum.None;

#if _SAVE_TIME
			DateTime startTime = DateTime.Now;
#endif

			CommunicatorResultEnum result = CommunicatorResultEnum.None;
			string errorDescription = null;
			int i = 0;
			for (; i < _getResponseRepeats; i++)
			{


				byte[] id = _idBuffersPool.Take(_cancellationToken);


				var buffer = ConvertToData(mcuParam, data.Value, ref id, data.IsSet);
				CommService.Send(buffer);


				try
				{
					int intVal = 0;
					if (data.IsSet)
					{
						try
						{
							intVal = GetValueAsInt(data.Value);
						}
						catch (Exception ex)
						{
							errorDescription = "Failed to handle get/set.\r\n" +
							"Parameter: " + mcuParam.Name + "\r\n" +
							"Value: " + mcuParam.Value;
							LoggerService.Error(this, errorDescription, ex);
							result = CommunicatorResultEnum.Error;

							System.Threading.Thread.Sleep(1);
							continue;
						}
					}

					result = WaitForResponse(
						mcuParam,
						out errorDescription,
						data.IsSet,
						id,
						intVal);

				}
				catch (Exception ex)
				{
					errorDescription = "Failed to handle get/set.\r\n" +
						"Parameter: " + mcuParam.Name + "\r\n" +
						"Value: " + mcuParam.Value;
					LoggerService.Error(this, errorDescription, ex);
					result = CommunicatorResultEnum.Error;
				}

				if(mcuParam.Name.Contains("Manual Throttle")) { }

				if (result == CommunicatorResultEnum.OK)
					break;

				System.Threading.Thread.Sleep(1);
			}


			data.Callback?.Invoke(mcuParam, result, errorDescription);

#if _SAVE_TIME
			TimeSpan diff = (DateTime.Now - startTime);
			//LoggerService.Debug(this,
			//	"Parameter: " + mcuParam.Name +
			//		" - Value: " + mcuParam.Value +
			//		" - Callback status: " + result +
			//		" - Err description: " + errorDescription +
			//		" - Iteration: " + i +
			//		" - Time: " + diffBefore.TotalMilliseconds +
			//		": " + diff.TotalMilliseconds);
			_commTimeList.Add((diff, mcuParam.Name));
#endif // _SAVE_TIME

			return result;
		}

		private int GetValueAsInt(object valOb)
		{
			double d = 0;
			if(valOb is string str)
				double.TryParse(str, out d);
			else
			{
				d = Convert.ToDouble(valOb);
			}

			return Convert.ToInt32(d);
			
		}

		private object _lockObj = new object();
		private CommunicatorResultEnum WaitForResponse(
			MCU_ParamData mcuParam,
			out string errorDescription,
			bool isSet,
			byte[] paramId,
			int setValue = 0)
		{
			

			lock (_lockObj)
			{
				CommunicatorResultEnum isSuccess = CommunicatorResultEnum.None;
				errorDescription = null;

				_isTimeout = false;
				_timeoutTimer.Start();

				byte[] readBuffer = null;

				readBuffer = null;
				GetBuffer(ref readBuffer, paramId);

				
				if (readBuffer != null)
				{
					isSuccess = HandleBuffer(
						readBuffer,
						mcuParam,
						isSet,
						setValue,
						out errorDescription);
				}

				if (isSuccess == CommunicatorResultEnum.OK)
					return isSuccess;

				System.Threading.Thread.Sleep(1);


				if ((readBuffer == null || _isTimeout) && isSuccess == CommunicatorResultEnum.None)
				{
					isSuccess =  CommunicatorResultEnum.NoResponse;
				}

				_timeoutTimer.Stop();
				_isTimeout = false;

				return isSuccess;
			}

			
		}

		private void GetBuffer(
			ref byte[] readBuffer,
			byte[] paramId)
		{
			uint id = GetIdFromBuffer(paramId);

			while (readBuffer == null)
			{
				System.Threading.Thread.Sleep(1);
				if (_isTimeout)
					break;

				lock (_messagesDict)
				{
					if (_messagesDict.ContainsKey(id) == false)
						continue;

					readBuffer = _messagesDict[id];
					_messagesDict[id] = null;
				}

				break;
			}
		}

		private CommunicatorResultEnum HandleBuffer(
			byte[] readBuffer,
			MCU_ParamData mcuParam,
			bool isSet,
			int setValue,
			out string errDescription)
		{
			int? value = null;
			errDescription = null;


			try
			{
				
				int isSuccess = IsErr(readBuffer);


				if (isSuccess == 0)
				{
					value = readBuffer[4] << 24 | readBuffer[5] << 16 | readBuffer[6] << 8 | readBuffer[7];

					var is_negative = ((readBuffer[3] & _negativeMask) >> _negativeShift == 0x01) ? -1 : 1;

					value *= is_negative;

					double dvalue = (double)value / mcuParam.Scale;

					if(dvalue == 0 && 
						mcuParam.Value is double prevValue &&
						prevValue != 0)
					{
						LoggerService.Inforamtion(this, "Value = 0 for " + mcuParam.Name);
					}

					mcuParam.Value = dvalue;

				}
				else
				{
					errDescription = "Unknown error: " + isSuccess;
					if (_mcuErrorToDescription.ContainsKey((int)isSuccess))
						errDescription = _mcuErrorToDescription[(int)isSuccess];
					return CommunicatorResultEnum.Error;
				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to handle received message", ex);
				errDescription = "Internal error";
				return CommunicatorResultEnum.Error;
			}

			int dsetValue = (int)Math.Round(setValue * mcuParam.Scale);
			if (isSet && value != (int?)dsetValue)
			{
				LoggerService.Error(this,
					mcuParam.Name + ": ValueNotSet: Original=" + value + "; Ack=" + dsetValue);
				return CommunicatorResultEnum.ValueNotSet;
			}
			


			return CommunicatorResultEnum.OK;


		}



		private byte[] ConvertToData(
			MCU_ParamData mcu_Message,
			double value,
			ref byte[] id,
			bool isSet)
		{
			value = value * mcu_Message.Scale;

			byte[] data = _buffersPool.Take(_cancellationToken);

			
			//! Split command and value:
			int write_permission = isSet ? 1 : 0;

			//! Get 3 msb command hash in md5
			mcu_Message.GetMessageID(ref id);

			//! Copy the id to data
			Array.Copy(id, 0, data, 0, 3);

			int iValue = Convert.ToInt32(value);


			iValue = (write_permission == 1) ? iValue : 0;

			//! If command is negative save it to send seperatly
			int is_negative = (iValue < 0) ? 1 : 0;

			//! Get positive value only
			iValue = Math.Abs(iValue);


			//! convert the value to bytes and copy to data:
			var value_bytes = BitConverter.GetBytes(iValue);

			//! We need to change endian type:
			Array.Reverse(value_bytes);

			//! Copy the value to data
			Array.Copy(value_bytes, 0, data, 4, 4);

			//! Data[3] contains info about the param, negative sign is located in bit 0
			data[3] |= (byte)((is_negative << _negativeShift) & _negativeMask);

			data[3] |= (byte)((write_permission << _writeShift) & _writeMask);



			return data;
		}

		private int IsErr(
			byte[] buffer)
		{

			var err = (buffer[3] & _errMask) >> _errShift;

			if (err != 0)
			{
				return err;
			}

			return 0;
		}

		private void ErrorEventHandler(string errorMessage)
		{

		}


		private void TimoutElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			_isTimeout = true;
		}

		private void PoolBuildTimerElapsed(object sender, ElapsedEventArgs e)
        {
            while (_buffersPool.Count < _maxNumOfBuffers)
            {
				try
				{
					_buffersPool.Add(new byte[8], _cancellationToken);
					System.Threading.Thread.Sleep(1);
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}

            while (_idBuffersPool.Count < _maxNumOfIds)
            {
				try
				{
					_idBuffersPool.Add(new byte[3], _cancellationToken);
					System.Threading.Thread.Sleep(1);
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}
        }

#region CAN Message

		public override void SendMessage(bool isExtended, uint id, byte[] buffer, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				CommunicatorIOData_SendMessage data = new CommunicatorIOData_SendMessage()
				{
					IsExtented = isExtended,
					ID = id,
					Buffer = buffer,
					Callback = callback,
				};
				_parameterQueue.Add(data, _cancellationToken);
			}
			catch (OperationCanceledException)
			{

			}
		}

		private void SendCANMessage(CommunicatorIOData_SendMessage sendMessage)
		{
			CanService.Send(sendMessage.Buffer, sendMessage.ID, sendMessage.IsExtented);
			sendMessage.Callback?.Invoke(null, CommunicatorResultEnum.OK, null);
		}



		#endregion DBC



		private void SyncMessageHandler(byte[] buffer)
		{
			if (_messagesDict == null)
			{
				return;
			}

			lock (_messagesDict)
			{
				DateTime start = DateTime.Now;

				byte[] idBuffer = _idBuffersPool.Take(_cancellationToken);
				Array.Copy(buffer, idBuffer, 3);

				uint id = GetIdFromBuffer(buffer);

				_messagesDict[id] = buffer;

				TimeSpan diff = DateTime.Now - start;
			}
		}

		private void AsyncMessageHandler(byte[] buffer)
		{
			AsyncIdMessageReceived?.Invoke(buffer);
		}

		private uint GetIdFromBuffer(byte[] buffer) 
		{
			uint id = 0;
			for(int i = 0; i < 3; i++) 
			{
				id += (uint)(buffer[i] << (i * 8));
			}

			return id;
		}

		#endregion Methods

		#region Events

		public event Action<byte[]> AsyncIdMessageReceived;

		#endregion Events
	}


}
