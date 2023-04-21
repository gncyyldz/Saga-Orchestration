using MassTransit;
using Order.API.Enums;
using Order.API.Models.Context;
using Shared.Events.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderFailedEventConsumer : IConsumer<OrderFailedEvent>
    {
        readonly ApplicationDbContext _context;
        public OrderFailedEventConsumer(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            Models.Order order = await _context.FindAsync<Models.Order>(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatus = OrderStatus.Fail;
                await _context.SaveChangesAsync();
                Console.WriteLine(context.Message.Message);
            }
        }
    }
}
