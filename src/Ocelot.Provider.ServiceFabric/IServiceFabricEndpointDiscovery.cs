using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ocelot.Provider.ServiceFabric
{
    public interface IServiceFabricEndpointDiscovery
    {
        Task Discover(IEnumerable<string> serviceNames);
    }
}
