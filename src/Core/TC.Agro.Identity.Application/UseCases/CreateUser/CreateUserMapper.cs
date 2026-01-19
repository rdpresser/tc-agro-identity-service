using Ardalis.Result;
using TC.Agro.Contracts.Events.Identity;
using TC.Agro.Identity.Domain.Aggregates;
using static TC.Agro.Identity.Domain.Aggregates.UserAggregate;

namespace TC.Agro.Identity.Application.UseCases.CreateUser
{
    public static class CreateUserMapper
    {
        public static Result<UserAggregate> ToAggregate(
            CreateUserCommand r)
        {
            return Create(
                r.Name,
                r.Email,
                r.Username,
                r.Password,
                r.Role);
        }

        public static CreateUserResponse FromAggregate(UserAggregate e)
        {
            return new CreateUserResponse
            (
                Id: e.Id,
                Name: e.Name,
                Username: e.Username,
                Email: e.Email,
                Role: e.Role
            );
        }

        public static UserCreatedIntegrationEvent ToIntegrationEvent(UserCreatedDomainEvent domainEvent)
        => new(
            domainEvent.AggregateId,
            domainEvent.Name,
            domainEvent.Email,
            domainEvent.Username,
            domainEvent.Role,
            domainEvent.OccurredOn
        );
    }
}
