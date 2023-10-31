using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.SwitchRelay32
{
    public class SwitchRelay_ParamData : DeviceParameterData, IParamWithDropDown
    {
        public string Command { set; get; }
        public int TypeCommand { set; get; } // 0 - not return  result , Number- How many parameters return from the switch relay
        public int Channel_SW { set; get; }


		public List<DropDownParamData> DropDown { get; set; }
    }
}
