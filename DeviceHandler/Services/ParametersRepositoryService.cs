
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace DeviceHandler.Services
{
	/// <summary>
	/// ParametersRepositoryService is a class that holds a list of parameters and 
	/// get their values preiodically.
	/// </summary>
	public class ParametersRepositoryService: ObservableObject
	{
		#region Properties

		public bool IsInitialized
		{
			get => _communicator.IsInitialized;
		}

		/// <summary>
		/// Thre required acquisition rate
		/// </summary>
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

		/// <summary>
		/// Thre actual acquisition rate
		/// </summary>
		public double ActualAcquisitionRate { get; set; }

		#endregion Properties

		#region Fields

		private int _acquisitionRate;

		private const int _maxNumOfParams = 3000;

		protected ConcurrentDictionary<string, RepositoryParam> _nameToRepositoryParamList;

		protected System.Timers.Timer _communicationTimer;
		protected System.Timers.Timer _timeoutTimer;

		protected DateTime _start;

		private DeviceCommunicator _communicator;

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


			_communicationTimer = new System.Timers.Timer(1000 / AcquisitionRate);
			_communicationTimer.Elapsed += CommunicationTimerElapsed;

			_timeoutTimer = new System.Timers.Timer(100);
			_timeoutTimer.Elapsed += _timeoutTimer_Elapsed;

		}

		#endregion Constructor

		#region Methods

		public void Init()
		{
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

				if (_nameToRepositoryParamList.Count == 0)
				{
					_communicationTimer.Interval = 1000 / AcquisitionRate;
					_communicationTimer.Start();
					ActualAcquisitionRate = 0;
				}
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to remove parameter", ex);
			}
		}

		#endregion Add/Remove

		
		private void _timeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			_timeoutTimer.Stop();

			_communicationTimer.Interval = 1000 / AcquisitionRate;
			_communicationTimer.Start();
			ActualAcquisitionRate = 0;
		}

		private void CommunicationTimerElapsed(object sender, ElapsedEventArgs e)
		{

			if (_nameToRepositoryParamList == null || _nameToRepositoryParamList.Count == 0)
			{
				ActualAcquisitionRate = 0;
				return;
			}

			_communicationTimer.Stop();

			

			foreach (RepositoryParam param in _nameToRepositoryParamList.Values)
			{

				if (_isDisposed)
					return;

				if (param.Parameter.DeviceType == Entities.Enums.DeviceTypesEnum.DBC)
					continue;

				if (param.Parameter is ICalculatedParamete calculated)
				{
					calculated.Calculate();
					continue;
				}

				param.IsReceived = CommunicatorResultEnum.None;
				_communicator.GetParamValue(param.Parameter, GetValueCallback);

				System.Threading.Thread.Sleep(1);
			}

			_start = DateTime.Now;

			_timeoutTimer.Start();

			//_communicationTimer.Start();
		}


		private void GetValueCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{
			_timeoutTimer.Stop();
			if (_isDisposed)
				return;

			if (_nameToRepositoryParamList == null || _nameToRepositoryParamList.Count == 0)
			{
				return;
			}

			if (_nameToRepositoryParamList.ContainsKey(param.Name) == false)
			{
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

			if (result != CommunicatorResultEnum.OK)
			{
				param.Value = double.NaN;
			}

			RepositoryParam lastParam =
				_nameToRepositoryParamList.Values.ElementAt(_nameToRepositoryParamList.Values.Count - 1);
			if (param == lastParam.Parameter)
			{
				LastCallbackHandling();
				_communicationTimer.Start();
			}

			
		}

		protected virtual void CallbackHandling()
		{

		}

		protected virtual void LastCallbackHandling()
		{

		}

		#endregion Methods
	}


	


}
