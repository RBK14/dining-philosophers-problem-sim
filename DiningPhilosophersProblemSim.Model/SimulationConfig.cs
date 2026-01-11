namespace DiningPhilosophersProblemSim.Model;
public class SimulationConfig
{
    public int PhilosophersCount { get; set; } = 5;
    public int ForksCount { get; set; } = 5;
    public int MinThinkTimeMs { get; set; } = 800;
    public int MaxThinkTimeMs { get; set; } = 1200;
    public int MinEatTimeMs { get; set; } = 800;
    public int MaxEatTimeMs { get; set; } = 1200;
    public string StrategyType { get; set; } = "Hierarchy"; // "Hierarchy", "Arbitrator", Timeout, "Sequential", "Naive"
    public int SimulationTimeSeconds { get; set; } = 0;
    public bool WriteResults { get; set; } = false;
}
