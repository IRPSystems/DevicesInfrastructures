
using DeviceCommunicators.Enums;
using Services.Services;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System;
using Communication.Interfaces;
using DeviceCommunicators.Models;

namespace DeviceCommunicators.General
{
	public class DeviceCommunicator
	{
		#region Fields



		public ICommService CommService;


		protected CancellationTokenSource _cancellationTokenSource;
		protected CancellationToken _cancellationToken;



		protected BlockingCollection<CommunicatorIOData> _parameterQueue;

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

		public DeviceCommunicator()
		{

			_parameterQueue = new BlockingCollection<CommunicatorIOData>();

			InitErrorsDictionary();

		}

		#endregion Constructor

		#region Methods

		protected void InitBase()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			HandleInputs();
		}

		protected virtual void InitErrorsDictionary()
		{

		}

		public virtual void Dispose()
		{
			if (CommService != null)
			{
				CommService.Dispose();
			}

			if (_cancellationTokenSource != null)
				_cancellationTokenSource.Cancel();
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
				_parameterQueue.Add(data, _cancellationToken);
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
				_parameterQueue.Add(data, _cancellationToken);
			}
			catch (OperationCanceledException)
			{

			}
		}





		protected void HandleInputs()
		{
			Task.Run(() =>
			{
				while (!_cancellationToken.IsCancellationRequested)
				{
					DateTime startTime = DateTime.Now;

					CommunicatorIOData data = null;

					try
					{


						try
						{
							data = _parameterQueue.Take(_cancellationToken);
						}
						catch (OperationCanceledException)
						{
							break;
						}


						if (data == null)
							continue;

						CommunicatorResultEnum result = HandleRequests(data);
						if (result == CommunicatorResultEnum.NoResponse &&
							_parameterQueue.Count >= 100)
						{
							while (_parameterQueue.Count > 50)
							{
								CommunicatorIOData item;
								_parameterQueue.TryTake(out item);
							}
						}

					}
					catch (Exception ex)
					{
						LoggerService.Error(this, "Failed to handle get/set", ex);
						data.Callback?.Invoke(data.Parameter, CommunicatorResultEnum.Error, "Exception on get/set");
					}

					System.Threading.Thread.Sleep(1);

				}
			}, _cancellationToken);
		}

		protected virtual CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
		{
			return CommunicatorResultEnum.OK;
		}

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