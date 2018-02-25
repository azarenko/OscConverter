using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OscConverter.Input
{
    public class mwf : InputInterface
    {
        public mwf(string file)
        {
            byte[] buffer = new byte[256];

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
            for (int i = 0; i < chennelsCount; i++)
            {
                _Channels.Add(new Channel());
            }

            fs.Read(buffer, 0, 6); // 0x00, 0x00, 0x22, 0x00, 0x03, 0x00

            // Read seamples rate
            fs.Read(buffer, 0, 4);
            int inputSampleRateHz = BitConverter.ToInt32(buffer, 0);
            double inputSampleRateMs = (1.0 / inputSampleRateHz) * 1000.0;

            fs.Read(buffer, 0, 12); // 0x00, 0x50, 0x00, 0x20, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00

            for (int i = 0; i < chennelsCount; i++)
            {
                fs.Read(buffer, 0, 4); // 0x20, 0x12, 0x60, 0xFF
                fs.Read(buffer, 0, 0x40); // name
                GetChannelInputLevelCode(_Channels[i], (byte)fs.ReadByte());
                fs.Read(buffer, 0, 5); // 0x12, 0x00, 0x00, 0x00, 0x00
                fs.Read(buffer, 0, 1); // number
                fs.Read(buffer, 0, 7); // 0x00, 0xFE, 0xFE, 0xFE, 0x00, 0x12, 0x00
            }

            fs.Read(buffer, 0, 4); // crc

            fs.Read(buffer, 0, 2); // 0x57, 0x44

            // Read data size
            fs.Read(buffer, 0, 8);
            long dataSize = BitConverter.ToInt64(buffer, 0);

            long sampleCount = (dataSize / 2) / chennelsCount;

            for (int j = 0; j < chennelsCount; j++)
            {
                _Channels[j].Data = new double[sampleCount];
                _Channels[j].TimeStep = inputSampleRateMs;
            }

            for (long i = 0; i < sampleCount; i++)
            {
                for (int j = 0; j < chennelsCount; j++)
                {
                    fs.Read(buffer, 0, 2);
                    short data = BitConverter.ToInt16(buffer, 0);
                    _Channels[j].Data[i] = data / GetChannelK(_Channels[j]);
                }
            }

            fs.Close();
        }

        private double GetChannelK(Channel ch)
        {
            if (ch.Max < 0.2 && ch.Min > -0.2)
            {
                return 8192.0 / 0.4;
            }

            if (ch.Max < 0.5 && ch.Min > -0.5)
            {
                return 8192.0 / 1.0;
            }

            if (ch.Max < 1.0 && ch.Min > -1.0)
            {
                return 8192.0 / 2.0;
            }

            if (ch.Max < 2.0 && ch.Min > -2.0)
            {
                return 8192.0 / 4.0;
            }

            if (ch.Max < 5.0 && ch.Min > -5.0)
            {
                return 8192.0 / 10.0;
            }

            if (ch.Max < 10.0 && ch.Min > -10.0)
            {
                return 8192.0 / 20.0;
            }

            if (ch.Max < 20.0 && ch.Min > -20.0)
            {
                return 8192.0 / 40.0;
            }

            if (ch.Max < 50.0 && ch.Min > -50.0)
            {
                return 8192.0 / 100.0;
            }

            if (ch.Max < 100.0 && ch.Min > -100.0)
            {
                return 8192.0 / 200.0;
            }

            if (ch.Max < 200.0 && ch.Min > -200.0)
            {
                return 8192.0 / 400.0;
            }

            if (ch.Max < 500.0 && ch.Min > -500.0)
            {
                return 8192.0 / 1000.0;
            }

            if (ch.Max < 1000.0 && ch.Min > -1000.0)
            {
                return 8192.0 / 2000.0;
            }

            return 8192.0 / 2000.0;
        }

        private void GetChannelInputLevelCode(Channel ch, byte code)
        {
            if (code == 0x02)
            {
                ch.Max = 0.2;
                ch.Min = -0.2;
                return;
            }

            if (code == 0x05)
            {
                ch.Max = 0.5;
                ch.Min = -0.5;
                return;
            }

            if (code == 0x11)
            {
                ch.Max = 1.0;
                ch.Min = -1.0;
                return;
            }

            if (code == 0x12)
            {
                ch.Max = 2.0;
                ch.Min = -2.0;
                return;
            }

            if (code == 0x15)
            {
                ch.Max = 5.0;
                ch.Min = -5.0;
                return;
            }

            if (code == 0x21)
            {
                ch.Max = 10.0;
                ch.Min = -10.0;
                return;
            }

            if (code == 0x22)
            {
                ch.Max = 20.0;
                ch.Min = -20.0;
                return;
            }

            if (code == 0x25)
            {
                ch.Max = 50.0;
                ch.Min = -50.0;
                return;
            }

            if (code == 0x31)
            {
                ch.Max = 100.0;
                ch.Min = -100.0;
                return;
            }

            if (code == 0x32)
            {
                ch.Max = 200.0;
                ch.Min = -200.0;
                return;
            }

            if (code == 0x35)
            {
                ch.Max = 500.0;
                ch.Min = -500.0;
                return;
            }

            if (code == 0x41)
            {
                ch.Max = 1000.0;
                ch.Min = -1000.0;
                return;
            }

            ch.Max = 1000.0;
            ch.Min = -1000.0;
        }

        List<Channel> _Channels = new List<Channel>();
        public List<Channel> GetChannels()
        {
            return this._Channels;
        }
    }
}
