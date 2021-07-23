using System.Threading.Tasks;
using NServiceBus;

class MyHandler : IHandleMessages<MyBusinessMessage>
{
    public Task Handle(MyBusinessMessage message, IMessageHandlerContext context)
    {
        _ = Program.cde.Signal();
        return Task.CompletedTask;
    }
}