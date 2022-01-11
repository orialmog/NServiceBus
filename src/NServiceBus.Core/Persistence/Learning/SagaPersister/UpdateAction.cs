namespace NServiceBus
{
    using NServiceBus.Extensions.Diagnostics;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    class UpdateAction : StorageAction
    {
        public UpdateAction(IContainSagaData sagaData, Dictionary<string, SagaStorageFile> sagaFiles, SagaManifestCollection sagaManifests) : base(sagaData, sagaFiles, sagaManifests)
        {
        }

        public override async Task Execute(CancellationToken cancellationToken = default)
        {
            using (var activity = NServiceBusActivitySource.ActivitySource.StartActivity("UpdateAction"))
            {
                var sagaFile = GetSagaFile();

                await sagaFile.Write(sagaData, cancellationToken);
            }
        }
    }
}