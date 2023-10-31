using System;
using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Model;
using Entities.Models;
using Services.Services;


namespace DeviceCommunicators.SwitchRelay32
{
    public class SwitchCommunicator : DeviceCommunicator, IDisposable
    {
		#region Fields


        private string _iPaddres;
        private int _port;

		#endregion Fields

		#region Properties

		private ITcpStaticService _switch_communiction32
        {
            get => CommService as ITcpStaticService;
        }

		#endregion Properties


		#region Constructor

		public SwitchCommunicator()
        {
		}

		#endregion Constructor

		#region Methods

		public void Init(
			bool isUdpSimulation,
			string iPaddres, 
            int port,
			int rxPort = 0,
			int txPort = 0)
		{
			_iPaddres = iPaddres;
			_port = port;

            if(isUdpSimulation)
				CommService = new TcpUdpSimulationService(rxPort, txPort, iPaddres);
            else
			    CommService = new TcpStaticService(iPaddres, port);
            CommService.Init(false);

			InitBase();

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

		private void SetParamValue_Do(DeviceParameterData param, double value,Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is SwitchRelay_ParamData sw_ParamData))
                    return;

				if (param.Name.ToLower() == ("single channel").ToLower())
				{
					if(sw_ParamData.Channel_SW == 0)
					{
						callback?.Invoke(param, CommunicatorResultEnum.Error, "The channel is 0");
						return;
					}
					string message = sw_ParamData.Command + "," + sw_ParamData.Channel_SW + "," + value;
					send(message);
				}
				else if (param.Name.ToLower().Contains("all channels"))
				{
					string message = sw_ParamData.Command + ",1,1";
					send(message);

				}
				else if (param.Name.ToLower() == ("relay registers").ToLower())
				{
					string data = GetRegistersValues(value.ToString());
					if (data == null)
					{
						return;
					}

					string message = sw_ParamData.Command + "," + data;
					send(message);
				}


				//Read_data(sw_ParamData, callback);
				callback?.Invoke(param, CommunicatorResultEnum.OK, null);
			}

            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to set Command for Switch relay" + param.Name, ex);
            }
        }


        private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is SwitchRelay_ParamData sw_ParamData))
                    return;


				if (param.Name.ToLower() == ("Switch status").ToLower())
				{
					string message = sw_ParamData.Command + "," + sw_ParamData.Channel_SW;
					send(message);

					Read_data(sw_ParamData, callback);
				}
				else if (sw_ParamData.Name.ToLower() == ("All relay status").ToLower())
				{
					string message = sw_ParamData.Command;
					send(message);

					Read_data(sw_ParamData, callback);
				}

			}
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }








		

        
        private void send(string data)
        {
            _switch_communiction32.Send(data);
        }


    

        private void Read_data(
            SwitchRelay_ParamData param,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            if (_switch_communiction32 == null)
            {
				callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
				return;
            }

			for (int i = 0; i < 5; i++)
			{
				string data;
				_switch_communiction32.Read(out data);
				if (data == null)
				{
					System.Threading.Thread.Sleep(1);
					continue;
				}
				
				data = data.Replace("\0", string.Empty);
				

				string[] msgPartsList = data.Split(',');
				if (msgPartsList[msgPartsList.Length - 1] == "ERROR")
				{
					callback?.Invoke(param, CommunicatorResultEnum.Error, "Switch relay returned error of parameter \"" + param.Name + "\"");
					return;
				}

				if (param.Name.ToLower().Contains("Switch status"))
				{
					byte value;
					byte.TryParse(msgPartsList[2], out value);
					param.Value = value;

					callback?.Invoke(param, CommunicatorResultEnum.OK, null);
					return;
				}
				else if (param.Name.ToLower() == ("All relay status").ToLower())
				{
					param.Value = 0;

					byte byte1;
					byte byte2;
					byte byte3;
					byte byte4;
					byte.TryParse(msgPartsList[1], out byte1);
					byte.TryParse(msgPartsList[2], out byte2);
					byte.TryParse(msgPartsList[3], out byte3);
					byte.TryParse(msgPartsList[4], out byte4);
					param.Value = 
						(uint)byte4 +
						(uint)(byte3 << 8) +
						(uint)(byte2 << 16) +
						(uint)(byte1 << 24);
					

					callback?.Invoke(param, CommunicatorResultEnum.OK, null);
					return;
				}
				else
				{
					callback?.Invoke(param, CommunicatorResultEnum.OK, null);
				}

				System.Threading.Thread.Sleep(1);

			}

			callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
		}


        private string GetRegistersValues(string sendData)
        {
            ulong val;
            bool res = ulong.TryParse(sendData, out val);
            if(!res)
                return null;

            byte byte1 = (byte)val;
			byte byte2 = (byte)(val >> 8);
			byte byte3 = (byte)(val >> 16);
			byte byte4 = (byte)(val >> 24);

            return byte4 + "," + byte3 + "," + byte2 + "," + byte1;
		}


		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion Methods
	}
}
