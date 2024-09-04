
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

	
	public class ATE_ParamData : MCU_ParamData
	{
		public List<DropDownParamData> ATECommand { get; set; }

	}

	

	
}
