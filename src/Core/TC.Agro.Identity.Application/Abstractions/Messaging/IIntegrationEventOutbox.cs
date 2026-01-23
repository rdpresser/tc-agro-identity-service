namespace TC.Agro.Identity.Application.Abstractions.Messaging
{
    public interface IIntegrationEventOutbox
    {
        ValueTask EnqueueAsync<T>(T message, CancellationToken ct = default);
        ValueTask EnqueueAsync<T>(IReadOnlyCollection<T> messages, CancellationToken ct = default);
    }
}
