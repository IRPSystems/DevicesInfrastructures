using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.NumatoGPIO
{
    public class NumatoGPIO_ParamData : DeviceParameterData, IParamWithDropDown
    {
        public int Io_port { set; get; } // select port 
        //public string Status_paramter { set; get; }// Read only (R) or write (RW)
        public string Cmd { get; set; }
        public List<DropDownParamData> DropDown { get; set; }
    }
}
