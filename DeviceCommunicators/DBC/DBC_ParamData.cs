
using DBCFileParser.Model;
using DeviceCommunicators.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace DeviceCommunicators.DBC
{
	public class DBC_ParamData: DeviceParameterData
	{
		public Signal Signal { get; set; }
		public Message ParentMessage { get; set; }
	}

	public class DBC_ParamGroup : DeviceParameterData
	{
		public uint ID { get; set; }

		public ObservableCollection<DBC_ParamData> ParamsList { get; set; }

		public Message Message { get; set; }

		public override string ToString()
		{
			return Name + " - " + ID;
		}

		public void HideNotVisibleGroups()
		{
			bool isVisible = false;
			foreach (DBC_ParamData param in ParamsList)
			{
				if (param.Visibility == Visibility.Visible)
				{
					isVisible = true;
					break;
				}
			}

			if (isVisible)
				Visibility = Visibility.Visible;
			else
				Visibility = Visibility.Collapsed;
		}
	}

	public class DBC_File : DeviceParameterData
	{
		public string FilePath { get; set; }
		public ObservableCollection<DBC_ParamGroup> ParamsList { get; set; }

		public void HideNotVisibleGroups()
		{
			foreach (DBC_ParamGroup group in ParamsList)
			{
				group.HideNotVisibleGroups();
			}
		}
	}
}
