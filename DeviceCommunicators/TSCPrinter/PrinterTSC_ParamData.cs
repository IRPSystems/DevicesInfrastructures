
using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.TSCPrinter
{
	public class PrinterTSC_ParamData : DeviceParameterData
	{
        public string Cmd { get; set; }
        public string SerialNumber { get; set; }
        public string PartNumber { get; set; }
        public string CustomerPartNumber { get; set; }
        public string Spec { get; set; }
        public string HW_Version { get; set; }
        public string MCU_Version { get; set; }
        public string Prn_Design { get; set; } 
    }
}
