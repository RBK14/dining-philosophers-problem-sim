using DiningPhilosophersProblemSim.Model.Entities;

namespace DiningPhilosophersProblemSim.Model.Strategies
{
    public class HierarchyStrategy : IEatingStrategy
    {
        public async Task EatAsync(Philosopher p, Fork left, Fork right, Action onForksAcquired, CancellationToken token)
        {
            // Blokuje zawsze widelec o niższym ID
            Fork first = left.Id < right.Id ? left : right;
            Fork second = left.Id < right.Id ? right : left;

            await first.Lock.WaitAsync(token);
            try
            {
                await second.Lock.WaitAsync(token);
                try
                {
                    onForksAcquired?.Invoke();

                    await p.PerformEatingAction(left, right, token);
                }
                finally
                {
                    second.Lock.Release();
                }
            }
            finally
            {
                first.Lock.Release();
            }
        }
    }
}