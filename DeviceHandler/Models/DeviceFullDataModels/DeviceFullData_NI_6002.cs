
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.NI_6002;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.SwitchRelay32;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_NI_6002 : DeviceFullData
	{
		public DeviceFullData_NI_6002(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "NI_6002Connect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new NI6002_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as NI6002ConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new NI6002ConncetViewModel();
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => ((NI6002_ParamData)p).command_to_device == "Read Anolog input 0");

			if(data == null) 
			{
				data = new NI6002_ParamData()
				{
					command_to_device = "Read Anolog input 0",
					Name = "Read Anolog input 0"
				};
			}

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"NI_6002");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as NI6002_Communicator).Init(
						(ConnectionViewModel as NI6002ConncetViewModel).DeviceName);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as NI6002_Communicator).Init(
						(ConnectionViewModel as NI6002ConncetViewModel).DeviceName,
						true);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is NI6002ConncetViewModel ni))
				return true;

			return ni.IsUdpSimulation;
		}
	}
}
