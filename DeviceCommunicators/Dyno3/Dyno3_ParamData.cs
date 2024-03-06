using DeviceCommunicators.Models;
using Entities.Models;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;

namespace DeviceCommunicators.Dyno3Control
{

    public class Dyno3ParamData : DeviceParameterData, IParamWithDropDown
    { 
        
        public string command_to_device { set; get; }

        public List<DropDownParamData> DropDown { get; set; }
    }
}
