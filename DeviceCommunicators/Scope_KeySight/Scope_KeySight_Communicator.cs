using Communication.Services;
using DeviceCommunicators.General;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using DeviceCommunicators.Enums;
using Entities.Models;
using Entities.Enums;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;
using DeviceCommunicators.Models;

namespace DeviceCommunicators.Scope_KeySight
{
    public  class Scope_KeySight_Communicator  : DeviceCommunicator, IDisposable
    {

		 #region Fields

		public int channel = 1; // channel of scope
		private string file_name = "Scope";




		private string _data = null;
       
        private string _iPaddres;
        private int _port;




        public void AddJson()
        {
            DeviceData device = new DeviceData()
            {
                Name = "Scope_KeySight",
                DeviceType = DeviceTypesEnum.ScopeKeysight
            };

            device.ParemetersList = new ObservableCollection<DeviceParameterData>()
             {
                new Scope_KeySight_ParamData() { Name = "channel to config",        Command = "Choose channel" ,      DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                
                
                
                new Scope_KeySight_ParamData() { Name = "Channel Turn on/off",      Command = ":CHANnel<channel>:DISPlay" ,       DeviceType = DeviceTypesEnum.ScopeKeysight,   DropDown = new List<DropDownParamData>() { new DropDownParamData() {Name = "Channel OFF", Value = "0" }, new DropDownParamData() {Name = "Channel ON", Value = "1" } } },
                new Scope_KeySight_ParamData() { Name = "Channel to measurement",   Command = "Set signal" ,            data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Volte/Ampere",            Command = "Probe  Volte/Ampere",   data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Clear all ",               Command = "Clear all mesure" ,      data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Run/Stop",                Command = "Run Control" ,           data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Acquire",                  Command = "Acquire" ,               data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },                      
                new Scope_KeySight_ParamData() { Name = "Time scaling",             Command = "Time scaling" ,          data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Measurement Scaling",      Command = "Measurement Scaling" ,   data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Scaling prob ",            Command = "Scaling prob" ,          data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Triger mode",              Command = "Triger mode" ,           data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Triger slope parameter",   Command = "Triger slope" ,          data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "File_name",                Command = "file_name" ,             data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Save ",                    Command = "Save" ,                  data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
               
                new Scope_KeySight_ParamData() { Name = "CYCLe DC",                 Command = "CYCLe,DC" ,              data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "DISPlay DC",               Command = "DISPlay,DC" ,            data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "CYCLe AC",                 Command = "CYCLe,AC" ,              data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "DISPlay AC",               Command = "DISPlay,AC" ,            data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Peak to peak",             Command = "VPP" ,                   data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "VAMPlitude",               Command = "VAMPlitude" ,            data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "VTOP",                     Command = "VTOP" ,                  data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Average CYCLe",            Command = "VAVerage CYCLe" ,        data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
                new Scope_KeySight_ParamData() { Name = "Average DISPlay",          Command = "VAVerage DISPlay" ,      data="",    DeviceType = DeviceTypesEnum.ScopeKeysight,  },
            };






            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;
            var sz = JsonConvert.SerializeObject(device, settings);
            File.WriteAllText(@"C:\dev\Infrastructure_Evva\Evva\Data\Device Communications\Scope_KeySight.json", sz);

        }







        #endregion Fields




        #region Properties






        private TcpStaticService TCPCommService
        {
            get => CommService as TcpStaticService;
        }

        #endregion Properties



        #region Constructor

        public Scope_KeySight_Communicator (LogLineListService logLineList) :
			base(logLineList)
		{ 
            
        }

		#endregion Constructor





		#region Methods

		public void Init(
            bool isUSBSimulator,
            string iPaddres, 
            int port)
		{

			try
			{
				_iPaddres = iPaddres;
				_port = port;

				CommService = new TcpStaticService(_iPaddres, _port);
				CommService.Init(true);



				InitBase();
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to init the ScopeKeySight", ex);
			}


		}


		protected override CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
        {

            if (data.IsSet)
            {
                SetParamValue_Do(
                    data.Parameter,
                    data.Value,
                    data.Callback);
            }
            else
            {
                GetParamValue_Do(
                    data.Parameter,
                    data.Callback);
            }

            return CommunicatorResultEnum.OK;

        }
        
 
        public void SetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is Scope_KeySight_ParamData scopeKeySight))
                    return;

                Send_command(scopeKeySight, value);

                callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }

            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to set Command for Switch relay" + param.Name, ex);
            }
        }
        

        public void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is Scope_KeySight_ParamData scopeKeySight))
                    return;

                // The only parameter that requires response is the identification
                // parameter used for keepalive
                if(scopeKeySight.Command == null ||
                    scopeKeySight.Command.StartsWith("*IDN") == false)
                {
					callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                    return;
				}

                
				param.Value = Read_command(scopeKeySight);

                Thread.Sleep(10);


                if (param.Value == null)
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                else
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);

            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }

       







        public void Send_command(Scope_KeySight_ParamData parameter, double dVal)
        {
            if(parameter.Name == "Save Data")
            {
                SaveCommand(parameter, dVal);
                return;
            }

			string cmd = parameter.Command;

			// Add channel number
			cmd = cmd.Replace("<channel>", parameter.Channel.ToString());

			cmd += "\n";

			TCPCommService.Send(cmd);

			

		}

        private void SaveCommand(Scope_KeySight_ParamData parameter, double dVal)
        {
            
            if (dVal == 0)
			{
				TCPCommService.Send(":SAVE:IMAGe:FORMat PNG\n");
				TCPCommService.Send(":SAVE:IMAGe:STARt \"" + parameter.data + "\"\n");
			}
			else
			{
				TCPCommService.Send(":SAVE:WAVeform:FORMat CSV\n");
				TCPCommService.Send(":SAVE:WAVeform:STAR \"" + file_name + "\"\n");
			}


			Thread.Sleep(500);
            
        }



		//public string Read_command(string command, string data, int channel, string interval, string type)

		private  string Read_command(Scope_KeySight_ParamData parameter)
        {


			//////////////////////////////////////////////////////////////////////////////////////////
			string cmd = parameter.Command;
			if(string.IsNullOrEmpty(cmd))
				return null;

			// Add channel number
			cmd = cmd.Replace("<channel>", parameter.Channel.ToString());

			// Add Question mark
			int index = cmd.IndexOf(' ');
			if (index < 0)
				cmd += "?";
			else
				cmd = cmd.Insert(index, "?");

			TCPCommService.Send(cmd + "\n");

			string response;
			TCPCommService.Read(out response);

			return response;

			
		}

		public override bool Equals(object obj)
        {
            return obj is Scope_KeySight_Communicator  communication &&
                    EqualityComparer<TcpStaticService>.Default.Equals(TCPCommService, communication.TCPCommService) &&
                    _data == communication._data &&
                    IsInitialized == communication.IsInitialized;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

       


        #endregion Methods
    }
}
