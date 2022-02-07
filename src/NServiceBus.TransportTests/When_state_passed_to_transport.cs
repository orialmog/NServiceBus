namespace NServiceBus.TransportTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Transport;

    public class When_state_passed_to_transport : NServiceBusTransportTest
    {
        [TestCase(TransportTransactionMode.None)]
        [TestCase(TransportTransactionMode.ReceiveOnly)]
        [TestCase(TransportTransactionMode.SendsAtomicWithReceive)]
        [TestCase(TransportTransactionMode.TransactionScope)]
        public async Task Should_pass_the_state_to_the_on_delegates(TransportTransactionMode transactionMode)
        {
            var onMessageState = new object();
            var onErrorState = new object();

            var onMessageStateCompletionSource = CreateTaskCompletionSource<object>();
            var onErrorStateCompletionSource = CreateTaskCompletionSource<object>();

            await StartPump(
                (_, state, ___) =>
                {
                    onMessageStateCompletionSource.SetResult(state);
                    throw new Exception("Simulated exception");
                },
                (_, state, ___) =>
                {
                    onErrorStateCompletionSource.SetResult(state);
                    return Task.FromResult(ErrorHandleResult.Handled);
                },
                transactionMode,
                onMessageState: onMessageState,
                onErrorState: onErrorState);

            await SendMessage(InputQueueName, new Dictionary<string, string> { { "MyHeader", "MyValue" } }, body: new byte[] { 1, 2, 3 });

            await Task.WhenAll(onMessageStateCompletionSource.Task, onErrorStateCompletionSource.Task);

            Assert.AreSame(onMessageState, await onMessageStateCompletionSource.Task);
            Assert.AreSame(onErrorState, await onErrorStateCompletionSource.Task);
        }
    }
}