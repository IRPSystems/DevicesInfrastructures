using Communication.Services;
using DeviceCommunicators.General;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using DeviceCommunicators.Enums;
using Entities.Models;
using DeviceCommunicators.NI_6002;
using Entities.Enums;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.IO;
using DeviceCommunicators.Models;

namespace DeviceCommunicators.Scop_MSOX3104T
{
    public  class Scope3104TCommunicator:DeviceCommunicator, IDisposable
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
                Name = "Scop_MSOX3104T",
                DeviceType = DeviceTypesEnum.KeySight
            };

            device.ParemetersList = new ObservableCollection<DeviceParameterData>()
             {
                new Scop_MSOX3104T_ParamData() { User_command = "channel to config",        Command = "Choose channel" ,      DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                
                
                
                new Scop_MSOX3104T_ParamData() { User_command = "Channel Turn on ",         Command = "channel ON\\OFF" ,       data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R", DropDown = new List<DropDownParamData>() { new DropDownParamData() {Name = "Channel OFF", Value = "0" }, new DropDownParamData() {Name = "Channel ON", Value = "1" } } },
                new Scop_MSOX3104T_ParamData() { User_command = "Channel to measurement",   Command = "Set signal" ,            data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Volte\\Ampere",            Command = "Probe  Volte\\Ampere",   data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Clear all ",               Command = "Clear all mesure" ,      data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Run\\Stop",                Command = "Run Control" ,           data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Acquire",                  Command = "Acquire" ,               data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},                      
                new Scop_MSOX3104T_ParamData() { User_command = "Time scaling",             Command = "Time scaling" ,          data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Measurement Scaling",      Command = "Measurement Scaling" ,   data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Scaling prob ",            Command = "Scaling prob" ,          data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Triger mode",              Command = "Triger mode" ,           data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Triger slope parameter",   Command = "Triger slope" ,          data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "File_name",                Command = "file_name" ,             data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
                new Scop_MSOX3104T_ParamData() { User_command = "Save ",                    Command = "Save" ,                  data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="R"},
               
                new Scop_MSOX3104T_ParamData() { User_command = "CYCLe DC",                 Command = "CYCLe,DC" ,              data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="Rw"},
                new Scop_MSOX3104T_ParamData() { User_command = "DISPlay DC",               Command = "DISPlay,DC" ,            data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="Rw "},
                new Scop_MSOX3104T_ParamData() { User_command = "CYCLe AC",                 Command = "CYCLe,AC" ,              data="",    DeviceType = DeviceTypesEnum.KeySight,   Status_paramter="Rw"},
                new Scop_MSOX3104T_ParamData() { User_command = "DISPlay AC",               Command = "DISPlay,AC" ,            data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="Rw"},
                new Scop_MSOX3104T_ParamData() { User_command = "Peak to peak",             Command = "VPP" ,                   data="",    DeviceType = DeviceTypesEnum.KeySight,  Status_paramter="Rw "},
                new Scop_MSOX3104T_ParamData() { User_command = "VAMPlitude",               Command = "VAMPlitude" ,            data="",    DeviceType = DeviceTypesEnum.KeySight,   Status_paramter="Rw"},
                new Scop_MSOX3104T_ParamData() { User_command = "VTOP",                     Command = "VTOP" ,                  data="",    DeviceType = DeviceTypesEnum.KeySight,   Status_paramter="Rw"},
                new Scop_MSOX3104T_ParamData() { User_command = "Average CYCLe",            Command = "VAVerage CYCLe" ,        data="",    DeviceType = DeviceTypesEnum.KeySight,   Status_paramter="Rw"},
                new Scop_MSOX3104T_ParamData() { User_command = "Average DISPlay",          Command = "VAVerage DISPlay" ,      data="",    DeviceType = DeviceTypesEnum.KeySight,   Status_paramter="Rw"},
            };






            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;
            var sz = JsonConvert.SerializeObject(device, settings);
            File.WriteAllText(@"C:\dev\Infrastructure_Evva\Evva\Data\Device Communications\Scop_MSOX3104T.json", sz);

        }







        #endregion Fields




        #region Properties






        private List<string[]> Measurement = new List<string[]>
        { 
        
            new string[]{"DC RMS N CYcle",          ":MEASure:VRMS CYCLe,DC,",      ":MEASure:VRMS? CYCLe,DC,"},
            new string[]{"DC RMS Full Screen",      ":MEASure:VRMS DISPlay,DC,",    ":MEASure:VRMS? DISPlay,DC,"},
            new string[]{"AC RMS N CYcle",          ":MEASure:VRMS CYCLe,AC,",      ":MEASure:VRMS? CYCLe,AC,"},
            new string[]{"AC RMS N Full Screen",    ":MEASure:VRMS DISPlay,AC,",    ":MEASure:VRMS? DISPlay,AC,"},
            new string[]{"Pk-pk",                   ":MEASure:VPP ",                ":MEASure:VPP? " },
            new string[]{"Amplitude",               ":MEASure:VAMPlitude ",         ":MEASure:VAMPlitude?" },
            new string[]{"TOP",                     ":MEASure:VTOP ",               ":MEASure:VTOP? "  },
            new string[]{"Average N Cycle",         ":MEASure:VAVerage CYCLe,",     ":MEASure:VAVerage? CYCLe,"},
            new string[]{"Average Full Screen",     ":MEASure:VAVerage DISPlay,",   ":MEASure:VAVerage? DISPlay,"},




        };











        private TcpStaticService _ScopeMS0X3104T
        {
            get => CommService as TcpStaticService;
        }

        #endregion Properties



        #region Constructor

        public Scope3104TCommunicator(string iPaddres, int port)
        { 
            _iPaddres = iPaddres;
            _port = port;
        }

        #endregion Constructor





        #region Methods

        
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
                if (!(param is Scop_MSOX3104T_ParamData scopeMSOX3104T))
                    return;

                Send_command(scopeMSOX3104T);

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
                if (!(param is Scop_MSOX3104T_ParamData scopeMSOX3104T))
                    return;

                string data_from_scop;

                data_from_scop= Read_command(scopeMSOX3104T);

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

       





        public void Init()
        {

            try
            {
                CommService = new TcpStaticService(_iPaddres, _port);
                CommService.Init(true);



                //InitBase();
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to init the ScopeMS0X3104T", ex);
            }


        }


        private void send(string data)
        {
            
            _ScopeMS0X3104T.Send(data+"\n");
        }




        private string Read_data()
        {
            _ScopeMS0X3104T.Read(out _data);
            return _data;
        }


        //public void Send_command(String command, string data, int channel, string interval, string type, double number)

        public void Send_command(Scop_MSOX3104T_ParamData parameter)
        {
            double Value = 0;
            Value= Convert .ToDouble(parameter.Value);

            if (parameter.Command.ToLower() == ("Choose channel").ToLower())
            {
                if (parameter.data.ToLower() == ("channel1").ToLower() || parameter.data.ToLower() == ("channel2").ToLower() || parameter.data.ToLower() == ("channel3").ToLower() || parameter.data.ToLower() == ("channel4").ToLower())
                    channel = Convert.ToInt32(parameter.data);
            }
            else if (parameter.Command.ToLower() == ("channel ON\\OFF").ToLower())// channel ON\OFF
            {

                if (Value == 0)
                {
                    send(":" + "channel" + channel + ":DISPlay " + "ON");
                }
                if (Value == 1)
                {
                    send(":" + "channel" + channel + ":DISPlay " + "OFF");
                }
                //send(":" + "channel" + channel + ":DISPlay " + parameter.data);
            }
            else if (parameter.Command.ToLower() == ("Set signal").ToLower())
            {
                send(Measurement[Convert.ToInt32(parameter.data) - 1][1] + "channel" + channel);

                //send(":MEASure:" + data + " " + interval + "," + type + "," + channel);
            }
            else if (parameter.Command.ToLower() == ("Probe  Volte\\Ampere").ToLower())// volt\amp  <units> ::= {VOLT | AMPere}
            {
                send(":CHANnel" + channel + ":UNITs " + parameter.data);
            }
            else if (parameter.Command.ToLower() == ("Clear all mesure").ToLower())
            {
                send(":MEASure:CLEar");
            }
            else if (parameter.Command.ToLower() == ("Run Control").ToLower())
            {
                if (parameter.data.ToLower() == ("Run").ToLower())
                {
                    send(":RUN");
                }
                else if (parameter.data.ToLower() == ("Stop").ToLower())
                {
                    send(":STOP ");
                }
                else if (parameter.data.ToLower() == ("SINGle").ToLower())
                {
                    send(":SINGle ");
                }
            }
            else if (parameter.Command.ToLower() == ("Acquire").ToLower())
            {
                send(":ACQuire:TYPE HRESolution");
            }
            else if (parameter.Command.ToLower() == ("Time scaling").ToLower())
            {
                send(":TIMebase:SCALe " + parameter.data);
            }
            else if (parameter.Command.ToLower() == ("Measurement Scaling").ToLower())
            {
                send(":CHANnel" + channel + ":SCALe " + parameter.data);
            }
            else if (parameter.Command.ToLower() == ("Scaling prob").ToLower())
            {
                send(":CHANnel" + channel + ":PROBe " + parameter.data);
            }
            else if (parameter.Command.ToLower() == ("Triger mode").ToLower())
            {
                send(":TRIGger:MODE EDGE");
            }
            else if (parameter.Command.ToLower() == ("Triger slope").ToLower()) //(NEGative | POSitive | EITHer | ALTernate)
            {
                send(":TRIGger:EDGE:SLOPe "+parameter.data);
            }
            else if (parameter.Command.ToLower() == ("Triger value").ToLower())
            {
                send(":TRIGger:GLITch:LEVel "+parameter.data +",CHANnel"+channel);
            }          
            else if (parameter.Command.ToLower() == ("file_name").ToLower())
            {
               file_name= parameter.data;
            }
            else if (parameter.Command.ToLower() == ("Save").ToLower())
            {

             if (parameter.data.ToLower() == ("PNG").ToLower())
                {
                    send(":SAVE:IMAGe:FORMat PNG");
                    send(":SAVE:IMAGe:STARt \"" + file_name + "\"");
                    Thread.Sleep(500);
                }
                else if (parameter.data.ToLower() == ("CSV").ToLower())
                {
                    send(":SAVE:WAVeform:FORMat CSV");
                    send(":SAVE:WAVeform:STAR \"" + file_name + "\"");
                    Thread.Sleep(500);
                }
            }


        }



        //public string Read_command(string command, string data, int channel, string interval, string type)

        private  string Read_command(Scop_MSOX3104T_ParamData parameter)
        {
            if (parameter.Command.ToLower() == ("Read Measurement").ToLower())
            {
                //send(":MEASure:" + data + "? " + interval + "," + type + "," + "CHANnel" + channel);
                send(Measurement[Convert.ToInt32(parameter.data) - 1][2] + "channel" + channel);

                return Read_data();

            }
            else if (parameter.Command.ToLower() == ("CYCLe,DC").ToLower())
            {
                send(":MEASure:VRMS? CYCLe,DC," + "channel" + channel);

                return Read_data();
            }
            else if (parameter.Command.ToLower() == ("DISPlay,DC").ToLower())
            {
                send(":MEASure:VRMS? DISPlay,DC," + "channel" + channel);

                return Read_data();
            }
            else if (parameter.Command.ToLower() == ("CYCLe,AC").ToLower())
            {
                send(":MEASure:VRMS? CYCLe,AC," + "channel" + channel);

                return Read_data();
            }
            else if (parameter.Command.ToLower() == ("DISPlay,AC").ToLower())
            {
                send(":MEASure:VRMS? DISPlay,AC," + "channel" + channel);

                return Read_data();
            }

            else if (parameter.Command.ToLower() == ("VPP").ToLower())
            {
                send(":MEASure:VPP?" + "channel" + channel);

                return Read_data();
            }
            else if (parameter.Command.ToLower() == ("VAMPlitude").ToLower())
            {
                send(":MEASure:VAMPlitude?" + "channel" + channel);

                return Read_data();
            }
            else if (parameter.Command.ToLower() == ("VTOP").ToLower())
            {
                send(":MEASure:VTOP ?" + "channel" + channel);

                return Read_data();
            }
            else if (parameter.Command.ToLower() == ("VAVerage CYCLe").ToLower())
            {
                send(":MEASure:VAVerage? CYCLe," + "channel" + channel);

                return Read_data();
            }
            else if (parameter.Command.ToLower() == ("VAVerage DISPlay").ToLower())
            {
                send(":MEASure: VAVerage ? DISPlay," + "channel" + channel);

                return Read_data();
            }


            else if (parameter.Command.ToLower() == ("Read signal").ToLower())
            {
                return "";
            }
            else
            {
                return "";
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Scope3104TCommunicator communication &&
                    EqualityComparer<TcpStaticService>.Default.Equals(_ScopeMS0X3104T, communication._ScopeMS0X3104T) &&
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
