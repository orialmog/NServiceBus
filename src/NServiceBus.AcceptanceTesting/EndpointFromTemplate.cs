namespace NServiceBus.AcceptanceTesting
{
    using System.Threading.Tasks;
    using Configuration.AdvancedExtensibility;
    using Features;
    using Support;

    public class EndpointFromTemplate<TTemplate> : IEndpointConfigurationFactory where TTemplate : IEndpointSetupTemplate, new()
    {
        readonly EndpointCustomizationConfiguration configuration = new EndpointCustomizationConfiguration();

        protected virtual void Customize(EndpointConfiguration endpoint, EndpointCustomizationConfiguration configuration)
        {
        }

        public EndpointCustomizationConfiguration Get()
        {
            configuration.BuilderType = GetType();
            configuration.GetConfiguration = async runDescriptor =>
            {
                var endpointSetupTemplate = new TTemplate();
                var endpointConfiguration = await endpointSetupTemplate.GetConfiguration(runDescriptor, configuration, bc =>
                {
                    Customize(bc, configuration);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

                if (configuration.DisableStartupDiagnostics)
                {
                    endpointConfiguration.GetSettings().Set("NServiceBus.HostStartupDiagnostics", FeatureState.Disabled);
                }

                return endpointConfiguration;
            };

            return configuration;
        }

        public ScenarioContext ScenarioContext { get; set; }
    }
}