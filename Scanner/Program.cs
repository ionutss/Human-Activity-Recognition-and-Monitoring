﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Scanner
{
    struct Data
    {
        public double[] X;
        public double[] Y;
        public double[] Z;
    }

    class Program
    {
        static void Main(string[] args)
        {
            JObject json = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(@"c:\USC_HAD\Subject1\a9t1.json"));
            JObject o = (JObject)(json["readings"]);
            JArray arrayX = (JArray)(o["acc_x"]);
            JArray arrayY = (JArray)(o["acc_y"]);
            JArray arrayZ = (JArray)(o["acc_z"]);

            Data input;
            input.X = arrayX.ToObject<double[]>();
            input.Y = arrayY.ToObject<double[]>();
            input.Z = arrayZ.ToObject<double[]>();

            //var measurements = new List<Measurement>();

            int Fs = 100;
            int T = 1 / Fs;

            Console.Out.WriteLine(input.X.Length);
            //double[] output = input.X;

            FilterSignal(input.X, input.X);
            FilterSignal(input.Y, input.Y);
            FilterSignal(input.Z, input.Z);

            double[] buffer = InitializeBuffer(input.X, 100);
            double bufferThreshold = GetBufferThreshold(buffer);

            Console.Out.WriteLine(bufferThreshold);
            Console.Out.WriteLine("==========================]");


            //ProcessBuffer(buffer, "X");

            //for (var i = 100; i < 1000; i++)
            //{
            //    ProcessBuffer(buffer, "X");
            //    UpdateBuffer(buffer, input.X[i + 1]);
            //}

            CircularBuffer CircularBuffer = new CircularBuffer(1000); //buffer of size: 10s
            int ProccessWindowSize = 100;
            Data ProcessWindow = new Data { X = new double[ProccessWindowSize], Y = new double[ProccessWindowSize], Z = new double[ProccessWindowSize] };
            //ProcessWindow.X = new double[100];

            for (var i = 0; i < input.X.Length; i++)
            {
                if (!CircularBuffer.IsWriteOffsetBlocked())
                {
                    CircularBuffer.Write(input.X[i], input.Y[i], input.Z[i]);
                    CircularBuffer.IncrementWriteOffset();
                }
                if (!CircularBuffer.IsReadOffsetBlocked())
                {
                    if (CircularBuffer.IsReadyToProcess(ProccessWindowSize))
                    {
                        var tempBuf = CircularBuffer.Read(ProccessWindowSize);

                        for (var j =0; j< ProccessWindowSize; j++)
                        {
                            ProcessWindow.X[j] = tempBuf[j].X;
                           // ProcessWindow.Y[j] = tempBuf[j].Y;
                        }
                        ProcessBuffer(ProcessWindow.X, "X");
                        //ProcessBuffer(ProcessWindow.Y, "Y");
                        //Thread.Sleep(500);
                        CircularBuffer.IncrementReadOffset();
                    }

                }

            }

        }

        public static double[] InitializeBuffer(double[] array, int size)
        {
            double[] buf = new double[size];

            for(var i=0; i< size; i++)
            {
                buf[i] = array[i];
            }

            return buf;
        }

        public static void UpdateBuffer(double[] buffer, double nextValue)
        {
            for(var i=0; i<buffer.Length-1; i++)
            {
                buffer[i] = buffer[i + 1];
            }

            buffer[buffer.Length - 1] = nextValue;
        }

        public static double GetBufferThreshold(double[] buffer)
        {
            return (buffer.Max() + buffer.Min()) / 2;
        }

        public static bool ToLowTransition(double current, double next, double threshold)
        {
            if(current > threshold && next < threshold)
            {
                return true;
            }
            return false;
        }

        public static bool ToHighTransition(double current, double next, double threshold)
        {
            if (current < threshold && next > threshold)
            {
                return true;
            }
            return false;
        }

        public static void ProcessBuffer(double[] buffer, string direction)
        {
            var threshold = GetBufferThreshold(buffer);
            var tempArrayList = new List<double>();
            List<Step> BufferSteps = new List<Step>();
            

            for(var i=0; i<buffer.Length-1; i++)
            {
                if(ToLowTransition(buffer[i], buffer[i + 1], threshold) && tempArrayList.Count > 1)
                {
                    tempArrayList.Add(buffer[i]);
                    var tempBuffer = tempArrayList.ToArray();
                    
                    var tempThreshold = GetBufferThreshold(tempBuffer);
                    if(tempBuffer.Max() - threshold > 0.03)
                    {
                        BufferSteps.Add(new Step(1, tempBuffer));
                        Console.Out.WriteLine("HIGH: " + buffer[i] + " - " + buffer[i+1]);
                    }

                    tempArrayList.Clear();
                    tempArrayList.TrimExcess();
                    
                }
                else if(ToHighTransition(buffer[i], buffer[i + 1], threshold) && tempArrayList.Count > 1)
                {
                    tempArrayList.Add(buffer[i]);
                    var tempBuffer = tempArrayList.ToArray();
                    var tempThreshold = GetBufferThreshold(tempBuffer);
                    if (threshold - tempBuffer.Min() > 0.03)
                    {
                        BufferSteps.Add(new Step(0, tempBuffer));
                        Console.Out.WriteLine("LOW");
                    }

                    tempArrayList.Clear();
                    tempArrayList.TrimExcess();

                }
                else
                {
                    tempArrayList.Add(buffer[i]);
                }
            }

            var upStepCounter = 0;
            foreach(var step in BufferSteps)
            {
                if(step.Type == 1)
                {
                    upStepCounter++;
                }
            }

            

            if (IsStatic(BufferSteps))
            {
                Console.Out.WriteLine("STATIC");
                if(direction == "X")
                {
                    Console.Out.WriteLine("STANDING");
                }
                else if(direction == "Y")
                {
                    Console.Out.WriteLine("LAYING/SLEEPING");
                }
            }
            else
            {
                Console.Out.WriteLine("DYNAMIC");
                if (IsPeriodic(BufferSteps))
                {
                    Console.Out.WriteLine("PERIODIC");
                    switch (upStepCounter)
                    {
                        case 2:
                            Console.Out.WriteLine("WALKING");
                            break;
                        case 3:
                            Console.Out.WriteLine("RUNNING");
                            break;
                        default:
                            Console.Out.WriteLine("STAIRS");
                            break;
                    }

              
                }
                else
                {
                    Console.Out.WriteLine("PERIODIC");
                }

            }

            



        }

        public static bool IsStatic(List<Step> list)
        {
            if (list.Count > 0)
            {
                return false;
            }
            return true;
        }

        public static bool IsPeriodic(List<Step> list)
        {
            if (list.Count > 2)
            {
                var lastWidth = list[0].ValuesArray.Length + list[1].ValuesArray.Length;

                for (var i = 0; i < list.Count-2; i=i+2)
                {
                    var currentWidth = list[i].ValuesArray.Length + list[i + 1].ValuesArray.Length;
                    //30 is a minimum difference between step signals length
                    if (Math.Abs(currentWidth - lastWidth) > 30)
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
           
        }

        public static void FilterSignal(double[] input, double[] output)
        {
            MedianFilter(input, output);
            MovingAverageFilter(input, output);
            MovingAverageFilter(input, output);
            MovingAverageFilter(input, output);
            MovingAverageFilter(input, output);
        }


        public static void MedianFilter(double[] input, double[] output)
        {
            double[] temp = new double[5];

            output[0] = input[0];

            for (int i = 1; i < input.Length - 3; i++)
            {
                temp[0] = input[i - 1];
                temp[1] = input[i];
                temp[2] = input[i + 1];
                temp[3] = input[i + 2];
                temp[4] = input[i + 3];
                Array.Sort(temp);
                output[i] = temp[1];
            }
        }

        public static void MovingAverageFilter(double[] input, double[] output)
        {
            output[0] = input[0];
            output[1] = (input[0] + input[1] + input[2]) / 3;
            output[2] = (input[0] + input[1] + input[2] + input[3] + input[4]) / 5;
            output[3] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6]) / 7;
            output[4] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8]) / 9;
            output[5] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8] + input[9] + input[10]) / 11;
            for (int i = 6; i < input.Length - 6; i++)
            {
                output[i] = (input[i - 6] + input[i - 5] + input[i - 4] + input[i - 3] + input[i - 2] + input[i - 1] + input[i] + input[i + 1] + input[i + 2] + input[i + 3] + input[i + 4] + input[i + 5] + input[i + 6]) / 13;
            }

        }
    }
}
