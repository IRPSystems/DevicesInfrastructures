
using DeviceCommunicators.Models;
using DeviceCommunicators.Dyno3;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_Dyno3 : DeviceFullData
	{
		public DeviceFullData_Dyno3(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "Dyno3Connect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new Dyno3_Comunicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as IPAddressOnlyConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new IPAddressOnlyConncetViewModel();
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Controller Efficiency");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"Dyno3");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as Dyno3_Comunicator).Init(
				(ConnectionViewModel as IPAddressOnlyConncetViewModel).Address,
				false);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as Dyno3_Comunicator).Init(
				(ConnectionViewModel as IPAddressOnlyConncetViewModel).Address,
				true);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is IPAddressOnlyConncetViewModel dyno3))
				return true;

			return dyno3.IsUdpSimulation;
		}
	}
}
