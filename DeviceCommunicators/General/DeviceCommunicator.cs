//#define _SAVE_TIME
using DeviceCommunicators.Enums;
using Services.Services;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;
using Communication.Interfaces;
using DeviceCommunicators.Models;
#if _SAVE_TIME
using System.Collections.Generic;
using System.IO;
#endif

namespace DeviceCommunicators.General
{
	public abstract class DeviceCommunicator
	{
		#region Fields



		public ICommService CommService;


		protected CancellationTokenSource _cancellationTokenSource;
		protected CancellationToken _cancellationToken;



		protected BlockingCollection<CommunicatorIOData> _parameterQueue_Set;
		protected BlockingCollection<CommunicatorIOData> _parameterQueue_Get;

		protected object _lockObject;

		protected LogLineListService _logLineList;

#if _SAVE_TIME
		private List<(TimeSpan, string)> _commTimeList;
#endif

		#endregion Fields

		#region Properties

		public virtual bool IsInitialized
		{
			get
			{
				if (CommService == null)
					return false;
				return CommService.IsInitialized;
			}
		}

		#endregion Properties

		#region Constructor

		public DeviceCommunicator(LogLineListService logLineList)
		{
			_logLineList = logLineList;

#if _SAVE_TIME
			_commTimeList = new List<(TimeSpan, string)>();
#endif

			_parameterQueue_Set = new BlockingCollection<CommunicatorIOData>();
			_parameterQueue_Get = new BlockingCollection<CommunicatorIOData>();
			_lockObject = new object();

			InitErrorsDictionary();

		}

		#endregion Constructor

		#region Methods

		protected void InitBase()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			HandleInputs(_parameterQueue_Set);
			HandleInputs(_parameterQueue_Get);
		}

		protected virtual void InitErrorsDictionary()
		{

		}

		public virtual void Dispose()
		{
			

			if (_cancellationTokenSource != null)
				_cancellationTokenSource.Cancel();

			System.Threading.Thread.Sleep(100);

			if (CommService != null)
				CommService.Dispose();

			if (_parameterQueue_Get != null)
			{
				while (_parameterQueue_Set.Count > 0)
				{
					CommunicatorIOData item;
					_parameterQueue_Set.TryTake(out item);
				}
			}

			if (_parameterQueue_Get != null)
			{
				while (_parameterQueue_Get.Count > 0)
				{
					CommunicatorIOData item;
					_parameterQueue_Get.TryTake(out item);
				}
			}

#if _SAVE_TIME
			try
			{
				//LoggerService.Inforamtion(this, "MCU time");
				using (StreamWriter sw = new StreamWriter("Device param Time.txt"))
				{
					foreach ((TimeSpan,string) time in _commTimeList)
					{
						string name = string.Empty;
						if(!string.IsNullOrEmpty(time.Item2))
							name = time.Item2.Replace("\n", "-");
						sw.WriteLine($"{time.Item1.TotalMilliseconds}\t\t\t{name}");
						//LoggerService.Debug(this, time.TotalMilliseconds.ToString());
					}
				}
			}
			catch { }
#endif
		}




		public void SetParamValue(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				CommunicatorIOData data = new CommunicatorIOData()
				{
					IsSet = true,
					Parameter = param,
					Value = value,
					Callback = callback,
				};
				_parameterQueue_Set.Add(data, _cancellationToken);
			}
			catch (OperationCanceledException)
			{

			}
		}

		public void GetParamValue(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				CommunicatorIOData data = new CommunicatorIOData()
				{
					IsSet = false,
					Parameter = param,
					Callback = callback,
				};
				_parameterQueue_Get.Add(data, _cancellationToken);
			}
			catch (OperationCanceledException)
			{

			}
		}





		protected void HandleInputs(BlockingCollection<CommunicatorIOData> parameterQueue)
		{
			Task.Run(() =>
			{
				while (!_cancellationToken.IsCancellationRequested)
				{
				//	lock (_lockObject)
					{
						
						CommunicatorIOData data = null;

						try
						{


							try
							{
								data = parameterQueue.Take(_cancellationToken);
							}
							catch (OperationCanceledException)
							{
								break;
							}


							if (data == null)
								continue;

#if _SAVE_TIME
			DateTime startTime = DateTime.Now;
#endif

							CommunicatorResultEnum result = CommunicatorResultEnum.None;
							lock (_lockObject)
								result = HandleRequests(data);

							//LoggerService.Inforamtion(this, $"Queue={parameterQueue.Count}");
							if (result == CommunicatorResultEnum.NoResponse &&
								parameterQueue.Count >= 100)
							{
								LoggerService.Inforamtion(this, $"Clearing the queue={parameterQueue.Count}");
								while (parameterQueue.Count > 50)
								{
									CommunicatorIOData item;
									parameterQueue.TryTake(out item);
								}
							}

#if _SAVE_TIME
							TimeSpan diff = (DateTime.Now - startTime);

							_commTimeList.Add((diff, data.Parameter.Name));
#endif // _SAVE_TIME

						}
						catch (Exception ex)
						{
							LoggerService.Error(this, "Failed to handle get/set", ex);
							data.Callback?.Invoke(data.Parameter, CommunicatorResultEnum.Error, "Exception on get/set");
						}

					}



					System.Threading.Thread.Sleep(1);

				}
			}, _cancellationToken);
		}

		protected abstract CommunicatorResultEnum HandleRequests(CommunicatorIOData data);

		protected void FireConnectionEvent()
		{
			ConnectionEvent?.Invoke();
		}

		public virtual void SendMessage(bool isExtended, uint id, byte[] buffer, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			throw new NotImplementedException();
		}

		#endregion Methods

		#region Events

		public event Action ConnectionEvent;

		#endregion Events
	}
}