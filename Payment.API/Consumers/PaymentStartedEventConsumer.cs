using MassTransit;
using Shared.Events.PaymentEvents;
using Shared.Settings;

namespace Payment.API.Consumers
{
    public class PaymentStartedEventConsumer : IConsumer<PaymentStartedEvent>
    {
        readonly ISendEndpointProvider _sendEndpointProvider;
        readonly IPublishEndpoint _publishEndpoint;
        public PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider,
            IPublishEndpoint publishEndpoint)
        {
            this._sendEndpointProvider = sendEndpointProvider;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));
            if (context.Message.TotalPrice <= 10000)
                await sendEndpoint.Send(new PaymentCompletedEvent(context.Message.CorrelationId));
            else
                await sendEndpoint.Send(new PaymentFailedEvent(context.Message.CorrelationId)
                {
                    Message = "Bakiye yetersiz!\nBune olum soygun mu yapıyon lan! İptal et siparişi kapat adamın canını sıkma",
                    OrderItems = context.Message.OrderItems
                });
        }
    }
}
