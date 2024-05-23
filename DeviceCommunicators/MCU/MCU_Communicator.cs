
//#define _SAVE_TIME
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Entities.Models;
using Newtonsoft.Json.Linq;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
#if _SAVE_TIME
using System.Collections.Generic;
using System.IO;
#endif
using System.Linq;
using System.Timers;

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
		private const int _getResponsesTime = 20;

		public ConcurrentDictionary<int, string> _mcuErrorToDescription;

		private BlockingCollection<byte[]> _buffersPool;
		private BlockingCollection<byte[]> _idBuffersPool;

		private System.Timers.Timer _poolBuildTimer;


		private uint _syncID;

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
			uint syncID,
			uint asyncID,
			ushort hwId = 0,
			int rxPort = 0,
			int txPort = 0,
			string address = "")
		{
			LoggerService.Inforamtion(
				this,
				"Initiating communication - Adapter: " + canAdapterType);

			_syncID = syncID;

			if (canAdapterType == "PCAN")
			{
				CommService = new CanPCanService(baudrate, hwId, syncID, 0x1FFFFFFF);
			}
			else if (canAdapterType == "UDP Simulator")
			{
				CommService = new CanUdpSimulationService(baudrate, syncID, asyncID, rxPort, txPort, address);
			}


			CommService.Init(false);
			CommService.Name = "MCU_Communicator";
			CanService.CanMessageReceivedEvent += AsyncMessageWasReceived;



			_poolBuildTimer.Start();

			FireConnectionEvent();

			InitBase();
		}

		public override void Dispose()
		{
			LoggerService.Inforamtion(this, "Disposing");


			_poolBuildTimer.Stop();

			base.Dispose();

			FireConnectionEvent();

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
				byte[] id = null;
				try
				{
					id = _idBuffersPool.Take(_cancellationToken);
				}
				catch (OperationCanceledException)
				{
					break;
				}

				byte[] buffer = _buffersPool.Take(_cancellationToken);
				ConvertToData(mcuParam, data.Value, ref id, ref buffer, data.IsSet);
				CommService.Send(buffer);


				try
				{
					//int intVal = 0;
					//if (data.IsSet)
					//{
					//	try
					//	{
					//		intVal = GetValueAsInt(data.Value);
					//	}
					//	catch (Exception ex)
					//	{
					//		errorDescription = "Failed to handle get/set.\r\n" +
					//		"Parameter: " + mcuParam.Name + "\r\n" +
					//		"Value: " + mcuParam.Value;
					//		LoggerService.Error(this, errorDescription, ex);
					//		result = CommunicatorResultEnum.Error;

					//		System.Threading.Thread.Sleep(1);
					//		continue;
					//	}
					//}

					result = WaitForResponse(
						mcuParam,
						out errorDescription,
						data.IsSet,
						id,
						mcuParam.Value);

				}
				catch (Exception ex)
				{
					errorDescription = "Failed to handle get/set.\r\n" +
						"Parameter: " + mcuParam.Name + "\r\n" +
						"Value: " + mcuParam.Value;
					LoggerService.Error(this, errorDescription, ex);
					result = CommunicatorResultEnum.Error;
				}

				if (result == CommunicatorResultEnum.OK/* || result == CommunicatorResultEnum.Error*/)
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
			if (valOb is string str)
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
			object setValue = null)
		{


			lock (_lockObj)
			{
				CommunicatorResultEnum isSuccess = CommunicatorResultEnum.None;
				errorDescription = null;


				byte[] readBuffer = null;

				readBuffer = null;
				ReadBuffer(ref readBuffer);


				if (readBuffer != null)
				{
					isSuccess = HandleBuffer(
						paramId,
						readBuffer,
						mcuParam,
						isSet,
						setValue,
						out errorDescription);
				}

				if (isSuccess == CommunicatorResultEnum.OK)
					return isSuccess;

				System.Threading.Thread.Sleep(1);


				if (readBuffer == null && isSuccess == CommunicatorResultEnum.None)
				{
					isSuccess = CommunicatorResultEnum.NoResponse;
				}

				return isSuccess;
			}


		}

		private void ReadBuffer(ref byte[] readBuffer)
		{
			uint readNode = 0;

			Stopwatch timeout = new Stopwatch();
			timeout.Start();
			while (readBuffer == null && (timeout.ElapsedMilliseconds < _getResponsesTime))
			{

				CanService.Read(out readBuffer, out readNode);


				if (readBuffer != null)
				{
					if (readNode != _syncID)
					{
						readBuffer = null;
						continue;
					}
					break;
				}

				System.Threading.Thread.Sleep(1);

			}
		}

		private CommunicatorResultEnum HandleBuffer(
			byte[] paramId,
			byte[] readBuffer,
			MCU_ParamData mcuParam,
			bool isSet,
			object setValue,
			out string errDescription)
		{
			int? value = null;
			errDescription = null;
			double dvalue = 0;

			try
			{

				int isSuccess = IsErr(readBuffer, paramId);

				
				if (isSuccess == 0)
				{
					value = readBuffer[4] << 24 | readBuffer[5] << 16 | readBuffer[6] << 8 | readBuffer[7];

					var is_negative = ((readBuffer[3] & _negativeMask) >> _negativeShift == 0x01) ? -1 : 1;

					value *= is_negative;

					dvalue = (double)value / mcuParam.Scale;
					
					//if (dvalue == 0 &&
					//	mcuParam.Value is double prevValue &&
					//	prevValue != 0)
					//{
					//	LoggerService.Inforamtion(this, "Value = 0 for " + mcuParam.Name);
					//}

					

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

			// Make sure the ack value is like the set value
			if (setValue is double dSetValue)
			{
				int dsetValue = (int)Math.Round(dSetValue * mcuParam.Scale);
				if (isSet && value != (int?)dsetValue)
				{
					LoggerService.Error(this,
						mcuParam.Name + ": ValueNotSet: Original=" + value + "; Ack=" + dsetValue);
					return CommunicatorResultEnum.ValueNotSet;
				}
			}

			// If this is a drop down parameter, set the value to the item Name
			if (mcuParam.DropDown != null && mcuParam.DropDown.Count > 0)
			{

				DropDownParamData dd =
					mcuParam.DropDown.Find((i) => i.Value == dvalue.ToString());
				if (dd != null) 
				{
					mcuParam.Value = dd.Name;
				}
				else mcuParam.Value = dvalue;
			}
			else if(string.IsNullOrEmpty(mcuParam.Format) == false)
			{
				mcuParam.Value = GetFormatedValuesService.GetString(mcuParam.Format, dvalue);
			}
			else 
				mcuParam.Value = dvalue;

			return CommunicatorResultEnum.OK;


		}



		public static byte[] ConvertToData(
			MCU_ParamData mcu_Message,
			double value,
			ref byte[] id,
			ref byte[] data,
			bool isSet)
		{
			value = value * mcu_Message.Scale;

			


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
			byte[] buffer,
			byte[] out_id)
		{

			byte[] in_id = _idBuffersPool.Take(_cancellationToken);

			Array.Copy(buffer, 0, in_id, 0, 3);

			//! Check if id sent is id received
			if (!Enumerable.SequenceEqual(in_id, out_id))
			{
				return 4;
			}

			//! err bits locates in H nibble in data byte[3]
			var err = (buffer[3] & _errMask) >> _errShift;

			if (err != 0)
			{
				return err;
			}

			return 0;
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


		private void AsyncMessageWasReceived(uint node, byte[] buffer)
		{
			AsyncMessageReceivedEvent?.Invoke(node, buffer);
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
				_parameterQueue_Set.Add(data, _cancellationToken);
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

		#endregion Methods

		#region Events

		public event Action<uint, byte[]> AsyncMessageReceivedEvent;

		#endregion Events
	}


}
