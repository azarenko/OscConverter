using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OscConverter.Output
{
    class wav : outInterface
    {
        string filename = string.Empty;

        public wav(string filename)
        {
            this.filename = filename;
        }

        public void SetChannels(List<Channel> chennels)
        {
            double maxA = double.MinValue;
            double maxB = double.MinValue;
            double minA = double.MaxValue;
            double minB = double.MaxValue;

            for(int i = 0; i < chennels[0].Data.LongLength; i++)
            {
                if (maxA < chennels[0].Data[i])
                    maxA = chennels[0].Data[i];

                if (maxB < chennels[1].Data[i])
                    maxB = chennels[1].Data[i];

                if (minA > chennels[0].Data[i])
                    minA = chennels[0].Data[i];

                if (minB > chennels[1].Data[i])
                    minB = chennels[1].Data[i];
            }

            int sizeOfTheDataSection = chennels[0].Data.Length * 2 * 2;

            FileStream fs = null;
            fs = new FileStream(filename, FileMode.Create);

            byte[] buffer = new byte[256];

            // RIFF
            fs.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);

            // File size - 8
            int fileSize = sizeOfTheDataSection + 44 - 8;
            fs.Write(BitConverter.GetBytes(fileSize), 0, 4);

            // WAVE
            fs.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);

            // fmt_
            fs.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);

            // PCM format
            fs.Write(BitConverter.GetBytes((int)16), 0, 4);

            // Compress format
            fs.Write(BitConverter.GetBytes((short)1), 0, 2);

            // 2 channels
            fs.Write(BitConverter.GetBytes((short)2), 0, 2);

            //Sample Rate
            int sampleRate = (int)(1.0 / (chennels[0].TimeStep / 1000.0));
            fs.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            //(Sample Rate * BitsPerSample * Channels) / 8
            int dataItemLen = (sampleRate * 16 * 2) / 8;
            fs.Write(BitConverter.GetBytes(dataItemLen), 0, 4);

            //(BitsPerSample * Channels) / 8
            short sampleLen = (16 * 2) / 8;
            fs.Write(BitConverter.GetBytes(sampleLen), 0, 2);

            //Bits per sample
            fs.Write(BitConverter.GetBytes((short)16), 0, 2);

            // data
            fs.Write(Encoding.ASCII.GetBytes("data"), 0, 4);

            // Size of the data section.
            fs.Write(BitConverter.GetBytes(sizeOfTheDataSection), 0, 4);

            double kA = (maxA - minA);
            double kB = (maxB - minB);

            for (int i = 0; i < chennels[0].Data.Length; i++)
            {
                double a = (chennels[0].Data[i] - minA) / kA;
                short _a = (short)((65535 * a) + short.MinValue);
                fs.Write(BitConverter.GetBytes(_a), 0, 2);

                double b = (chennels[1].Data[i] - minB) / kB;
                short _b = (short)((65535 * b) + short.MinValue);
                fs.Write(BitConverter.GetBytes(_b), 0, 2);
            }

            fs.Close();
        }
    }    
}
