namespace NServiceBus.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;


    class IncomingPhysicalMessageDiagnostics : Behavior<IIncomingPhysicalMessageContext>
    {
        readonly IActivityEnricher _activityEnricher;
        readonly DiagnosticListener _diagnosticListener;
        const string EventName = ActivityNames.IncomingPhysicalMessage + ".Processed";

        public IncomingPhysicalMessageDiagnostics(IActivityEnricher activityEnricher)
        {
            _diagnosticListener = new DiagnosticListener(ActivityNames.IncomingPhysicalMessage);
            _activityEnricher = activityEnricher;
        }

        public override async Task Invoke(
            IIncomingPhysicalMessageContext context,
            Func<Task> next)
        {
            EnrichActivity(context);

            try
            {
                await next().ConfigureAwait(false);

                if (_diagnosticListener.IsEnabled(EventName))
                {
                    _diagnosticListener.Write(EventName, context);
                }
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                throw;
            }

        }

        void EnrichActivity(IIncomingPhysicalMessageContext context)
        {
            var activity = Activity.Current;

            if (activity == null)
            {
                return;
            }

            var headers = context.MessageHeaders;

            if (!headers.TryGetValue(Diagnostics.Headers.TraceParentHeaderName, out var parentId))
            {
                headers.TryGetValue(Headers.RequestIdHeaderName, out parentId);
            }

            string traceStateString = default;
            var baggageItems = new List<KeyValuePair<string, string>>();

            if (string.IsNullOrEmpty(parentId))
            {
                return;
            }

            if (headers.TryGetValue(Headers.TraceStateHeaderName, out var traceState))
            {
                traceStateString = traceState;
            }

            if (headers.TryGetValue(Headers.BaggageHeaderName, out var baggageValue) || headers.TryGetValue(Headers.CorrelationContextHeaderName, out baggageValue))
            {
                var baggage = baggageValue.Split(',');
                if (baggage.Length > 0)
                {
                    foreach (var item in baggage)
                    {
                        if (NameValueHeaderValue.TryParse(item, out var baggageItem))
                        {
                            baggageItems.Add(new KeyValuePair<string, string>(baggageItem.Name, Uri.UnescapeDataString(baggageItem.Value)));
                        }
                    }
                }
            }

            activity.TraceStateString = traceStateString;

            _activityEnricher.Enrich(activity, context);

            foreach (var baggageItem in baggageItems)
            {
                activity.AddBaggage(baggageItem.Key, baggageItem.Value);
            }
        }
    }
}
