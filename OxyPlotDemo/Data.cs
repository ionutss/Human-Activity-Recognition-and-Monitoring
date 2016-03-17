using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OxyPlotDemo
{
    public class Data
    {
        public static List<Measurement> GetData()
        {

            JObject json = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(@"c:\USC_HAD\Subject4\a5t1.json"));
            JObject o = (JObject)(json["readings"]);
            JArray array = (JArray)(o["acc_x"]);
            double[] input = array.ToObject<double[]>(); 
            
            var measurements = new List<Measurement>();

            int Fs = 100;
            int T = 1 / Fs;
            double[] input2 = new double[2500];           

            //20 masurari pe secunda
            //input2 = SetSamplingRate(input, 100);

            //valorile negative devin 0
            //for (int i = 0; i < input2.Length; i++)
            //{
            //    if (input2[i] < 0)
            //    {
            //        input2[i] = 0;
            //    }
            //}

            double[] output = input;

            MedianFilter(output, output);
            MovingAverageFilter(output, output);
            MovingAverageFilter(output, output);
            MovingAverageFilter(output, output);
            MovingAverageFilter(output, output);
            //MovingAverageFilter(output, output);

            


            for (int j = 0; j < 500; j++)
            {
                measurements.Add(new Measurement() { DetectorId = 1, x = j, Value = output[j] });
            }

            return measurements;
        }

        public static List<Measurement> GetUpdateData(DateTime dateTime)
        {
            var measurements = new List<Measurement>();
            var r = new Random();

            for (int i = 0; i < 5; i++)
            {
                measurements.Add(new Measurement() { DetectorId = i, x = i, Value = r.Next(1, 30) });
            }
            return measurements;
        }

        public static double[] SetSamplingRate(double[] input, int Fs){
            double[] temp = new double[2500/(100/ Fs)];
            int k = 0;
            int it = 100 / Fs;
            for (int i = 0; i < input.Length; i = i + it)
            {
                temp[k] = input[i];
                k++;
            }

            return temp;
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
            //output[0] = input[0];
            //output[1] = (input[0] + input[1] + input[2]) / 3;
            //output[2] = (input[0] + input[1] + input[2] + input[3] + input[4]) / 5;
            //output[3] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6]) / 7;
            //output[4] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8]) / 9;
            //output[5] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8] + input[9] + input[10]) / 11;
            //output[6] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8] + input[9] + input[10] + input[11] + input[12]) / 13;
            //output[7] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8] + input[9] + input[10] + input[11] + input[12] + input[13] + input[14]) / 15;
            //for (int i = 8; i < input.Length - 8; i++)
            //{
            //    output[i] = (input[i-8]+input[i-7]+input[i-6]+input[i - 5] + input[i - 4] + input[i - 3] + input[i - 2] + input[i - 1] + input[i] + input[i + 1] + input[i + 2] + input[i + 3] + input[i + 4] + input[i + 5] +input[i+6]+input[i+7]+input[i+8]) / 17;
            //}

            //output[0] = input[0];
            //output[1] = (input[0] + input[1] + input[2]) / 3;
            //output[2] = (input[0] + input[1] + input[2] + input[3] + input[4]) / 5;
            //output[3] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6]) / 7;
            //output[4] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8]) / 9;
            //output[5] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8] + input[9] + input[10]) / 11;
            //output[6] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8] + input[9] + input[10] + input[11] + input[12]) / 13;
            //for (int i = 7; i < input.Length - 7; i++)
            //{
            //    output[i] = (input[i - 7] + input[i - 6] + input[i - 5] + input[i - 4] + input[i - 3] + input[i - 2] + input[i - 1] + input[i] + input[i + 1] + input[i + 2] + input[i + 3] + input[i + 4] + input[i + 5] + input[i + 6] + input[i + 7] ) / 15;
            //}

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

            //output[0] = input[0];
            //output[1] = (input[0] + input[1] + input[2]) / 3;
            //output[2] = (input[0] + input[1] + input[2] + input[3] + input[4]) / 5;
            //output[3] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6]) / 7;
            //output[4] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6] + input[7] + input[8]) / 9;
            //for (int i = 5; i < input.Length - 5; i++)
            //{
            //    output[i] = (input[i - 5] + input[i - 4] + input[i - 3] + input[i - 2] + input[i - 1] + input[i] + input[i + 1] + input[i + 2] + input[i + 3] + input[i + 4] + input[i + 5]) / 11;
            //}


            //output[0] = input[0];
            //output[1] = (input[0] + input[1] + input[2]) / 3;
            //output[2] = (input[0] + input[1] + input[2] + input[3] + input[4]) / 5;
            //output[3] = (input[0] + input[1] + input[2] + input[3] + input[4] + input[5] + input[6]) / 7;
            //for (int i = 4; i < input.Length - 4; i++)
            //{
            //    output[i] = (input[i - 4] + input[i - 3] + input[i - 2] + input[i - 1] + input[i] + input[i + 1] + input[i + 2] + input[i + 3] + input[i + 4]) / 9;
            //}


            //output[0] = input[0];
            //output[1] = (input[0] + input[1] + input[2]) / 3;
            //output[2] = (input[0] + input[1] + input[2] + input[3] + input[4]) / 5;
            //for (int i = 3; i < input.Length - 3; i++)
            //{
            //    output[i] = (input[i - 3] + input[i - 2] + input[i - 1] + input[i] + input[i + 1] + input[i + 2] + input[i + 3]) / 7;
            //}

            //output[0] = input[0];
            //output[1] = (input[0] + input[1] + input[2]) / 3;
            //for (int i = 2; i < input.Length - 2; i++)
            //{
            //    output[i] = (input[i - 2] + input[i - 1] + input[i] + input[i + 1] + input[i + 2]) / 5;
            //}

        }


    }

    public class Measurement
    {
        public int DetectorId { get; set; }
        public double Value { get; set; }
        public int x { get; set; }
    }
}
