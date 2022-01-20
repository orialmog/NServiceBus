namespace NServiceBus
{
    using NServiceBus.Diagnostics;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    class CompleteAction : StorageAction
    {
        public CompleteAction(IContainSagaData sagaData, Dictionary<string, SagaStorageFile> sagaFiles, SagaManifestCollection sagaManifests) : base(sagaData, sagaFiles, sagaManifests)
        {
        }

        public override Task Execute(CancellationToken cancellationToken = default)
        {
            using (var activity = NServiceBusActivitySource.ActivitySource.StartActivity("CompleteAction"))
            {
                var sagaFile = GetSagaFile();

                sagaFile.MarkAsCompleted();
            }
            return Task.CompletedTask;
        }
    }
}