using DiningPhilosophersProblemSim.Model;
using DiningPhilosophersProblemSim.Model.Entities;
using DiningPhilosophersProblemSim.Model.Strategies;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace DiningPhilosophersProblemSim.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = LoadConfig();

            Console.WriteLine($"Liczba filozofów (Wątki): {config.PhilosophersCount}");
            Console.WriteLine($"Liczba widelców (Zasoby): {config.ForksCount}");
            Console.WriteLine($"Strategia:                {config.StrategyType}");
            Console.WriteLine($"Czas myślenia:            {config.MinThinkTimeMs} - {config.MaxThinkTimeMs} ms");
            Console.WriteLine($"Czas jedzenia:            {config.MinEatTimeMs} - {config.MaxEatTimeMs} ms");

            string simulationTimeText = config.SimulationTimeSeconds > 0
                ? $"{config.SimulationTimeSeconds} sekund"
                : "Nieograniczony (Ciągła)";
            Console.WriteLine($"Czas trwania:             {simulationTimeText}");
            Console.WriteLine($"Zapisz wyniki:            {config.WriteResults}");

            Console.WriteLine("\nNaciśnij [ENTER], aby zakończyć symulację i zobaczyć raport.");

            int startRowForSimulation = Console.CursorTop + 1;
            var logger = new ConsoleLogger(topOffset: startRowForSimulation);

            var forks = new List<Fork>();
            for (int i = 0; i < config.ForksCount; i++)
                forks.Add(new Fork(i));

            IEatingStrategy baseStrategy = null!;
            SemaphoreSlim sharedSemaphore;

            if (config.StrategyType == "Arbitrator")
            {
                int maxDiners = Math.Max(1, config.ForksCount - 1);
                sharedSemaphore = new SemaphoreSlim(maxDiners, maxDiners);
                baseStrategy = new ArbitratorStrategy(sharedSemaphore);
            }
            else if (config.StrategyType == "Sequential")
            {
                sharedSemaphore = new SemaphoreSlim(1, 1);
                baseStrategy = new SequentialStrategy(sharedSemaphore);
            }

            var philosophers = new List<Philosopher>();
            var tasks = new List<Task>();
            var cts = new CancellationTokenSource();

            for (int i = 0; i < config.PhilosophersCount; i++)
            {
                var leftForkIndex = i % config.ForksCount;
                var rightForkIndex = (i + 1) % config.ForksCount;

                if (leftForkIndex == rightForkIndex)
                {
                    Console.WriteLine($"[OSTRZEŻENIE]: Filozof {i} pominięty (brak pary widelców).");
                    continue;
                }

                var leftFork = forks[leftForkIndex];
                var rightFork = forks[rightForkIndex];

                IEatingStrategy strategyToUse;

                if (config.StrategyType == "Arbitrator" || config.StrategyType == "Sequential")
                {
                    strategyToUse = baseStrategy;
                }
                else if (config.StrategyType == "Timeout")
                {
                    strategyToUse = new TimeoutStrategy();
                }
                else if (config.StrategyType == "Naive")
                {
                    strategyToUse = new NaiveStrategy();
                }
                else
                {
                    strategyToUse = new HierarchyStrategy();
                }

                var p = new Philosopher(i, logger, strategyToUse, config);
                philosophers.Add(p);
                tasks.Add(p.RunCycleAsync(leftFork, rightFork, cts.Token));
            }

            if (config.SimulationTimeSeconds > 0)
            {
                var delayTask = Task.Delay(config.SimulationTimeSeconds * 1000);
                var inputTask = Task.Run(() => Console.ReadLine());
                await Task.WhenAny(delayTask, inputTask);
            }
            else
            {
                Console.ReadLine();
            }

            cts.Cancel();
            int targetRow = startRowForSimulation + config.PhilosophersCount + 1;
            if (targetRow >= Console.BufferHeight)
                targetRow = Console.BufferHeight - 1;

            Console.SetCursorPosition(0, targetRow);
            Console.WriteLine("Zakończono symulacje. Zatrzymywanie wątków...");

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) { }

            PrintConsoleReport(philosophers);

            if (config.WriteResults)
                SaveCsvReport(philosophers, config);

            Console.WriteLine("\nNaciśnij dowolny klawisz, aby zamknąć...");
            Console.ReadKey();
        }

        static SimulationConfig LoadConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configRoot = builder.Build();
            var settings = new SimulationConfig();
            configRoot.GetSection("Simulation").Bind(settings);
            return settings;
        }

        static void PrintConsoleReport(List<Philosopher> philosophers)
        {
            Console.WriteLine("\n\n\n\n\n\n\n\n");
            Console.WriteLine("--- RAPORT WYDAJNOŚCI ---");
            Console.WriteLine($"{"ID",-5} | {"Posiłki",-10} | {"Śr. Czas Czekania",-20}");
            Console.WriteLine(new string('-', 40));

            long totalMeals = 0;
            foreach (var p in philosophers)
            {
                double avgWait = p.MealsEaten > 0 ? (double)p.TotalWaitTimeMs / p.MealsEaten : (double)p.TotalWaitTimeMs;
                Console.WriteLine($"{p.Id,-5} | {p.MealsEaten,-10} | {avgWait:F0} ms");
                totalMeals += p.MealsEaten;
            }

            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"Liczba zjedzonych posiłków: {totalMeals}");
        }

        static void SaveCsvReport(List<Philosopher> philosophers, SimulationConfig config)
        {
            try
            {
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("ID;Posiłki;Średni Czas Czekania [ms]");

                long totalMeals = 0;
                foreach (var p in philosophers)
                {
                    double avgWait = p.MealsEaten > 0 ? (double)p.TotalWaitTimeMs / p.MealsEaten : (double)p.TotalWaitTimeMs;
                    csvBuilder.AppendLine($"{p.Id};{p.MealsEaten};{avgWait:F2}");
                    totalMeals += p.MealsEaten;
                }

                csvBuilder.AppendLine($";Suma;{totalMeals}");

                string folderName = "results";

                Directory.CreateDirectory(folderName);

                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"{config.StrategyType}_{timeStamp}.csv";

                string fullPath = Path.Combine(folderName, fileName);

                File.WriteAllText(fullPath, csvBuilder.ToString(), Encoding.UTF8);

                Console.WriteLine($"\n[PLIK] Raport zapisano pomyślnie: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[BŁĄD ZAPISU] Nie udało się utworzyć pliku CSV: {ex.Message}");
            }
        }
    }
}