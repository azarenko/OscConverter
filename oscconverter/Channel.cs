using System;
using System.Collections.Generic;
using System.Text;

namespace OscConverter
{
    public class Channel
    {
        public Channel()
        {
            this.Max = double.MinValue;
            this.Min = double.MaxValue;
        }

        public double[] Data;

        public double TimeStep;

        public double Max;

        public double Min;

        public new string ToString()
        {
            string info = "Record length: " + (Data.Length * (TimeStep / 1000.0)) + " sec\r\n";
            info += "Seample rates: " +  (1.0 / TimeStep) + " kHz\r\n";
            info += "Max. amplitude: " + Max.ToString("N3") +" V\r\n";
            info += "Min. amplitude: " + Min.ToString("N3") + " V\r\n";
            return info;
        }

        public double outTimeStep;
    }
}
