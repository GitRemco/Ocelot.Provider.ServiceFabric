# Ocelot.Provider.ServiceFabric

The current `Service Fabric` support in [Ocelot API Gateway](https://github.com/ThreeMammals/Ocelot) 
requires the `Service Fabric reverse proxy` to be enabled.   
The `Service Fabric reverse proxy` is not always enabled.


This extension adds a `Service Fabric provider` to Ocelot.  
It uses the Service Fabric `FabricClient` and `ServicePartitionResolver` as service discovery and to resolve service endpoints.

## How to use

#### Startup
```csharp
services.AddOcelot()
        .AddServiceFabric();
```
#### Ocelot configuration
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/values",
      "UpstreamPathTemplate": "/test",
      "UpstreamHttpMethod": [
        "Get"
      ],
      "DownstreamScheme": "http",
      "ServiceName": "sfApplication/myapi"
    }
  ],
  "GlobalConfiguration": {
    "RequestIdKey": "OcRequestId",
    "ServiceDiscoveryProvider": {
      "Type": "servicefabricprovider",
      "ResolveClientsOnStartup": true,
      "UpdateOnClusterChanges": true
    }
  }
}
```
> :warning: Use `"ReRoutes": [` in version < 3.0
- ResolveClientsOnStartup
  - A hosted service will resolve all configured services on startup.
- UpdateOnClusterChanges
  - The `FabricClient` service endpoints cache gets updates by notification on cluster changes.  
     Adding a notification for a service will take ~1.5 seconds on the first resolve.  
     Setting both `ResolveClientsOnStartup` and `UpdateOnClusterChanges` on `true` will add the notification and fills the  `FabricClient`cache on startup. So the initial resolve when using Ocelot gateway is fast.  

## Download

https://www.nuget.org/packages/Ocelot.Provider.ServiceFabric/1.0.0

