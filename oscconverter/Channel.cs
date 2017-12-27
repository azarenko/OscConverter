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
            string info = "Время записи: " + (Data.Length * (TimeStep / 1000.0)) + " с\r\n";
            info += "Частота дискретезации: " +  (1.0 / TimeStep) + " кГц\r\n";
            info += "Макс. значение: " + Max.ToString("N3") +" В\r\n";
            info += "Мин. значение: " + Min.ToString("N3") + " В\r\n";
            return info;
        }

        public double outTimeStep;
    }
}
