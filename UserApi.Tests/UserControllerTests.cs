using Moq;
using UserApi.Controllers;
using UserApi.Data;
using UserApi.Models;
using UserApi.Events;
using UserApi.Hubs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace UserApi.Tests;

public class UserControllerTests
{
    [Fact]
    public async Task CreateUser_ReturnsCreatedUser()
    {
        // This creates a real instance of DbContext in memory.
        // EF Core handles all logic internally — no mocking needed.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com"
        };

        // Mock SignalR
        var mockHubContext = new Mock<IHubContext<UserHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);

        mockClients.Setup(x => x.All).Returns(mockClientProxy.Object);
        mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

        // Mock publisher
        var mockPublisher = new Mock<IPublishEndpoint>();
        mockPublisher
            .Setup(p => p.Publish(It.IsAny<UserCreatedEvent>(), default))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<UserController>>();

        var controller = new UserController(
            context,
            mockHubContext.Object,
            mockPublisher.Object,
            mockLogger.Object
        );

        // Act
        var result = await controller.CreateUser(user);

        // Assert
        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedUser = Assert.IsType<User>(createdAt.Value);
        Assert.Equal(user.Username, returnedUser.Username);
        Assert.Equal(user.Email, returnedUser.Email);
    }
}
