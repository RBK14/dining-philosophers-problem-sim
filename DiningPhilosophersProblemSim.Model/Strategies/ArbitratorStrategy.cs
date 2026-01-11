using DiningPhilosophersProblemSim.Model.Entities;

namespace DiningPhilosophersProblemSim.Model.Strategies
{
    public class ArbitratorStrategy : IEatingStrategy
    {
        private readonly SemaphoreSlim _waiter;

        public ArbitratorStrategy(SemaphoreSlim sharedWaiter)
        {
            _waiter = sharedWaiter;
        }

        public async Task EatAsync(Philosopher p, Fork left, Fork right, Action onForksAcquired, CancellationToken token)
        {
            await _waiter.WaitAsync(token);
            try
            {
                await left.Lock.WaitAsync(token);
                try
                {
                    await right.Lock.WaitAsync(token);
                    try
                    {
                        onForksAcquired?.Invoke();
                        await p.PerformEatingAction(left, right, token);
                    }
                    finally
                    {
                        right.Lock.Release();
                    }
                }
                finally
                {
                    left.Lock.Release();
                }
            }
            finally
            {
                _waiter.Release();
            }
        }
    }
}