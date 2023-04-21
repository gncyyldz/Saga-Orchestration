using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Models.Context;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCompletedEventConsumer>();
    configure.AddConsumer<OrderFailedEventConsumer>();

    configure.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration["RabbitMQ"]);

        configurator.ReceiveEndpoint(RabbitMQSettings.Order_OrderCompletedEventQueue,
                                     e => e.ConfigureConsumer<OrderCompletedEventConsumer>(context));
        configurator.ReceiveEndpoint(RabbitMQSettings.Order_OrderFailedEventQueue,
                                     e => e.ConfigureConsumer<OrderFailedEventConsumer>(context));

    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
