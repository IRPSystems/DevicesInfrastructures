using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using Entities.Models;
using NationalInstruments.Restricted;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

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
        private List<string> _dumpParamsList;

		public bool IsInitialized { get; set; }


		public YokogawaWT1804E_CommandSimulation()
        {
            random_number= Random_s.Next(0, 1000);

          

            ReadDevicesFileService readDevicesFileService = new ReadDevicesFileService();
            ObservableCollection<DeviceData> list = new ObservableCollection<DeviceData>();

            string path = Directory.GetCurrentDirectory();
            path = Path.Combine(path, @"Data\Device Communications\YOKOGAWA WT1804E.json");


            readDevicesFileService.ReadFromJson("Data\\Device Communications", path, list);

			_deviceData = list[0] as DeviceData;

            _nameToParam = new Dictionary<string, DeviceParameterData>();

			_dumpParamsList = new List<string>()
            {
				"Phase Voltage-U",
				"Phase Voltage-V",
				"Phase Voltage-W",
				"DC Bus Voltage",
				"Phase Current-U",
				"Phase Current-V",
				"Phase Current-W",
				"DC Bus current",
				"System Efficiency",
				"Motor Efficiency",
				"Regenerative Controller Efficiency",
				"Controller Efficiency",
				"Motor power",
				"Power - U",
				"Power - V",
				"Power - W",
				"Power - DC BUS",
				"Controller Output Power",
				"Speed [RPM]",
				"Torque [Nm]",
				"Power Factor-U",
				"Power Factor-V",
				"Power Factor-W",
				"Power Factor-Controller",
				"φ angle-U",
				"φ angle-V",
				"φ angle-W",
				"φ angle-Controller",
			};


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

		public bool Send(string data)
        {
            if (data == "NUMeric:NORMal:VALue?")
            {
                return true;
            }

            int index = data.LastIndexOf(" ");
            string paramName = data.Substring(0, index);
			paramName = paramName.Trim();

            string value = data.Substring(index + 1);
            value = value.Trim();


			if (_nameToParam.ContainsKey(paramName) == false)
                return false;

             _parameter = _nameToParam[paramName];

            if(string.IsNullOrEmpty(value) == false)
                _parameter.Value = value;
          
            Console.WriteLine("Simulation command resive");

            return true;
        }

        public string Read_data()
        {
            return  _parameter.Value.ToString();
        }

        

		public int Receive(out StringBuilder temp)
		{
			temp = new StringBuilder();

			foreach (string param in _dumpParamsList)
			{
				var parameter = _nameToParam[param];
				temp.Append(parameter.Value.ToString() + ",");
			}

			return 0;
		}
	}
}
