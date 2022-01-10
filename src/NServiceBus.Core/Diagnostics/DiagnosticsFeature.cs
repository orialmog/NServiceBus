namespace NServiceBus.Extensions.Diagnostics
{
    using NServiceBus.Features;

    /// <summary>
    /// 
    /// </summary>
    public class DiagnosticsFeature : Feature
    {
        /// <summary>
        /// 
        /// </summary>
        public DiagnosticsFeature()
        {
            Defaults(settings => settings.SetDefault(new InstrumentationOptions
            {
                CaptureMessageBody = false
            }));
            EnableByDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        protected internal override void Setup(FeatureConfigurationContext context)
        {
            var activityEnricher = new SettingsActivityEnricher(context.Settings);

            context.Pipeline.Register(new IncomingPhysicalMessageDiagnostics(activityEnricher), "Parses incoming W3C trace information from incoming messages.");
            context.Pipeline.Register(new OutgoingPhysicalMessageDiagnostics(activityEnricher), "Appends W3C trace information to outgoing messages.");
            context.Pipeline.Register(new IncomingLogicalMessageDiagnostics(), "Raises diagnostic events for successfully processed messages.");
            context.Pipeline.Register(new OutgoingLogicalMessageDiagnostics(), "Raises diagnostic events for successfully sent messages.");
            context.Pipeline.Register(new InvokedHandlerDiagnostics(), "Raises diagnostic events when a handler/saga was invoked.");
        }
    }
}