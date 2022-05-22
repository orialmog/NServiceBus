namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using DataBus;

    /// <summary>
    /// Extension methods to configure data bus.
    /// </summary>
    public static partial class UseDataBusExtensions
    {
        /// <summary>
        /// Configures NServiceBus to use the given data bus definition.
        /// </summary>
        /// <param name="config">The <see cref="EndpointConfiguration" /> instance to apply the settings to.</param>
        public static DataBusExtensions<TDataBus> UseDataBus<TDataBus, TDataBusSerializer>(this EndpointConfiguration config)
            where TDataBus : DataBusDefinition, new()
            where TDataBusSerializer : IDataBusSerializer, new()
        {
            Guard.AgainstNull(nameof(config), config);

            var type = typeof(DataBusExtensions<>).MakeGenericType(typeof(TDataBus));
            var extension = (DataBusExtensions<TDataBus>)Activator.CreateInstance(type, config.Settings);
            var definition = (DataBusDefinition)Activator.CreateInstance(typeof(TDataBus));

            config.Settings.Set(Features.DataBus.SelectedDataBusKey, definition);
            config.Settings.Set(Features.DataBus.DataBusSerializerTypeKey, typeof(TDataBusSerializer));
            config.Settings.Set(Features.DataBus.AdditionalDataBusDeserializersKey, new List<IDataBusSerializer>());

            config.EnableFeature<Features.DataBus>();

            return extension;
        }
    }
}