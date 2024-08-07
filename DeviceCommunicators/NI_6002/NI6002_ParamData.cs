using DeviceCommunicators.Models;
using Entities.Models;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;

namespace DeviceCommunicators.NI_6002
{

    public class NI6002_ParamData : DeviceParameterData, IParamWithDropDown
    { 
        public int  Io_port { set; get; } // select port 
        public string Status_paramter { set; get; }// Read only (R) or write (RW)
        public string command_to_device { set; get; }
        public double shunt_resistor { set; get; }
        public List<DropDownParamData> DropDown { get; set; }
    }
}
