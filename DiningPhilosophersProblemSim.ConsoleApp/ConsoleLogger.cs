using DiningPhilosophersProblemSim.Model;

namespace DiningPhilosophersProblemSim.ConsoleApp
{
    public class ConsoleLogger : ISimulationLogger
    {
        private readonly object _lock = new object();
        private readonly int _topOffset;

        public ConsoleLogger(int topOffset = 2)
        {
            _topOffset = topOffset;
        }

        public void Log(string message)
        {
        }

        public void UpdateState(int philosopherId, string state, ConsoleColor color)
        {
            lock (_lock)
            {
                Console.SetCursorPosition(0, _topOffset + philosopherId);
                Console.Write($"Filozof {philosopherId}: ".PadRight(12));

                Console.Write(new string(' ', 40));
                Console.SetCursorPosition(12, _topOffset + philosopherId);

                Console.ForegroundColor = color;
                Console.Write(state);
                Console.ResetColor();
            }
        }
    }
}