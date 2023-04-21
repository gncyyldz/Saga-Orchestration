using MassTransit;
using MongoDB.Driver;
using Shared.Settings;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCreatedEventConsumer>();
    configure.AddConsumer<StockRollbackMessageConsumer>();

    configure.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(builder.Configuration["RabbitMQ"]);

        configurator.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue,
                                     e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        configurator.ReceiveEndpoint(RabbitMQSettings.Stock_RollbackMessageQueue,
                                     e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));

    });
});

builder.Services.AddSingleton<MongoDBService>();


using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDBService mongoDbService = scope.ServiceProvider.GetRequiredService<MongoDBService>();
if (!mongoDbService.GetCollection<Stock.API.Models.Stock>().FindSync(x => true).Any())
{
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 21,
        Count = 200
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 22,
        Count = 100
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 23,
        Count = 50
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 24,
        Count = 10
    });
    mongoDbService.GetCollection<Stock.API.Models.Stock>().InsertOne(new()
    {
        ProductId = 25,
        Count = 30
    });
}



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
