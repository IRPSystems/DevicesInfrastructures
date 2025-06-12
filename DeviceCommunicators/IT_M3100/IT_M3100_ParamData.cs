using DeviceCommunicators.Enums;
using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceCommunicators.IT_M3100
{
    public class IT_M3100_ParamData : DeviceParameterData
    {
        public string Cmd { get; set; }
        public int Max { get; set; }
        public bool HasValue { get; set; }

        public bool Queryable { get; set; }                   // Whether command supports `?` for querying

        public List<DropDownParamData> DropDown { get; set; }
    }
}
