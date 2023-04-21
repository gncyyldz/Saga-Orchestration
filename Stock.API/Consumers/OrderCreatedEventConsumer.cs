using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events.OrderEvents;
using Shared.Events.StockEvents;
using Shared.Settings;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        readonly MongoDBService _mongoDbService;
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(
            MongoDBService mongoDbService,
            ISendEndpointProvider sendEndpointProvider,
            IPublishEndpoint publishEndpoint)
        {
            _mongoDbService = mongoDbService;
            _sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            IMongoCollection<Models.Stock> collection = _mongoDbService.GetCollection<Models.Stock>();

            //Sipariş edilen ürünlerin stok miktarı sipariş adedinden fazla mı? değil mi?
            foreach (OrderItemMessage orderItem in context.Message.OrderItems)
                stockResult.Add((await collection.FindAsync(s => s.ProductId == orderItem.ProductId && s.Count > orderItem.Count)).Any());

            //Eğer fazlaysa sipariş edilen ürünlerin stok miktarı güncelleniyor.
            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));
            if (stockResult.TrueForAll(sr => sr.Equals(true)))
            {
                foreach (OrderItemMessage orderItem in context.Message.OrderItems)
                {
                    Models.Stock stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    stock.Count -= orderItem.Count;
                    await collection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
                }

                StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems
                };
                await sendEndpoint.Send(stockReservedEvent);
            }
            //Eğer az ise siparişin iptal edilmesi için gerekli event gönderiliyor.
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                {
                    Message = "Stok yetersiz..."
                };

                await sendEndpoint.Send(stockNotReservedEvent);
            }
        }
    }
}
