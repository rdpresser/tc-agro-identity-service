namespace TC.Agro.Identity.Application.Abstractions.Ports
{
    public interface IUserAggregateRepository : IBaseRepository<UserAggregate>
    {
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    }
}
