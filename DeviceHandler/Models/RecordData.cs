using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHandler.Models
{
	public class RecordData : ObservableObject
	{
		public int Index { get; set; }
		public DeviceParameterData Data { get; set; }
	}
}
