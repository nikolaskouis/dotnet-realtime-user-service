﻿using MassTransit;
using UserApi.Events;

namespace UserApi.Consumers;

public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
{
    public Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var user = context.Message;
        Console.WriteLine($"[RabbitMQ] New user created: {user.Username} ({user.Email})");

        // Here you could trigger emails, logs, downstream APIs, etc.
        return Task.CompletedTask;
    }
}