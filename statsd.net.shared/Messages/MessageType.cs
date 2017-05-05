﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace statsd.net.shared.Messages
{
    public enum MessageType
    {
        Counter,
        Timing,
        Set,
        Gauge,
        Calendargram,
        Raw,
        Invalid,
        Unknown,
    }
}
