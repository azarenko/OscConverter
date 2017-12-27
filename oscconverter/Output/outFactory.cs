using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OscConverter.Output
{
    class outFactory
    {
        public static outInterface GetInstance(string filename)
        {
            switch (Path.GetExtension(filename))
            {
                case ".mwf": return new mwf(filename);
                case ".mt": return new mt(filename);
                default: throw new Exception("Unknown format");
            }
        }
    }
}
