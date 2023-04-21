using MassTransit;

namespace SagaStateMachine.Service.StateInstances
{
    /// <summary>
    /// OrderStateInstance : Her bir sipariş eklendiğinde(tetikleyici event geldiğinde)
    /// bu siparişe karşılık Saga State Machine'de tutulacak olan satırı Order State Instance
    /// olarak tarif etmekteyiz.
    /// </summary>
    public class OrderStateInstance : SagaStateMachineInstance
    {
        /// <summary>
        /// Her bir State Instance özünde bir siparişe özeldir. Haliyle bu State Instance'ları
        /// birbirinden ayırabilmek için CorrelationId(yani bildiğiniz unique id) kullanılmaktadır
        /// </summary>
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public int OrderId { get; set; }
        public int BuyerId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
