using System;
using System.Collections.Generic;
using System.Text;


namespace EmbeddedService
{
    public class Locator : IServiceLocator
    {
        public static Locator Instance = new Locator();
        private Locator() { }

        Dictionary<string, IEmbeddedService> services = new Dictionary<string, IEmbeddedService>();
        public IEmbeddedService GetService(string name)
        {
            IEmbeddedService service = null;

            if (services.ContainsKey(name))
            {
                service = services[name];
            }

            return service;
        }

        public void Register(string name, IEmbeddedService service)
        {
            services[name] = service;
        }
    }
}
