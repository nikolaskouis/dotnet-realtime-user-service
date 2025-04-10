using MassTransit;
using Microsoft.AspNetCore.Mvc;
using UserApi.Data;
using UserApi.Models;
using UserApi.Events;
using UserApi.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;


namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<UserHub> _hubContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserController> _logger;

    public UserController(
        AppDbContext context, 
        IHubContext<UserHub> hubContext, 
        IPublishEndpoint publishEndpoint,
        ILogger<UserController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>List of users</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return Ok(await _context.Users.ToListAsync());
    }
    
    /// <summary>
    /// Get a user by {id}
    /// </summary>
    /// <returns>A user</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(Guid id)
    {
        //get by id
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            _logger.LogInformation("Logger: User with id {@id}, was not found", user);
            return NotFound();
        }
        
        // the hub that notifies
        await _hubContext.Clients.All.SendAsync("UserFetched", user);

        //publish the event
        var evt = new UserFetchedEvent(user.Id, user.Username, user.Email);
        await _publishEndpoint.Publish(evt);
        
        _logger.LogInformation("Logger: New user created {@User}, on {Time}", user, DateTime.UtcNow);

        return Ok(user);
    }

    /// <summary>
    /// Creates users
    /// </summary>
    /// <returns>A user</returns>
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        user.Id = Guid.NewGuid();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // the hub that notifies
        await _hubContext.Clients.All.SendAsync("UserAdded", user);

        //publish the event
        var evt = new UserCreatedEvent(user.Id, user.Username, user.Email);
        await _publishEndpoint.Publish(evt);
        
        _logger.LogInformation("Logger: New user created {@User}, on {Time}", user, DateTime.UtcNow);

        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }
    
    /// <summary>
    /// Delete a user by {id}
    /// </summary>
    /// <returns>true/false</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<User>> DeleteUser(Guid id)
    {
        //get by id
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            _logger.LogInformation("Logger: User with id {@id}, was not found", user);
            return NotFound();
        }
        
        //delete
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        
        // the hub that notifies
        await _hubContext.Clients.All.SendAsync("UserDeleted", user);

        //publish the event
        var evt = new UserDeletedEvent(user.Id, user.Username, user.Email);
        await _publishEndpoint.Publish(evt);
        
        _logger.LogInformation("Logger:User {@User}, was deleted on {Time}", user, DateTime.UtcNow);

        return Ok(user);
    }
    
    /// <summary>
    /// Updates a user by {id}
    /// </summary>
    /// <returns>Updated user</returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<User>> PatchUser(Guid id, User updatedUser)
    {
        var existingUser = await _context.Users.FindAsync(id);

        if (existingUser == null)
        {
            _logger.LogInformation("Logger: User with id {Id} was not found", id);
            return NotFound();
        }

        // If we want to update username or email
        existingUser.Username = updatedUser.Username ?? existingUser.Username;
        existingUser.Email = updatedUser.Email ?? existingUser.Email;

        _context.Users.Update(existingUser);
        await _context.SaveChangesAsync();
        
        await _hubContext.Clients.All.SendAsync("UserUpdated", existingUser);
        
        var evt = new UserUpdatedEvent(existingUser.Id, existingUser.Username, existingUser.Email);
        await _publishEndpoint.Publish(evt);

        _logger.LogInformation("Logger: User {User} was updated on {Time}", existingUser, DateTime.UtcNow);

        return Ok(existingUser);
    }

}