using TC.Agro.Identity.Application.Abstractions.Messaging;
using Wolverine.EntityFrameworkCore;

namespace TC.Agro.Identity.Infrastructure.Messaging
{
    public sealed class WolverineEfCoreOutbox : IIntegrationEventOutbox
    {
        private readonly IDbContextOutbox<ApplicationDbContext> _outbox;

        public WolverineEfCoreOutbox(IDbContextOutbox<ApplicationDbContext> outbox)
        {
            _outbox = outbox;
        }

        public ValueTask EnqueueAsync<T>(T message, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return _outbox.PublishAsync(message);
        }

        public ValueTask EnqueueAsync<T>(IReadOnlyCollection<T> messages, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return _outbox.PublishAsync(messages.ToArray());
        }
    }
}
