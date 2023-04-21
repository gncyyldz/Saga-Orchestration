using MassTransit;
using Order.API.Enums;
using Order.API.Models.Context;
using Shared.Events.OrderEvents;

namespace Order.API.Consumers
{
    public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
    {
        readonly ApplicationDbContext _applicationDbContext;
        public OrderCompletedEventConsumer(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            Models.Order order = await _applicationDbContext.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.OrderStatus = OrderStatus.Completed;
                await _applicationDbContext.SaveChangesAsync();
            }
        }
    }
}
