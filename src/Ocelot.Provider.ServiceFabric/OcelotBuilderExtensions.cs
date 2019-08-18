using System;
using System.Fabric;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Client;
using Ocelot.DependencyInjection;
using Ocelot.ServiceDiscovery;

namespace Ocelot.Provider.ServiceFabric
{
    public static class OcelotBuilderExtensions
    {
        private static readonly string GlobalConfiguration = "GlobalConfiguration:ServiceDiscoveryProvider";

        public static IOcelotBuilder AddServiceFabric(this IOcelotBuilder builder)
        {
            var service = builder.Services.First(x => x.ServiceType == typeof(IConfiguration));
            var configuration = (IConfiguration)service.ImplementationInstance;

            return builder.AddServiceFabric(configuration);
        }

        public static IOcelotBuilder AddServiceFabric(this IOcelotBuilder builder, IConfiguration configuration)
        {
            var services = builder.Services;
            services.Configure<ServiceFabricProviderOptions>(configuration.GetSection(GlobalConfiguration));
            var config = configuration.GetSection(GlobalConfiguration).Get<ServiceFabricProviderOptions>();

            return builder.AddServiceFabric(o =>
            {
                o.ResolveClientsOnStartup = config.ResolveClientsOnStartup;
                o.UpdateOnClusterChanges = config.ResolveClientsOnStartup;
            });
        }

        public static IOcelotBuilder AddServiceFabric(this IOcelotBuilder builder, Action<ServiceFabricProviderOptions> options)
        {
            var services = builder.Services;
            services.AddSingleton<FabricClient>();
            services.Configure(options);

            services.TryAddSingleton<IServicePartitionResolver>(provider =>
            {
                var serviceFabricProviderOptions = provider.GetRequiredService<IOptions<ServiceFabricProviderOptions>>().Value;
                var servicePartitionResolver = new ServicePartitionResolver();
                if (!serviceFabricProviderOptions.UpdateOnClusterChanges)
                {
                    servicePartitionResolver.DisableNotification();
                }
                return servicePartitionResolver;
            });

            services.TryAddSingleton<IServiceFabricEndpointDiscovery, ServiceFabricEndpointDiscovery>();
            services.AddSingleton<ServiceDiscoveryFinderDelegate>(ServiceFabricProviderFactory.Get);
            services.AddHostedService<ResolveFabricEndpointsHostedService>();

            return builder;
        }
    }
}
