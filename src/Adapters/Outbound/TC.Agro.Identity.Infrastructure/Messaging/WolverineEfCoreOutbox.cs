using TC.Agro.SharedKernel.Infrastructure.Messaging.Outbox;

namespace TC.Agro.Identity.Infrastructure.Messaging;

/// <summary>
/// Identity service specific Outbox binding.
/// Uses SharedKernel's generic WolverineEfCoreOutbox with ApplicationDbContext.
/// </summary>
public sealed class IdentityOutbox : WolverineEfCoreOutbox<ApplicationDbContext>
{
    public IdentityOutbox(IDbContextOutbox<ApplicationDbContext> outbox) : base(outbox) { }
}
