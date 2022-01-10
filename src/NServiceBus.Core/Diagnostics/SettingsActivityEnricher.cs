namespace NServiceBus.Extensions.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using NServiceBus.Pipeline;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transport;

    class SettingsActivityEnricher : IActivityEnricher
    {
        readonly IReadOnlySettings _settings;
        readonly InstrumentationOptions _options;

        public SettingsActivityEnricher(IReadOnlySettings settings)
        {
            _settings = settings;
            _options = settings.Get<InstrumentationOptions>();
        }

        public void Enrich(Activity activity, IIncomingPhysicalMessageContext context)
        {
            var destinationName = _settings.LocalAddress();
            const string operationName = "process";
            activity.DisplayName = $"{destinationName} {operationName}";

            activity.AddTag("messaging.message_id", context.Message.MessageId);
            activity.AddTag("messaging.operation", operationName);
            activity.AddTag("messaging.destination", destinationName);
            activity.AddTag("messaging.message_payload_size_bytes", context.Message.Body.Length.ToString());

            Enrich(activity, context.MessageHeaders);

            if (activity.IsAllDataRequested && _options.CaptureMessageBody)
            {
                activity.AddTag("messaging.message_payload", Encoding.UTF8.GetString(context.Message.Body.ToArray()));
            }
        }

        public void Enrich(Activity activity, IOutgoingPhysicalMessageContext context)
        {
            var routes = context.RoutingStrategies
                .Select(r => r.Apply(context.Headers))
                .Select(t => t switch
                {
                    UnicastAddressTag u => u.Destination,
                    MulticastAddressTag m => m.MessageType.FullName,
                    _ => null
                })
                .ToList();

            var destinationName = string.Join(", ", routes);
            const string operationName = "send";

            activity.DisplayName = $"{destinationName} {operationName}";

            activity.AddTag("messaging.message_id", context.MessageId);
            activity.AddTag("messaging.operation", operationName);
            activity.AddTag("messaging.message_payload_size_bytes", context.Body.Length.ToString());
            activity.AddTag("messaging.destination", destinationName);

            Enrich(activity, context.Headers);

            if (activity.IsAllDataRequested && _options.CaptureMessageBody)
            {
                activity.AddTag("messaging.message_payload", Encoding.UTF8.GetString(context.Body.ToArray()));
            }
        }

        void Enrich(Activity activity, IReadOnlyDictionary<string, string> contextHeaders)
        {
            var transportDefinition = _settings.Get<TransportDefinition>();
            activity.AddTag("messaging.system", transportDefinition.GetType().Name.Replace("Transport", null).ToLowerInvariant());
            if (contextHeaders.TryGetValue(NServiceBus.Headers.ConversationId, out var conversationId))
            {
                activity.AddTag("messaging.conversation_id", conversationId);
            }

            // TODO: Unknown how to resolve OutboundRoutingPolicy

            //if (contextHeaders.TryGetValue(NServiceBus.Headers.MessageIntent, out var intent) && Enum.TryParse<MessageIntentEnum>(intent, out var intentValue))
            //{
            //    var routingPolicy = _settings.Get<TransportInfrastructure>().OutboundRoutingPolicy;

            //    var kind = GetDestinationKind(intentValue, routingPolicy);

            //    if (kind != null)
            //    {
            //        activity.AddTag("messaging.destination_kind", kind);
            //    }
            //}

            foreach (var header in contextHeaders.Where(header => header.Key.StartsWith("NServiceBus.")))
            {
                activity.AddTag($"messaging.{header.Key.ToLowerInvariant()}", header.Value);
            }
        }

        //static string GetDestinationKind(MessageIntent intentValue, OutboundRoutingPolicy routingPolicy) =>
        //    intentValue switch
        //    {
        //        MessageIntent.Send => ConvertPolicyToKind(routingPolicy.Sends),
        //        MessageIntent.Publish => ConvertPolicyToKind(routingPolicy.Publishes),
        //        MessageIntent.Subscribe => ConvertPolicyToKind(routingPolicy.Sends),
        //        MessageIntent.Unsubscribe => ConvertPolicyToKind(routingPolicy.Sends),
        //        MessageIntent.Reply => ConvertPolicyToKind(routingPolicy.Replies),
        //        _ => null
        //    };

        //static string ConvertPolicyToKind(OutboundRoutingType type) =>
        //    type switch
        //    {
        //        OutboundRoutingType.Unicast => "queue",
        //        OutboundRoutingType.Multicast => "topic",
        //        _ => null
        //    };
    }
}