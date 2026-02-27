namespace TC.Agro.Identity.Application.Abstractions.Ports
{
    public interface IUserAggregateRepository : IBaseRepository<UserAggregate>
    {
        Task<UserAggregate?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    }
}
