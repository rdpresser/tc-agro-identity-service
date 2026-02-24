namespace TC.Agro.Identity.Application.UseCases.ReSyncUsers
{
    public interface IReSyncUsersUseCase
    {
        Task<Result<ReSyncUsersResponse>> ExecuteAsync(CancellationToken ct = default);
    }
}
