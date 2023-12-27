using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.YokogawaWT1804E
{
     public class YokogawaWT1804E_ParamData : DeviceParameterData, IParamWithDropDown

    {

        public string Command { set; get; }
        public string data { set; get; } // 0 - not return  result , Number- How many parameters return from the switch relay


        public List<DropDownParamData> DropDown { get; set; }
    }
}
