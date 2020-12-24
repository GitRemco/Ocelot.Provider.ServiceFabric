using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Ocelot.Configuration.Repository;

namespace Ocelot.Provider.ServiceFabric
{
    public class ResolveFabricEndpointsHostedService : BackgroundService
    {
        private readonly IInternalConfigurationRepository _internalConfigurationRepository;
        private readonly IServiceFabricEndpointDiscovery _serviceFabricEndpointDiscovery;

        public ResolveFabricEndpointsHostedService(IInternalConfigurationRepository internalConfigurationRepository, IServiceFabricEndpointDiscovery serviceFabricEndpointDiscovery)
        {
            _internalConfigurationRepository = internalConfigurationRepository;
            _serviceFabricEndpointDiscovery = serviceFabricEndpointDiscovery;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var foundConfig = false;
            while (!stoppingToken.IsCancellationRequested && !foundConfig)
            {
                // Delay because the HostedService is executed before the middleware. 
                // The Ocelot configuration is added in the `UseOcelot` middleware.
                await Task.Delay(2000, stoppingToken);
                var config = _internalConfigurationRepository.Get();
                foundConfig = config.Data != null;
                if (foundConfig)
                {
                    var serviceNames = config.Data.Routes.SelectMany(x => x.DownstreamRoute.Select(downStreamRoute => downStreamRoute.ServiceName));
                    await _serviceFabricEndpointDiscovery.Discover(serviceNames);
                }
            }
        }
    }
}