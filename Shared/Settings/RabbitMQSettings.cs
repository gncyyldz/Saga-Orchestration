using Shared.Settings;

namespace Shared.Settings
{
    public struct RabbitMQSettings
    {
        private const string suffix = "queue";

        public const string StateMachine = $"state-machine-{suffix}";
        public const string Stock_OrderCreatedEventQueue = $"stock-order-created-{suffix}";
        public const string Payment_StartedEventQueue = $"payment-started-{suffix}";
        public const string Order_OrderCompletedEventQueue = $"order-order-completed-{suffix}";
        public const string Order_OrderFailedEventQueue = $"order-order-failed-{suffix}";
        public const string Stock_RollbackMessageQueue = $"stock-roolback-{suffix}";
    }
}