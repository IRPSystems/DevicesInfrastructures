using DeviceCommunicators.Interfaces;
using System;
using DeviceCommunicators.Enums;
using System.Collections.Generic;
using Entities.Models;

namespace DeviceCommunicators.NI_6002
{
    public class NI6002_CommandSimulation : INiCommands
    {

        #region Fields

        public string _deviceName { get; set; }
        public string _Port_Io { get; set; } = "port0";
        public double _Min_level_voltage { get; set; } = -10;
        public double _Max_level_voltage { get; set; } = 10;


        // Parameter to test 
        public bool[] Digital_port_input { get; set; } = new bool[8];
        public double[] Analog_port_input { get; set; } = new double[8];
        public int[] Digital_port_output { get; set; } = new int[8];
        public double[] Analog_port_output { get; set; } = new double[8];



        









        public NI6002_CommandSimulation()
        {
			Random random_s = new Random();
			int random_number = random_s.Next(1, 8);

            for (int i = (random_number-1); i < 7;i=i+random_number) 
            {
                Digital_port_input[i] = true;
            }


            random_number = random_s.Next(0, 24);

            for (int i = 1; i < 7; i++)
            {
                Analog_port_input[i] = random_number+i;
            }


        }

        #endregion Fields
            

        #region Methods 

        public void DigitalIO_output(IO_Output output ,int State)
        {
            if (State > 0)
            {
                Digital_port_output[(int)output] = Convert.ToInt32(Math.Pow(2, (int)output));
            }
            else
            {
                Digital_port_output[(int)output] = 0;
            }
         
            
        }


        /// <summary>
        /// Read digital input 
        /// Data received  2^(Pin number)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string DigitalIO_input(IO_Pin input)
        {
           if (Digital_port_input[(int)input] ==true)
            {
               
                return "0X"+((int)Math.Pow(2, (int)input)).ToString("X");
            }
            else
            {
                return Convert.ToString(0);
            }
           
        }


       public void Anolog_output(AO_Output output, double volt)
        {
            Analog_port_output[(int)output] = volt;
           
        }
        
        public string Anolog_input(IO_Pin input)
        {

            return Convert.ToString(Analog_port_input[(int)input]);
          
        }

		public string Anolog_input_current(IO_Pin input)
        {
			return Convert.ToString(Analog_port_input[(int)input]);
		}


		#endregion Methods
	}
}
