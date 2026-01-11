namespace DiningPhilosophersProblemSim.Model
{
    public interface ISimulationLogger
    {
        void Log(string message);
        void UpdateState(int philosopherId, string state, ConsoleColor color);
    }
}
