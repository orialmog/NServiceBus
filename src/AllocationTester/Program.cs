using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    const int NrOfMessages = 10000;
    public static CountdownEvent cde = new CountdownEvent(NrOfMessages);

    static async Task Main()
    {
        var t = new LearningTransport { StorageDirectory = @"R:\.learningtransport" };
        t.TransportTransactionMode = TransportTransactionMode.ReceiveOnly;

        var cfg = new EndpointConfiguration("test");
        _ = cfg.UseTransport(t);
        cfg.LimitMessageProcessingConcurrencyTo(50);
        cfg.PurgeOnStartup(true);
        var instance = await Endpoint.Start(cfg)
            .ConfigureAwait(false);

        var msg = new object();

        var tasks = new List<Task>();
        var s = new SemaphoreSlim(Environment.ProcessorCount);

        for (int i = 0; i < NrOfMessages; i++)
        {
            await s.WaitAsync()
                .ConfigureAwait(false);
            tasks.Add(instance.SendLocal(msg).ContinueWith(t => s.Release()));
        }

        await Task.WhenAll(tasks)
            .ConfigureAwait(false);

        cde.Wait();

        await instance.Stop()
            .ConfigureAwait(false);
    }
}