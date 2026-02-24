namespace TC.Agro.Identity.Application.UseCases.ReSyncUsers
{
    public sealed record ReSyncUsersResponse(
        int TotalActiveUsers,
        int RepublishedUsers,
        DateTimeOffset ExecutedAtUtc);
}
