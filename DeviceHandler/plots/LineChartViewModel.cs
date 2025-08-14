

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.MCU;
using Services.Services;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace DeviceHandler.Plots
{
	public class LineChartViewModel : ObservableObject, IPlotControl
	{
		public class SeriesData : ObservableObject
		{
			public double Value { get; set; }
			public DateTime Time { get; set; }
		}

		#region Properties

		public ChartSeriesCollection LineSeriesList { get; set; }

		public bool IsExtendable
		{
			get => true;
		}

		public bool IsPloting { get; set; }

		public int ChartIntervalSec
		{
			get => _chartIntervalSec;
			set
			{
				_chartIntervalSec = value;
				SetMaxNumOfPoints();
			}
		}



		public DateTime XAxisMin { get; set; }
		public DateTime XAxisMax { get; set; }

		#endregion Properties

		#region Fields

		private System.Timers.Timer _chartUpdateTimer;

		private Dictionary<string, MCU_ParamData> _nameToParamDataDict;

		private int _maxNumOfPoints;

		private const int _updateInterval = 50;

		private int _chartIntervalSec;

		private Dictionary<string, FastLineSeries> _nameToSeriesesDict;

		#endregion Fields


		#region Constructor

		public LineChartViewModel()
		{
			LineSeriesList = new ChartSeriesCollection();
			_nameToParamDataDict = new Dictionary<string, MCU_ParamData>();
			_nameToSeriesesDict = new Dictionary<string, FastLineSeries>();


			_chartUpdateTimer = new System.Timers.Timer(_updateInterval);
			_chartUpdateTimer.Elapsed += _chartUpdateTimer_Elapsed;
			_chartUpdateTimer.Start();

			IsPloting = true;

			PauseCommand = new RelayCommand(Pause);
			ContinueCommand = new RelayCommand(Continue);

			ChartIntervalSec = 10;
			SetMaxNumOfPoints();
		}

		#endregion Constructor


		#region Methods

		public void Kill()
		{
			_chartUpdateTimer.Stop();
			RemoveAllSeries();
		}

		private void SetMaxNumOfPoints()
		{
			TimeSpan interval = TimeSpan.FromSeconds(ChartIntervalSec);

			double percentageOfIntervalToShow = interval.TotalMilliseconds * 0.66;

			_maxNumOfPoints = (int)(percentageOfIntervalToShow / (double)_updateInterval);

		}

		#region Add/Remove series

		public void AddSeries(MCU_ParamData paramData)
		{
			if (paramData == null) 
				return;

			FastLineSeries newSeries = new FastLineSeries();
			newSeries.StrokeThickness = 2;
			newSeries.Label = paramData.Name;
			newSeries.ItemsSource = new ObservableCollection<SeriesData>();
			newSeries.XBindingPath = "Time";
			newSeries.YBindingPath = "Value";

			LineSeriesList.Add(newSeries);

			_nameToSeriesesDict.Add(paramData.Name, newSeries);
			_nameToParamDataDict.Add(paramData.Name, paramData);
		}

		public void RemoveSeries(MCU_ParamData paramData)
		{
			if (_nameToSeriesesDict.ContainsKey(paramData.Name) == false)
				return;

			FastLineSeries series = _nameToSeriesesDict[paramData.Name];
			_nameToSeriesesDict.Remove(paramData.Name);
			_nameToParamDataDict.Remove(paramData.Name);
			LineSeriesList.Remove(series);
		}

		public void RemoveSeries(string paramName)
		{
			if (_nameToParamDataDict.ContainsKey(paramName) == false)
				return;

			MCU_ParamData paramData = _nameToParamDataDict[paramName];
			RemoveSeries(paramData);
		}

		private void RemoveAllSeries()
		{
			foreach (FastLineSeries series in _nameToSeriesesDict.Values)
			{
				string title = series.Label;
				RemoveSeries(title);
			}
		}

		#endregion Add/Remove series

		private void _chartUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				_chartUpdateTimer.Stop();


				DateTime now = DateTime.Now;

				foreach (FastLineSeries series in _nameToSeriesesDict.Values)
				{
					if (Application.Current == null)
						return;

					string seriesLable = string.Empty;
					Application.Current.Dispatcher.Invoke(() =>
					{
						seriesLable = series.Label;
					});

					if (_nameToParamDataDict.ContainsKey(seriesLable) == false)
						continue;

					MCU_ParamData param = _nameToParamDataDict[seriesLable];
					if (param.Value == null)
						continue;

					double dVal;
					bool res = double.TryParse(param.Value.ToString(), out dVal);
					if (res == false)
						continue;


					Application.Current.Dispatcher.Invoke(() =>
					{
						ObservableCollection<SeriesData> itemsSource =
							series.ItemsSource as ObservableCollection<SeriesData>;

						if (dVal.ToString() != "NaN")
						{

							SeriesData seriesData = new SeriesData
							{
								Time = now,
								Value = dVal
							};

							try
							{
								itemsSource.Add(seriesData);
							}
							catch { }
						}

						while (itemsSource.Count > _maxNumOfPoints)
							itemsSource.RemoveAt(0);
					});



					System.Threading.Thread.Sleep(1);
				}

				if (_nameToSeriesesDict.Count > 0 && Application.Current != null)
				{
					Application.Current.Dispatcher.Invoke(() =>
					{
						ObservableCollection<SeriesData> itemsSource =
							_nameToSeriesesDict.Values.ElementAt(0).ItemsSource as ObservableCollection<SeriesData>;

						if (itemsSource != null && itemsSource.Count > 0)
						{
							XAxisMin = itemsSource[0].Time;
							double chartIntervalSec20Percent = ChartIntervalSec * 0.2;
							XAxisMax = itemsSource[itemsSource.Count - 1].Time + TimeSpan.FromSeconds(chartIntervalSec20Percent);
						}
					});
				}


			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to add point the chart", ex);
			}

			_chartUpdateTimer.Start();
		}

		private void Pause()
		{
			IsPloting = false;
			_chartUpdateTimer.Stop();
		}

		private void Continue()
		{
			IsPloting = true;
			_chartUpdateTimer.Start();
		}

		#endregion Methods

		#region Commands

		public RelayCommand PauseCommand { get; private set; }
		public RelayCommand ContinueCommand { get; private set; }

		#endregion Commands

	}
}
