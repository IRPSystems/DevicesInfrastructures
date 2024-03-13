using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using DeviceCommunicators.Interfaces;
using Sharp7;

namespace DeviceCommunicators.Dyno3
{
    public class CommandDyno3 
    {
        S7Client pls = new S7Client();
        int result = 0;
        string Derection = "FW";
        public   int max_acceleration = 500;
        public  int max_speed = 5000;

     

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

        public int speed_send(int speed)
        {
            var bufer_wr = new byte[2];
            short send_value = 0;
            send_value = Convert.ToInt16(speed);
            S7.SetIntAt(bufer_wr, 0, send_value);
            result  = pls.DBWrite(9, 10, 2, bufer_wr);
            return  result;
        }
        public int  Torque_load(int torque_load)
        {
            var bufer_wr = new byte[2];
            short send_value = 0;
            send_value = Convert.ToInt16(torque_load);
            S7.SetIntAt(bufer_wr, 0, send_value);
            result = pls.DBWrite(9, 2, 2, bufer_wr);
            return result;
        }

        public int  AccelerationDeceleration_send(int acc_dec_send)
        {
            {
                var bufer_wr = new byte[2];
                short send_value = 0;
                send_value = Convert.ToInt16(acc_dec_send);
                S7.SetIntAt(bufer_wr, 0, send_value);
                result = pls.DBWrite(9, 22, 2, bufer_wr);
                return result;
            }
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
                string  a =Console.ReadLine();
               
               // parameter_from_PLS=Convert.ToString(S7.GetDIntAt(buffer, 0));
                //return parameter_from_PLS;
                
                
                return a;
            }
            else if (Param.ToLower() == "Torque".ToLower())
            {
                parameter_from_PLS = Convert.ToString(S7.GetDIntAt(buffer, 4));
                return parameter_from_PLS;
            }
            return "";
        }

        public int speed_command_to_dyno(int speed)
        {
            
            if (speed > max_speed)
            {
                return 1; // speed command above max speed 
            }
            else if (speed< 0)
            {
                return 2; // speed negative value 
            }
            else if (speed <= max_speed)
            {
                return speed_send(speed);
            }
            return 4; //error
        }


        public int AccelerationDeceleration_command_to_dyno (int ACC_dec)
        {
            int send_value;
           if (ACC_dec > max_acceleration)
            {
                send_value = 1000000/max_acceleration;
                return AccelerationDeceleration_send(send_value);
            }
           else if(ACC_dec< 50)
            {
                send_value = 1000000/50;
                return AccelerationDeceleration_send(send_value);
            }
           else if (ACC_dec >50 && ACC_dec<500 )
            {
                send_value = 1000000/ACC_dec;
                return AccelerationDeceleration_send(send_value);
            }

            return 0;
        }
    }




    
}
