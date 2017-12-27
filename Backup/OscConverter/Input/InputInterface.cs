using System;
using System.Collections.Generic;
using System.Text;

namespace OscConverter.Input
{
    interface InputInterface
    {
        List<Channel> GetChannels();
    }
}
