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
                DeviceType = DeviceTypesEnum.KeySight
            };

            device.ParemetersList = new ObservableCollection<DeviceParameterData>()
             {
                new Scope_KeySight_ParamData() { Name = "channel to config",        Command = "Choose channel" ,      DeviceType = DeviceTypesEnum.KeySight,  },
                
                
                
                new Scope_KeySight_ParamData() { Name = "Channel Turn on/off",      Command = ":CHANnel<channel>:DISPlay" ,       DeviceType = DeviceTypesEnum.KeySight,   DropDown = new List<DropDownParamData>() { new DropDownParamData() {Name = "Channel OFF", Value = "0" }, new DropDownParamData() {Name = "Channel ON", Value = "1" } } },
                new Scope_KeySight_ParamData() { Name = "Channel to measurement",   Command = "Set signal" ,            data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Volte/Ampere",            Command = "Probe  Volte/Ampere",   data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Clear all ",               Command = "Clear all mesure" ,      data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Run/Stop",                Command = "Run Control" ,           data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Acquire",                  Command = "Acquire" ,               data="",    DeviceType = DeviceTypesEnum.KeySight,  },                      
                new Scope_KeySight_ParamData() { Name = "Time scaling",             Command = "Time scaling" ,          data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Measurement Scaling",      Command = "Measurement Scaling" ,   data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Scaling prob ",            Command = "Scaling prob" ,          data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Triger mode",              Command = "Triger mode" ,           data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Triger slope parameter",   Command = "Triger slope" ,          data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "File_name",                Command = "file_name" ,             data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Save ",                    Command = "Save" ,                  data="",    DeviceType = DeviceTypesEnum.KeySight,  },
               
                new Scope_KeySight_ParamData() { Name = "CYCLe DC",                 Command = "CYCLe,DC" ,              data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "DISPlay DC",               Command = "DISPlay,DC" ,            data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "CYCLe AC",                 Command = "CYCLe,AC" ,              data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "DISPlay AC",               Command = "DISPlay,AC" ,            data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Peak to peak",             Command = "VPP" ,                   data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "VAMPlitude",               Command = "VAMPlitude" ,            data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "VTOP",                     Command = "VTOP" ,                  data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Average CYCLe",            Command = "VAVerage CYCLe" ,        data="",    DeviceType = DeviceTypesEnum.KeySight,  },
                new Scope_KeySight_ParamData() { Name = "Average DISPlay",          Command = "VAVerage DISPlay" ,      data="",    DeviceType = DeviceTypesEnum.KeySight,  },
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

        public Scope_KeySight_Communicator ()
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

                string data_from_scop;

                data_from_scop= Read_command(scopeKeySight);

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

			lock (_lockObj)
			{
				TCPCommService.Send(cmd);
			}

			//double Value = 0;
			//Value= Convert .ToDouble(parameter.Value);

			//if (parameter.Command.ToLower() == ("Choose channel").ToLower())
			//{
			//    if (parameter.data.ToLower() == ("channel1").ToLower() || parameter.data.ToLower() == ("channel2").ToLower() || parameter.data.ToLower() == ("channel3").ToLower() || parameter.data.ToLower() == ("channel4").ToLower())
			//        channel = Convert.ToInt32(parameter.data);
			//}
			//else if (parameter.Command.ToLower() == ("channel ON\\OFF").ToLower())// channel ON\OFF
			//{

			//    if (Value == 0)
			//    {
			//        send(":" + "channel" + channel + ":DISPlay " + "ON");
			//    }
			//    if (Value == 1)
			//    {
			//        send(":" + "channel" + channel + ":DISPlay " + "OFF");
			//    }
			//    //send(":" + "channel" + channel + ":DISPlay " + parameter.data);
			//}
			//else if (parameter.Command.ToLower() == ("Set signal").ToLower())
			//{
			//    send(Measurement[Convert.ToInt32(parameter.data) - 1][1] + "channel" + channel);

			//    //send(":MEASure:" + data + " " + interval + "," + type + "," + channel);
			//}
			//else if (parameter.Command.ToLower() == ("Probe  Volte\\Ampere").ToLower())// volt\amp  <units> ::= {VOLT | AMPere}
			//{
			//    send(":CHANnel" + channel + ":UNITs " + parameter.data);
			//}
			//else if (parameter.Command.ToLower() == ("Clear all mesure").ToLower())
			//{
			//    send(":MEASure:CLEar");
			//}
			//else if (parameter.Command.ToLower() == ("Run Control").ToLower())
			//{
			//    if (parameter.data.ToLower() == ("Run").ToLower())
			//    {
			//        send(":RUN");
			//    }
			//    else if (parameter.data.ToLower() == ("Stop").ToLower())
			//    {
			//        send(":STOP ");
			//    }
			//    else if (parameter.data.ToLower() == ("SINGle").ToLower())
			//    {
			//        send(":SINGle ");
			//    }
			//}
			//else if (parameter.Command.ToLower() == ("Acquire").ToLower())
			//{
			//    send(":ACQuire:TYPE HRESolution");
			//}
			//else if (parameter.Command.ToLower() == ("Time scaling").ToLower())
			//{
			//    send(":TIMebase:SCALe " + parameter.data);
			//}
			//else if (parameter.Command.ToLower() == ("Measurement Scaling").ToLower())
			//{
			//    send(":CHANnel" + channel + ":SCALe " + parameter.data);
			//}
			//else if (parameter.Command.ToLower() == ("Scaling prob").ToLower())
			//{
			//    send(":CHANnel" + channel + ":PROBe " + parameter.data);
			//}
			//else if (parameter.Command.ToLower() == ("Triger mode").ToLower())
			//{
			//    send(":TRIGger:MODE EDGE");
			//}
			//else if (parameter.Command.ToLower() == ("Triger slope").ToLower()) //(NEGative | POSitive | EITHer | ALTernate)
			//{
			//    send(":TRIGger:EDGE:SLOPe "+parameter.data);
			//}
			//else if (parameter.Command.ToLower() == ("Triger value").ToLower())
			//{
			//    send(":TRIGger:GLITch:LEVel "+parameter.data +",CHANnel"+channel);
			//}          
			//else if (parameter.Command.ToLower() == ("file_name").ToLower())
			//{
			//   file_name= parameter.data;
			//}
			//else if (parameter.Command.ToLower() == ("Save").ToLower())
			//{

			// if (parameter.data.ToLower() == ("PNG").ToLower())
			//    {
			//        send(":SAVE:IMAGe:FORMat PNG");
			//        send(":SAVE:IMAGe:STARt \"" + file_name + "\"");
			//        Thread.Sleep(500);
			//    }
			//    else if (parameter.data.ToLower() == ("CSV").ToLower())
			//    {
			//        send(":SAVE:WAVeform:FORMat CSV");
			//        send(":SAVE:WAVeform:STAR \"" + file_name + "\"");
			//        Thread.Sleep(500);
			//    }
			//}


		}

        private void SaveCommand(Scope_KeySight_ParamData parameter, double dVal)
        {
			lock (_lockObj)
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
			}

			Thread.Sleep(500);
            
        }



		//public string Read_command(string command, string data, int channel, string interval, string type)

		private  string Read_command(Scope_KeySight_ParamData parameter)
        {


			//////////////////////////////////////////////////////////////////////////////////////////
			string cmd = parameter.Command;
			// Add channel number
			cmd = cmd.Replace("<channel>", parameter.Channel.ToString());

			// Add Question mark
			int index = cmd.IndexOf(' ');
			if (index < 0)
				cmd += "?";
			else
				cmd = cmd.Insert(index, "?");

			lock (_lockObj)
			{
				TCPCommService.Send(cmd + "\n");
			}

			string response;
			TCPCommService.Read(out response);

			return response;

			//////////////////////////////////////////////////////////////////////////////////////////

			//if (parameter.Command.ToLower() == ("Read Measurement").ToLower())
			//{
			//    //send(":MEASure:" + data + "? " + interval + "," + type + "," + "CHANnel" + channel);
			//    send(Measurement[Convert.ToInt32(parameter.data) - 1][2] + "channel" + channel);

			//    return Read_data();

			//}
			//else if (parameter.Command.ToLower() == ("CYCLe,DC").ToLower())
			//{
			//    send(":MEASure:VRMS? CYCLe,DC," + "channel" + channel);

			//    return Read_data();
			//}
			//else if (parameter.Command.ToLower() == ("DISPlay,DC").ToLower())
			//{
			//    send(":MEASure:VRMS? DISPlay,DC," + "channel" + channel);

			//    return Read_data();
			//}
			//else if (parameter.Command.ToLower() == ("CYCLe,AC").ToLower())
			//{
			//    send(":MEASure:VRMS? CYCLe,AC," + "channel" + channel);

			//    return Read_data();
			//}
			//else if (parameter.Command.ToLower() == ("DISPlay,AC").ToLower())
			//{
			//    send(":MEASure:VRMS? DISPlay,AC," + "channel" + channel);

			//    return Read_data();
			//}

			//else if (parameter.Command.ToLower() == ("VPP").ToLower())
			//{
			//    send(":MEASure:VPP?" + "channel" + channel);

			//    return Read_data();
			//}
			//else if (parameter.Command.ToLower() == ("VAMPlitude").ToLower())
			//{
			//    send(":MEASure:VAMPlitude?" + "channel" + channel);

			//    return Read_data();
			//}
			//else if (parameter.Command.ToLower() == ("VTOP").ToLower())
			//{
			//    send(":MEASure:VTOP ?" + "channel" + channel);

			//    return Read_data();
			//}
			//else if (parameter.Command.ToLower() == ("VAVerage CYCLe").ToLower())
			//{
			//    send(":MEASure:VAVerage? CYCLe," + "channel" + channel);

			//    return Read_data();
			//}
			//else if (parameter.Command.ToLower() == ("VAVerage DISPlay").ToLower())
			//{
			//    send(":MEASure: VAVerage ? DISPlay," + "channel" + channel);

			//    return Read_data();
			//}


			//else if (parameter.Command.ToLower() == ("Read signal").ToLower())
			//{
			//    return "";
			//}
			//else
			//{
			//    return "";
			//}
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
