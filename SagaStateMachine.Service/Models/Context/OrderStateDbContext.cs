using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.StateMaps;

namespace SagaStateMachine.Service.Models.Context
{
    public class OrderStateDbContext : SagaDbContext
    {
        public OrderStateDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// ISagaClassMap : Order State Instance'da ki property'lerin
        /// validasyon ayarlarının yapılmasını sağlar.
        /// </summary>
        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get
            {
                yield return new OrderStateMap();
            }
        }
    }
}
