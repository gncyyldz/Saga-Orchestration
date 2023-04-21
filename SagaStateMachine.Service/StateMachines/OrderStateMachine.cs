using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared.Events.OrderEvents;
using Shared.Events.PaymentEvents;
using Shared.Events.StockEvents;
using Shared.Messages.StockMessages;
using Shared.Settings;

namespace SagaStateMachine.Service.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
    {
        public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
        public Event<StockReservedEvent> StockReservedEvent { get; set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }

        public State OrderCreated { get; set; }
        public State StockReserved { get; set; }
        public State PaymentCompleted { get; set; }
        public State PaymentFailed { get; set; }
        public State StockNotReserved { get; set; }

        public OrderStateMachine()
        {
            //State Instance'da ki hangi property'nin sipariş sürecindeki state'i tutacağı bildiriliyor.
            //Yani artık tüm event'ler CurrentState property'sin de tutulacaktır!
            InstanceState(instance => instance.CurrentState);

            //Eğer gelen event OrderStartedEvent ise CorrelateBy metodu ile veritabanında(database)
            //tutulan Order State Instance'da ki OrderId'si ile gelen event'te ki(@event) OrderId'yi
            //kıyasla. Bu kıyas neticesinde eğer ilgili instance varsa kaydetme. Yani yeni bir korelasyon
            //üretme! Yok eğer yoksa yeni bir korelasyon üret(SelectId)
            Event(() => OrderStartedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId)
                .SelectId(e => Guid.NewGuid()));

            //StockReservedEvent fırlatıldığında veritabanındaki hangi correlationid değerine sahip state
            //instance'ın state'ini değiştirecek bunu belirtmiş olduk!
            Event(() => StockReservedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            //StockNotReservedEvent fırlatıldığında veritabanındaki hangi correlationid değerine sahip state
            //instance'ın state'ini değiştirecek bunu belirtmiş olduk!
            Event(() => StockNotReservedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            //PaymentCompletedEvent fırlatıldığında veritabanındaki hangi correlationid değerine sahip state
            //instance'ın state'ini değiştirecek bunu belirtmiş olduk!
            Event(() => PaymentCompletedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            //PaymentFailedEvent fırlatıldığında veritabanındaki hangi correlationid değerine sahip state
            //instance'ın state'ini değiştirecek bunu belirtmiş olduk!
            Event(() => PaymentFailedEvent,
                orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            //İlgili instance'ın state'i initial/başlangıç aşamasındayken(Initially) 'OrderStartedEvent'
            //tetikleyici event'i geldiyse(When) şu işlemleri yap(Then). Ardından bu işlemler yapıldıktan
            //sonra ilgili instance'ı 'OrderCreated' state'ine geçir(TransitionTo). Ardından 'Stock.API'ı
            //tetikleyebilmek/haberdar edebilmek için 'OrderCreatedEvent' event'ini gönder(Publish/Send)
            Initially(When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Instance.BuyerId = context.Data.BuyerId;
                    context.Instance.OrderId = context.Data.OrderId;
                    context.Instance.TotalPrice = context.Data.TotalPrice;
                    context.Instance.CreatedDate = DateTime.Now;
                })
                .Then(context => Console.WriteLine("Ara işlem 1"))
                .Then(context => Console.WriteLine("Ara işlem 2"))
                .TransitionTo(OrderCreated)
                .Then(context => Console.WriteLine("Ara işlem 3"))
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                      context => new OrderCreatedEvent(context.Instance.CorrelationId)
                      {
                          OrderItems = context.Data.OrderItems
                      }));

            //Eğer state 'OrderCreated' ise(During) ve o anda 'StockReservedEvent' event'i geldiyse(When)
            //o zaman state'i 'StockReserved' olarak değiştir(TransitionTo) ve belirtilen kuyruğa 
            //'PaymentStartedEvent' event'ini gönder(Send)
            During(OrderCreated,
                When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"),
                      context => new PaymentStartedEvent(context.Instance.CorrelationId)
                      {
                          OrderItems = context.Data.OrderItems,
                          TotalPrice = context.Instance.TotalPrice
                      }),
                //Yok eğer State 'OrderCreated' iken(During) 'StockNotReservedEvent' event'i geldiyse(When)
                //o zaman state'i 'StockNotReserved' olarak değiştir(TransitionTo) ve belirtilen
                //kuyruğa 'OrderFailedEvent' event'ini gönder.
                When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                      context => new OrderFailedEvent()
                      {
                          OrderId = context.Instance.OrderId,
                          Message = context.Data.Message
                      }));

            //Eğer ilgili sipariş 'StockReserved' durumunda iken(During) 'PaymentCompletedEvent' event'i geldiyse(When)
            //'PaymentCompleted' state'i olarak değiştir(TransitionTo) ve ardından belirtilen kuyruğa 
            //'OrderCompletedEvent' event'ini gönder. Ayrıca artık bu sipariş başarılı olacağından dolayı
            //State Machine tarafından bu State Instance'ı başarıyla sonlandır(Finalize) Haliyle böylece sonuç olarak
            //ilgili instance'ın state'inde 'Final' yazacaktır!
            During(StockReserved,
                When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
                      context => new OrderCompletedEvent
                      {
                          OrderId = context.Instance.OrderId
                      })
                .Finalize(),
                //Yok eğer mevcut state 'StockReserved' iken(During) 'PaymentFailedEvent' event'i gelirse(When)
                //o zaman state'i 'PaymentFailed' olarak değiştir(TransitionTo) ve belirtilen kuyruklara 
                //'OrderFailedEvent' ve 'StockRollBackMessage' event'lerini gönder(Send).
                When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                      context => new OrderFailedEvent()
                      {
                          OrderId = context.Instance.OrderId,
                          Message = context.Data.Message
                      })
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollbackMessageQueue}"),
                      context => new StockRollBackMessage
                      {
                          OrderItems = context.Data.OrderItems
                      }));

            //Finalize olan instance'ları veritabanından kaldırıyoruz!
            SetCompletedWhenFinalized();
        }
    }
}
