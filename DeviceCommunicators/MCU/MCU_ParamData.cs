
using Entities.Models;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using DeviceCommunicators.Models;

namespace DeviceCommunicators.MCU
{

	
	public class MCU_ParamData : DeviceParameterData, IParamWithDropDown
	{

		public void GetMessageID(ref byte[] id)
		{
			using (var md5 = MD5.Create())
			{
				Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(Cmd)), 0, id, 0, 3);
			}
		}

		public string GroupName { get; set; }

		/// <summary>
		/// Describes the parameter
		/// </summary>
		public String Description { get; set; }

		/// <summary>
		/// parameter default
		/// </summary>
		public String Default { get; set; }

		/// <summary>
		/// parameter command
		/// </summary>
		public String Cmd { get; set; }

		/// <summary>
		/// parameter range
		/// </summary>
		public List<double> Range { get; set; }

		/// <summary>
		/// Allow formating the value
		/// </summary>
		public string Format { get; set; }

		/// <summary>
		/// parameter scale
		/// </summary>
		private double _scale = 1;
		public double Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = (value != 0) ? value : 1;
			}

		}

		public String Note { get; set; }

		/// <summary>
		/// parameter need save button
		/// </summary>
		public bool Save { get; set; } = false;

		/// <summary>
		/// 
		/// </summary>
		public List<DropDownParamData> DropDown { get; set; }

		[JsonIgnore]
		public object Data { get; set; }

		
	}

	public class ParamGroup: DeviceParameterData
	{
		public string GroupName { get; set; }

		public string GroupDescription { get; set; }

		public GroupType GroupType { get; set; }

		public ObservableCollection<MCU_ParamData> ParamList { get; set; }

		public override object Clone()
		{
			ParamGroup paramGroup = MemberwiseClone() as ParamGroup;
			paramGroup.ParamList = new ObservableCollection<MCU_ParamData>();

			foreach (MCU_ParamData data in ParamList)
			{
				paramGroup.ParamList.Add(data.Clone() as MCU_ParamData);
			}

			return paramGroup;
		}

		public void HideNotVisibleGroups()
		{
			bool isVisible = false;
			foreach(MCU_ParamData param in ParamList)
			{
				if(param.Visibility == Visibility.Visible)
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

	public enum GroupType
	{
		GENERAL = 1,
		DRIVING_PROFILE = 2,
		REAL_TIME = 3,
		DEVICE_INFO = 4,
		APP_CONFIG = 5
	}

	
}
