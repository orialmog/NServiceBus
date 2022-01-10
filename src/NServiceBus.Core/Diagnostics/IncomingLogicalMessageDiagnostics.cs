namespace NServiceBus.Extensions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;

    class IncomingLogicalMessageDiagnostics : Behavior<IIncomingLogicalMessageContext>
    {
        readonly DiagnosticListener _diagnosticListener;
        const string EventName = ActivityNames.IncomingLogicalMessage + ".Processed";

        public IncomingLogicalMessageDiagnostics(DiagnosticListener diagnosticListener)
            => _diagnosticListener = diagnosticListener;

        public IncomingLogicalMessageDiagnostics()
            : this(new DiagnosticListener(ActivityNames.IncomingLogicalMessage))
        {
        }

        public override async Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            await next().ConfigureAwait(false);

            if (_diagnosticListener.IsEnabled(EventName))
            {
                _diagnosticListener.Write(EventName, context);
            }
        }
    }
}