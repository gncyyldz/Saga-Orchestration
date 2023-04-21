using MassTransit;

namespace Shared.Events.PaymentEvents
{
    public class PaymentCompletedEvent : CorrelatedBy<Guid>
    {
        public PaymentCompletedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; }
    }
}
