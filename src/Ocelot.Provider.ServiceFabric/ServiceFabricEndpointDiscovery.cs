using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Client;
using Ocelot.Logging;

namespace Ocelot.Provider.ServiceFabric
{
    public class ServiceFabricEndpointDiscovery : IServiceFabricEndpointDiscovery
    {
        private readonly FabricClient _fabricClient;
        private readonly IServicePartitionResolver _servicePartitionResolver;
        private readonly ServiceFabricProviderOptions _options;
        private readonly IOcelotLogger _logger;

        public ServiceFabricEndpointDiscovery(FabricClient fabricClient, IServicePartitionResolver servicePartitionResolver, IOptions<ServiceFabricProviderOptions> options, IOcelotLoggerFactory factory)
        {
            _fabricClient = fabricClient;
            _servicePartitionResolver = servicePartitionResolver;
            _options = options.Value;
            _logger = factory.CreateLogger<ServiceFabricEndpointDiscovery>();
        }

        /// <summary>
        ///  Used: https://stackoverflow.com/a/40256772
        /// </summary>
        /// <returns></returns>
        public async Task Discover(IEnumerable<string> serviceNames)
        {
            if (!_options.ResolveClientsOnStartup)
            {
                return;
            }

            try
            {
                foreach (var serviceName in serviceNames?.Distinct())
                {
                    _logger.LogInformation($"Resolving service: '{serviceName}'.");

                    var serviceNameUri = ServiceFabricUriBuilder.Build(serviceName);
                    var partitions = await _fabricClient.QueryManager.GetPartitionListAsync(serviceNameUri);
                    foreach (var partition in partitions)
                    {
                        _logger.LogInformation($"Discovered service partition: '{partition.PartitionInformation.Kind}':'{partition.PartitionInformation.Id}'");
                        var key = partition.PartitionInformation.Kind switch
                        {
                            ServicePartitionKind.Singleton => ServicePartitionKey.Singleton,
                            _ => throw new ArgumentOutOfRangeException($"Partitionkind: '{partition.PartitionInformation.Kind}' unknown."),
                        };
                        try
                        {
                            var resolved = await _servicePartitionResolver.ResolveAsync(serviceNameUri, key, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), CancellationToken.None);
                            foreach (var endpoint in resolved.Endpoints)
                            {
                                _logger.LogInformation($"Discovered service endpoint: '{endpoint.Address}'");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Could not resolve service: '{serviceNameUri}'.", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
