// unset

using System.Threading.Tasks;
using NServiceBus;

class MyHandler : IHandleMessages<object>
{
    public Task Handle(object message, IMessageHandlerContext context)
    {
        _ = Program.cde.Signal();
        return Task.CompletedTask;
    }
}