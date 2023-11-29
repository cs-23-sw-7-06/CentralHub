using CentralHub.Api.Threading;

namespace CentralHub.Api.Tests;

public sealed class CancellableMutexTests
{
    private CancellableMutex<bool> _cancellableMutex;

    [SetUp]
    public void Setup()
    {
        _cancellableMutex = new CancellableMutex<bool>(false);
    }

    [TearDown]
    public void Teardown()
    {
        _cancellableMutex.Dispose();
    }

    [Test]
    public async Task MutuallyExclusive()
    {
        await _cancellableMutex.Lock(_ =>
        {
            var tokenSource = new CancellationTokenSource();
            var task = Task.Run(() =>
            {
                // This task tries to lock but we have already locked in the first line of this function.
                // So after 100 milliseconds in the cancelTask we cancel and we should expect to get an OperationCanceledException.
                Assert.Throws<OperationCanceledException>(() => _cancellableMutex.Lock(_ => { }, tokenSource.Token).GetAwaiter().GetResult());
            }, default);

            var cancelTask = Task.Run(async () =>
            {
                // This makes the test always at least take 100 milliseconds to complete.
                // Too bad :)
                await Task.Delay(100, default);
                tokenSource.Cancel();
            }, default);

            Task.WaitAll(task, cancelTask);
        }, default);
    }
}