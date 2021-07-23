using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

class Program
{
    const int NrOfMessages = 5000;
    public static CountdownEvent cde = new CountdownEvent(NrOfMessages);

    static async Task Main()
    {
        try
        {
            //LogManager.Use<DefaultFactory>().Level(LogLevel.Debug);

            var transport = new LearningTransport { StorageDirectory = @"R:\.learningtransport" };
            transport.TransportTransactionMode = TransportTransactionMode.ReceiveOnly;

            var cfg = new EndpointConfiguration("test");
            cfg.SendOnly();
            _ = cfg.UseTransport(transport);
            cfg.LimitMessageProcessingConcurrencyTo(50);
            cfg.PurgeOnStartup(true);
            var instance = await Endpoint.Start(cfg)
                .ConfigureAwait(false);

            var tasks = new List<Task>();
            var s = new SemaphoreSlim(Environment.ProcessorCount);

            WL("Sending");
            var pool = new ConcurrentBag<MyBusinessMessage>();

            var r = new Random(0);
            for (int i = 0; i < NrOfMessages; i++)
            {
                await s.WaitAsync().ConfigureAwait(false);

                if (!pool.TryTake(out var msg))
                {
                    msg = new MyBusinessMessage { Data = new byte[r.Next(1000)] };
                    r.NextBytes(msg.Data);
                }

                var t = instance.Send("test", msg);

                tasks.Add(t.ContinueWith(t =>
                {
                    pool.Add(msg);
                    _ = s.Release();
                    return;
                }));
            }

            await Task.WhenAll(tasks)
                .ConfigureAwait(false);

            WL("All send!");

            // cde.Wait();
            //
            // WL("All processed!");

            await instance.Stop()
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.ReadLine();
            throw;
        }
    }

    static Stopwatch sw = Stopwatch.StartNew();
    static void WL(string value)
    {
        _ = Console.Out.WriteLineAsync($"{sw.Elapsed.TotalSeconds,5:N1}s: {value}");
    }
}

class MyBusinessMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;
    public byte[] Data { get; set; }
}