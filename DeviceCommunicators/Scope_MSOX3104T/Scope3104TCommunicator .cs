using Communication.Services;
using DeviceCommunicators.General;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using DeviceCommunicators.Enums;
using Entities.Models;
using DeviceCommunicators.Model;

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
                if (!(param is WT1804E_ParamData scopeMSOX3104T))
                    return;

                Send_command(scopeMSOX3104T.Command, scopeMSOX3104T.data);

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
                if (!(param is WT1804E_ParamData scopeMSOX3104T))
                    return;

                string data_from_scop;

                data_from_scop= Read_command(scopeMSOX3104T.Command,scopeMSOX3104T.data);

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

        private void Send_command(String command, string data)
        {
            if (command.ToLower() == ("Choose channel").ToLower())
            {
                if (data.ToLower() == ("channel1").ToLower() || data.ToLower() == ("channel2").ToLower() || data.ToLower() == ("channel3").ToLower() || data.ToLower() == ("channel4").ToLower())
                    channel = Convert.ToInt32(data);
            }
            else if (command.ToLower() == ("channel ON\\OFF").ToLower())// channel ON\OFF
            {
                send(":" + "channel" + channel + ":DISPlay " + data);
            }
            else if (command.ToLower() == ("Set signal").ToLower())
            {
                send(Measurement[Convert.ToInt32(data) - 1][1] + "channel" + channel);

                //send(":MEASure:" + data + " " + interval + "," + type + "," + channel);
            }
            else if (command.ToLower() == ("Probe  Volte\\Ampere").ToLower())// volt\amp  <units> ::= {VOLT | AMPere}
            {
                send(":CHANnel" + channel + ":UNITs " + data);
            }
            else if (command.ToLower() == ("Clear all mesure").ToLower())
            {
                send(":MEASure:CLEar");
            }
            else if (command.ToLower() == ("Run Control").ToLower())
            {
                if (data.ToLower() == ("Run").ToLower())
                {
                    send(":RUN");
                }
                else if (data.ToLower() == ("Stop").ToLower())
                {
                    send(":STOP ");
                }
                else if (data.ToLower() == ("SINGle").ToLower())
                {
                    send(":SINGle ");
                }
            }
            else if (command.ToLower() == ("Acquire").ToLower())
            {
                send(":ACQuire:TYPE HRESolution");
            }
            else if (command.ToLower() == ("Time scaling").ToLower())
            {
                send(":TIMebase:SCALe " + data);
            }
            else if (command.ToLower() == ("Measurement Scaling").ToLower())
            {
                send(":CHANnel" + channel + ":SCALe " + data);
            }
            else if (command.ToLower() == ("Scaling prob").ToLower())
            {
                send(":CHANnel" + channel + ":PROBe " + data);
            }
            else if (command.ToLower() == ("Triger mode").ToLower())
            {
                send(":TRIGger:MODE EDGE");
            }
            else if (command.ToLower() == ("Triger slope").ToLower()) //(NEGative | POSitive | EITHer | ALTernate)
            {
                send(":TRIGger:EDGE:SLOPe "+data);
            }
            else if (command.ToLower() == ("Triger value").ToLower())
            {
                send(":TRIGger:GLITch:LEVel "+data+",CHANnel"+channel);
            }          
            else if (command.ToLower() == ("file_name").ToLower())
            {
               file_name= data;
            }
            else if (command.ToLower() == ("Save").ToLower())
            {

             if (data.ToLower() == ("PNG").ToLower())
                {
                    send(":SAVE:IMAGe:FORMat PNG");
                    send(":SAVE:IMAGe:STARt \"" + file_name + "\"");
                    Thread.Sleep(500);
                }
                else if (data.ToLower() == ("CSV").ToLower())
                {
                    send(":SAVE:WAVeform:FORMat CSV");
                    send(":SAVE:WAVeform:STAR \"" + file_name + "\"");
                    Thread.Sleep(500);
                }
            }


        }



        //public string Read_command(string command, string data, int channel, string interval, string type)

        private  string Read_command(string command, string data)
        {
            if (command.ToLower() == ("Read Measurement").ToLower())
            {
                //send(":MEASure:" + data + "? " + interval + "," + type + "," + "CHANnel" + channel);
                send(Measurement[Convert.ToInt32(data) - 1][2] + "channel" + channel);

                return Read_data();

            }
            else if (command.ToLower() == ("Reasd signal").ToLower())
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
