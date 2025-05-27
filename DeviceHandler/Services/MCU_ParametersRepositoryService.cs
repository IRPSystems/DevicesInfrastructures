
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
			base(communicator, 0)
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

			if(dictionary.ContainsKey(parameter.Name) == false) 
				return;

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

					dbcParam.Value = dbcParam.GetValue(buffer);
				}
				catch(Exception ex)
				{
					LoggerService.Error(this, "Failed to extract field from DBC message", ex);
				}

				System.Threading.Thread.Sleep(1);
			}
		}

		#endregion Methods
	}





}
