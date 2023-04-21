using MassTransit;
using MongoDB.Driver;
using Shared.Messages.StockMessages;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class StockRollbackMessageConsumer : IConsumer<StockRollBackMessage>
    {
        readonly MongoDBService _mongoDbService;
        public StockRollbackMessageConsumer(MongoDBService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }
        public async Task Consume(ConsumeContext<StockRollBackMessage> context)
        {
            var collection = _mongoDbService.GetCollection<Models.Stock>();

            foreach (var item in context.Message.OrderItems)
            {
                Models.Stock stock = await (await collection.FindAsync(s => s.ProductId == item.ProductId)).FirstOrDefaultAsync();
                if (stock != null)
                {
                    stock.Count += item.Count;
                    await collection.FindOneAndReplaceAsync(s => s.ProductId == item.ProductId, stock);
                }
            }
        }
    }
}
