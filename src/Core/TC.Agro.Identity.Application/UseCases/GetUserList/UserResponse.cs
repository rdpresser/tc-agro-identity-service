namespace TC.Agro.Identity.Application.UseCases.GetUserList
{
    public sealed class UserResponse
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Username { get; init; }
        public required string Email { get; init; }
        public required string Role { get; init; }
        public required bool IsActive { get; init; }
    }
}
