using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedService
{
    public interface IServiceLocator
    {
        public IEmbeddedService GetService(string name);
        public void Register(string name, IEmbeddedService svc);
    }
}
