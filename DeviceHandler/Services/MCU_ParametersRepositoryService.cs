
using DBCFileParser.Model;
using DeviceCommunicators.DBC;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using Entities.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace DeviceHandler.Services
{
	public class MCU_ParametersRepositoryService: ParametersRepositoryService
	{

		#region Fields


		private ConcurrentDictionary<uint, ConcurrentDictionary<string, RepositoryParam>> _msgIdToNameToRepositoryParamList;

		private MCU_Communicator _mcuCommunicator;

		#endregion Fields

		#region Constructor

		public MCU_ParametersRepositoryService(
			DeviceCommunicator communicator):
			base(communicator)
		{
			if (communicator is MCU_Communicator mcuCommunicator)
			{
				_mcuCommunicator = mcuCommunicator;
				_mcuCommunicator.AsyncMessageReceivedEvent += McuCommunicator_AsyncMessageReceivedEvent;

				_msgIdToNameToRepositoryParamList = new ConcurrentDictionary<uint, ConcurrentDictionary<string, RepositoryParam>>();
			}
		}

		#endregion Constructor

		#region Methods

		#region Add/Remove

		public override void Add(
			DeviceParameterData parameter,
			RepositoryPriorityEnum priority,
			Action<DeviceParameterData, CommunicatorResultEnum, string> receivedMessageCallback)
		{
			if (parameter == null)
				return;

			// Call the base remove for the none DBC parameters
			base.Add(parameter, priority, receivedMessageCallback);


			// Handle DBC parameters

			if (!(parameter is DBC_ParamData dbcParam))
				return;

			RepositoryParam repositoryParam = null;
			ConcurrentDictionary<string, RepositoryParam> dictionary = null;
			if (_msgIdToNameToRepositoryParamList.ContainsKey(dbcParam.ParentMessage.ID))
			{
				dictionary = _msgIdToNameToRepositoryParamList[dbcParam.ParentMessage.ID];
			}
			else
			{
				dictionary = new ConcurrentDictionary<string, RepositoryParam>();
				_msgIdToNameToRepositoryParamList[dbcParam.ParentMessage.ID] = dictionary;
			}

			if (dictionary.ContainsKey(parameter.Name))
			{
				repositoryParam = dictionary[parameter.Name];
				if (repositoryParam.Priority < priority)
					repositoryParam.Priority = priority;
				if (receivedMessageCallback != null)
					repositoryParam.ReceivedMessageEvent += receivedMessageCallback;
			}
			else
			{
				repositoryParam = new RepositoryParam();
				repositoryParam.Parameter = parameter;
				repositoryParam.Priority = priority;
				repositoryParam.Counter = 0;
				if (receivedMessageCallback != null)
					repositoryParam.ReceivedMessageEvent += receivedMessageCallback;
				dictionary[parameter.Name] = repositoryParam;
			}

			repositoryParam.Counter++;
		}

		public override void Remove(
			DeviceParameterData parameter,
			Action<DeviceParameterData, CommunicatorResultEnum, string> receivedMessageCallback)
		{
			if (parameter == null)
				return;

			// Call the base remove for the none DBC parameters
			base.Remove(parameter, receivedMessageCallback);


			// Handle DBC parameters

			if (_msgIdToNameToRepositoryParamList == null)
				return;

			if (!(parameter is DBC_ParamData dbcParam))
				return;

			if (_msgIdToNameToRepositoryParamList.ContainsKey(dbcParam.ParentMessage.ID) == false)
				return;

			ConcurrentDictionary<string, RepositoryParam> dictionary = 
				_msgIdToNameToRepositoryParamList[dbcParam.ParentMessage.ID];

			RepositoryParam repositoryParam =
					dictionary[parameter.Name];
			if (repositoryParam == null)
				return;

			repositoryParam.Counter--;
			if (receivedMessageCallback != null)
				repositoryParam.ReceivedMessageEvent -= receivedMessageCallback;
			if (repositoryParam.Counter == 0)
			{
				dictionary.TryRemove(
					new KeyValuePair<string, RepositoryParam>(parameter.Name, repositoryParam));
			}

			
		}

		#endregion Add/Remove



		private void McuCommunicator_AsyncMessageReceivedEvent(uint node, byte[] buffer)
		{
			if (_msgIdToNameToRepositoryParamList == null)
				return;

			if (_msgIdToNameToRepositoryParamList.ContainsKey(node) == false)
				return;

			ConcurrentDictionary<string, RepositoryParam> dictionary =
				_msgIdToNameToRepositoryParamList[node];

			if(dictionary == null || dictionary.Count == 0)
				return;


			foreach(RepositoryParam repositoryParam in dictionary.Values)
			{
				try
				{
					if (repositoryParam == null)
						continue;

					if (!(repositoryParam.Parameter is DBC_ParamData dbcParam))
						continue;

					int byteLength = dbcParam.Signal.Length / 8;  // dbcParam.Signal.Length is in bits
					int startByte = dbcParam.Signal.StartBit / 8; // dbcParam.Signal.StartBit is in bits

					switch (byteLength)
					{
						case 1: dbcParam.Value = buffer[startByte]; break;
						case 2:
							if (dbcParam.Signal.ValueType == DbcValueType.Unsigned)
								dbcParam.Value = BitConverter.ToUInt16(buffer, startByte);
							else if (dbcParam.Signal.ValueType == DbcValueType.Signed)
								dbcParam.Value = BitConverter.ToInt16(buffer, startByte);
							break;
						case 3:
						case 4:
							byte[] buffer4Bytes = new byte[4];
							Array.Copy(buffer, startByte, buffer4Bytes, 0, byteLength);
							Get4BytesValue(dbcParam, buffer4Bytes, startByte);
							break;
						case 5:
						case 6:
						case 7:
						case 8:
							byte[] buffer8Bytes = new byte[8];
							Array.Copy(buffer, startByte, buffer8Bytes, 0, byteLength);
							Get8BytesValue(dbcParam, buffer8Bytes, startByte);
							break;
					}

					double dVal = Convert.ToDouble(dbcParam.Value);
					dVal += dbcParam.Signal.Offset;
					dVal *= dbcParam.Signal.Factor;

					dbcParam.Value = dVal;
				}
				catch(Exception ex)
				{
					LoggerService.Error(this, "Failed to extract field from DBC message", ex);
				}

				System.Threading.Thread.Sleep(1);
			}
		}

		private void Get4BytesValue(
			DBC_ParamData dbcParam,
			byte[] buffer4Bytes,
			int startByte)
		{
			if (dbcParam.Signal.ValueType == DbcValueType.Unsigned)
				dbcParam.Value = BitConverter.ToUInt32(buffer4Bytes, startByte);
			else if (dbcParam.Signal.ValueType == DbcValueType.Signed)
				dbcParam.Value = BitConverter.ToInt32(buffer4Bytes, startByte);
			else if (dbcParam.Signal.ValueType == DbcValueType.IEEEFloat)
				dbcParam.Value = BitConverter.ToSingle(buffer4Bytes, startByte);
		}

		private void Get8BytesValue(
			DBC_ParamData dbcParam,
			byte[] buffer8Bytes,
			int startByte)
		{
			if (dbcParam.Signal.ValueType == DbcValueType.Unsigned)
				dbcParam.Value = BitConverter.ToUInt64(buffer8Bytes, startByte);
			else if (dbcParam.Signal.ValueType == DbcValueType.Signed)
				dbcParam.Value = BitConverter.ToInt64(buffer8Bytes, startByte);
			else if (dbcParam.Signal.ValueType == DbcValueType.IEEEDouble)
				dbcParam.Value = BitConverter.ToDouble(buffer8Bytes, startByte);
		}

		#endregion Methods
	}





}
