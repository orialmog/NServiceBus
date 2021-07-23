using Microsoft.Extensions.DependencyInjection;
using NServiceBus.Features;
using NServiceBus.Serialization;
using NServiceBus.Unicast.Messages;

class FeatureReplacingExistingStage : Feature
{
    internal FeatureReplacingExistingStage()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        context.RegisterStartupTask(s =>
            new SerializeMessageConnectorEx(
                s.GetRequiredService<IMessageSerializer>(),
                s.GetRequiredService<MessageMetadataRegistry>()
                ));

        
        var pipeline = context.Pipeline;
        pipeline.Replace("NServiceBus.SerializeMessageConnector", new SerializeMessageConnectorEx());
   }
}