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

    public UserController(
        AppDbContext context, 
        IHubContext<UserHub> hubContext, 
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _hubContext = hubContext;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return Ok(await _context.Users.ToListAsync());
    }

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
        
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }
}