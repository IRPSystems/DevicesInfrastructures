using Communication.Interfaces;
using Communication.Services;
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
		private System.Timers.Timer _timerTimeout;

		private bool _isDataReceived;


		#endregion Fields


		#region Properties

		public int NumberOfChannels 
		{
			get => 12;
		}

		private ISerialService SerialService 
		{
			get => CommService as ISerialService;
		}

		#endregion Properties

		#region Constructor

		public BTMTempLogger_Communicator(LogLineListService logLineList) :
			base(logLineList)
		{
			_channelTemp = new ConcurrentDictionary<int, double>();


			_timerTimeout = new System.Timers.Timer(1000);
			_timerTimeout.Elapsed += _timerTimeout_Elapsed;


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

			SerialService.MessageReceivedEvent += MessageReceived;


			CommService.Init(true);

			InitBase();

			HandleReceivedText();

			
			//_message = string.Empty;
			//_timer.Start();

		}

		public override void Dispose()
		{
			_timerTimeout.Stop();
			_isDataReceived = false;
			base.Dispose();
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
			_timerTimeout.Stop();

			lock (_totalMessage)
			{
				//LoggerService.Inforamtion(this, "Message received *********");
				_isDataReceived = true;
				var str = System.Text.Encoding.Default.GetString(buffer);
				str = str.Replace("\0", string.Empty);

				_totalMessage += str.Replace("\0", string.Empty);
			}

			_timerTimeout.Start();
		}

		private enum HandleReceivedTextStateEnum
		{
			Start, FirstByte, EndOfMessage
		}


		private HandleReceivedTextStateEnum _handleReceivedTextState;
		private void HandleReceivedText()
		{
			Task.Run(() =>
			{

				while (!_cancellationToken.IsCancellationRequested)
				{
					try
					{
						string str = string.Empty;
						lock (_totalMessage)
						{
							if (string.IsNullOrEmpty(_totalMessage))
								continue;


							switch (_handleReceivedTextState)
							{
								case HandleReceivedTextStateEnum.Start:
									int startIndex = _totalMessage.IndexOf(_startOfText);
									if (startIndex >= 0)
									{
										_totalMessage = _totalMessage.Substring(startIndex + 1);
										_handleReceivedTextState = HandleReceivedTextStateEnum.FirstByte;
									}

									break;

								case HandleReceivedTextStateEnum.FirstByte:
									if (_totalMessage.StartsWith('4') == false)
									{
										_handleReceivedTextState = HandleReceivedTextStateEnum.Start;
										break; ;
									}

									_handleReceivedTextState = HandleReceivedTextStateEnum.EndOfMessage;

									break;

								case HandleReceivedTextStateEnum.EndOfMessage:
									int endOfMessageIndex = _totalMessage.IndexOf('\r');
									if (endOfMessageIndex >= 0)
									{
										str = _totalMessage.Substring(0, endOfMessageIndex);
										_totalMessage = _totalMessage.Substring(endOfMessageIndex + 1);
										_handleReceivedTextState = HandleReceivedTextStateEnum.Start;
									}

									break;
							}
						}

						if (string.IsNullOrEmpty(str) == false)
						{
							HandleMessage(str);
							str = string.Empty;
						}

						System.Threading.Thread.Sleep(1);



					}
					catch (Exception ex) 
					{
						LoggerService.Error(this, "Error in parssing the message", ex);
					}


				}

				

			}, _cancellationToken);
		}

		private void HandleMessage(string message)
		{
			if (message.Contains("\u0018\u0018\u0018\u0018"))
				return;

			if (message.Length < 14)
				return;

			int nch;
			int.TryParse(message[1].ToString(), NumberStyles.HexNumber, null, out nch);

			// Get floating point
			int nfPoint;
			int.TryParse(message[5].ToString(), out nfPoint);

			char polarity = message[4];

			// Build value string
			string value = message.Substring(6, 8);
			value = value.TrimStart('0');
			value = value.Insert(value.Length - nfPoint, ".");
			if (polarity == '1')
				value = "-" + value;

			double dVal;
			double.TryParse(value, out dVal);

			_channelTemp[nch] = dVal;
		}

		private void _timerTimeout_Elapsed(object sender, ElapsedEventArgs e)
		{
			_timerTimeout.Stop();
			_channelTemp.Clear();
			_isDataReceived = false;
		}


		#endregion Methods

	}
}
