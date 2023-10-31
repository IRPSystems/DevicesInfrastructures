
using System;

namespace DeviceHandler.Interfaces
{
	public interface IConnectionViewModel
	{
		bool IsConnectButtonEnabled { get; set; }
		bool IsDisconnectButtonEnabled { get; set; }


		void RefreshProperties();

		#region Events

		event Action ConnectEvent;
		event Action DisconnectEvent;

		#endregion Events
	}
}
