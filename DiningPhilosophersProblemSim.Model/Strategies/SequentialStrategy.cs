using DiningPhilosophersProblemSim.Model.Entities;

namespace DiningPhilosophersProblemSim.Model.Strategies
{
    public class SequentialStrategy : IEatingStrategy
    {
        private readonly SemaphoreSlim _globalLock;

        public SequentialStrategy(SemaphoreSlim globalLock)
        {
            _globalLock = globalLock;
        }

        public async Task EatAsync(Philosopher p, Fork left, Fork right, Action onForksAcquired, CancellationToken token)
        {
            await _globalLock.WaitAsync(token);
            try
            {
                await left.Lock.WaitAsync(token);
                await right.Lock.WaitAsync(token);

                try
                {
                    onForksAcquired?.Invoke();

                    await p.PerformEatingAction(left, right, token);
                }
                finally
                {
                    right.Lock.Release();
                    left.Lock.Release();
                }
            }
            finally
            {
                _globalLock.Release();
            }
        }
    }
}