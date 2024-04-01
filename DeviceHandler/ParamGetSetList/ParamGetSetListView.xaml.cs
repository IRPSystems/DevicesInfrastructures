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

namespace DeviceHandler.ParamGetSetList
{
	/// <summary>
	/// Interaction logic for ParamGetSetListView.xaml
	/// </summary>
	public partial class ParamGetSetListView : UserControl
	{
		public ParamGetSetListView()
		{
			InitializeComponent();
		}

		private void DataGridKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
			}
		}

		private void cb_DropDownOpened(object sender, EventArgs e)
		{
			if (!(DataContext is ParamGetSetListViewModel vm))
				return;

			if (vm.IsShowButtons)
				return;


			if (!(sender is ComboBox comboBox))
				return;

			comboBox.IsDropDownOpen = false;
		}
    }
}
