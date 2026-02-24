using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Ardalis.Result;
using FastEndpoints;
using TC.Agro.Contracts.Events.Identity;
using TC.Agro.Identity.Application.Abstractions.Ports;
using TC.Agro.Identity.Application.UseCases.ReSyncUsers;
using TC.Agro.SharedKernel.Infrastructure.Messaging;
using TC.Agro.SharedKernel.Infrastructure.UserClaims;
using Wolverine;

namespace TC.Agro.Identity.Tests.Application.UseCases.ReSyncUsers
{
    public sealed class ReSyncUsersCommandHandlerTests
    {
        static ReSyncUsersCommandHandlerTests()
        {
            var services = new ServiceCollection();
            Factory.AddServicesForUnitTesting(services);
        }

        public ReSyncUsersCommandHandlerTests()
        {
            Factory.RegisterTestServices(_ => { });
        }

        private readonly IUserReadStore _userReadStore = A.Fake<IUserReadStore>();
        private readonly IUserContext _userContext = A.Fake<IUserContext>();
        private readonly IMessageBus _messageBus = A.Fake<IMessageBus>();
        private readonly ILogger<ReSyncUsersCommandHandler> _logger = A.Fake<ILogger<ReSyncUsersCommandHandler>>();

        [Fact]
        public async Task ExecuteAsync_WhenUserIsNotAdmin_ShouldReturnUnauthorized()
        {
            A.CallTo(() => _userContext.IsAdmin).Returns(false);

            var handler = CreateHandler();

            var result = await handler.ExecuteAsync(CancellationToken.None);

            result.Status.ShouldBe(ResultStatus.Unauthorized);

            A.CallTo(() => _userReadStore.GetActiveUsersForReSyncAsync(A<CancellationToken>._))
                .MustNotHaveHappened();
            A.CallTo(() => _messageBus.PublishAsync(A<EventContext<UserCreatedIntegrationEvent>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_WhenNoActiveUsers_ShouldReturnSuccessWithZeroAndNotPublish()
        {
            A.CallTo(() => _userContext.IsAdmin).Returns(true);
            A.CallTo(() => _userContext.Id).Returns(Guid.NewGuid());
            A.CallTo(() => _userReadStore.GetActiveUsersForReSyncAsync(A<CancellationToken>._))
                .Returns(Array.Empty<ActiveUserReadModel>());

            var handler = CreateHandler();

            var result = await handler.ExecuteAsync(CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.TotalActiveUsers.ShouldBe(0);
            result.Value.RepublishedUsers.ShouldBe(0);

            A.CallTo(() => _messageBus.PublishAsync(A<EventContext<UserCreatedIntegrationEvent>>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_WhenAdminAndActiveUsersExist_ShouldPublishAllUsers()
        {
            var adminId = Guid.NewGuid();
            var activeUsers = new[]
            {
                new ActiveUserReadModel(Guid.NewGuid(), "John Smith", "john.smith@gmail.com", "john.smith", "Admin"),
                new ActiveUserReadModel(Guid.NewGuid(), "Jane Doe", "jane.doe@gmail.com", "jane.doe", "Producer")
            };

            A.CallTo(() => _userContext.IsAdmin).Returns(true);
            A.CallTo(() => _userContext.IsAuthenticated).Returns(true);
            A.CallTo(() => _userContext.Id).Returns(adminId);
            A.CallTo(() => _userContext.CorrelationId).Returns("corr-id-123");
            A.CallTo(() => _userReadStore.GetActiveUsersForReSyncAsync(A<CancellationToken>._))
                .Returns(activeUsers);
            var publishedEvents = new List<EventContext<UserCreatedIntegrationEvent>>();
            A.CallTo(() => _messageBus.PublishAsync(A<EventContext<UserCreatedIntegrationEvent>>._))
                .Invokes(call =>
                {
                    var publishedEvent = call.GetArgument<EventContext<UserCreatedIntegrationEvent>>(0);
                    publishedEvent.ShouldNotBeNull();
                    publishedEvents.Add(publishedEvent!);
                });

            var handler = CreateHandler();

            var result = await handler.ExecuteAsync(CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.TotalActiveUsers.ShouldBe(2);
            result.Value.RepublishedUsers.ShouldBe(2);

            A.CallTo(() => _messageBus.PublishAsync(A<EventContext<UserCreatedIntegrationEvent>>._))
                .MustHaveHappenedTwiceExactly();

            publishedEvents.Count.ShouldBe(2);

            publishedEvents[0].EventData.OwnerId.ShouldBe(activeUsers[0].Id);
            publishedEvents[0].EventData.Email.ShouldBe(activeUsers[0].Email);
            publishedEvents[0].EventData.Role.ShouldBe(activeUsers[0].Role);
            publishedEvents[0].UserId.ShouldBe(adminId.ToString());
            publishedEvents[0].CorrelationId.ShouldBe("corr-id-123");
            var source = publishedEvents[0].Source;
            source.ShouldNotBeNull();
            source!.ShouldContain(nameof(ReSyncUsersCommandHandler));

            publishedEvents[1].EventData.OwnerId.ShouldBe(activeUsers[1].Id);
            publishedEvents[1].EventData.Email.ShouldBe(activeUsers[1].Email);
            publishedEvents[1].EventData.Role.ShouldBe(activeUsers[1].Role);
        }

        private ReSyncUsersCommandHandler CreateHandler()
            => new(_userReadStore, _userContext, _messageBus, _logger);
    }
}
