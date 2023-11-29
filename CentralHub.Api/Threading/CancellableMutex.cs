namespace CentralHub.Api.Threading;

public sealed class CancellableMutex<T> : IDisposable
{
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    private readonly T _innerObject;

    public CancellableMutex(T innerObject)
    {
        _innerObject = innerObject;
    }

    public async Task Lock(Action<T> user, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            user(_innerObject);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<TResult> Lock<TResult>(Func<T, TResult> user, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return user(_innerObject);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task Lock(Func<T, Task> user, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            await user(_innerObject);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<TResult> Lock<TResult>(Func<T, Task<TResult>> user, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await user(_innerObject);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public void Dispose()
    {
        _semaphoreSlim.Dispose();
        if (_innerObject is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}