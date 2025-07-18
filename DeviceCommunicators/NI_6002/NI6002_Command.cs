﻿using DeviceCommunicators.Interfaces;
using NationalInstruments.DAQmx;
using System;
using DeviceCommunicators.Enums;
using Task = NationalInstruments.DAQmx.Task;
using Services.Services;
using System.Windows;
using System.Threading;
using MicroLibrary;
using System.Diagnostics;
using NationalInstruments;
using System.Windows.Documents;
using System.Collections.Generic;

namespace DeviceCommunicators.NI_6002
{
    public class NI6002_Command: INiCommands
    {

        #region Fields

        public string _deviceName { get; set; }
        public string _Port_Io { get; set; } = "port0";
        public double _Min_level_voltage { get; set; } = -10;
        public double _Max_level_voltage { get; set; } = 10;
        public int numberOfCounts { get; set; } = 200;


        // Parameter to test
        public bool[] Digital_port_input { get; set; } = new bool[8];
        public double[] Analog_port_input { get; set; } = new double[8];
        public int[] Digital_port_output { get; set; } = new int[8];
        public double[] Analog_port_output { get; set; } = new double[8];

        private Task myTask;
        public bool IsTaskDisposed { get; private set; }

        //rpm counter
        static AutoResetEvent rpmCounterAutoResetEvent = new AutoResetEvent(false);
        private int initialCount = 0;
        private CounterSingleChannelReader myCounterReader;
        private CICountEdgesActiveEdge edgeType = CICountEdgesActiveEdge.Rising;
        double rpm;
        uint countReading;
        MicroTimer Timer_counterTryRead = new MicroTimer();
        MicroTimer Timer_revolutions = new MicroTimer();
        private Stopwatch stopwatch = new Stopwatch();
       // private static double revoultionsTimerElapsed = 0; // Track elapsed seconds
       // double counterTimerElapsed = 0;

        #endregion Fields



        #region Constructor
        public NI6002_Command(string device_name)
        {
            _deviceName = device_name;
            Timer_counterTryRead.MicroTimerElapsed += CounterTryRead;
            Timer_revolutions.MicroTimerElapsed += CalculateRevolutions;
        }

        #endregion Constructor

        #region command 
        public void DigitalIO_output(int port,int portLine ,int State)
        {
            string commannd_to_device = "";

            commannd_to_device = _deviceName + "/" + "port" + port + "/line" + portLine;
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
        public string DigitalIO_input(int port, int portLine)
        {
            string commannd_to_device = "";

            commannd_to_device = _deviceName + "/" + "port"+ port + "/line" + portLine;

            //LoggerService.Inforamtion(this, "command to device : " + commannd_to_device);


            Task digReadTaskPort = new Task();
            digReadTaskPort.DIChannels.CreateChannel(
                 commannd_to_device,
                 "",
                    ChannelLineGrouping.OneChannelForAllLines);
            DigitalSingleChannelReader DI_readerPort = new DigitalSingleChannelReader(digReadTaskPort.Stream);
            UInt32 DigIndatapPort = DI_readerPort.ReadSingleSamplePortUInt32();

            if(DigIndatapPort > 0)
            {
                DigIndatapPort = 1;
            }

            //LoggerService.Inforamtion(this, "result: "+ DigIndatapPort);

            return String.Format("0x{0:X}", DigIndatapPort);
        }


       public void Anolog_output(int port, double volt)
        {
            using (Task task = new Task())
            {
                string commannd_to_device = "";

                commannd_to_device = _deviceName + "/" + "ao" + port;

                // Configure analog output channel
                task.AOChannels.CreateVoltageChannel(commannd_to_device, "", _Min_level_voltage, _Max_level_voltage, AOVoltageUnits.Volts);


                AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(task.Stream);
                writer.WriteSingleSample(true, volt);
            }
        }
        
        public string Anolog_input(int port)
        {
            double sample;
			
			using (Task task2 = new Task())
            {



                string commannd_to_device = "";

                commannd_to_device = _deviceName + "/" + "ai" + port;
                try
                {
                    task2.AIChannels.CreateVoltageChannel(commannd_to_device, "",
                        AITerminalConfiguration.Rse, _Min_level_voltage,
                        _Max_level_voltage, AIVoltageUnits.Volts);
                    AnalogMultiChannelReader reader = new AnalogMultiChannelReader(task2.Stream);
                    double[] data = reader.ReadSingleSample();
                    sample = data[0];
                }
                catch (DaqException)
                {
                    // Display Errors
                    //MessageBox.Show("Failed to get analog input port: " + port.ToString() + "Due to:\r\n" + "Daq Exception:\r\n" + exception.Message);
                    return "Error";

                }
            }
			
			return sample.ToString();
        }

        public string Anolog_input_current(int port, double shuntResistor)
        {
            try
            {

                double sample;
                double tempMinValueNumeric = -300;
                double tempMaxValueNumeric = 300;
                double tempSamplesToReadNumeric = 1000;
                double currentSensorMeasGain = 2000;

                if (shuntResistor == 0)
                    shuntResistor = 17.8;
                string commannd_to_device = "";

                commannd_to_device = _deviceName + "/" + "ai" + port;

                // Create a new task
                using (var myTask = new Task())
                {
                    Thread.Sleep(200);

                    //_deviceName = "Dev2";

                    LoggerService.Inforamtion(this, "Analog input current: Port" + port.ToString() + " shuntresistor: " + shuntResistor.ToString());
                    LoggerService.Inforamtion(this, "Analog input command :" + commannd_to_device);


                    // Create a virtual channel // can be internal too
                    myTask.AIChannels.CreateCurrentChannel(commannd_to_device, "",
                        AITerminalConfiguration.Differential, Convert.ToDouble(tempMinValueNumeric),
                        Convert.ToDouble(tempMaxValueNumeric), Convert.ToDouble(shuntResistor / currentSensorMeasGain),
                        AICurrentUnits.Amps);

                    myTask.Timing.ConfigureSampleClock("", Convert.ToDouble(tempSamplesToReadNumeric),
                        SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, 1000);

                    // Verify the Task
                    myTask.Control(TaskAction.Verify);


                   // Continue execution after the delay
                   AnalogMultiChannelReader reader = new AnalogMultiChannelReader(myTask.Stream);
                   double[] data = reader.ReadSingleSample();
                   sample = data[0];
                   LoggerService.Inforamtion(this, "Analog input current: Sample: " + sample.ToString());



                    return sample.ToString();
                }
            }
            catch (DaqException exception)
            {
                // Display Errors
                MessageBox.Show("Failed to get analog input current: Daq Exception Due to:\r\n" + exception.Message);
                return "Error";

            }

        }
        int flag = 0;
        Stopwatch SW_TimeBetweenCounts = new Stopwatch();
        List<double> Rpm_500 = new ();
        double maxAllowedInrervalTolerance;
        double minAllowedInrervalTolerance;
        public string Digital_Counter(int numofcounts , int expectedrpm)
        {
            //timer
            try
            {
            
                numberOfCounts = numofcounts;
                double calculatedMotorPeriodInterval = (1.0 / (expectedrpm / 60.0)) * 1000.0;
                maxAllowedInrervalTolerance = calculatedMotorPeriodInterval + calculatedMotorPeriodInterval * 0.4;
                minAllowedInrervalTolerance = calculatedMotorPeriodInterval -  calculatedMotorPeriodInterval * 0.4;

                isReachedCounts = false;
                //counterTimerElapsed = 0;
                LoggerService.Error(this, "Digital_Counter");
                rpmCounterAutoResetEvent.Reset();
                myTask = new Task();
                countReading = 0;
                string commannd_to_device = _deviceName + "/" + "ctr0";

                myTask.CIChannels.CreateCountEdgesChannel(commannd_to_device, "Count Edges",
                    edgeType, Convert.ToInt64(initialCount), CICountEdgesCountDirection.Up);

                myCounterReader = new CounterSingleChannelReader(myTask.Stream);


                Timer_counterTryRead.Interval = 1000;



                myTask.Start();
                countReading = myCounterReader.ReadSingleSampleUInt32();
                previousRead = countReading;
                Timer_counterTryRead.Start();
                
                
                stopwatch.Start();

                rpmCounterAutoResetEvent.WaitOne();

                Timer_counterTryRead.Enabled = false;
                Timer_revolutions.Enabled = false;

                Timer_counterTryRead.Stop();
                Timer_revolutions.Stop();

                stopwatch.Reset();

                //This delay is to make sure that the Timer_counterTryRead stops before we dispose myTask object
                Thread.Sleep(100);
                if(IsTaskDisposed)
                {
                    IsTaskDisposed = false;
                    return "Error";
                }

                myTask.Dispose();

                flag++;
                if(flag == 3)
                {
                    flag = 0;
                }
                return rpm.ToString();
            }
            catch (DaqException exception)
            {
                if (!IsTaskDisposed)
                {
                    MessageBox.Show("Failed to get Digital Counter: Daq Exception Due to:\r\n" + exception.Message);
                    myTask.Dispose();
                }
                return "Error";// Return 0 or handle the exception as needed
            }
        }


        bool isReachedCounts = false;
        uint validCount = 0;
        uint previousRead = 0;
        bool isTimeBetweenCountStarted = false;
        double timeSpanBetweenCounts;


        private void CounterTryRead(object sender, MicroTimerEventArgs e)
        {
            try
            {
                if(!isTimeBetweenCountStarted)
                {
                    SW_TimeBetweenCounts.Start();

                    isTimeBetweenCountStarted = true;
                }

                if (IsTaskDisposed)
                {
                    DigitalCounter_ClearParams();
                    rpmCounterAutoResetEvent.Set();
                    return;
                }
                countReading = myCounterReader.ReadSingleSampleUInt32();

                if (countReading > previousRead)
                {
                    SW_TimeBetweenCounts.Stop();
                    timeSpanBetweenCounts = SW_TimeBetweenCounts.Elapsed.TotalMilliseconds;
                    Rpm_500.Add(timeSpanBetweenCounts);
                    isTimeBetweenCountStarted = false;
                    if (timeSpanBetweenCounts < maxAllowedInrervalTolerance && timeSpanBetweenCounts > minAllowedInrervalTolerance)
                    {
                        validCount++;
                    }
                    SW_TimeBetweenCounts.Reset();
                }

                previousRead = countReading;

                if (validCount >= numberOfCounts && !isReachedCounts)
                { 
                    double timeElapsed = stopwatch.Elapsed.TotalMilliseconds;
                    rpm = (numberOfCounts * 60) / (timeElapsed / 1000);
                    DigitalCounter_ClearParams();
                    rpmCounterAutoResetEvent.Set();
                }
            }
            catch (DaqException exception)
            {
                if (!IsTaskDisposed)
                {
                    MessageBox.Show("Failed to get Digital Counter: Daq Exception at CounterTryRead Due to:\r\n" + exception.Message);
                    myTask.Dispose();
                }
                rpmCounterAutoResetEvent.Set();
                DigitalCounter_ClearParams();
                return;
            }
            Thread.Sleep(1);
        }

        private void DigitalCounter_ClearParams()
        {
            Timer_counterTryRead.Stop();
            SW_TimeBetweenCounts.Stop();
            SW_TimeBetweenCounts.Reset();
            stopwatch.Reset();
            Timer_counterTryRead.Stop();
            Rpm_500.Clear();
            previousRead = 0;
            validCount = 0;
            isReachedCounts = true;
        }

        public void Dispose()
        {
            if (IsTaskDisposed || myTask == null) return;
            try 
            {
                myTask.Dispose();
                IsTaskDisposed = true;
            }
            catch(DaqException ex) { LoggerService.Debug(this, ex.Message); }

        }

        private void CalculateRevolutions(object sender, MicroTimerEventArgs e)
        {
            //ReadOnce();
            //uint _countReading = countReading;
            //double time = stopwatch.Elapsed.TotalMilliseconds;
            //revoultionsTimerElapsed = stopwatch.Elapsed.TotalMilliseconds / 1000;
            //rpm = (_countReading * 60) / revoultionsTimerElapsed;

            //LoggerService.Error(this, "RPM: " + rpm.ToString() + " TimerElapsed: " + revoultionsTimerElapsed.ToString());
            //revoultionsTimerElapsed = 0;
            //// Reset revolution count for the next interval
            //countReading = 0;
            //rpmCounterAutoResetEvent.Set();
        }

        #endregion command 
    }
}
