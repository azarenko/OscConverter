using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OscConverter.Input
{
    class InputFactory
    {
        public static InputInterface GetInstance(string filename)
        {
            switch (Path.GetExtension(filename))
            {
                case ".dex": return new dex(filename);
                case ".drc": return new drc(filename);
                default: throw new Exception("Неподдерживаемый формат файла");
            }
        }
    }
}
