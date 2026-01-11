namespace DiningPhilosophersProblemSim.Model.Entities
{
    public class Fork
    {
        public int Id { get; }
        public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);

        public Fork(int id) => Id = id;
    }
}
