using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Markup;
using DeviceCommunicators.Interfaces;
using Sharp7;

namespace DeviceCommunicators.Dyno3Control
{
    public class CommandDyno3 
    {
        S7Client pls = new S7Client();
        int result = 0;
        string Derection = "FW";

     

        public CommandDyno3(string IP)
           {
                result = pls.ConnectTo(IP,0,1);
           }

        public void TurnON(bool turn_On)
        {
            if (turn_On == true)
            {
               
                    var bufer_wr = new byte[2];
                    S7.SetBitAt(bufer_wr, 0, 0, true);// start  motor from PC 
                    S7.SetBitAt(bufer_wr, 0, 1, false);
                    S7.SetBitAt(bufer_wr, 0, 2, false);
                    S7.SetBitAt(bufer_wr, 0, 3, false);
                    S7.SetBitAt(bufer_wr, 0, 4, false);
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, false);
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 0, 1, bufer_wr);





                    S7.SetBitAt(bufer_wr, 0, 0, false);
                    S7.SetBitAt(bufer_wr, 0, 1, false);
                    S7.SetBitAt(bufer_wr, 0, 2, false);
                    S7.SetBitAt(bufer_wr, 0, 3, false);
                    S7.SetBitAt(bufer_wr, 0, 4, false);
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, true); //stop mode hold shaft 
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 7, 1, bufer_wr);

                    S7.SetBitAt(bufer_wr, 0, 0, false);
                    S7.SetBitAt(bufer_wr, 0, 1, true);
                    S7.SetBitAt(bufer_wr, 0, 2, false);
                    S7.SetBitAt(bufer_wr, 0, 3, false);
                    S7.SetBitAt(bufer_wr, 0, 4, false);
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, true ); //start  R6
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 8, 1, bufer_wr);



                    S7.SetBitAt(bufer_wr, 0, 0, false);
                    S7.SetBitAt(bufer_wr, 0, 1, false);
                    S7.SetBitAt(bufer_wr, 0, 2, false);
                    S7.SetBitAt(bufer_wr, 0, 3, false);
                    S7.SetBitAt(bufer_wr, 0, 4, true); //PTC1_Resistance
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, false);
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 11, 1, bufer_wr);

                    S7.SetBitAt(bufer_wr, 0, 0, false);
                    S7.SetBitAt(bufer_wr, 0, 1, false);
                    S7.SetBitAt(bufer_wr, 0, 2, true);// ST_VFD_1 (stat fan ,enable R6 ,timer,error bits)
                    S7.SetBitAt(bufer_wr, 0, 3, false);
                    S7.SetBitAt(bufer_wr, 0, 4, false);
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, false);
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 6, 1, bufer_wr);

                }
            else if(turn_On == false)
            {
                var bufer_wr = new byte[2];
                S7.SetBitAt(bufer_wr, 0, 0, false);// start  motor from PC 
                S7.SetBitAt(bufer_wr, 0, 1, false);
                S7.SetBitAt(bufer_wr, 0, 2, false);
                S7.SetBitAt(bufer_wr, 0, 3, false);
                S7.SetBitAt(bufer_wr, 0, 4, false);
                S7.SetBitAt(bufer_wr, 0, 5, false);
                S7.SetBitAt(bufer_wr, 0, 6, false);
                S7.SetBitAt(bufer_wr, 0, 7, false);
                result = pls.DBWrite(11, 0, 1, bufer_wr);


                
                    S7.SetBitAt(bufer_wr, 0, 0, false);
                    S7.SetBitAt(bufer_wr, 0, 1, false);
                    S7.SetBitAt(bufer_wr, 0, 2, false);
                    S7.SetBitAt(bufer_wr, 0, 3, false);
                    S7.SetBitAt(bufer_wr, 0, 4, false);
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, false);
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 4, 1, bufer_wr); // fw mode 
                
              
                
                    S7.SetBitAt(bufer_wr, 0, 0, false);
                    S7.SetBitAt(bufer_wr, 0, 1, false);
                    S7.SetBitAt(bufer_wr, 0, 2, false);
                    S7.SetBitAt(bufer_wr, 0, 3, false);
                    S7.SetBitAt(bufer_wr, 0, 4, false);
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, false);
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 5, 1, bufer_wr); // fw mode 
                



                S7.SetBitAt(bufer_wr, 0, 0, false);
                S7.SetBitAt(bufer_wr, 0, 1, false);
                S7.SetBitAt(bufer_wr, 0, 2, false);
                S7.SetBitAt(bufer_wr, 0, 3, false);
                S7.SetBitAt(bufer_wr, 0, 4, false);
                S7.SetBitAt(bufer_wr, 0, 5, false);
                S7.SetBitAt(bufer_wr, 0, 6, false); //stop mode hold shaft 
                S7.SetBitAt(bufer_wr, 0, 7, false);
                result = pls.DBWrite(11, 7, 1, bufer_wr);



                Set_direct(Derection);


                S7.SetBitAt(bufer_wr, 0, 0, false);
                S7.SetBitAt(bufer_wr, 0, 1, false);
                S7.SetBitAt(bufer_wr, 0, 2, false);
                S7.SetBitAt(bufer_wr, 0, 3, false);
                S7.SetBitAt(bufer_wr, 0, 4, false);
                S7.SetBitAt(bufer_wr, 0, 5, false);
                S7.SetBitAt(bufer_wr, 0, 6, false); //start  R6
                S7.SetBitAt(bufer_wr, 0, 7, false);
                result = pls.DBWrite(11, 8, 1, bufer_wr);



                S7.SetBitAt(bufer_wr, 0, 0, false);
                S7.SetBitAt(bufer_wr, 0, 1, false);
                S7.SetBitAt(bufer_wr, 0, 2, false);
                S7.SetBitAt(bufer_wr, 0, 3, false);
                S7.SetBitAt(bufer_wr, 0, 4, false); //PTC1_Resistance
                S7.SetBitAt(bufer_wr, 0, 5, false);
                S7.SetBitAt(bufer_wr, 0, 6, false);
                S7.SetBitAt(bufer_wr, 0, 7, false);
                result = pls.DBWrite(11, 11, 1, bufer_wr);

                S7.SetBitAt(bufer_wr, 0, 0, false);
                S7.SetBitAt(bufer_wr, 0, 1, false);
                S7.SetBitAt(bufer_wr, 0, 2, false);// ST_VFD_1 (stat fan ,enable R6 ,timer,error bits)
                S7.SetBitAt(bufer_wr, 0, 3, false);
                S7.SetBitAt(bufer_wr, 0, 4, false);
                S7.SetBitAt(bufer_wr, 0, 5, false);
                S7.SetBitAt(bufer_wr, 0, 6, false);
                S7.SetBitAt(bufer_wr, 0, 7, false);
                result = pls.DBWrite(11, 6, 1, bufer_wr);
            }
            
           
           

           

        }

        public void speed (int speed)
        {
            var bufer_wr = new byte[2];
            short send_value = 0;
            send_value = Convert.ToInt16(speed);
            S7.SetIntAt(bufer_wr, 0, send_value);
            result  = pls.DBWrite(9, 10, 2, bufer_wr);
        }
        public void Torque_load(int torque_load)
        {
            var bufer_wr = new byte[2];
            short send_value = 0;
            send_value = Convert.ToInt16(torque_load);
            S7.SetIntAt(bufer_wr, 0, send_value);
            result = pls.DBWrite(9, 2, 2, bufer_wr);
        }

        public void Set_direct (string direct)
        {
            var bufer_wr = new byte[2];
            if (direct.ToLower()== "FW".ToLower())
            {


                
                S7.SetBitAt(bufer_wr, 0, 0, false);
                S7.SetBitAt(bufer_wr, 0, 1, false);
                S7.SetBitAt(bufer_wr, 0, 2, false);
                S7.SetBitAt(bufer_wr, 0, 3, false);
                S7.SetBitAt(bufer_wr, 0, 4, false);
                S7.SetBitAt(bufer_wr, 0, 5, false);
                S7.SetBitAt(bufer_wr, 0, 6, false);
                S7.SetBitAt(bufer_wr, 0, 7, false);
                result = pls.DBWrite(11, 5, 1, bufer_wr); // rev mode  set to  false 




                S7.SetBitAt(bufer_wr, 0, 0, false);
                S7.SetBitAt(bufer_wr, 0, 1, false);
                S7.SetBitAt(bufer_wr, 0, 2, false);
                S7.SetBitAt(bufer_wr, 0, 3, false);
                S7.SetBitAt(bufer_wr, 0, 4, true);
                S7.SetBitAt(bufer_wr, 0, 5, false);
                S7.SetBitAt(bufer_wr, 0, 6, false);
                S7.SetBitAt(bufer_wr, 0, 7, false);
                result = pls.DBWrite(11, 4, 1, bufer_wr); // fw mode 
                
               



                Derection = "FW";
            }
            else if (direct.ToLower() =="Rev".ToLower())
            {

                    S7.SetBitAt(bufer_wr, 0, 0, false);
                    S7.SetBitAt(bufer_wr, 0, 1, false);
                    S7.SetBitAt(bufer_wr, 0, 2, false);
                    S7.SetBitAt(bufer_wr, 0, 3, false);
                    S7.SetBitAt(bufer_wr, 0, 4, false);
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, false);
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 4, 1, bufer_wr); // fw mode set to false 



                    S7.SetBitAt(bufer_wr, 0, 0, false);
                    S7.SetBitAt(bufer_wr, 0, 1, false);
                    S7.SetBitAt(bufer_wr, 0, 2, false);
                    S7.SetBitAt(bufer_wr, 0, 3, true);
                    S7.SetBitAt(bufer_wr, 0, 4, false);
                    S7.SetBitAt(bufer_wr, 0, 5, false);
                    S7.SetBitAt(bufer_wr, 0, 6, false);
                    S7.SetBitAt(bufer_wr, 0, 7, false);
                    result = pls.DBWrite(11, 5, 1, bufer_wr); // rev mode 
                
                Derection = "Rev";
            }
        }

       public string read_parameter(string Param)
        {
            var buffer =new byte [654];
            result = pls.DBRead(10, 0, 654, buffer);
            string parameter_from_PLS;
            if (Param.ToLower() == "speed".ToLower())
            {
                parameter_from_PLS=Convert.ToString(S7.GetDIntAt(buffer, 0));
                return parameter_from_PLS;
            }
            else if (Param.ToLower() == "Torque".ToLower())
            {
                parameter_from_PLS = Convert.ToString(S7.GetDIntAt(buffer, 4));
                return parameter_from_PLS;
            }
            return "";
        }




    }




    
}
