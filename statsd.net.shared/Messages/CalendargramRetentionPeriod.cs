﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace statsd.net.shared.Messages
{
    public enum CalendargramRetentionPeriod
    {
        OneMinute,
        FiveMinute,
        Hour,
        Day,
        DayOfWeek,
        Week,
        Month
    }
}
