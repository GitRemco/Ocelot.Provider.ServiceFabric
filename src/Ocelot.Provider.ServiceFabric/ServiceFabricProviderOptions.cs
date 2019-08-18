namespace Ocelot.Provider.ServiceFabric
{
    public class ServiceFabricProviderOptions
    {
        /// <summary>
        /// Resolve the clients on startup, this is useful in combination with <see cref="UpdateOnClusterChanges"></see>
        /// </summary>
        public bool ResolveClientsOnStartup { get; set; }

        /// <summary>
        /// Adding the notification for updating the fabric client cache takes time (~1.5sec). This is done the first time the service is resolved. 
        /// By adding the notification the fabric client will update on cluster changes (like port switching)
        /// </summary>
        public bool UpdateOnClusterChanges { get; set; }
    }
}
