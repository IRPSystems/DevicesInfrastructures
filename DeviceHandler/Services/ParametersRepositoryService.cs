﻿
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using DeviceHandler.Interfaces;
using DeviceHandler.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Timers;
using System.Windows;

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

			_timeoutTimer = new System.Timers.Timer(1000);
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

			string name = parameter.Name;
			if(parameter is MCU_ParamData mcuParam)
				name = mcuParam.Cmd;

			if (_nameToRepositoryParamList.ContainsKey(name))
			{
				repositoryParam = _nameToRepositoryParamList[name];
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
				_nameToRepositoryParamList[name] = repositoryParam;
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

				string name = parameter.Name;
				if (parameter is MCU_ParamData mcuParam)
					name = mcuParam.Cmd;

				if (_nameToRepositoryParamList.ContainsKey(name) == false)
					return;
				if (_nameToRepositoryParamList[name] == null)
					return;

				

				RepositoryParam repositoryParam =
					_nameToRepositoryParamList[name];
				if (repositoryParam == null)
					return;

				repositoryParam.Counter--;
				if (receivedMessageCallback != null)
					repositoryParam.ReceivedMessageEvent -= receivedMessageCallback;
				if (repositoryParam.Counter == 0)
				{
					_nameToRepositoryParamList.TryRemove(
						new KeyValuePair<string, RepositoryParam>(name, repositoryParam));
					//_repositoryParamList.Remove(repositoryParam);
				}

				if (_nameToRepositoryParamList.Count == 0)
				{
					LoggerService.Inforamtion(this, "The list is empty");
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

			LoggerService.Inforamtion(this, "_timeoutTimer_Elapsed");

			_communicationTimer.Stop();
			_communicationTimer.Interval = 1000 / AcquisitionRate;
			_communicationTimer.Start();
			ActualAcquisitionRate = 0;
		}

		private void CommunicationTimerElapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				if (_nameToRepositoryParamList == null || _nameToRepositoryParamList.Count == 0)
				{
					ActualAcquisitionRate = 0;
					return;
				}

				_communicationTimer.Stop();
				_start = DateTime.Now;
				//LoggerService.Inforamtion(this, "_communicationTimer stopped");


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

				

				_timeoutTimer.Start();

				//_communicationTimer.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to handle response\r\n{ex}");
			}
		}


		private void GetValueCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{
			try
			{
				
				if (_isDisposed)
					return;

				if (_nameToRepositoryParamList == null || _nameToRepositoryParamList.Count == 0)
				{
					return;
				}

				string name = param.Name;
				if (param is MCU_ParamData mcuParam)
					name = mcuParam.Cmd;

				if (_nameToRepositoryParamList.ContainsKey(name) == false)
				{
					return;
				}

				RepositoryParam repositoryParam =
						_nameToRepositoryParamList[name];
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
					_timeoutTimer.Stop();
					//LoggerService.Inforamtion(this, "_communicationTimer started");
				}

			}
			catch (Exception ex) 
			{
				MessageBox.Show($"Failed to handle response\r\n{ex}");
			}
		}

		protected virtual void CallbackHandling()
		{

		}

		protected virtual void LastCallbackHandling()
		{
			LastCallbackEvent?.Invoke();

			TimeSpan diff = DateTime.Now - _start;
			double reducedTime = diff.TotalMilliseconds;

			double refreshTime = 1000 / AcquisitionRate;

			double actualRate = 0;

			//_communicationTimer.Interval = (reducedTime < refreshTime) ? (double)refreshTime - reducedTime : 1;

			//actualRate = /*(reducedTime > refreshTime) ?*/ 1000 / (reducedTime + 1);// : (double)1000 / (refreshTime);

			if (reducedTime > refreshTime)
			{
				_communicationTimer.Interval = reducedTime;
				actualRate = 1000.0 / (reducedTime + 1);
			}
			else
			{
				_communicationTimer.Interval = refreshTime;
				actualRate = 1000.0 / refreshTime;
			}

			if (actualRate != 0)
				ActualAcquisitionRate = actualRate;

			if (ActualAcquisitionRate < 5)
			{
				LoggerService.Inforamtion(this, $"ActualAcquisitionRate={ActualAcquisitionRate}");
			}
		}

		#endregion Methods

		#region Events

		public event Action LastCallbackEvent;

		#endregion Events
	}





}
