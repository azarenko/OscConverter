using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OscConverter.Output
{
    class mwf : outInterface
    {
        string filename = string.Empty;

        public mwf(string filename)
        {
            this.filename = filename;
        }        

        public void SetChannels(List<Channel> chennels)
        {
            FileStream fs = null;
            fs = new FileStream(filename, FileMode.Create);

            // Тип файла
            fs.WriteByte(0x4d);
            fs.WriteByte(0x77);

            // Размер заголовков каналов
            short headSize = (short)((chennels.Count * 0x52) + 0x18);
            fs.Write(BitConverter.GetBytes(headSize), 0, 2);            

            fs.Write(new byte[] { 0x00, 0x00, 0x22, 0x00, 0x03, 0x00 }, 0, 6);
                            
            // Частота дискретезации
            int inputSampleRate = (int)(1.0 / (chennels[0].TimeStep / 1000.0));
            byte[] inputSampleRateBuffer = BitConverter.GetBytes(inputSampleRate);
            fs.Write(inputSampleRateBuffer, 0, inputSampleRateBuffer.Length);

            fs.Write(new byte[] { 0x00, 0x50, 0x00, 0x20, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 12);

            for(int i = 0; i < chennels.Count; i++)
            {
                fs.Write(new byte[] { 0x20, 0x12, 0x60, 0xFF }, 0, 4);
                byte[] name = new byte[0x40];
                name[0] = (byte)'C';
                name[1] = (byte)'h';
                name[2] = (byte)':';
                name[3] = (byte)((int)'0' + i);
                fs.Write(name, 0, 0x40);
                fs.WriteByte(GetChannelInputLevelCode(chennels[i]));  
                fs.Write(new byte[] { 0x12, 0x00,0x00,0x00,0x00 }, 0, 5);
                fs.WriteByte((byte)i);
                fs.Write(new byte[] { 0x00,0xFE,0xFE,0xFE,0x00,0x12,0x00 }, 0, 7);
            }

            // Расчет контрольной суммы
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

            // Кол-во выборок данных   
            long dataSize = ((chennels[0].Data.LongLength * chennels.Count) * 2);
            byte[] countSamplesBuffer = BitConverter.GetBytes(dataSize);
            fs.Write(countSamplesBuffer, 0, countSamplesBuffer.Length);

            // Запись блоков данных
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

            fs.Close();
        }

        private double GetChannelK(Channel ch)
        {
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

        private byte GetChannelInputLevelCode(Channel ch)
        {
            if (ch.Max < 5.0 && ch.Min > -5.0)
            {
                return 0x15;
            }

            if (ch.Max < 10.0 && ch.Min > -10.0)
            {
                return 0x21;
            }

            if (ch.Max < 20.0 && ch.Min > -20.0)
            {
                return 0x22;
            }

            if (ch.Max < 50.0 && ch.Min > -50.0)
            {
                return 0x25;
            }

            if (ch.Max < 100.0 && ch.Min > -100.0)
            {
                return 0x31;
            }

            if (ch.Max < 200.0 && ch.Min > -200.0)
            {
                return 0x32;
            }

            if (ch.Max < 500.0 && ch.Min > -500.0)
            {
                return 0x35;
            }

            if (ch.Max < 1000.0 && ch.Min > -1000.0)
            {
                return 0x41;
            }

            return 0x41;
        }
    }    

    class Crc32
    {
        uint[] table;

        public uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        public Crc32()
        {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
        }
    }
}
