using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceCommunicators.Models
{
    public class CommSendResLog
    {
        public string SendCommand { get; set; }
        public string ReceivedValue { get; set; }
        public string CommErrorMsg { get; set; }
        public int NumberOfTries { get; set; } = 1;
        public string ParamName { get; set; }
        public string Device { get; set; }
        public string Tool { get; set; }
        public string StepName { get; set; }
    }
}
