using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Services;
using Entities.Models;
using NationalInstruments.Restricted;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace DeviceCommunicators.YokogawaWT1804E
{
    public class YokogawaWT1804E_CommandSimulation : IWT1804E
    {
        public int device_index { get; set; } = 0;
        private  Random Random_s= new Random();
        public double random_number = 0;

        private DeviceParameterData _parameter;
        private DeviceData _deviceData;
        private Dictionary<string, DeviceParameterData> _nameToParam;

		public bool IsInitialized { get; set; }


		public YokogawaWT1804E_CommandSimulation()
        {
            random_number= Random_s.Next(0, 1000);

          

            ReadDevicesFileService readDevicesFileService = new ReadDevicesFileService();
            ObservableCollection<DeviceBase> list = new ObservableCollection<DeviceBase>();

            string path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, @"Data\Device Communications");

            readDevicesFileService.ReadFromWT1804EJson(path, list);
            _deviceData = list[0] as DeviceData;

            _nameToParam = new Dictionary<string, DeviceParameterData>();



            foreach (DeviceParameterData parameter in _deviceData.ParemetersList)
            {
                _nameToParam.Add((parameter as YokogawaWT1804E_ParamData).Command, parameter);
                parameter.Value = random_number++;
            }

        }

        public void Init(string ip)
        {
            IsInitialized = true;

			Console.WriteLine("Init pass IP =" + ip);

        }


		public void Dispose()
		{
			IsInitialized = false;

		}

		public bool send(string data)
        {
            if (data == "NUMeric:NORMal:VALue?")
                return true;

            if (_nameToParam.ContainsKey(data) == false)
                return false;

             _parameter = _nameToParam[data];
          
            Console.WriteLine("Simulation command resive");

            return true;
        }

        public string Read_data()
        {
            return  _parameter.Value.ToString();
        }
    }
}
