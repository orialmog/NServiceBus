namespace NServiceBus.Core.Tests.DataBus
{
    using System.Collections.Generic;
    using NServiceBus.DataBus;
    using NUnit.Framework;

    [TestFixture]
    public class DataBusConfigurationTests
    {
        [Test]
        public void Should_allow_multiple_deserializers_to_be_used()
        {
            var endpointConfiguration = new EndpointConfiguration("MyEndpoint");

            endpointConfiguration.UseDataBus<FileShareDataBus, SystemJsonDataBusSerializer>()
                .AddDeserializer(new FakeDataBusSerializer("content-type-1"))
                .AddDeserializer(new FakeDataBusSerializer("content-type-2"));

            Assert.AreEqual(endpointConfiguration.Settings.Get<List<IDataBusSerializer>>(NServiceBus.Features.DataBus.AdditionalDataBusDeserializersKey).Count, 2);
        }
    }
}
