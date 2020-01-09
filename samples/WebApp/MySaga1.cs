using System.Collections.Generic;
using System.Threading.Tasks;
using Foundatio.Lock;
using Microsoft.Extensions.Logging;
using SoftwarePioniere.Domain;
using SoftwarePioniere.FakeDomain;
using SoftwarePioniere.Messaging;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable NotAccessedField.Local

namespace WebApp
{
    public class MySaga1 : SagaBase2
        , IHandleMessage<FakeCommand>
        , IHandleAggregateEvent<FakeAggregate, FakeEvent>

    {
        private readonly IPersistentSubscriptionFactory _persistentSubscriptionFactory;

        public MySaga1(ILoggerFactory loggerFactory, ISagaServices services) : base(loggerFactory, services)
        {
            _persistentSubscriptionFactory = services.PersistentSubscriptionFactory;
        }

        public const string AngelegtGroup =
            "Logistik1";


        public const string AngelegtStream =
            "$projections-Logistik_TourDefinitionZeit_Angelegt-result";


        protected override async Task RegisterMessagesAsync()
        {
            await Bus.SubscribeCommand<FakeCommand>(HandleAsync, CancellationToken);
            await Bus.SubscribeAggregateDomainEvent<FakeAggregate, FakeEvent>(HandleAsync, CancellationToken);


            //await _persistentSubscriptionFactory.CreateAdapter<TourDefintionZeitAngelegt>().ConnectToPersistentSubscription(AngelegtStream,
            //    AngelegtGroup,
            //    Logger,
            //    TourDefinitionZeitAngelegtHandler);
        }

        private string LockId(FakeEvent arg1, AggregateTypeInfo<FakeAggregate> arg2)
        {
            return arg2.AggregateId;
        }

        private string LockId(FakeCommand arg)
        {
            return arg.ObjectId;
        }


        public async Task HandleAsync(FakeCommand message)
        {
            Logger.LogInformation("Handle FakeCommand {ObjectId}", message.ObjectId);
            
            var tasksToRun = new List<Task>();

            await LockProvider.TryUsingAsync(message.ObjectId, async token =>
                {
                    var ex = await Repository.CheckAggregateExists<FakeAggregate>(message.ObjectId);

                    FakeAggregate agg;

                    if (!ex)
                    {
                        agg = FakeAggregate.Factory.Create(message.ObjectId);
                    }
                    else
                    {
                        agg = await Repository.GetByIdAsync<FakeAggregate>(message.ObjectId);
                    }

                    agg.DoFakeEvent(message.Text);
                    var evnts = await Repository.SaveAsyncWithOutPush(agg, CancellationToken);

                    foreach (var evnt in evnts)
                    {
                        tasksToRun.Add(Bus.PublishAsync(evnt));
                    }
                    //await Repository.SaveAsync(agg, CancellationToken);
                },
            cancellationToken: CancellationToken);

            await Task.WhenAll(tasksToRun);
        }


        public async Task HandleAsync(FakeEvent message, AggregateTypeInfo<FakeAggregate> info)
        {
            Logger.LogInformation("Handle FakeEvent {AggregateId}", message.AggregateId);


            var tasksToRun = new List<Task>();

            await LockProvider.TryUsingAsync(message.AggregateId,
                async token =>
                {
                    var agg = await Repository.GetByIdAsync<FakeAggregate>(message.AggregateId);
                    agg.DoFakeEvent2("zweite runde 2");
                    //await Repository.SaveAsync(agg, CancellationToken);
                    var evnts = await Repository.SaveAsyncWithOutPush(agg, CancellationToken);

                    foreach (var evnt in evnts)
                    {
                        tasksToRun.Add(Bus.PublishAsync(evnt));
                    }
                },
            cancellationToken: CancellationToken);

            await Task.WhenAll(tasksToRun);
        }
    }
}
