using MassTransit;
using UserApi.Events;

namespace UserApi.Consumers;

public class UserUpdatedEventConsumer : IConsumer<UserUpdatedEvent>
{
    public Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var user = context.Message;
        Console.WriteLine($"[RabbitMQ] User {user.Username} ({user.Email}),  was updated:");
            
        // Emails, logs etc
        return Task.CompletedTask;
    }
}