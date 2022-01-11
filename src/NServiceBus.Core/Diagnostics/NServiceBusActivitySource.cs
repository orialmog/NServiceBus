namespace NServiceBus.Extensions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    static class NServiceBusActivitySource
    {
        internal static readonly ActivitySource ActivitySource = new ActivitySource("NServiceBus", GetVersion());

        static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}