namespace UserApi.Events;

public record UserCreatedEvent(Guid Id, string Username, string Email);