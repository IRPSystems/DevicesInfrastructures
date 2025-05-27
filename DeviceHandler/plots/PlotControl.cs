using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHandler.Plots
{
    public interface IPlotControl
    {

        bool IsExtendable { get;}
        void Kill();
    }
}
