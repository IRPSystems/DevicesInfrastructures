using Controls.ViewModels;
using System;
using System.Collections.Generic;
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

namespace DeviceHandler.Views
{
	/// <summary>
	/// Interaction logic for DesignParametersView.xaml
	/// </summary>
	public partial class ParametersView : UserControl
	{
		public ParametersView()
		{
			InitializeComponent();

			foreach(FrameworkElement element in grdMain.Children)
			{
				if (element is MultiSelectTreeView)
					element.Name = "tvParameters";
			}
		}
    }
}
