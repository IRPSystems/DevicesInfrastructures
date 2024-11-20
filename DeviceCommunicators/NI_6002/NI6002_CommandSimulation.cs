using DeviceCommunicators.Interfaces;
using System;

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
        public bool[] Digital_port { get; set; } = new bool[8];
        public double[] Analog_port { get; set; } = new double[8];





        //private double avgCurrentRead;







        public NI6002_CommandSimulation()
        {
			Random random_s = new Random();
			int random_number = random_s.Next(1, 8);

            for (int i = (random_number-1); i < 7;i=i+random_number) 
            {
                Digital_port[i] = true;
            }


            random_number = random_s.Next(0, 24);

            for (int i = 1; i < 7; i++)
            {
                Analog_port[i] = random_number+i;
            }


        }

        #endregion Fields
            

        #region Methods 

        public void DigitalIO_output(int output,int line ,int State)
        {
            if (State > 0)
            {
                Digital_port[(int)output] = true;
            }
            else
            {
                Digital_port[(int)output] = false;
            }
         
            
        }


        /// <summary>
        /// Read digital input 
        /// Data received  2^(Pin number)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string DigitalIO_input(int input, int line)
        {

           if (Digital_port[(int)input] == true)
            {
               
                return "1";
            }
            else
            {
                return "0";
            }
           
        }


       public void Anolog_output(int  output, double volt)
        {

            Analog_port[(int)output] = volt;
           
        }
        
        public string Anolog_input(int input)
        {
            return Convert.ToString(Analog_port[(int)input]);
          
        }

        public string Anolog_input_current(int input, double shuntResistor)
        {
            return Convert.ToString(Analog_port[(int)input]);
        }

        public string Digital_Counter(int numofcounts)
        {
            return "";
        }


        #endregion Methods
    }
}
