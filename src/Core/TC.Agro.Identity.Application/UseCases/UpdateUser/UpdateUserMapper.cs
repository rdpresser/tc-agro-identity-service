namespace TC.Agro.Identity.Application.UseCases.UpdateUser
{
    public static class UpdateUserMapper
    {
        /// <summary>
        /// Maps the domain event into an integration event for external systems.
        /// </summary>
        public static UserUpdatedIntegrationEvent ToIntegrationEvent(UserAggregate.UserUpdatedDomainEvent domainEvent)
            => new(
                domainEvent.AggregateId,
                domainEvent.Name,
                domainEvent.Email,
                domainEvent.Username,
                domainEvent.OccurredOn
            );

        /// <summary>
        /// Maps aggregate state into a response DTO.
        /// </summary>
        public static UpdateUserResponse FromAggregate(UserAggregate aggregate)
            => new(
                Id: aggregate.Id,
                Name: aggregate.Name,
                Email: aggregate.Email,
                Username: aggregate.Username,
                Role: aggregate.Role
            );
    }
}
