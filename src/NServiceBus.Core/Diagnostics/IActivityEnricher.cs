namespace NServiceBus.Diagnostics
{
    using System.Diagnostics;
    using NServiceBus.Pipeline;

    /// <summary>
    /// 
    /// </summary>
    public interface IActivityEnricher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="context"></param>
        void Enrich(Activity activity, IIncomingPhysicalMessageContext context);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="context"></param>
        void Enrich(Activity activity, IOutgoingPhysicalMessageContext context);
    }
}