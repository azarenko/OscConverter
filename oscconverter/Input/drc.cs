using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OscConverter.Input
{
    public class drc : InputInterface
    {
        double[] TimeDivision = new double[]{
                0.000001,
                0.000002,
                0.000005,
                0.00001,
                0.00002,
                0.00005,
                0.0001,
                0.0002,
                0.0005,
                // mks
                0.001,
                0.002,
                0.005,
                0.01,
                0.02,
                0.05,
                0.1,
                0.2,
                0.5,
                // ms
                1.0,
                2.0,
                5.0,
                10.0,
                20.0,
                50.0,
                100.0,
                200.0,
                500.0,
                // s
                1000.0,
                2000.0,
                5000.0,
                10000.0,
                20000.0,
                50000.0,
                100000.0,
                200000.0,
                500000.0,
                1000000.0,
                2000000.0,
                5000000.0,
                10000000.0,
                20000000.0};

        double[] DividerValue = new double[]{
                1.0,
                10.0,
                100.0,
                1000.0,
                10000.0,
                0.5,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0
            };

        double[] ValueDivision = new double[] {
                0.01,
                0.02,
                0.05,
                0.1,
                0.2,
                0.5,
                1.0,
                2.0,
                5.0
         };

        class ChannelSettings        
        {
            public int InputLevel;
            public int Divider;
            public List<double> Data = new List<double>();
        }

        public drc(string file)
        {
            short SampleCount;

            FileStream fs = null;
            fs = new FileStream(file, FileMode.Open);

            fs.Seek(0x13, SeekOrigin.Begin);

            List<ChannelSettings> settings = new List<ChannelSettings>();

            // Get Channel info
            for (byte i = 0; i < 8; i++)
            {                                
                fs.Seek(0x01, SeekOrigin.Current);
                bool Enable = fs.ReadByte() == 0x01;
                if (!Enable)
                {
                    fs.Seek(0x02, SeekOrigin.Current);
                }
                else
                {
                    ChannelSettings channelSettings = new ChannelSettings();

                    fs.Seek(0x03, SeekOrigin.Current);
                    channelSettings.InputLevel = (byte)fs.ReadByte();
                    fs.Seek(0x03, SeekOrigin.Current);
                    channelSettings.Divider = (byte)fs.ReadByte();
                    fs.Seek(0x09, SeekOrigin.Current);
                    fs.ReadByte(); //Offset
                    fs.ReadByte(); //Offset
                    fs.Seek(0x02, SeekOrigin.Current);
                    fs.ReadByte(); // R
                    fs.ReadByte(); // G
                    fs.ReadByte(); // B
                    fs.Seek(0x04, SeekOrigin.Current);

                    settings.Add(channelSettings);
                }
            }

            // get time division index
            fs.Seek(0x17, SeekOrigin.Current);
            int TimeIndex = (short)fs.ReadByte();

            // goto data
            fs.Seek(0xB81, SeekOrigin.Current);

            double TimeStep = 0.0;

            while (fs.Position < fs.Length)
            {
                for (int i = 0; i < settings.Count; i++)
                {
                    SampleCount = (short)fs.ReadByte();
                    SampleCount |= (short)(fs.ReadByte() << 8);

                    TimeStep = (TimeDivision[TimeIndex] * 10) / SampleCount;

                    fs.Seek(0x06, SeekOrigin.Current);                    

                    double k = (ValueDivision[settings[i].InputLevel] * 8.0) / 4096.0;

                    for (short j = 0; j < SampleCount; j++)
                    {
                        short tmp = (short)fs.ReadByte();
                        tmp |= (short)(fs.ReadByte() << 8);

                        double value = tmp * k * DividerValue[settings[i].Divider];
                        settings[i].Data.Add(value);
                    }                    
                }
            }

            for (int i = 0; i < settings.Count; i++)
            {
                Channel channel = new Channel();
                channel.Data = settings[i].Data.ToArray();

                foreach (double value in channel.Data)
                {
                    if (channel.Max < value)
                        channel.Max = value;

                    if (channel.Min > value)
                        channel.Min = value;
                }

                channel.TimeStep = TimeStep;

                _Channels.Add(channel);
            }

            fs.Close();
        }

        List<Channel> _Channels = new List<Channel>();
        public List<Channel> GetChannels() 
        { 
            return this._Channels; 
        }
    }
}
