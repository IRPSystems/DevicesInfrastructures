using Communication.Services;
using DeviceCommunicators.Dyno;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.PowerSupplayKeysight;
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
            ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as TcpConncetViewModel;
        }
        protected override void ConstructConnectionViewModel(LogLineListService logLineList)
        {

            ConnectionViewModel = new TcpConncetViewModel(5555, 14323, 14320, "");
            (ConnectionViewModel as TcpConncetViewModel).UdpCheckboxVisibility = Visibility.Hidden;
            (ConnectionViewModel as TcpConncetViewModel).EASearchIPEvent +=
                TcpConncetVM_RigolM300SearchIPEvent;
        }
        protected override void ConstructCheckConnection()
        {
            DeviceParameterData data = new RigolM300_ParamData()
            {
                Name = "Identify",
                Cmd = "*OPC?"
            };

            CheckCommunication = new CheckCommunicationService(
                this,
                data,
                "Rigol M300");

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

           (DeviceCommunicator as RigolM300_Communicator).Init(
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
