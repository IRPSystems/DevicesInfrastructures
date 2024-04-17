
using DeviceCommunicators.Enums;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.Interfaces
{
	public interface IPSParamData: IParamWithDropDown
	{
		ParamTypeEnum ParamType { get; set; }
	}
}
