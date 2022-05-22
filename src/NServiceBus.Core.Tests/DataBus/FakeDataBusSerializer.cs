namespace NServiceBus.Core.Tests.DataBus
{
    using System;
    using System.IO;
    using NServiceBus.DataBus;

    public class FakeDataBusSerializer : IDataBusSerializer
    {
        public FakeDataBusSerializer(string contentType = "some-content-type")
        {
            Name = contentType;
        }
        public string Name { get; }

        public object Deserialize(Type propertyType, Stream stream)
        {
            return "test";
        }

        public void Serialize(object databusProperty, Stream stream)
        {
        }
    }
}