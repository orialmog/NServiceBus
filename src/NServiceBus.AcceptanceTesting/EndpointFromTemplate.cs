namespace NServiceBus.AcceptanceTesting
{
    using System.Threading.Tasks;
    using Configuration.AdvancedExtensibility;
    using Features;
    using Support;

    public class EndpointFromTemplate<TTemplate> : IEndpointConfigurationFactory where TTemplate : IEndpointSetupTemplate, new()
    {
        readonly EndpointCustomizationConfiguration configuration = new EndpointCustomizationConfiguration();

        protected virtual void CustomizeConfiguration(EndpointCustomizationConfiguration configuration)
        {
        }

        protected virtual void CustomizeEndpoint(EndpointConfiguration endpointConfiguration)
        {
        }

        protected virtual void ConfigurePublishers(PublisherMetadata publisherMetadata)
        {
        }

        public EndpointCustomizationConfiguration Get()
        {
            configuration.BuilderType = GetType();
            configuration.GetConfiguration = async runDescriptor =>
            {
                CustomizeConfiguration(configuration);
                var endpointSetupTemplate = new TTemplate();
                var endpointConfiguration = await endpointSetupTemplate.GetConfiguration(runDescriptor, configuration, bc =>
                {
                    CustomizeEndpoint(bc);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

                if (configuration.DisableStartupDiagnostics)
                {
                    endpointConfiguration.GetSettings().Set("NServiceBus.HostStartupDiagnostics", FeatureState.Disabled);
                }

                return endpointConfiguration;
            };
            ConfigurePublishers(configuration.PublisherMetadata);

            return configuration;
        }

        public ScenarioContext ScenarioContext { get; set; }
    }
}