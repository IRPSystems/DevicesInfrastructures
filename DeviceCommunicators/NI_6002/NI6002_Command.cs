using DeviceCommunicators.Interfaces;
using NationalInstruments.DAQmx;
using System;
using DeviceCommunicators.Enums;
using Task = NationalInstruments.DAQmx.Task;
using Services.Services;
using NationalInstruments;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DeviceCommunicators.NI_6002
{
    public class NI6002_Command: INiCommands
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

        //For current method
        private double avgCurrentRead;

        private Task myTask;

        #endregion Fields



        #region Constructor
        public NI6002_Command(string device_name)
        {
            _deviceName = device_name;
        }

        #endregion Constructor

        #region command 
        public void DigitalIO_output(IO_Output output ,int State)
        {
            string commannd_to_device = "";

            commannd_to_device = _deviceName + "/" + _Port_Io + "/line" + (int)output;
            //deviceName += "/" + "port0/line" + "0"
            Task digitalWriteTask_Port = new Task();
            //  Create an Digital Output channel and name it.
            digitalWriteTask_Port.DOChannels.CreateChannel(commannd_to_device, _Port_Io,
                ChannelLineGrouping.OneChannelForAllLines);

            //  Write digital port data. WriteDigitalSingChanSingSampPort writes a single sample
            //  of digital data on demand, so no timeout is necessary.
            DigitalSingleChannelWriter digitalWriter_Port = new DigitalSingleChannelWriter(digitalWriteTask_Port.Stream);
            //command to device
            digitalWriter_Port.WriteSingleSamplePort(true, State);
        }


        /// <summary>
        /// Read digital input 
        /// Data received  2^(Pin number)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string DigitalIO_input(IO_Pin input)
        {
            string commannd_to_device = "";

            commannd_to_device = _deviceName + "/" + _Port_Io + "/line" + (int)input;



            Task digReadTaskPort = new Task();
            digReadTaskPort.DIChannels.CreateChannel(
                commannd_to_device,
                "",
                ChannelLineGrouping.OneChannelForAllLines);
            DigitalSingleChannelReader DI_readerPort = new DigitalSingleChannelReader(digReadTaskPort.Stream);
            UInt32 DigIndatapPort = DI_readerPort.ReadSingleSamplePortUInt32();
            return String.Format("0x{0:X}", DigIndatapPort);
        }


       public void Anolog_output(AO_Output output, double volt)
        {
            using (Task task = new Task())
            {
                string commannd_to_device = "";

                commannd_to_device = _deviceName + "/" + "ao" + (int)output;

                // Configure analog output channel
                task.AOChannels.CreateVoltageChannel(commannd_to_device, "", _Min_level_voltage, _Max_level_voltage, AOVoltageUnits.Volts);


                AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(task.Stream);
                writer.WriteSingleSample(true, volt);
            }
        }
        
        public string Anolog_input(IO_Pin input)
        {
            double sample;
			
			using (Task task2 = new Task())
            {
				

				string commannd_to_device = "";

                commannd_to_device = _deviceName + "/" + "ai" + (int)input;
                try
                {
                    task2.AIChannels.CreateVoltageChannel(commannd_to_device, "",
                        AITerminalConfiguration.Rse, _Min_level_voltage,
                        _Max_level_voltage, AIVoltageUnits.Volts);
                    AnalogMultiChannelReader reader = new AnalogMultiChannelReader(task2.Stream);
                    double[] data = reader.ReadSingleSample();
                    sample = data[0];
                }
                catch(Exception ex)
                {
					LoggerService.Error(this, "Failed to get analog input", ex);
					return "Error";
                }
            }
			
			return sample.ToString();
        }

        public string Anolog_input_current(IO_Pin input, double shuntResistor)
        {
            try
            {

                double sample;
                double tempMinValueNumeric = 0;
                double tempMaxValueNumeric = 0.020;
                double tempSamplesToReadNumeric = 1000;
                double currentSensorMeasGain = 2000;

                Thread.Sleep(1000);

                // Create a new task
                myTask = new Task();

                string commannd_to_device = "";

                LoggerService.Error(this, "Analog input current: Port" + input.ToString() + " shuntresistor: " + shuntResistor.ToString());

                commannd_to_device = _deviceName + "/" + "ai" + (int)input;

                // Create a virtual channel // can be internal too
                myTask.AIChannels.CreateCurrentChannel(commannd_to_device, "",
                    AITerminalConfiguration.Differential, Convert.ToDouble(tempMinValueNumeric),
                    Convert.ToDouble(tempMaxValueNumeric), Convert.ToDouble(shuntResistor / currentSensorMeasGain),
                    AICurrentUnits.Amps);

                myTask.Timing.ConfigureSampleClock("", Convert.ToDouble(tempSamplesToReadNumeric),
                    SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 1000);

                // Verify the Task
                myTask.Control(TaskAction.Verify);

                AnalogMultiChannelReader reader = new AnalogMultiChannelReader(myTask.Stream);
                double[] data = reader.ReadSingleSample();
                sample = data[0];
                LoggerService.Error(this, "Analog input current: Sample: " + sample.ToString());

                myTask.Dispose();

                return sample.ToString();

            }
            catch (DaqException exception)
            {
                // Display Errors
                MessageBox.Show(exception.Message);
                myTask.Dispose();
                return "Error";

            }
        }

        #endregion command 
    }
}
