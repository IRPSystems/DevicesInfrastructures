using DeviceCommunicators.MCU;
using System.Windows;
using System.Windows.Controls;

namespace DeviceHandler.Views
{
    /// <summary>
    /// Interaction logic for DescriptionBallon.xaml
    /// </summary>
    public partial class DescriptionBallon : UserControl
    {
		#region Parameter

		public static readonly DependencyProperty ParameterProperty = DependencyProperty.Register(
			"Parameter", typeof(MCU_ParamData), typeof(DescriptionBallon));

		public MCU_ParamData Parameter
		{
			get => (MCU_ParamData)GetValue(ParameterProperty);
			set => SetValue(ParameterProperty, value);
		}

		#endregion Parameter

		public DescriptionBallon()
        {
            InitializeComponent();

            
        }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Parameter == null)
				return;

			if (Parameter.Name != null)
			{
				NameText.Text = Parameter.Name;
			}

			if (Parameter.Description != null)
			{
				DescriptionText.Text = Parameter.Description;
			}

			if (Parameter.Default != null)
			{
				DefaultText.Text = Parameter.Default;
			}

			if (Parameter.Cmd != null)
			{
				//! Disabled for costumer integration
				//cmdText.Text = data.Cmd;
			}

			if (Parameter.Range != null)
			{
				rangeText.Text = "[ " + Parameter.Range[0].ToString() + " , " + Parameter.Range[1].ToString() + " ]";
			}

			if (Parameter.Units != null)
			{
				unitsText.Text = Parameter.Units;
			}
			else
			{
				unitsText.Text = "int";
			}
			if (Parameter.Scale != 0)
			{
				scaleText.Text = Parameter.Scale.ToString();
			}

			if(string.IsNullOrEmpty(Parameter.Format) == false)
			{
				foramtText.Text = GetFormat(Parameter.Format);
			}
		}

		private string GetFormat(string paramFormat) 
		{
			if (paramFormat.ToLower().Contains("b"))
				return "Binary";
			else if (paramFormat.ToLower().Contains("d"))
				return "Decimal";
			else if (paramFormat.ToLower().Contains("e"))
				return "Exponential";
			else if (paramFormat.ToLower().Contains("x"))
				return "Hex";

			return paramFormat;
		}
	}
}
