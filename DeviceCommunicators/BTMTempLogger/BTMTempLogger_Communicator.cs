﻿using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;

namespace DeviceCommunicators.BTMTempLogger
{
    public class BTMTempLogger_Communicator: DeviceCommunicator, IDataLoggerCommunicator
	{
		private enum WorkState 
		{ 
			StartNotFound,
			StartFound,
			EndFound
		}

		#region Fields

		private const string _startOfText = "\u0002";

		private ConcurrentDictionary<int, double> _channelTemp;

		private string _name_comport;
		private int _boud_rate;

        public string Name;

		private string _totalMessage;
		//private System.Timers.Timer _timer;

		private bool _isDataReceived;

		#endregion Fields


		#region Properties

		public int NumberOfChannels 
		{
			get => 12;
		}

		#endregion Properties

		#region Constructor

		public BTMTempLogger_Communicator()
        {
			_channelTemp = new ConcurrentDictionary<int, double>();


			//_timer = new System.Timers.Timer(500);
			//_timer.Elapsed += _timer_Elapsed;
			//_workState = WorkState.StartNotFound;
			_totalMessage = string.Empty;

		}

		

		#endregion Constructor

		#region Methods

		public void Init(
			bool isUdpSimulation,
			string comName,
			int baudtate,
			int rxPort = 0,
			int txPort = 0,
			string address = "")
        {
            _name_comport = comName;
            _boud_rate = baudtate;

			_isDataReceived = false;


			if (isUdpSimulation)
				CommService = new SerialUdpSimulationService(rxPort, txPort, address);
            else
			    CommService = new SerialService(_name_comport, _boud_rate);

			CommService.MessageReceivedEvent += MessageReceived;


			CommService.Init(true);

			InitBase();
			//_message = string.Empty;
			//_timer.Start();

		}

		protected override CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
		{

			if (data.IsSet)
			{
				SetParamValue_Do(
					data.Parameter,
					data.Value,
					data.Callback);
			}
			else
			{
				GetParamValue_Do(
					data.Parameter,
					data.Callback);
			}

			return CommunicatorResultEnum.OK;
		}


		private void SetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			throw new NotImplementedException();
		}


		private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			if (!(param is BTMTempLogger_ParamData btmParam))
				return;

			if(!_isDataReceived)
			{
				callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
				return;
			}

			if(btmParam.Name == "Check Communication")
			{
				callback?.Invoke(param, CommunicatorResultEnum.OK, null);

				return;
			}


			double value;
			bool res = GetChannelValue(btmParam.Channel, out value);
			if(res == false)
			{
				callback?.Invoke(
					param, 
					CommunicatorResultEnum.ValueNotSet, 
					"The value for channel " + btmParam.Channel + " was not set.");
				return;
			}

			param.Value = value;
			callback?.Invoke(param, CommunicatorResultEnum.OK, null);
		}

		public bool GetChannelValue(int channel, out double value)
		{
			value = 0;
			if (_channelTemp.ContainsKey(channel) == false)
				return false;

			value = _channelTemp[channel];
			return true;
		}

		private void MessageReceived(byte[] buffer)
		{
			_isDataReceived = true;
			var str = System.Text.Encoding.Default.GetString(buffer);
			lock(_totalMessage)
				_totalMessage += str.Replace("\0", string.Empty);
		}

		

		private void HandleReceivedText(object sender, ElapsedEventArgs e)
		{
			Task.Run(() =>
			{
				while(!_cancellationToken.IsCancellationRequested)
				{
					HandleMessage();

					Task.Delay(100);
				}

			}, _cancellationToken);
		}

		private void HandleMessage()
		{
			try
			{
				string str = string.Empty;
				lock (_totalMessage)
				{
					if(string.IsNullOrEmpty(_totalMessage))
					{
						_isDataReceived = false;
						return;
					}

					str = _totalMessage;
					_totalMessage = string.Empty;
				}

				string[] channelsList = str.Split(_startOfText);

				foreach (string channel in channelsList)
				{
					string channelTemp = channel.Trim('\0');
					if (channelTemp.StartsWith('4') == false)
						continue;
					if (channelTemp.Contains("\u0018\u0018\u0018\u0018"))
						continue;
					if (channelTemp.Length < 15)
						continue;
					if (channelTemp.EndsWith('\r') == false)
						continue;

					// Get channel
					int nch;
					int.TryParse(channelTemp[1].ToString(), NumberStyles.HexNumber, null, out nch);

					// Get floating point
					int nfPoint;
					int.TryParse(channelTemp[5].ToString(), out nfPoint);

					char polarity = channelTemp[4];

					// Build value string
					string value = channelTemp.Substring(6, 8);
					value = value.TrimStart('0');
					value = value.Insert(value.Length - nfPoint, ".");
					if (polarity == '1')
						value = "-" + value;

					double dVal;
					double.TryParse(value, out dVal);

					_channelTemp[nch] = dVal;
				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Error on parssing channel temperature", ex);
			}

		}

		#endregion Methods

	}
}
