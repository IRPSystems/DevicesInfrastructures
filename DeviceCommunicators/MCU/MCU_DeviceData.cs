
using DeviceCommunicators.Models;
using Entities.Enums;
using Entities.Models;
using System.Collections.ObjectModel;

namespace DeviceCommunicators.MCU
{
	public class MCU_DeviceData: DeviceData
	{
		public ObservableCollection<DeviceParameterData> MCU_FullList { get; set; }

		public ObservableCollection<ParamGroup> MCU_GroupList { get; set; }

		public override ObservableCollection<DeviceParameterData> ParemetersList 
		{
			get => MCU_FullList;
		}

		public MCU_DeviceData(string name, DeviceTypesEnum deviceTypes)
		{
			DeviceType = deviceTypes;
			Name = name;
		}

		public override object Clone()
		{
			MCU_DeviceData mcu_DeviceData = MemberwiseClone() as MCU_DeviceData;
			mcu_DeviceData.MCU_GroupList = new ObservableCollection<ParamGroup>();

			foreach (ParamGroup group in MCU_GroupList)
			{
				mcu_DeviceData.MCU_GroupList.Add(group.Clone() as ParamGroup);
			}

			mcu_DeviceData.MCU_FullList = new ObservableCollection<DeviceParameterData>();
			foreach (ParamGroup group in mcu_DeviceData.MCU_GroupList)
			{
				foreach (MCU_ParamData data in group.ParamList)
					mcu_DeviceData.MCU_FullList.Add(data);
			}
			
			return mcu_DeviceData;
		}

		public void HideNotVisibleGroups()
		{
			foreach(ParamGroup group in MCU_GroupList) 
			{
				group.HideNotVisibleGroups();
			}
		}
	}
}
