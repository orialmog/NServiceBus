namespace NServiceBus.AcceptanceTests.Audit
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Support;
    using EndpointTemplates;
    using NUnit.Framework;

    public class When_audit_is_overridden_in_code : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_audit_to_target_queue()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<UserEndpoint>(b => b.When(session => session.SendLocal(new MessageToBeAudited())))
                .WithEndpoint<AuditSpy>()
                .Done(c => c.MessageAudited)
                .Run();

            Assert.True(context.MessageAudited);
        }

        public class UserEndpoint : EndpointFromTemplate<DefaultServer>
        {
            protected override void Customize(EndpointConfiguration endpointConfiguration, EndpointCustomizationConfiguration configuration) =>
                endpointConfiguration.AuditProcessedMessagesTo("audit_with_code_target");

            class Handler : IHandleMessages<MessageToBeAudited>
            {
                public Task Handle(MessageToBeAudited message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }
            }
        }

        public class AuditSpy : EndpointFromTemplate<DefaultServer>
        {
            protected override void Customize(EndpointConfiguration endpoint, EndpointCustomizationConfiguration configuration) =>
                configuration.CustomEndpointName = "audit_with_code_target";

            class AuditMessageHandler : IHandleMessages<MessageToBeAudited>
            {
                public AuditMessageHandler(Context context)
                {
                    testContext = context;
                }

                public Task Handle(MessageToBeAudited message, IMessageHandlerContext context)
                {
                    testContext.MessageAudited = true;
                    return Task.FromResult(0);
                }

                Context testContext;
            }
        }

        public class Context : ScenarioContext
        {
            public bool MessageAudited { get; set; }
        }


        public class MessageToBeAudited : IMessage
        {
        }
    }
}