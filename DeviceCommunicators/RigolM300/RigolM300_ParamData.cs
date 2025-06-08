using DeviceCommunicators.Enums;
using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceCommunicators.RigolM300
{
    public class RigolM300_ParamData : DeviceParameterData
    {
        public string Cmd { get; set; }                       
        public int Max { get; set; }                          

        public int? Slot { get; set; }                      
        public int? Channel { get; set; }                   
        public string ValueType { get; set; }                 

        public bool Queryable { get; set; }                   // Whether command supports `?` for querying

        public List<DropDownParamData> DropDown { get; set; } 
    }
}
