namespace TC.Agro.Identity.Infrastructure.Messaging;

/// <summary>
/// Wolverine-based implementation of ITransactionalOutbox.
/// Provides atomic EF Core persistence and message publishing via the Outbox Pattern.
/// </summary>
public sealed class WolverineEfCoreOutbox : ITransactionalOutbox
{
    private readonly IDbContextOutbox<ApplicationDbContext> _outbox;

    public WolverineEfCoreOutbox(IDbContextOutbox<ApplicationDbContext> outbox)
    {
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
    }

    /// <inheritdoc />
    public ValueTask EnqueueAsync<T>(T message, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _outbox.PublishAsync(message);
    }

    /// <inheritdoc />
    public ValueTask EnqueueAsync<T>(IReadOnlyCollection<T> messages, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _outbox.PublishAsync(messages.ToArray());
    }

    /// <inheritdoc />
    /// <summary>
    /// Commits EF Core changes and flushes Wolverine outbox messages in a single transaction.
    /// This ensures atomicity between data persistence and message publishing.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        await _outbox.SaveChangesAndFlushMessagesAsync(ct).ConfigureAwait(false);

        // Return 1 to indicate success (Wolverine doesn't expose the actual count)
        return 1;
    }
}
