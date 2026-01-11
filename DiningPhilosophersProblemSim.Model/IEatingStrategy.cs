using DiningPhilosophersProblemSim.Model.Entities;

namespace DiningPhilosophersProblemSim.Model
{
    public interface IEatingStrategy
    {
        Task EatAsync(Philosopher philosopher, Fork left, Fork right, Action onForksAcquired, CancellationToken token);
    }
}
