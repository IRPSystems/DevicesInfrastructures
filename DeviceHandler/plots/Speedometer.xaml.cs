using DeviceCommunicators.MCU;
using DeviceHandler.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
    public partial class Speedometer : UserControl, IPlotControl
    {
        


        public bool IsExtendable { get; } = false;

        //public event PropertyChangedEventHandler PropertyChanged;

        public double ArcEndAngle { get; set; }

        public double Max { get; set; }
		public double Min { get; set; }
        //private bool _take_abs;


		#region ParamData

		public static readonly DependencyProperty ParamDataProperty = DependencyProperty.Register(
			"ParamData", typeof(MCU_ParamData), typeof(Speedometer));

		public MCU_ParamData ParamData
		{
			get => (MCU_ParamData)GetValue(ParamDataProperty);
			set => SetValue(ParamDataProperty, value);
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

            Init(paramData);

		}

		public Speedometer()
		{
			InitializeComponent();

          //  DataContext = this;

		}

		public void Init(MCU_ParamData paramData)
        {
            if (paramData.Value is string)
            {
                ParamData = null;
				Max = 100;
				Min = 0;
				return;
            }


			ParamData = paramData;

			if(ParamData.Range != null)
            {
                Max = (int)ParamData.Range[1];
                Min = (int)ParamData.Range[0];
			}
            else if(ParamData.Value != null) 
            {
                double d = 0;

				if (ParamData.Value is string str)
                {
                    if (string.IsNullOrEmpty(str))
                        return;

                    double.TryParse(str, out d);
					ParamData.Value = d;

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
    }
}
