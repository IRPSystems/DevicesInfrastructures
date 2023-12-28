
using CommunityToolkit.Mvvm.ComponentModel;
using CsvHelper;
using DeviceCommunicators.Enums;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using DeviceHandler.Models.DeviceFullDataModels;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TestDevices
{
	public class ParamRecordingService: ObservableObject
	{
		#region Properties

		public bool IsRecording { get; set; }

		#endregion Properties

		#region Fields



		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private TextWriter _textWriter;
		private CsvWriter _csvWriter;

		private ObservableCollection<DeviceParameterData> _logParametersList;

		


		private bool _isFirstReceived;
		private int _receivedCounter;

		private DateTime _prevTime;
		private double _secCounter;

		#endregion Fields

		#region Constructor

		public ParamRecordingService()
		{
		}

		#endregion Constructor

		#region Methods


		public void StartRecording(
			string recordingPath,
			int recordingRate,
			ObservableCollection<DeviceParameterData> logParametersList,
			DeviceFullData deviceFullData)
		{
			_logParametersList = logParametersList;

			IsRecording = false;

			try
			{
				if (Application.Current == null)
					return;

				Application.Current.Dispatcher.Invoke(() =>
				{
					Mouse.OverrideCursor = Cursors.Wait;
				});

				_isFirstReceived = false;
				_receivedCounter = 0;



				if (Directory.Exists(recordingPath) == false)
				{
					LoggerService.Error(this, "The recording path \"" + recordingPath + "\" was not found", "Run Error");

					Application.Current.Dispatcher.Invoke(() =>
					{
						Mouse.OverrideCursor = null;
					});
					return;
				}


				string path = Path.Combine(
					recordingPath,
					deviceFullData.Device.Name + " " +
						DateTime.Now.ToString("dd-MMM-yyyy HH-mm-ss") + ".csv");

				_textWriter = new StreamWriter(path, false, System.Text.Encoding.UTF8);
				_csvWriter = new CsvWriter(_textWriter, CultureInfo.CurrentCulture);


				if (_csvWriter == null)
					return;



				


				_csvWriter.WriteField("Time [sec]");
				foreach (DeviceParameterData data in _logParametersList)
				{
					_csvWriter.WriteField(data.Name + " [" + data.Units + "]");
				}


				_csvWriter.NextRecord();




				

				foreach (DeviceParameterData data in _logParametersList)
				{
					deviceFullData.ParametersRepository.Add(data, RepositoryPriorityEnum.Medium, ParamReceived);
				}

				if (_cancellationTokenSource != null)
					_cancellationTokenSource.Cancel();
				_cancellationTokenSource = new CancellationTokenSource();
				_cancellationToken = _cancellationTokenSource.Token;

				_secCounter = 0;
				HandleLogParam(recordingRate);

				IsRecording = true;

				Application.Current.Dispatcher.Invoke(() =>
				{
					Mouse.OverrideCursor = null;
				});
			}
			catch(Exception ex) 
			{
				LoggerService.Error(this, "Failed to start the recording", "Run Error", ex);

				Application.Current.Dispatcher.Invoke(() =>
				{
					Mouse.OverrideCursor = null;
				});
			}
		}

		public void StopRecording(DeviceFullData deviceFullData)
		{
			if (!IsRecording)
				return;

			lock (_lockObj)
			{
				if(_csvWriter != null)
					_csvWriter.Dispose();
				_csvWriter = null;

				if (_textWriter != null)
					_textWriter.Close();

				if(_cancellationTokenSource != null)
					_cancellationTokenSource.Cancel();
			}

			

			foreach (DeviceParameterData data in _logParametersList)
			{
				deviceFullData.ParametersRepository.Remove(data, ParamReceived);
			}

			IsRecording = false;
		}

		
		private void ParamReceived(DeviceParameterData param, CommunicatorResultEnum result, string errDescription)
		{
			if(!_isFirstReceived)
			{
				_receivedCounter++;
				if(_receivedCounter > _logParametersList.Count) 
				{
					_isFirstReceived = true;
				}
			}
		}


		private object _lockObj = new object();
		private void HandleLogParam(int recordingRate)
		{
			Task.Run(() =>
			{
				while (!_cancellationToken.IsCancellationRequested)
				{
					if (!_isFirstReceived)
						continue;

					try
					{
						lock (_lockObj)
						{
							if (_csvWriter == null)
								break;


							DateTime now = DateTime.UtcNow;
							if(_csvWriter.Row > 2)
							{ 
								TimeSpan diff = now - _prevTime;
								_secCounter += diff.TotalSeconds;
							}

							_csvWriter.WriteField(_secCounter);
							_prevTime = now;

							foreach (DeviceParameterData paramData in _logParametersList)
							{
								try
								{

									if (paramData.Value == null)
									{
										_csvWriter.WriteField("");
										continue;
									}

									if (paramData.Value.GetType().Name == "String")
									{
										if (string.IsNullOrEmpty((string)paramData.Value))
											LoggerService.Inforamtion(this, "string empty ");
										LoggerService.Inforamtion(this, "string: " + paramData.Value.ToString());

										_csvWriter.WriteField("NaN");
										continue;
									}

									double value = Convert.ToDouble(paramData.Value);

									_csvWriter.WriteField(value);

									System.Threading.Thread.Sleep(1);
								}
								catch (Exception ex) 
								{
									LoggerService.Error(this, "Failed to write record field", ex);
								}
							}

							_csvWriter.NextRecord();
							System.Threading.Thread.Sleep(1000 / recordingRate);
						}
					}
					catch (Exception ex)
					{
						LoggerService.Error(this, "Failed to log data", ex);
					}

				}
			}, _cancellationToken);
		}

		#endregion Methods

		#region Events

		#endregion Events
	}
}
