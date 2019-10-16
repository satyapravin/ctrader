using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmbeddedService
{
    public interface IEmbeddedService
    {
        void Start();
        bool Stop();
    }
}
