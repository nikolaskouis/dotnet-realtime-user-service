namespace UserApi.Events;

public record UserCreatedEvent(Guid Id, string Username, string Email);
public record UserFetchedEvent(Guid Id, string Username, string Email);
public record UserDeletedEvent(Guid Id, string Username, string Email);
public record UserUpdatedEvent(Guid Id, string Username, string Email);