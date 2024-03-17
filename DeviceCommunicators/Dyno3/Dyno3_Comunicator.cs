using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;
using DeviceCommunicators.NI_6002;
using Entities.Enums;
using Entities.Models;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
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
                new Dyno3_ParamData() { Name = "spin directiont",   DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="Rw"  },
                 new Dyno3_ParamData() { Name = "Speed",            DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="R"  },
                 new Dyno3_ParamData() { Name = "Torque",           DeviceType = DeviceTypesEnum.Dyno3,   Status_paramter="R"  },


             };


            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;
            var sz = JsonConvert.SerializeObject(device, settings);
            File.WriteAllText(@"C:\dev\Project EVVA\ Dyno3.json", sz);

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
           _Command_to_Dyno = new CommandDyno3(IP); ///("10.0.0.100")

           

            //try
            //{
            //    if (!simulation)
            //        _commmand_to_device = new NI6002_Command(IP);
            //    else
            //        _commmand_to_device = new NI6002_CommandSimulation();


            //    InitBase();
            //}
            //catch (Exception ex)
            //{
            //    LoggerService.Error(this, "Failed to init the NI", ex);
            //}


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
                LoggerService.Error(this, "Failed to set Command for Ni" + param.Name, ex);
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
            if ((niParamData.command_to_device).ToLower() == "Enable".ToLower()) // ON or OFF 
            {
                bool value_b = Convert.ToBoolean(niParamData.Value);
                _Command_to_Dyno.TurnON(value_b);
            }
            else if ((niParamData.command_to_device).ToLower() == "speed set point".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.speed_command_to_dyno(Value_int);
            }
            else if ((niParamData.command_to_device).ToLower() == "Acc\\Dec rate".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.AccelerationDeceleration_command_to_dyno(Value_int);
            }
            else if ((niParamData.command_to_device).ToLower() == "Torque load".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.Torque_load(Value_int);
            }
            else if((niParamData.command_to_device).ToLower() == "spin direction".ToLower()) //Forward or  Revers
            { 
                string Data_string =Convert.ToString(niParamData.Value);
                _Command_to_Dyno.Set_direct(Data_string); 
            }
            return true;

        }



        public string Read_command(Dyno3_ParamData niParamData)
        {
            string data_return = "";

             if ((niParamData.command_to_device).ToLower() == "Speed".ToLower())
            {
                data_return = _Command_to_Dyno.read_parameter("speed");
                return data_return;

            }
            else if ((niParamData.command_to_device).ToLower() == "Torque".ToLower())
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