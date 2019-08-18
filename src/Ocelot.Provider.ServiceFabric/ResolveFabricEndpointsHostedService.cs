using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Ocelot.Configuration.Repository;

namespace Ocelot.Provider.ServiceFabric
{
    public class ResolveFabricEndpointsHostedService : IHostedService
    {
        private readonly IInternalConfigurationRepository _internalConfigurationRepository;
        private readonly IServiceFabricEndpointDiscovery _serviceFabricEndpointDiscovery;

        public ResolveFabricEndpointsHostedService(IInternalConfigurationRepository internalConfigurationRepository, IServiceFabricEndpointDiscovery serviceFabricEndpointDiscovery)
        {
            _internalConfigurationRepository = internalConfigurationRepository;
            _serviceFabricEndpointDiscovery = serviceFabricEndpointDiscovery;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var config = _internalConfigurationRepository.Get();
            var serviceNames = config.Data.ReRoutes.SelectMany(x => x.DownstreamReRoute.Select(y => y.ServiceName));

            await _serviceFabricEndpointDiscovery.Discover(serviceNames);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}