namespace Shared.Messages.StockMessages
{
    public class StockRollBackMessage
    {
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
