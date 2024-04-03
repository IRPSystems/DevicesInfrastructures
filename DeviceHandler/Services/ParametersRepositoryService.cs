
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
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
	public class ParametersRepositoryService
	{
		#region Properties

		public bool IsInitialized
		{
			get => _communicator.IsInitialized;
		}

		public int AcquisitionRate 
		{
			get => _acquisitionRate;
			set
			{
				_acquisitionRate = value;
				if (_communicationTimer != null && _acquisitionRate != 0)
				{
					_communicationTimer.Stop();
					_communicationTimer.Interval = 1000 / _acquisitionRate;
					_communicationTimer.Start();
				}
			}
		}

		#endregion Properties

		#region Fields

		private int _acquisitionRate;

		private const int _maxNumOfParams = 3000;

		private ConcurrentDictionary<string, RepositoryParam> _nameToRepositoryParamList;

		private System.Timers.Timer _communicationTimer;


		private DeviceCommunicator _communicator;

		private bool _isGetMedium;
		private int _lowGetCounter;

		private bool _isDisposed;

		public string Name;

		#endregion Fields

		#region Constructor

		public ParametersRepositoryService(
			DeviceCommunicator communicator)
		{
			_communicator = communicator;

			_nameToRepositoryParamList = new ConcurrentDictionary<string, RepositoryParam>();

			AcquisitionRate = 1;


			//_cancellationTokenSource = new CancellationTokenSource();
			//_cancellationToken = _cancellationTokenSource.Token;
			//_waitGetCallback = new ManualResetEvent(false);

			_isGetMedium = false;
			_lowGetCounter = 0;



			_communicationTimer = new System.Timers.Timer(1000 / AcquisitionRate);
			_communicationTimer.Elapsed += CommunicationTimerElapsed;
			



		}

		#endregion Constructor

		#region Methods

		public void Init()
		{
			//_cancellationTokenSource = new CancellationTokenSource();
			//_cancellationToken = _cancellationTokenSource.Token;

			_isDisposed = false;
			_communicationTimer.Start();

		}

		public void Dispose()
		{
			_communicationTimer.Stop();

			_isDisposed = true;
		}


		#region Add/Remove

		public virtual void Add(
			DeviceParameterData parameter,
			RepositoryPriorityEnum priority,
			Action<DeviceParameterData, CommunicatorResultEnum,string> receivedMessageCallback)
		{
			if (parameter == null)
				return;


			RepositoryParam repositoryParam;

			if (_nameToRepositoryParamList.ContainsKey(parameter.Name))
			{
				repositoryParam = _nameToRepositoryParamList[parameter.Name];
				if (repositoryParam.Priority < priority)
					repositoryParam.Priority = priority;
				if(receivedMessageCallback != null)
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
				_nameToRepositoryParamList[parameter.Name] = repositoryParam;
			}

			repositoryParam.Counter++;
		}

		public virtual void Remove(
			DeviceParameterData parameter,
			Action<DeviceParameterData, CommunicatorResultEnum,string> receivedMessageCallback) 
		{
			try
			{
				if (parameter == null || _nameToRepositoryParamList == null)
					return;

				if (_nameToRepositoryParamList.ContainsKey(parameter.Name) == false)
					return;
				if (_nameToRepositoryParamList[parameter.Name] == null)
					return;


				RepositoryParam repositoryParam =
					_nameToRepositoryParamList[parameter.Name];
				if (repositoryParam == null)
					return;

				repositoryParam.Counter--;
				if (receivedMessageCallback != null)
					repositoryParam.ReceivedMessageEvent -= receivedMessageCallback;
				if (repositoryParam.Counter == 0)
				{
					_nameToRepositoryParamList.TryRemove(
						new KeyValuePair<string, RepositoryParam>(parameter.Name, repositoryParam));
					//_repositoryParamList.Remove(repositoryParam);
				}
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to remove parameter", ex);
			}
		}

		#endregion Add/Remove


		private void CommunicationTimerElapsed(object sender, ElapsedEventArgs e)
		{
			GetParams(RepositoryPriorityEnum.High);

			if (_isDisposed)
				return;

			GetParams(RepositoryPriorityEnum.Medium);
			
			if (_isDisposed)
				return;

			GetParams(RepositoryPriorityEnum.Low);
			
		}

		private object _lockObj = new object();
		private void GetParams(RepositoryPriorityEnum priority)
		{
			if (_nameToRepositoryParamList == null)
				return;

			lock (_lockObj)
			{
				List<RepositoryParam> repositoryParamsList =
					_nameToRepositoryParamList.Values.Where((rp) => rp.Priority == priority).ToList();
				foreach (RepositoryParam param in repositoryParamsList)
				{

					if (_isDisposed)
						return;

					if (param.Parameter.DeviceType == Entities.Enums.DeviceTypesEnum.DBC)
						continue;

					if(param.Parameter is ICalculatedParamete calculated)
					{
						calculated.Calculate();
						continue;
					}

					param.IsReceived = CommunicatorResultEnum.None;
					_communicator.GetParamValue(param.Parameter, GetValueCallback);

					System.Threading.Thread.Sleep(1);

				}
			}
		}


		private void GetValueCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{
			if (_isDisposed)
				return;

			if (_nameToRepositoryParamList == null || _nameToRepositoryParamList.Count == 0)
			{
			//	_waitGetCallback.Set();
				return;
			}

			if (_nameToRepositoryParamList.ContainsKey(param.Name) == false)
			{
			//	_waitGetCallback.Set();
				return;
			}

			RepositoryParam repositoryParam =
					_nameToRepositoryParamList[param.Name];
			if (repositoryParam != null)
			{
				repositoryParam.IsReceived = result;
				repositoryParam.ErrDescription = resultDescription;
				repositoryParam.RaisEvent(result, resultDescription);
			}

			if(result != CommunicatorResultEnum.OK)
			{
				param.Value = double.NaN;
			}

			//_waitGetCallback.Set();
		}



		#endregion Methods
	}


	


}
