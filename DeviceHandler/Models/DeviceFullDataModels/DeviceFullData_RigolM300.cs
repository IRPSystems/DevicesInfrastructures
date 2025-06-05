using Communication.Services;
using DeviceCommunicators.Dyno;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.RigolM300;
using DeviceHandler.Interfaces;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DeviceHandler.Models.DeviceFullDataModels
{
    public class DeviceFullData_RigolM300 : DeviceFullData
    {
        private RigolM300_Communicator _rigolm300Communicator;

        public override DeviceCommunicator DeviceCommunicator
        {
            get
            {
                if (ConnectionViewModel == null)
                    return null;

                return _rigolm300Communicator;

            }

        }

        public DeviceFullData_RigolM300(DeviceData deviceData) :
            base(deviceData)
        {
        }
        protected override string GetConnectionFileName()
        {
            return "RigolM300Connect.json";
        }
        protected override void ConstructCommunicator(LogLineListService logLineList)
        {
            DeviceCommunicator = new RigolM300_Communicator(logLineList);
        }
        protected override void DeserializeConnectionViewModel(
            string jsonString,
            JsonSerializerSettings settings,
            LogLineListService logLineList)
        {
            //ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as RigolM300ConncetViewModel;
        }
        protected override void ConstructConnectionViewModel(LogLineListService logLineList)
        {
            ConnectionViewModel = new SerialAndTCPViewModel(
                115200, "", 14323, 14320,
                5555, "", "Tcp",
                "", "RIGOL TECHNOLOGIES,M300", "*IDN?");
            (ConnectionViewModel as SerialAndTCPViewModel).CommTypesList = new List<string>()
            {
               "TCP"
            };
            (ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.UdpCheckboxVisibility = Visibility.Hidden;
            _rigolm300Communicator = new RigolM300_Communicator(logLineList);

            (ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.EASearchIPEvent +=
                TcpConncetVM_RigolM300SearchIPEvent;
        }
        protected override void ConstructCheckConnection()
        {
            // Implement check connection logic if needed
        }

        private void TcpConncetVM_RigolM300SearchIPEvent()
        {
            Task task = Task.Run(() =>
            {
                RigolM300SearchIP();
            });

        }

        protected override void InitRealCommunicator()
        {

            if ((ConnectionViewModel as SerialAndTCPViewModel).SelectedCommType == "Tcp")
            {
                _rigolm300Communicator.Init(
                    (ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.Address,
                    (ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.Port,
                    (ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.RxPort,
                    (ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.TxPort);
            }

        }

        protected override void InitSimulationCommunicator()
        {
        }

        protected override bool IsSumulation()
        {

            return false;
        }

        private void RigolM300SearchIP()
        {
            if (!(DeviceCommunicator is PowerSupplayEA_ModbusTcp eaCommunicator))
                return;

            if (!(ConnectionViewModel is SerialAndTCPViewModel serialTcpConncet))
                return;

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    serialTcpConncet.TcpConncetVM.SearchNoticeVisibility =
                        System.Windows.Visibility.Visible;
                    serialTcpConncet.IsEnabled = false;
                });
            }

            List<string> ipsList = eaCommunicator.FindEaIPs();



            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    serialTcpConncet.TcpConncetVM.EAIPsList =
                    new ObservableCollection<string>(ipsList);
                    serialTcpConncet.TcpConncetVM.Address = ipsList[0];

                    serialTcpConncet.TcpConncetVM.SearchNoticeVisibility =
                        System.Windows.Visibility.Collapsed;
                    serialTcpConncet.IsEnabled = true;
                });
            }

            InitCheckConnection();
        }
    }
}
