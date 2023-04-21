using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service;
using SagaStateMachine.Service.Models.Context;
using SagaStateMachine.Service.StateInstances;
using SagaStateMachine.Service.StateMachines;
using Shared.Settings;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMassTransit(configurator =>
        {
            configurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
                 .EntityFrameworkRepository(options =>
                 {
                     options.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
                     {
                         builder.UseSqlServer(hostContext.Configuration.GetConnectionString("SQLServer"));
                     });
                 });

            configurator.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(hostContext.Configuration["RabbitMQ"]);

                cfg.ReceiveEndpoint(RabbitMQSettings.StateMachine,
                                    e => e.ConfigureSaga<OrderStateInstance>(provider));
            }));
        });

        //services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
