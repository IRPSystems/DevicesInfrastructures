using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Entities.Enums;
using Entities.Models;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.IO;


namespace DeviceCommunicators.Dyno3
{
    public class Dyno3_Comunicator : DeviceCommunicator, IDisposable
    {

        #region Fields and Properties


        public void AddJson()
        {
            DeviceData device = new DeviceData()
            {
                Name = "Dyno3",
                DeviceType = DeviceTypesEnum.Dyno3,
            };

            device.ParemetersList = new ObservableCollection<DeviceParameterData>()
             {
                new Dyno3_ParamData() { Name = "Enable",            DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="Rw" ,DropDown=new List<DropDownParamData>(){ new DropDownParamData() {Name= "ON", Value = "1"}, new DropDownParamData() {Name= "OFF", Value="0" } } },
                new Dyno3_ParamData() { Name = "speed set point",   DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="Rw"  },
                new Dyno3_ParamData() { Name = "Acc\\Dec rate",     DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="Rw"  },
                new Dyno3_ParamData() { Name = "Torque load",       DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="Rw"  },
                new Dyno3_ParamData() { Name = "spin direction",   DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="Rw" ,DropDown=new List<DropDownParamData>(){ new DropDownParamData() {Name= "FW", Value = "1"}, new DropDownParamData() {Name= "REV", Value="0" } } },
                 new Dyno3_ParamData() { Name = "Speed",            DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="R"  },
                 new Dyno3_ParamData() { Name = "Torque",           DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="R"  },


             };


            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;
            var sz = JsonConvert.SerializeObject(device, settings);
            File.WriteAllText(@"C:\Dev\Dyno3.json", sz);

        }






        Dyno3_ParamData Param_dyno3 = new Dyno3_ParamData();
        CommandDyno3 _Command_to_Dyno;



        private bool _isInitialized;
        public override bool IsInitialized => _isInitialized;
       

        #endregion Fields and Properties



        #region Constructor

        public Dyno3_Comunicator()
        {
            _isInitialized = false;
        }

        #endregion Constructor

        #region Methods

        public void Init(
            string IP,
            bool simulation = false)
        {
          

            try
            {
                if (!simulation)
                    _Command_to_Dyno = new CommandDyno3(IP);
                //else
                //    _Command_to_Dyno = new NI6002_CommandSimulation();
                _Command_to_Dyno.config_system_before_start();
                _Command_to_Dyno.TurnON(false);
                InitBase();
            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to init the NI", ex);
            }


            _isInitialized = true;
        }




        public override void Dispose()
        {
            base.Dispose();

            _isInitialized = false;
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
                if (!(param is Dyno3_ParamData niParamData))
                    return;
                //need add to send data
                bool res = Send_command(niParamData);

                if (res)
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                else
                    callback?.Invoke(param, CommunicatorResultEnum.Error, null);
            }

            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to set Command for Dyno 3" + param.Name, ex);
            }
        }


        public void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is Dyno3_ParamData niParamData))
                    return;

                // need change to send data
                string message = Read_command(niParamData);
                if (string.IsNullOrEmpty(message) || message == "Error") 
                {
                    callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
                    return;
                }


                Thread.Sleep(10);


                param.Value = message;
                callback?.Invoke(param, CommunicatorResultEnum.OK, null);

            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }



        public bool Send_command(Dyno3_ParamData niParamData)
        { 
           string value = Convert.ToString(niParamData.Value);
            bool value_b = false;
            if (niParamData.Name.ToLower() == "Enable".ToLower()) // ON or OFF 
            {
                if (value == "1")
                {
                    value_b = true;
                }
                else 
                { 
                    value_b = false;
                }

                //bool value_b = Convert.ToBoolean(value);
                _Command_to_Dyno.TurnON(value_b);
            }
            else if (niParamData.Name.ToLower() == "speed set point".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.speed_command_to_dyno(Value_int);
            }
            else if (niParamData.Name.ToLower() == "Acc\\Dec rate".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.AccelerationDeceleration_command_to_dyno(Value_int);
            }
            else if (niParamData.Name.ToLower() == "Torque load".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.Torque_load(Value_int);
            }
            else if(niParamData.Name.ToLower() == "spin direction".ToLower()) //Forward or  Revers
            {

                if (value == "1")
                {
                    value_b = true;
                }
                else if (value=="0")
                {
                    value_b = false;
                }
                ///string Data_string =Convert.ToString(niParamData.Value);
                _Command_to_Dyno.Set_direct(value_b); 
            }
            else if (niParamData.Name.ToLower() == "Start".ToLower()) // not include in json
            {
                string Data_string = Convert.ToString(niParamData.Value);
                _Command_to_Dyno.config_system_before_start();
            }

             return true;

        }



        public string Read_command(Dyno3_ParamData niParamData)
        {
            string data_return = "";

             if (niParamData.Name.ToLower() == "Speed".ToLower())
            {
                data_return = _Command_to_Dyno.read_parameter("speed");
                return data_return;

            }
            else if (niParamData.Name.ToLower() == "Torque".ToLower())
            {
                data_return = _Command_to_Dyno.read_parameter("Torque");
                return data_return;
            }



            return "";
        }


       


        public override bool Equals(object obj)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion Methods
    }

}