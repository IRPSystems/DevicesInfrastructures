using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;
using System.Security.RightsManagement;

namespace DeviceCommunicators.PowerSupplayKeysight
{
     public class PowerSupplayKeysight_ParamData : DeviceParameterData, IParamWithDropDown

    {
        public string Command { set; get; }
        //public string data { set; get; } // 0 - not return  result , Number- How many parameters return from the switch relay
        //public int Channel { set; get; }
        
        public List<DropDownParamData> DropDown { get; set; }
    }
}
