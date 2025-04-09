using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Hubs;
using MassTransit;
using Microsoft.OpenApi.Models;
using UserApi.Consumers;
using Serilog;
using UserApi.DependencyInjection;

//logger
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

//Even though I am not using Autofac, I added it here
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new AutofacModule());
});

// If I had an Email Service, it would look like this, to inject it.
// builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();


builder.Host.UseSerilog();
// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


//Mass Transit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedEventConsumer>();
    x.AddConsumer<UserFetchedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        //rabbitmq credentials
        cfg.Host("host.docker.internal", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("user-created-queue", e =>
        {
            e.ConfigureConsumer<UserCreatedEventConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("user-fetched-queue", e =>
        {
            e.ConfigureConsumer<UserFetchedEventConsumer>(context);
        });
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });
});


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