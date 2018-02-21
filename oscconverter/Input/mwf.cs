using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OscConverter.Input
{
    public class mwf : InputInterface
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

        public mwf(string file)
        {
            byte[] buffer = new byte[32];

            FileStream fs = null;
            fs = new FileStream(file, FileMode.Open);

            int head1 = fs.ReadByte(); // 0x4d
            int head2 = fs.ReadByte(); // 0x77

            if (head1 != 0x4d || head2 != 0x77)
                throw new Exception("Unsupported file format");

            // Get channels count
            fs.Read(buffer, 0, 2);
            short headSize = BitConverter.ToInt16(buffer, 0);
            int chennelsCount = (headSize - 0x18) / 0x52;

            _Channels.Clear();
            for (int i=0; i< chennelsCount; i++)
            {
                _Channels.Add(new Channel());
            }

            fs.Read(buffer, 0, 6); // 0x00, 0x00, 0x22, 0x00, 0x03, 0x00

            // Read seamples rate
            fs.Read(buffer, 0, 4);
            int inputSampleRateHz = BitConverter.ToInt32(buffer, 0);
            double inputSampleRateMs = ((1.0 / inputSampleRateHz) / 1000.0);


            /*

            int inputSampleRate = (int)(1.0 / (chennels[0].TimeStep / 1000.0));
            byte[] inputSampleRateBuffer = BitConverter.GetBytes(inputSampleRate);
            fs.Write(inputSampleRateBuffer, 0, inputSampleRateBuffer.Length);

            fs.Write(new byte[] { 0x00, 0x50, 0x00, 0x20, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 12);

            for (int i = 0; i < chennels.Count; i++)
            {
                fs.Write(new byte[] { 0x20, 0x12, 0x60, 0xFF }, 0, 4);
                byte[] name = new byte[0x40];
                name[0] = (byte)'C';
                name[1] = (byte)'h';
                name[2] = (byte)':';
                name[3] = (byte)((int)'0' + i);
                fs.Write(name, 0, 0x40);
                fs.WriteByte(GetChannelInputLevelCode(chennels[i]));
                fs.Write(new byte[] { 0x12, 0x00, 0x00, 0x00, 0x00 }, 0, 5);
                fs.WriteByte((byte)i);
                fs.Write(new byte[] { 0x00, 0xFE, 0xFE, 0xFE, 0x00, 0x12, 0x00 }, 0, 7);
            }

            int curr_pos = (int)fs.Position;
            fs.Seek(0, SeekOrigin.Begin);
            byte[] crc_buffer = new byte[curr_pos];
            fs.Read(crc_buffer, 0, curr_pos);
            Crc32 crc = new Crc32();
            byte[] crc_value = crc.ComputeChecksumBytes(crc_buffer);
            fs.Seek(curr_pos, SeekOrigin.Begin);
            fs.Write(crc_value, 0, 4);

            fs.WriteByte(0x57);
            fs.WriteByte(0x44);

            long dataSize = ((chennels[0].Data.LongLength * chennels.Count) * 2);
            byte[] countSamplesBuffer = BitConverter.GetBytes(dataSize);
            fs.Write(countSamplesBuffer, 0, countSamplesBuffer.Length);

            long sampleCount = chennels[0].Data.LongLength;
            for (long i = 0; i < sampleCount; i++)
            {
                for (int j = 0; j < chennels.Count; j++)
                {
                    short value = (short)(GetChannelK(chennels[j]) * chennels[j].Data[i]);
                    byte[] buffer = BitConverter.GetBytes(value);
                    fs.Write(buffer, 0, 2);
                }
            }
            */
            fs.Close();

            /*
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

            fs.Close()*/
        }

        List<Channel> _Channels = new List<Channel>();
        public List<Channel> GetChannels()
        {
            return this._Channels;
        }
    }
}
