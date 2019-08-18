# Ocelot.Provider.ServiceFabric

## How to use

#### Startup
```csharp
services.AddOcelot()
        .AddServiceFabric();
```
#### Ocelot configuration
```json
{
  "ReRoutes": [
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
      "ResolveClientsOnStartup": "true",
      "UpdateOnClusterChanges": "true"
    }
  }
}
```
- ResolveClientsOnStartup
  - A hosted service will resolve all configured services on startup.
- UpdateOnClusterChanges
  - The `FabricClient` service endpoints cache gets updates by notification on cluster changes.  
     Adding a notification for a service will take ~1.5 seconds on the first resolve.  
     Setting both `ResolveClientsOnStartup` and `UpdateOnClusterChanges` on `true` will add the notification and fills the  `FabricClient`cache on startup. So the initial resolve when using Ocelot gateway is fast.  
