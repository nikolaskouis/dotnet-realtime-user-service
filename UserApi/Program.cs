using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Hubs;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//allow client, mostly for test.html to work
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();

app.UseCors("AllowClient");
app.UseAuthorization();

app.MapControllers();
app.MapHub<UserHub>("/hubs/user");


app.Run();