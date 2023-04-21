using MassTransit;

namespace Shared.Events.PaymentEvents
{
    public class PaymentStartedEvent : CorrelatedBy<Guid>
    {
        public PaymentStartedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
        public decimal TotalPrice { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
