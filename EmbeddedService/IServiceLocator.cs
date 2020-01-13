using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedService
{
    public interface IServiceLocator
    {
        IEmbeddedService GetService(ServiceType svcType);
        void Register(IEmbeddedService svc);
    }
}
