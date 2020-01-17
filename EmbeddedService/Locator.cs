using System;
using System.Collections.Generic;
using System.Text;


namespace EmbeddedService
{
    public class Locator : IServiceLocator
    {
        public static Locator Instance = new Locator();
        private Locator() { }

        readonly Dictionary<ServiceType, IEmbeddedService> services = new Dictionary<ServiceType, IEmbeddedService>();
        public IEmbeddedService GetService(ServiceType serviceType)
        {
            IEmbeddedService service = null;

            if (services.ContainsKey(serviceType))
            {
                service = services[serviceType];
            }

            return service;
        }

        public void Register(IEmbeddedService service)
        {
            services[service.Service] = service;
        }
    }
}
