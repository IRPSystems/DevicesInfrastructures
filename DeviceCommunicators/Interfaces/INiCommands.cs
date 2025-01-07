
using DeviceCommunicators.Enums;
using NationalInstruments.DAQmx;
using System;

namespace DeviceCommunicators.Interfaces
{
    public interface INiCommands
    {



         


        void DigitalIO_output(int port , int portLine, int State);


        /// <summary>
        /// Read digital input 
        /// Data received  2^(Pin number)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string DigitalIO_input(int port, int portLine);


        void Anolog_output(int port , double volt);

        string Anolog_input(int port);

        string Anolog_input_current(int port, double shuntResistor);

        string Digital_Counter(int numofcounts , int expectedrpm);

    }
}
