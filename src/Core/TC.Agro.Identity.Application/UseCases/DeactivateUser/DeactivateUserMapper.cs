namespace TC.Agro.Identity.Application.UseCases.DeactivateUser
{
    internal static class DeactivateUserMapper
    {
        public static UserDeactivatedIntegrationEvent ToIntegrationEvent(UserDeactivatedDomainEvent domainEvent, UserAggregate aggregate)
        {
            return new UserDeactivatedIntegrationEvent(
                OwnerId: aggregate.Id,
                OccurredOn: domainEvent.OccurredOn);
        }
    }
}
