namespace NServiceBus.Extensions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;

    class OutgoingLogicalMessageDiagnostics : Behavior<IOutgoingLogicalMessageContext>
    {
        readonly DiagnosticListener _diagnosticListener;
        const string EventName = ActivityNames.OutgoingLogicalMessage + ".Sent";

        public OutgoingLogicalMessageDiagnostics(DiagnosticListener diagnosticListener)
            => _diagnosticListener = diagnosticListener;

        public OutgoingLogicalMessageDiagnostics()
            : this(new DiagnosticListener(ActivityNames.OutgoingLogicalMessage)) { }

        public override async Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            await next().ConfigureAwait(false);

            if (_diagnosticListener.IsEnabled(EventName))
            {
                _diagnosticListener.Write(EventName, context);
            }
        }
    }
}