namespace NServiceBus.Diagnostics
{
    static class Headers
    {
        public const string TraceParentHeaderName = "traceparent";
        public const string TraceStateHeaderName = "tracestate";
        public const string CorrelationContextHeaderName = "Correlation-Context";
        public const string BaggageHeaderName = "baggage";
        public const string RequestIdHeaderName = "Request-Id";
    }
}