using DeviceCommunicators.General;
using Services.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceCommunicators.Enums;
using Entities.Models;
using DeviceCommunicators.Interfaces;
using TmctlAPINet;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using DeviceCommunicators.Models;
using System.Timers;
using System.Diagnostics;

namespace DeviceCommunicators.YokogawaWT1804E
{
    public class YokogawaWT1804E_Communicator : DeviceCommunicator, IDisposable
    {

        #region Fields

        public int _device_index = 0;

        private System.Timers.Timer _timer;


        Stopwatch time = new Stopwatch();

       // private TMCTL _connct_to_yoko = new TMCTL();
		private StringBuilder _temp = new StringBuilder(1000);
		private YokogawaWT1804E_ParamData _data = new YokogawaWT1804E_ParamData();
		private string _send_to_yoko = ":NUM:NUM 28 ;ITEM1   URMS,1; ITEM2    URMS,2;ITEM3    URMS,3;ITEM4    udc,4;" +
							"ITEM5     irms,1;ITEM6    irms,2;ITEM7    irms,3;ITEM8    idc,4;" +
							"ITEM9     Eta1;ITEM10     Eta2;ITEM11   Eta3;ITEM12   Eta4;" +
							"ITEM13    PM;ITEM14       P,1;ITEM15   P,2;ITEM16   P,3;" +
							"ITEM17    P,4; ITEM18 P,SIGMA; ITEM19 speed; ITEM20 torque;" +
							"ITEM21    LAMBda,1; ITEM22 LAMBda,2; ITEM23 LAMBda,3; ITEM24 LAMBda,SIGMA;" +
							"ITEM25  PHI,1; ITEM26  PHI,2;ITEM27    PHI,3; ITEM28  PHI,SIGMA;";


		private IWT1804E _yokogawa_communicator;


       

        public Dictionary<string, YokogawaWT1804E_parameters_to_dump> paramers_from_dump = new Dictionary<string, YokogawaWT1804E_parameters_to_dump>()
       {
            {"Phase Voltage-U",                     new YokogawaWT1804E_parameters_to_dump(){name_parameter="Phase Voltage-U",                      Units = "[Vrms]",      command_yoko="URMS,1"} },
            {"Phase Voltage-V",                     new YokogawaWT1804E_parameters_to_dump(){name_parameter="Phase Voltage-V",                      Units = "[Vrms]",      command_yoko="URMS,2"} },
            {"Phase Voltage-W",                     new YokogawaWT1804E_parameters_to_dump(){name_parameter = "Phase Voltage-W",                    Units = "[Vrms]",      command_yoko="URMS,3"} },
            {"DC Bus Voltage",                      new YokogawaWT1804E_parameters_to_dump(){name_parameter = "DC Bus Voltage",                     Units = "[V]",         command_yoko="udc,4" } },
            {"Phase Current-U",                     new YokogawaWT1804E_parameters_to_dump(){name_parameter="Phase Current-U",                      Units="[Arms]",        command_yoko="irms,1"} },
            {"Phase Current-V",                     new YokogawaWT1804E_parameters_to_dump(){name_parameter="Phase Current-V",                      Units="[Arms]",        command_yoko="irms,2"} },
            {"Phase Current-W",                     new YokogawaWT1804E_parameters_to_dump(){name_parameter="Phase Current-W",                      Units="[Arms]",        command_yoko="irms,3"} },
            {"DC Bus current",                      new YokogawaWT1804E_parameters_to_dump(){name_parameter="DC Bus current",                       Units="[A]",           command_yoko="irms,1"} },
            {"System Efficiency",                   new YokogawaWT1804E_parameters_to_dump(){name_parameter = "System Efficiency",                  Units = "[%]",         command_yoko = "Eta1"} },
            {"Motor Efficiency",                    new YokogawaWT1804E_parameters_to_dump(){name_parameter = "Motor Efficiency",                   Units = "[%]",         command_yoko = "Eta2"} },
            {"Regenerative Controller Efficiency",  new YokogawaWT1804E_parameters_to_dump(){name_parameter="Regenerative Controller Efficiency",   Units="[%]",           command_yoko="Eta3"} },
            {"Controller Efficiency",               new YokogawaWT1804E_parameters_to_dump(){name_parameter="Controller Efficiency",                Units="[%]",           command_yoko="Eta4"} },
            {"Motor power",                         new YokogawaWT1804E_parameters_to_dump(){name_parameter="Motor power",                          Units="[W]",           command_yoko="PM"} },
            {"Power - U",                           new YokogawaWT1804E_parameters_to_dump(){name_parameter="Power - U",                            Units="[W]",           command_yoko="P,1"} },
            {"Power - V",                           new YokogawaWT1804E_parameters_to_dump(){name_parameter="Power - V",                            Units="[W]",           command_yoko="P,2"} },
            {"Power - W",                           new YokogawaWT1804E_parameters_to_dump(){name_parameter="Power - W",                            Units="[W]",           command_yoko="P,3"} },
            {"Power - DC BUS",                      new YokogawaWT1804E_parameters_to_dump(){name_parameter="Power - DC BUS",                       Units="[W]",           command_yoko="P,4"} },
            {"Controller Output Power",             new YokogawaWT1804E_parameters_to_dump(){name_parameter="Controller Output Power",              Units="[W]",           command_yoko="P,SIGMA"} },
            {"Speed [RPM]",                         new YokogawaWT1804E_parameters_to_dump(){name_parameter="Speed [RPM]",                          Units="[RPM]",         command_yoko="speed"} },
            {"Torque [Nm]",                         new YokogawaWT1804E_parameters_to_dump(){name_parameter="Torque [Nm]",                          Units="[Nm]",          command_yoko="torque"} },
            {"Power Factor-U",                      new YokogawaWT1804E_parameters_to_dump(){name_parameter="Power Factor-U",                       Units="",              command_yoko="LAMBda,1"} },
            {"Power Factor-V",                      new YokogawaWT1804E_parameters_to_dump(){name_parameter="Power Factor-V",                       Units="",              command_yoko="LAMBda,2"} },
            {"Power Factor-W",                      new YokogawaWT1804E_parameters_to_dump(){name_parameter="Power Factor-W",                       Units="",              command_yoko="LAMBda,3"} },
            {"Power Factor-Controller",             new YokogawaWT1804E_parameters_to_dump(){name_parameter="Power Factor-Controller",              Units="",              command_yoko="LAMBda,SIGMA"} },
            {"φ angle-U",                           new YokogawaWT1804E_parameters_to_dump(){name_parameter="φ angle-U",                            Units="",              command_yoko="PHI,1"} },
            {"φ angle-V",                           new YokogawaWT1804E_parameters_to_dump(){name_parameter="φ angle-V",                            Units="",              command_yoko="PHI,2"} },
            {"φ angle-W",                           new YokogawaWT1804E_parameters_to_dump(){name_parameter="φ angle-W",                            Units="",              command_yoko="PHI,3"} },
            {"φ angle-Controller",                  new YokogawaWT1804E_parameters_to_dump(){name_parameter="φ angle-Controller",                   Units="",              command_yoko="PHI,SIGMA"} },
           
    };




        #endregion Fields




        #region Properties


        public override bool IsInitialized
        {
            get
            {
                if (_yokogawa_communicator == null)
                    return false;

                return _yokogawa_communicator.IsInitialized;

            }
        }


        #endregion Properties



        #region Constructor

        public YokogawaWT1804E_Communicator()
        {

            _timer = new System.Timers.Timer(50);
            _timer.Elapsed += timer_ElapsedEventHandler;
           
        }

        #endregion Constructor



        #region Methods

        public void Init(
            bool isUdpSimulation,
            string ip)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            try
            {
                if (isUdpSimulation == false)
                {
                    _yokogawa_communicator = new YokogawaWT1804E_Command();
                }
                else
                {
                    _yokogawa_communicator = new YokogawaWT1804E_CommandSimulation();

                }

                _yokogawa_communicator.Init(ip);
				_yokogawa_communicator.Send(_send_to_yoko);
                InitBase();

				_timer.Start();
			}
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to init the WT1804E", ex);
            }
        }

        public override void Dispose()
        {
			_timer.Stop();
			_yokogawa_communicator.Dispose();
            

            //if (_cancellationTokenSource != null)
            //{
            //    _cancellationTokenSource.Cancel();
            //}
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
                if (!(param is YokogawaWT1804E_ParamData Yoko))
                    return;

                bool isOK = _yokogawa_communicator.Send(Yoko.Command + " " + value);
                if (isOK)
                {
                   callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                }
                else
                    callback?.Invoke(param, CommunicatorResultEnum.Error, "Send failed");
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
                if (!(param is YokogawaWT1804E_ParamData yoko))
                    return;

                if(yoko.Name != "Controller Efficiency") { }

				bool isOK = Read_command(yoko);


             //   Thread.Sleep(10);

#if DEBUG

#endif

                if (isOK == false)
                {
                   callback?.Invoke(param, CommunicatorResultEnum.NoResponse, "Failed to get the value");
                }
                else
                {
                    if (param.Value == null)
                        callback?.Invoke(param, CommunicatorResultEnum.Error, "Failed to get the value");
                    else
                        callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                }

               // Thread.Sleep(1);

            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }



        private bool Read_command(YokogawaWT1804E_ParamData data)
        {

            if(paramers_from_dump.ContainsKey(data.Command))
            {
                YokogawaWT1804E_parameters_to_dump param = paramers_from_dump[data.Command];
                data.Value = param.value;
                return true;
            }


            return false;
          

                

          
        }

		private void timer_ElapsedEventHandler(object sender, System.Timers.ElapsedEventArgs e)
        {
			Read_Dump();
		}




        public void Read_Dump()
        {
            double start_time = 0;
            double end_time = 0;
            double sum_time = 0;

            time.Start();

            try
            {
                DateTime startTime = DateTime.Now;
                

                int rln = 1000;
                start_time = time.ElapsedMilliseconds;

				int ret = -1;
				for (int i = 0; i < 3; i++)
                {
					_yokogawa_communicator.Send("NUMeric:NORMal:VALue?");
                    ret = _yokogawa_communicator.Receive(out _temp);
                    if (ret != 1)
                        break;
                    System.Threading.Thread.Sleep(1);
                }

                if (ret == 1)
                {
                    for (int i = 0; i < paramers_from_dump.Values.Count; i++)
                    {
                        YokogawaWT1804E_parameters_to_dump param = paramers_from_dump.Values.ElementAt(i);
                        param.value = null;
                    }
                }
                else                  
                    parsing_to_parameters(_temp);


                end_time= time.ElapsedMilliseconds;

                sum_time = end_time - start_time;
                TimeSpan diff = DateTime.Now - startTime;
            }
            catch (Exception ) 
            {
            }

        }

        public  void parsing_to_parameters(StringBuilder temp)
        {
            string[] string_parameter;
            string_parameter = (temp.ToString()).Split(",");
           
            if(string_parameter.Length == 1)
            {
                for (int i = 0; i < paramers_from_dump.Values.Count; i++)
                {
                    YokogawaWT1804E_parameters_to_dump param = paramers_from_dump.Values.ElementAt(i);
                    param.value = null;
                }
            }
         
          for(int i = 0; i < string_parameter.Length && i < paramers_from_dump.Values.Count; i++)
            {

                YokogawaWT1804E_parameters_to_dump param = null;
                param = paramers_from_dump.Values.ElementAt(i);
                string value = string_parameter[i].Trim('\n');
                if (value.ToLower() == "error" || string.IsNullOrEmpty(value))
                    param.value = null;
                else if (value.ToLower() == "inf")
                    param.value = double.PositiveInfinity;
                else if(value.ToLower() == "nan")
                    param.value = double.NaN;
                else
                    param.value = Convert.ToDouble(value);
               
            }


        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }



    }



    #endregion Methods





}

