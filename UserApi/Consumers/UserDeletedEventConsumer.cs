using MassTransit;
using UserApi.Events;

namespace UserApi.Consumers;

public class UserDeletedEventConsumer : IConsumer<UserDeletedEvent>
{
    public Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var user = context.Message;
        Console.WriteLine($"[RabbitMQ] User: {user.Username} ({user.Email}), was deleted by the api");
        
        return Task.CompletedTask;
    }
}