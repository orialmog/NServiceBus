namespace NServiceBus.AcceptanceTests.Core.OpenTelemetry;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcceptanceTesting;
using EndpointTemplates;
using NUnit.Framework;

public class When_processing_is_cancelled : OpenTelemetryAcceptanceTest
{
    [Test]
    public async Task Should_do_a_thing()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        using var metricsListener = TestingMetricListener.SetupNServiceBusMetricsListener();
        var context = await Scenario.Define<Context>()
            .WithEndpoint<CancellingEndpoint>(e => e
                .DoNotFailOnErrorMessages()
                .ToCreateInstance(
                    config => Endpoint.Create(config),
                    endpoint => endpoint.Start(cancellationTokenSource.Token)
                )
                .When(session => session.SendLocal(new MessageThatWillBeCancelled()))
                .When(
                    ctx => ctx.MessageReceived,
                    session =>
                    {
                        cancellationTokenSource.Cancel();
                        return Task.CompletedTask;
                    })
            )
            .Done(ctx => cancellationTokenSource.IsCancellationRequested)
            .Run();

        Activity cancelledPipelineActivity = NServicebusActivityListener.CompletedActivities.GetReceiveMessageActivities().Single();
        Assert.AreEqual(ActivityStatusCode.Error, cancelledPipelineActivity.Status);
        cancelledPipelineActivity.VerifyTag("nservicebus.cancelled", true);

        Activity cancelledHandlerActivity = NServicebusActivityListener.CompletedActivities.GetInvokedHandlerActivities().Single();
        Assert.AreEqual(ActivityStatusCode.Error, cancelledHandlerActivity.Status);
        cancelledHandlerActivity.VerifyTag("nservicebus.cancelled", true);
    }

    public class CancellingEndpoint : EndpointConfigurationBuilder
    {
        public CancellingEndpoint()
        {
            EndpointSetup<DefaultServer>();
        }

        public class HandlerThatCancels : IHandleMessages<MessageThatWillBeCancelled>
        {
            Context scenarioContext;

            public HandlerThatCancels(Context scenarioContext) => this.scenarioContext = scenarioContext;

            public async Task Handle(MessageThatWillBeCancelled message, IMessageHandlerContext context)
            {
                scenarioContext.MessageReceived = true;
                await Task.Delay(TimeSpan.FromSeconds(120), context.CancellationToken);
            }
        }
    }

    public class MessageThatWillBeCancelled : IMessage { }

    public class Context : ScenarioContext
    {
        public bool MessageReceived { get; set; }
    }

}