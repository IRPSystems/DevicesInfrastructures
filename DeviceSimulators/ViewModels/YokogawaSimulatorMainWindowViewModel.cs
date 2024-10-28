

using DeviceCommunicators.Models;
using DeviceCommunicators.YokogawaWT1804E;
using DeviceHandler.ViewModels;

namespace DeviceSimulators.ViewModels
{
	public class YokogawaSimulatorMainWindowViewModel : DeviceSimulatorViewModel
	{
		

		#region Fields

		private YokogawaWT1804E_CommandSimulation _commandSimulator;

		private YokogawaWT1804EConncetViewModel _yokoConnectViewModel
		{
			get => ConnectVM as YokogawaWT1804EConncetViewModel;
		}


		#endregion Fields

		#region Constructor

		public YokogawaSimulatorMainWindowViewModel(DeviceData deviceData) :
			base(deviceData)
		{
			ConnectVM = new YokogawaWT1804EConncetViewModel();

			ConnectVM.ConnectEvent += Connect;
			ConnectVM.DisconnectEvent += Disconnect;

			_commandSimulator = new YokogawaWT1804E_CommandSimulation();
		}

		#endregion Constructor

		#region Methods

		private void Connect()
		{
		}

		public override void Disconnect()
		{
		}

		#endregion Methods
	}
}