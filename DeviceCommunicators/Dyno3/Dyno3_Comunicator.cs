﻿using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Services.Services;
using System;
using System.Threading;

namespace DeviceCommunicators.Dyno3
{
    public class Dyno3_Comunicator : DeviceCommunicator, IDisposable
    {

        #region Fields and Properties
        
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
            string ipAddress,
            bool simulation = false)
        {
           _Command_to_Dyno = new CommandDyno3(ipAddress); 


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
            if ((niParamData.command_to_device).ToLower() == "TurnON".ToLower())
            {
                bool value_b = Convert.ToBoolean(niParamData.Value);
                _Command_to_Dyno.TurnON(value_b);
            }
            else if ((niParamData.command_to_device).ToLower() == "speed".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.speed_command_to_dyno(Value_int);
            }
            else if ((niParamData.command_to_device).ToLower() == "acceleration".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.AccelerationDeceleration_command_to_dyno(Value_int);
            }
            else if ((niParamData.command_to_device).ToLower() == "torque".ToLower())
            {
                int Value_int = Convert.ToInt16(niParamData.Value);
                _Command_to_Dyno.Torque_load(Value_int);
            }
            else if((niParamData.command_to_device).ToLower() == "direct".ToLower())
            { 
                string Data_string =Convert.ToString(niParamData.Value);
                _Command_to_Dyno.Set_direct(Data_string); 
            }
            return true;

        }



        public string Read_command(Dyno3_ParamData niParamData)
        {
            string data_return = "";

             if ((niParamData.command_to_device).ToLower() == "speed".ToLower())
            {
                

            }
            else if ((niParamData.command_to_device).ToLower() == "torque".ToLower())
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