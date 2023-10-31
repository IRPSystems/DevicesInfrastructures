using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.Scop_MSOX3104T
{
     public class WT1804E_ParamData : DeviceParameterData, IParamWithDropDown

    {

        public string Command { set; get; }
        public string data { set; get; } // 0 - not return  result , Number- How many parameters return from the switch relay
        public int Channel { set; get; }
        public int value { set; get; }

        public List<DropDownParamData> DropDown { get; set; }
    }
}
