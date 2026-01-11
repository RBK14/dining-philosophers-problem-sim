using System.Diagnostics;

namespace DiningPhilosophersProblemSim.Model.Entities
{
    public class Philosopher
    {
        public int Id { get; }

        public long TotalWaitTimeMs { get; private set; } = 0;
        public int MealsEaten { get; private set; } = 0;

        private readonly ISimulationLogger _logger;
        private readonly IEatingStrategy _strategy;

        private readonly int _minThink;
        private readonly int _maxThink;
        private readonly int _minEat;
        private readonly int _maxEat;

        private readonly Random _rnd;

        public Philosopher(int id, ISimulationLogger logger, IEatingStrategy strategy, SimulationConfig config)
        {
            Id = id;
            _logger = logger;
            _strategy = strategy;

            _minThink = config.MinThinkTimeMs;
            _maxThink = config.MaxThinkTimeMs;
            _minEat = config.MinEatTimeMs;
            _maxEat = config.MaxEatTimeMs;

            _rnd = new Random(id + (int)DateTime.Now.Ticks);
        }

        public async Task RunCycleAsync(Fork leftFork, Fork rightFork, CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {
                try
                {
                    int thinkTime = _rnd.Next(_minThink, _maxThink);

                    _logger.UpdateState(Id, $"Myśli ({thinkTime}ms)", ConsoleColor.Cyan);
                    await Task.Delay(thinkTime, token);

                    _logger.UpdateState(Id, "Głodny (Czeka)", ConsoleColor.Yellow);
                    var stopwatch = Stopwatch.StartNew();

                    await _strategy.EatAsync(this, leftFork, rightFork,
                        onForksAcquired: () =>
                        {
                            stopwatch.Stop();
                            TotalWaitTimeMs += stopwatch.ElapsedMilliseconds;
                            MealsEaten++;
                        },
                        token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            _logger.UpdateState(Id, "Zakończył pracę", ConsoleColor.DarkGray);
        }

        public async Task PerformEatingAction(Fork left, Fork right, CancellationToken token)
        {
            int eatTime = _rnd.Next(_minEat, _maxEat + 1);
            string forksInfo = $"[Widelce: {left.Id},{right.Id}]";

            _logger.UpdateState(Id, $"Je {forksInfo} ({eatTime}ms)", ConsoleColor.Green);
            await Task.Delay(eatTime, token);

            _logger.UpdateState(Id, "Odłożył widelce", ConsoleColor.Gray);
        }
    }
}