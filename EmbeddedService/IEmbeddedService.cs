﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmbeddedService
{
    public enum ServiceType { EXCHANGE, MDS, OMS, PMS, MGS };
    public interface IEmbeddedService
    {
        ServiceType Service { get; }
        WaitHandle Start();
        bool Stop();
    }
}
