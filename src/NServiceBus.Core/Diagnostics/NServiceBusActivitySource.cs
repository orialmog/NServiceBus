namespace NServiceBus.Extensions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    static class NServiceBusActivitySource
    {
        internal static readonly AssemblyName AssemblyName = typeof(NServiceBusActivitySource).Assembly.GetName();
        internal static readonly string ActivitySourceName = AssemblyName.Name;
        internal static readonly Version Version = AssemblyName.Version;
        internal static readonly ActivitySource ActivitySource = new ActivitySource(ActivitySourceName, Version.ToString());
    }
}