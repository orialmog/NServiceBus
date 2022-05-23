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
            where TDataBusSerializer : IDataBusSerializer
        {
            Guard.AgainstNull(nameof(config), config);

            var dataBusType = typeof(DataBusExtensions<>).MakeGenericType(typeof(TDataBus));
            var dataBusExtension = (DataBusExtensions<TDataBus>)Activator.CreateInstance(dataBusType, config.Settings);
            var dataBusDefinition = (DataBusDefinition)Activator.CreateInstance(typeof(TDataBus));

            EnableDataBus(config, dataBusDefinition, typeof(TDataBusSerializer));

            return dataBusExtension;
        }

        /// <summary>
        /// Configures NServiceBus to use a custom <see cref="IDataBus" /> implementation.
        /// </summary>
        /// <param name="config">The <see cref="EndpointConfiguration" /> instance to apply the settings to.</param>
        /// <param name="dataBusType">The <see cref="IDataBus" /> <see cref="Type" /> to use.</param>
        /// <param name="dataBusSerializerType">The data bus serializer <see cref="Type" /> to use.</param>
        public static DataBusExtensions UseDataBus(this EndpointConfiguration config, Type dataBusType, Type dataBusSerializerType)
        {
            Guard.AgainstNull(nameof(config), config);
            Guard.AgainstNull(nameof(dataBusType), dataBusType);
            Guard.AgainstNull(nameof(dataBusSerializerType), dataBusSerializerType);

            if (!typeof(IDataBus).IsAssignableFrom(dataBusType))
            {
                throw new ArgumentException("The data bus type needs to implement IDataBus.", nameof(dataBusType));
            }

            if (!typeof(IDataBusSerializer).IsAssignableFrom(dataBusSerializerType))
            {
                throw new ArgumentException("The data bus serializer type needs to implement IDataBusSerializer.", nameof(dataBusSerializerType));
            }

            config.Settings.Set(Features.DataBus.CustomDataBusTypeKey, dataBusType);

            EnableDataBus(config, new CustomDataBus(), dataBusSerializerType);

            return new DataBusExtensions(config.Settings);
        }

        static void EnableDataBus(EndpointConfiguration config, DataBusDefinition selectedDataBus, Type dataBusSerializerType)
        {
            config.Settings.Set(Features.DataBus.SelectedDataBusKey, selectedDataBus);
            config.Settings.Set(Features.DataBus.DataBusSerializerTypeKey, dataBusSerializerType);
            config.Settings.Set(Features.DataBus.AdditionalDataBusDeserializersKey, new List<IDataBusSerializer>());

            config.EnableFeature<Features.DataBus>();
        }
    }
}