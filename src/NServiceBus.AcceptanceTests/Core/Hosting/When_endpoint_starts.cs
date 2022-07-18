﻿namespace NServiceBus.AcceptanceTests.Core.Hosting
{
    using System.IO;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AcceptanceTesting.Support;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_endpoint_starts : NServiceBusAcceptanceTest
    {
        static string basePath = Path.Combine(TestContext.CurrentContext.TestDirectory, TestContext.CurrentContext.Test.ID);

        [Test]
        public async Task Should_emit_config_diagnostics()
        {
            // TestContext.CurrentContext.Test.ID is stable across test runs,
            // therefore we need to clear existing diagnostics file to avoid asserting on a stale file
            if (Directory.Exists(basePath))
            {
                Directory.Delete(basePath, true);
            }

            await Scenario.Define<Context>()
                .WithEndpoint<MyEndpoint>()
                .Done(c => c.EndpointsStarted)
                .Run();

            var endpointName = Conventions.EndpointNamingConvention(typeof(MyEndpoint));
            var startupDiagnoticsFileName = $"{endpointName}-configuration.txt";

            var pathToFile = Path.Combine(basePath, startupDiagnoticsFileName);
            Assert.True(File.Exists(pathToFile));

            TestContext.WriteLine(File.ReadAllText(pathToFile));
        }

        class Context : ScenarioContext
        {
        }

        class MyEndpoint : EndpointFromTemplate<DefaultServer>
        {
            protected override void Customize(EndpointConfiguration endpoint, EndpointCustomizationConfiguration configuration)
            {
                endpoint.SetDiagnosticsPath(basePath);
                configuration.DisableStartupDiagnostics = false;
            }
        }

        class MyMessageHandler : IHandleMessages<MyMessage>
        {
            public Task Handle(MyMessage message, IMessageHandlerContext context)
            {
                throw new System.NotImplementedException();
            }
        }

        public class MyMessage : IMessage
        {
        }
    }
}