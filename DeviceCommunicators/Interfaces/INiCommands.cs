
using DeviceCommunicators.Enums;
using NationalInstruments.DAQmx;
using System;

namespace DeviceCommunicators.Interfaces
{
    public interface INiCommands
    {



         string _deviceName { get; set; } // Replace with the actual device name
         string _Port_Io { get; set; }
         double _Min_level_voltage { get; set; }
         double _Max_level_voltage { get; set; }




        // Parameter to test 
         bool[] Digital_port_input { get; set; } 
         double[] Analog_port_input { get; set; } 
         int[] Digital_port_output { get; set; } 
         double[] Analog_port_output { get; set; } 


        void DigitalIO_output(IO_Output output, int State);


        /// <summary>
        /// Read digital input 
        /// Data received  2^(Pin number)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string DigitalIO_input(IO_Pin input);


        void Anolog_output(AO_Output output, double volt);

        string Anolog_input(IO_Pin input);

        string Anolog_input_current(IO_Pin input, double shuntResistor);
    }
}
