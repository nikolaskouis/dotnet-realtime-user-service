using MassTransit;
using UserApi.Events;

namespace UserApi.Consumers;

public class UserFetchedEventConsumer : IConsumer<UserFetchedEvent>
{
    public Task Consume(ConsumeContext<UserFetchedEvent> context)
    {
        var user = context.Message;
        Console.WriteLine($"[RabbitMQ] User: {user.Username} ({user.Email}), was fetched by the api");
        
        return Task.CompletedTask;
    }
}