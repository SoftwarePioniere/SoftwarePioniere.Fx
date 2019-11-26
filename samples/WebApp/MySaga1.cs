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


        public async Task HandleAsync(FakeCommand message)
        {
            await LockProvider.TryUsingAsync(message.ObjectId,
                async token =>
                {
                    var ex = await Repository.CheckAggregateExists<FakeAggregate>(message.ObjectId, token);

                    if (!ex)
                    {
                        var agg = FakeAggregate.Factory.Create(message.ObjectId);
                        agg.DoFakeEvent(message.Text);
                        await Repository.SaveAsync(agg, CancellationToken);
                    }
                    else
                    {
                        var agg = await Repository.GetByIdAsync<FakeAggregate>(message.ObjectId, token);
                        agg.DoFakeEvent(message.Text);
                        await Repository.SaveAsync(agg, CancellationToken);
                    }
                },
                cancellationToken: CancellationToken);
        }

     
        private Task TourDefinitionZeitAngelegtHandler(TourDefintionZeitAngelegt model      
           )
        {
            //var message = JsonConvert.DeserializeObject<TourDefinitionZeitAngelegtEvent>(model.TourDefinitionZeitAngelegtEvent);
            //  await TourenAnlegenAsync(message, state);

            Logger.LogInformation(model.TourDefinitionId);

            return Task.CompletedTask;
        }


        public class TourDefintionZeitAngelegt
        {
            public string TourDefinitionId { get; set; }
            public string TourDefinitionZeitId { get; set; }
            public string TourDefinitionZeitAngelegtEvent { get; set; }

        }

        public async Task HandleAsync(FakeEvent message, AggregateTypeInfo<FakeAggregate> info)
        {
            await LockProvider.TryUsingAsync(message.AggregateId,
                async token =>
                {
                    var agg = await Repository.GetByIdAsync<FakeAggregate>(message.AggregateId, token);
                    agg.DoFakeEvent2("zweite runde 2");
                    await Repository.SaveAsync(agg, CancellationToken);
                },
                cancellationToken: CancellationToken);
        }
    }
}
