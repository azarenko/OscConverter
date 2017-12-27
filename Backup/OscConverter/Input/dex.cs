using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OscConverter.Input
{
    public class dex : InputInterface
    {
        double[] TimeDivision = new double[] {                
                1.0,
                2.0,
                5.0,
                10.0,
                20.0,
                50.0,
                100.0,
                200.0};

        double[] DividerValue = new double[]{
                1.0, 
                2.0, 
                10.0, 
                50.0
        };

        public dex(string file)
        {
            FileStream fs = null;
            fs = new FileStream(file, FileMode.Open);
           
            // Count samples
            int _Samples = fs.ReadByte();
            _Samples |= (fs.ReadByte() << 8);
            _Samples |= (fs.ReadByte() << 16);
            _Samples |= (fs.ReadByte() << 32);

            Channel channel1 = new Channel();

            int ch1InputLevel = (int)fs.ReadByte();

            fs.Seek(0x03, SeekOrigin.Current);

            int ch1Divider = (int)fs.ReadByte();  
          
            fs.Seek(0x03, SeekOrigin.Current);

            bool ch1Enable = fs.ReadByte() == 0x01;

            fs.Seek(sizeof(int) //Width
                + sizeof(int) //Offest
                + sizeof(double) //Gain
                + sizeof(double) //Divide
                + sizeof(double) //Scale
                , SeekOrigin.Current);

            Channel channel2 = new Channel();
            
            int ch2InputLevel = (int)fs.ReadByte();
            fs.Seek(0x03, SeekOrigin.Current);

            int ch2Divider = (int)fs.ReadByte();
            fs.Seek(0x03, SeekOrigin.Current);

            bool ch2Enable = fs.ReadByte() == 0x01;

            fs.Seek(sizeof(int) //Width
                + sizeof(int) //Offest
                + sizeof(double) //Gain
                + sizeof(double) //Divide
                + sizeof(double) //Scale
                , SeekOrigin.Current);

            int TimeIndex = (short)fs.ReadByte();
            TimeIndex |= (short)(fs.ReadByte() << 8);
            fs.Seek(0x02, SeekOrigin.Current);

            channel1.TimeStep = channel2.TimeStep = (TimeDivision[TimeIndex] * 10.0) / 1000.0; 

            if (ch1Enable)
            {
                channel1.Data = new double[_Samples];
                for (int i = 0; i < _Samples; i++)
                {
                    byte[] buff = new byte[sizeof(double)];
                    fs.Read(buff, 0, sizeof(double));
                    double value = BitConverter.ToDouble(buff, 0) * DividerValue[ch1Divider];

                    if (channel1.Max < value)
                    {
                        channel1.Max = value;
                    }

                    if (channel1.Min > value)
                    {
                        channel1.Min = value;
                    }

                    channel1.Data[i] = value;
                }               
                _Channels.Add(channel1);
            }
            if (ch2Enable)
            {
                channel2.Data = new double[_Samples];                    
                for (int i = 0; i < _Samples; i++)
                {                    
                    byte[] buff = new byte[sizeof(double)];
                    fs.Read(buff, 0, sizeof(double));
                    double value = BitConverter.ToDouble(buff, 0) * DividerValue[ch2Divider];

                    if (channel2.Max < value)
                    {
                        channel2.Max = value;
                    }

                    if (channel2.Min > value)
                    {
                        channel2.Min = value;
                    }

                    channel2.Data[i] = value;
                }                
                _Channels.Add(channel2);
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
