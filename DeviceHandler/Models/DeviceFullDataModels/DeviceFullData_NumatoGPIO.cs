using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using DeviceCommunicators.NumatoGPIO;
using DeviceCommunicators.ZimmerPowerMeter;
using DeviceHandler.Interfaces;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceHandler.Models.DeviceFullDataModels
{
    public class DeviceFullData_NumatoGPIO : DeviceFullData
    {
        public DeviceFullData_NumatoGPIO(DeviceData deviceData) :
            base(deviceData)
        {

        }

        protected override string GetConnectionFileName()
        {
            return "Numato GPIO.json";
        }
        protected override void ConstructCommunicator()
        {
            DeviceCommunicator = new NumatoGPIO_Communicator();
        }

        protected override void DeserializeConnectionViewModel(
            string jsonString,
            JsonSerializerSettings settings)
        {
            ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
        }

        protected override void ConstructConnectionViewModel()
        {
            ConnectionViewModel = new SerialConncetViewModel(115200, "COM1", 14323, 14320);
        }

        protected override void ConstructCheckConnection()
        {
            DeviceParameterData data = new ZimmerPowerMeter_ParamData()
            {
                Command = "ver",
                Name = "Identification"
            };

            CheckCommunication = new CheckCommunicationService(
            this,
            data,
                "ver"); //todo
        }


        protected override void InitRealCommunicator()
        {
            (DeviceCommunicator as NumatoGPIO_Communicator).Init(
                (ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
                (ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
            (ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
        }

        protected override void InitSimulationCommunicator()
        {
            (DeviceCommunicator as NumatoGPIO_Communicator).Init(
                        (ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
                        (ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
                        (ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
                        (ConnectionViewModel as SerialConncetViewModel).RxPort,
            (ConnectionViewModel as SerialConncetViewModel).TxPort,
                        (ConnectionViewModel as SerialConncetViewModel).Address);
        }

        protected override bool IsSumulation()
        {
            if (!(ConnectionViewModel is SerialConncetViewModel serialConncet))
                return true;

            return serialConncet.IsUdpSimulation;
        }
    }
}
