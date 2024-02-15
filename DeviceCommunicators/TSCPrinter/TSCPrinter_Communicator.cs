using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Entities.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DeviceCommunicators.TSCPrinter
{
    public class TSCPrinter_Communicator : DeviceCommunicator
	{
		#region Fields


		#endregion Fields


		#region Properties


		#endregion Properties

		#region Constructor

		public TSCPrinter_Communicator()
		{
			
		}

		#endregion Constructor

		#region Methods

		public void Init(
			bool isUdpSimulation,
			string comName,
			int baudtate,
			int rxPort = 0,
			int txPort = 0,
			string address = "")
        {
            


			InitBase();

		}

		public override void Dispose()
		{
            

			base.Dispose();
		}

		protected override CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
		{

			if (data.IsSet)
			{
				SetParamValue_Do(
					data.Parameter,
					data.Value,
					data.Callback);
			}
			else
			{
				GetParamValue_Do(
					data.Parameter,
					data.Callback);
			}

			return CommunicatorResultEnum.OK;

		}


		private void SetParamValue_Do(
			DeviceParameterData param,
			double value,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{ 
            try
            {
                if (!(param is TSCPrinter_ParamData tscPrinter_Param))
                    return;

                
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set value for parameter: " + param.Name, ex);
			}
		}

			

		private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{ 
            try
            {
                if (!(param is TSCPrinter_ParamData tscPrinter_Param))
                    return;

                
            }
            catch(Exception ex) 
            { 
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
		}

		

		#endregion Methods

	}
}
