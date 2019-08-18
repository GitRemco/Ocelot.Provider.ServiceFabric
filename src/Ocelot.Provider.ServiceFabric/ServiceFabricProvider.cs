using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using Ocelot.Logging;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;

namespace Ocelot.Provider.ServiceFabric
{
    public class ServiceFabricProvider : IServiceDiscoveryProvider
    {
        private readonly string _servicename;
        private readonly IServicePartitionResolver _servicePartitionResolver;
        private readonly IOcelotLogger _logger;

        public ServiceFabricProvider(string serviceName, IServicePartitionResolver servicePartitionResolver, IOcelotLoggerFactory factory)
        {
            _servicename = serviceName;
            _servicePartitionResolver = servicePartitionResolver;
            _logger = factory.CreateLogger<ServiceFabricProvider>();
        }

        public async Task<List<Service>> Get()
        {
            var serviceNameUri = ServiceFabricUriBuilder.Build(_servicename);
            var services = new List<Service>();
            try
            {
                var service = await _servicePartitionResolver.ResolveAsync(serviceNameUri, ServicePartitionKey.Singleton, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), CancellationToken.None);
                services.Add(BuildService(service));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not resolve service: '{serviceNameUri}'.", ex);
            }

            return services;
        }

        private class ResolvedEndpoint
        {
            public IDictionary<string, string> Endpoints { get; set; }
        }

        private static Service BuildService(ResolvedServicePartition serviceEntry)
        {
            // Endpoints is not a list, the json in the Address element is: {"Endpoints":{"name":"uri"}}, contains only one element.
            var endpoints = JsonConvert.DeserializeObject<ResolvedEndpoint>(serviceEntry.GetEndpoint().Address).Endpoints;
            var endpoint = endpoints.ElementAt(0);

            var name = !string.IsNullOrEmpty(endpoint.Key) ? endpoint.Key : serviceEntry.ServiceName.ToString();
            var endpointAddress = endpoint.Value;

            var uri = new Uri(endpointAddress);
            var host = uri.Host;
            var port = uri.Port;

            return new Service(
                name,
                new ServiceHostAndPort(host, port),
                string.Empty,
                string.Empty,
                Enumerable.Empty<string>());
        }
    }
}
