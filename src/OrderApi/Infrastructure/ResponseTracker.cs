using System.Collections.Concurrent;
using System.Threading.Channels;

namespace OrderApi.Infrastructure;

public class ResponseTracker
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Guid>> _pendingRequests = new();
    private readonly Channel<Guid> _timeoutChannel = Channel.CreateUnbounded<Guid>();
    
    public void RegisterRequest(Guid correlationId, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<Guid>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pendingRequests.TryAdd(correlationId, tcs))
        {
            throw new InvalidOperationException("Duplicate correlation ID");
        }
        // Обработка таймаутов в фоне
        Task.Delay(timeout).ContinueWith(_ =>
        {
            if (_pendingRequests.TryRemove(correlationId, out var removedTcs))
            {
                removedTcs.TrySetCanceled();
                _timeoutChannel.Writer.TryWrite(correlationId);
            }
        });
    }

    public bool TryCompleteRequest(Guid correlationId, Guid orderId)
    {
        if (!_pendingRequests.TryRemove(correlationId, out var tcs))
            return false;

        return tcs.TrySetResult(orderId);
    }

    public Task<Guid> WaitForResponseAsync(Guid correlationId) =>
        _pendingRequests.TryGetValue(correlationId, out var tcs)
            ? tcs.Task
            : Task.FromCanceled<Guid>(new CancellationToken(true));
}