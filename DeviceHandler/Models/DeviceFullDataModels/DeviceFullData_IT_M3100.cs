using Communication.Services;
using DeviceCommunicators.MX180TP;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
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
using DeviceCommunicators.IT_M3100;

namespace DeviceHandler.Models.DeviceFullDataModels
{
    public class DeviceFullData_IT_M3100 : DeviceFullData
    {

        public DeviceFullData_IT_M3100(DeviceData deviceData) :
            base(deviceData)
        {
        }
        protected override string GetConnectionFileName()
        {
            return "IT_M3100Connect.json";
        }
        protected override void ConstructCommunicator(LogLineListService logLineList)
        {
            DeviceCommunicator = new IT_M3100_Communicator(logLineList);
        }
        protected override void DeserializeConnectionViewModel(
            string jsonString,
            JsonSerializerSettings settings,
            LogLineListService logLineList)
        {
            ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as TcpConncetViewModel;
        }
        protected override void ConstructConnectionViewModel(LogLineListService logLineList)
        {

            ConnectionViewModel = new TcpConncetViewModel(5025, 14323, 14320, "");
            (ConnectionViewModel as TcpConncetViewModel).UdpCheckboxVisibility = Visibility.Hidden;
            (ConnectionViewModel as TcpConncetViewModel).EASearchIPEvent +=
                TcpConncetVM_ITM3100SearchIPEvent;
        }
        protected override void ConstructCheckConnection()
        {
            DeviceParameterData data = new IT_M3100_ParamData()
            {
                Name = "Identify",
                Cmd = "*IDN"
            };

            CheckCommunication = new CheckCommunicationService(
                this,
                data,
                "IT_M3100");
        }

        private void TcpConncetVM_ITM3100SearchIPEvent()
        {
            Task task = Task.Run(() =>
            {
                ITM3100SearchIP();
            });

        }

        protected override void InitRealCommunicator()
        {

            (DeviceCommunicator as IT_M3100_Communicator).Init(
                 (ConnectionViewModel as TcpConncetViewModel).Address,
                 (ConnectionViewModel as TcpConncetViewModel).Port,
                 (ConnectionViewModel as TcpConncetViewModel).RxPort,
                 (ConnectionViewModel as TcpConncetViewModel).TxPort);

        }

        protected override void InitSimulationCommunicator()
        {
        }

        protected override bool IsSumulation()
        {
            return false;
        }

        private void ITM3100SearchIP()
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
