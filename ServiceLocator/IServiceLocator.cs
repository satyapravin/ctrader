using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedService
{
    public interface IServiceLocator
    {
        public IEmbeddedService GetService(ServiceType svcType);
        public void Register(IEmbeddedService svc);
    }
}
