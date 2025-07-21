using DeviceCommunicators.MCU;
using DeviceHandler.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeviceHandler.Plots
{
    /// <summary>
    /// Interaction logic for Speedometer.xaml
    /// </summary>
    public partial class Speedometer : UserControl, IPlotControl, INotifyPropertyChanged
	{
        


        public bool IsExtendable { get; } = false;

        //public event PropertyChangedEventHandler PropertyChanged;

        private double _arcEndAngle;
		public double ArcEndAngle 
        {
            get => _arcEndAngle;

            set 
            {
				_arcEndAngle = value;
				OnPropertyChanged(nameof(ArcEndAngle));
			}
        }

        private double _max;
		public double Max
		{
			get => _max;

			set
			{
				_max = value;
				OnPropertyChanged(nameof(Max));
			}
		}

		private double _min;
		public double Min
		{
			get => _min;

			set
			{
				_min = value;
				OnPropertyChanged(nameof(Min));
			}
		}
		//private bool _take_abs;


		#region ParamData

		public static readonly DependencyProperty ParamDataProperty = DependencyProperty.Register(
			"ParamData", typeof(MCU_ParamData), typeof(Speedometer));

		public MCU_ParamData ParamData
		{
			get => (MCU_ParamData)GetValue(ParamDataProperty);
			set
			{
				SetValue(ParamDataProperty, value);
				OnPropertyChanged(nameof(ParamData));

				OnPropertyChanged(nameof(Title));
				OnPropertyChanged(nameof(Units));
			}
		}

		#endregion ParamData

		public string Title 
        { 
            get
            {
                if (ParamData == null)
                    return "No valid parameter";
                return ParamData.ToString();
            }
        }

        public string Units
        {
            get => (ParamData != null) ? ParamData.Units : "No Units";
		}


        public Speedometer(MCU_ParamData paramData)
        {
            InitializeComponent();

			DataContext = this;

			Init(paramData);

		}

		public void Init(MCU_ParamData paramData)
		{
			if (paramData == null)
				return;

			if (paramData.Value is string strVal && !string.IsNullOrEmpty(strVal))
			{
				ParamData = null;
				Max = 100;
				Min = 0;
				return;
			}


			ParamData = paramData;

			if (ParamData.Range != null)
			{
				Max = (int)ParamData.Range[1];
				Min = (int)ParamData.Range[0];
			}
			else if (ParamData.Value != null)
			{
				double d = 0;

				if (ParamData.Value is string str)
				{
					if (string.IsNullOrEmpty(str))
					{
						ParamData.Value = 0;
						Max = 0;
						Min = 100;
						return;
					}
					else
					{
						double.TryParse(str, out d);
						ParamData.Value = d;
					}
				}

				d = Convert.ToDouble(ParamData.Value);

				Max = Math.Round(d + (d * 0.5));
				Min = Math.Round(d - (d * 0.5));
			}
			else
			{
				Max = 0;
				Min = 100;
			}


			this.Visibility = Visibility.Visible;

			DataContext = this;
		}

        
        public void Kill()
        {
           
        }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
