using DiningPhilosophersProblemSim.Model.Entities;

namespace DiningPhilosophersProblemSim.Model.Strategies
{
    public class NaiveStrategy : IEatingStrategy
    {
        public async Task EatAsync(Philosopher p, Fork left, Fork right, Action onForksAcquired, CancellationToken token)
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
    }
}
