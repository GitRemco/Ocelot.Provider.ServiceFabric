using System;

namespace Ocelot.Provider.ServiceFabric
{
    public class ServiceFabricUriBuilder
    {
        private static readonly string ServiceFabricPrefix = "fabric:/";

        public static Uri Build(string serviceName)
        {
            return new Uri($"{ServiceFabricPrefix}{serviceName}");
        }
    }
}
