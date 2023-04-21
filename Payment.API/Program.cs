using MassTransit;
using Payment.API.Consumers;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<PaymentStartedEventConsumer>();

    configure.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration["RabbitMQ"]);

        configurator.ReceiveEndpoint(RabbitMQSettings.Payment_StartedEventQueue,
                                     e => e.ConfigureConsumer<PaymentStartedEventConsumer>(context));
    });
});

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
