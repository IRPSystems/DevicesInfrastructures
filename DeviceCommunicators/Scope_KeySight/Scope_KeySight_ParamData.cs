using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;
using System.Security.RightsManagement;

namespace DeviceCommunicators.Scope_KeySight
{
     public class Scope_KeySight_ParamData : DeviceParameterData, IParamWithDropDown

    {
        public string User_command {  get; set; } 
        public string Command { set; get; }
        public string Status_paramter { set; get; }// Read only (R) or write (RW)
        public string data { set; get; } // 0 - not return  result , Number- How many parameters return from the switch relay
        public int Channel { set; get; }
        
        public List<DropDownParamData> DropDown { get; set; }
    }
}
