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
            string info = "����� ������: " + (Data.Length * (TimeStep / 1000.0)) + " �\r\n";
            info += "������� �������������: " +  (1.0 / TimeStep) + " ���\r\n";
            info += "����. ��������: " + Max.ToString("N3") +" �\r\n";
            info += "���. ��������: " + Min.ToString("N3") + " �\r\n";
            return info;
        }

        public double outTimeStep;
    }
}
