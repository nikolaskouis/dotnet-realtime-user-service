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
    
    //Query
    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), Username = "user1", Email = "user1@example.com" },
            new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@example.com" }
        );
        await context.SaveChangesAsync();

        var controller = new UserController(
            context,
            Mock.Of<IHubContext<UserHub>>(),
            Mock.Of<IPublishEndpoint>(),
            Mock.Of<ILogger<UserController>>()
        );

        var result = await controller.GetUsers();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
        Assert.Equal(2, users.Count());
    }

    //Get
    [Fact]
    public async Task GetUserById_ReturnsUser_WhenUserExists()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var user = new User { Id = Guid.NewGuid(), Username = "user", Email = "user@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var mockClientProxy = new Mock<IClientProxy>();
        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

        var mockHub = new Mock<IHubContext<UserHub>>();
        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);


        var mockPublisher = new Mock<IPublishEndpoint>();
        mockPublisher.Setup(p => p.Publish(It.IsAny<UserFetchedEvent>(), default))
            .Returns(Task.CompletedTask);

        var controller = new UserController(context, mockHub.Object, mockPublisher.Object, Mock.Of<ILogger<UserController>>());

        var result = await controller.GetUserById(user.Id);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var fetchedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(user.Username, fetchedUser.Username);
    }

    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var controller = new UserController(context, Mock.Of<IHubContext<UserHub>>(), Mock.Of<IPublishEndpoint>(), Mock.Of<ILogger<UserController>>());

        var result = await controller.GetUserById(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    //Delete
    [Fact]
    public async Task DeleteUser_RemovesUser_WhenUserExists()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var user = new User { Id = Guid.NewGuid(), Username = "deleteUser", Email = "delete@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        var mockClientProxy = new Mock<IClientProxy>();

        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

        var mockHubContext = new Mock<IHubContext<UserHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        
        var mockPublisher = new Mock<IPublishEndpoint>();
        mockPublisher.Setup(p => p.Publish(It.IsAny<UserDeletedEvent>(), default))
            .Returns(Task.CompletedTask);

        var controller = new UserController(context, mockHubContext.Object, mockPublisher.Object, Mock.Of<ILogger<UserController>>());

        var result = await controller.DeleteUser(user.Id);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var deletedUser = Assert.IsType<User>(okResult.Value);
        Assert.Equal(user.Username, deletedUser.Username);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var controller = new UserController(context, Mock.Of<IHubContext<UserHub>>(), Mock.Of<IPublishEndpoint>(), Mock.Of<ILogger<UserController>>());

        var result = await controller.DeleteUser(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }
    
    //Patch
    [Fact]
    public async Task PatchUser_UpdatesUser_WhenUserExists()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);
        var user = new User { Id = Guid.NewGuid(), Username = "olduser", Email = "old@example.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var updatedInfo = new User { Username = "newuser", Email = "new@example.com" };

        var mockClientProxy = new Mock<IClientProxy>();
        mockClientProxy
            .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);

        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

        var mockHub = new Mock<IHubContext<UserHub>>();
        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);


        var mockPublisher = new Mock<IPublishEndpoint>();
        mockPublisher.Setup(p => p.Publish(It.IsAny<UserUpdatedEvent>(), default))
            .Returns(Task.CompletedTask);

        var controller = new UserController(context, mockHub.Object, mockPublisher.Object, Mock.Of<ILogger<UserController>>());

        var result = await controller.PatchUser(user.Id, updatedInfo);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedUser = Assert.IsType<User>(okResult.Value);

        Assert.Equal("newuser", updatedUser.Username);
        Assert.Equal("new@example.com", updatedUser.Email);
    }

    [Fact]
    public async Task PatchUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        var controller = new UserController(context, Mock.Of<IHubContext<UserHub>>(), Mock.Of<IPublishEndpoint>(), Mock.Of<ILogger<UserController>>());

        var result = await controller.PatchUser(Guid.NewGuid(), new User { Username = "x", Email = "x@example.com" });
        Assert.IsType<NotFoundResult>(result.Result);
    }

}
