namespace TC.Agro.Identity.Application.Abstractions.Ports
{
    public sealed record ActiveUserReadModel(
        Guid Id,
        string Name,
        string Email,
        string Username,
        string Role);
}
