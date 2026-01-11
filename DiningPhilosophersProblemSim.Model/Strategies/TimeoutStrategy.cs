using DiningPhilosophersProblemSim.Model.Entities;

namespace DiningPhilosophersProblemSim.Model.Strategies
{
    public class TimeoutStrategy : IEatingStrategy
    {
        public async Task EatAsync(Philosopher p, Fork left, Fork right, Action onForksAcquired, CancellationToken token)
        {
            var rnd = new Random();

            while (!token.IsCancellationRequested)
            {
                bool tookLeft = await left.Lock.WaitAsync(200, token);

                if (tookLeft)
                {
                    try
                    {
                        bool tookRight = await right.Lock.WaitAsync(0, token);

                        if (tookRight)
                        {
                            try
                            {
                                onForksAcquired?.Invoke();
                                await p.PerformEatingAction(left, right, token);
                                return;
                            }
                            finally
                            {
                                right.Lock.Release();
                            }
                        }
                    }
                    finally
                    {
                        left.Lock.Release();
                    }
                }

                int backoffDelay = rnd.Next(400, 800);
                await Task.Delay(backoffDelay, token);
            }
        }
    }
}