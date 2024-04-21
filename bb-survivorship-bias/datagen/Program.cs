using KaimiraGames;

namespace bbsurvivor;

internal class Program
{
    private static async Task Main(string[] args)
    {
        int numRuns = 10000;
        var runner = new RoomRunner(
            numRuns,
            new IRoomStrategy[]
            {
                new NoobStrat(), // Leave on first room. This should prove that the room visits == room distribution
                new QuickTurnaround(),
                new BailOnLower(),
                new CoastTilExtremeOrBetter(),
                new CoastTilUltimate(),
                new CoastTilUltimateRnd(),
            },
            new IRoomDistributionProvider[]
            {
                // numbers are just guesses, not accurate
                new DefaultRoomDistribution(), // 4, 3, 2, 1
                // new GreatHallRoomDistribution(), // 45, 35, 15, 5
                // new KeyedGreatHallRoomDistribution(), // 0, 70, 20, 10
            }
        );

        await runner.SimulateAsync();
    }
}

class RoomRunner
{
    private readonly IRoomStrategy[] _roomStrategies;
    private readonly IEnumerable<IRoomDistributionProvider> _roomDistributionProviders;
    private readonly int _numRuns;

    public RoomRunner(int numRuns, IRoomStrategy[] roomStrategies, IRoomDistributionProvider[] roomDistributionProviders)
    {
        _numRuns = numRuns;
        _roomStrategies = roomStrategies;
        _roomDistributionProviders = roomDistributionProviders;
    }

    public async Task SimulateAsync()
    {
        var tasks = new List<Task>();
        var writers = new List<StrategyWriter>();

        foreach (IRoomDistributionProvider distribution in _roomDistributionProviders)
        {
            var writer = new StrategyWriter(distribution.Name);
            writers.Add(writer);

            // Logging for debug/visibility
            var rooms = distribution.GetRoomDistributions();
            writer.WriteResult("Room Distribution",rooms.GetWeightOf(RoomLevel.Standard),rooms.GetWeightOf(RoomLevel.Super),rooms.GetWeightOf(RoomLevel.Extreme),rooms.GetWeightOf(RoomLevel.Ultimate));
            await Console.Out.WriteLineAsync(
                $"""
                {string.Format("{0}: {1:P},{2:P},{3:P},{4:P}", distribution.Name,rooms.GetWeightOf(RoomLevel.Standard) / (double)rooms.TotalWeight,rooms.GetWeightOf(RoomLevel.Super) / (double)rooms.TotalWeight,rooms.GetWeightOf(RoomLevel.Extreme) / (double)rooms.TotalWeight,rooms.GetWeightOf(RoomLevel.Ultimate) / (double)rooms.TotalWeight)}
                """);

            // queue all the strategies for parallel execution
            foreach (IRoomStrategy strategy in _roomStrategies)
            {
                tasks.Add(Task.Run(async () => await RunStrategy(writer, distribution, strategy)));
            }
        }

        await Task.WhenAll(tasks);
        foreach (var writer in writers)
        {
            writer.Dispose();
        }
    }

    private async Task RunStrategy(StrategyWriter writer, IRoomDistributionProvider roomDistributionProvider, IRoomStrategy strategy)
    {

        var roomCounts = new Dictionary<RoomLevel, int>
        {
            {RoomLevel.Standard, 0},
            {RoomLevel.Super, 0},
            {RoomLevel.Extreme, 0},
            {RoomLevel.Ultimate, 0},
        };
        WeightedList<RoomLevel> roomDistributions = roomDistributionProvider.GetRoomDistributions();

        for (int i = 1; i <= _numRuns; i++)
        {
            RoomLevel currentRoom = roomDistributions.Next();
            RoomLevel nextRoom = roomDistributions.Next();
            roomCounts[currentRoom] += 20;

            while (strategy.ShouldAdvance(currentRoom, nextRoom))
            {
                currentRoom = nextRoom;
                nextRoom = roomDistributions.Next();

                roomCounts[nextRoom] += 20;
            }

            roomCounts[currentRoom] += 20;
        }

        writer.WriteResult(strategy.Name, roomCounts[RoomLevel.Standard], roomCounts[RoomLevel.Super], roomCounts[RoomLevel.Extreme], roomCounts[RoomLevel.Ultimate]);

        double roomSum = roomCounts.Sum(kvp => kvp.Value);
        await Console.Out.WriteLineAsync(
            $"""
            Done. Distribution: '{roomDistributionProvider.Name}', Strategy: '{strategy.Name}'");
            {string.Format("{0},{1},{2},{3},{4}", strategy.Name, roomCounts[RoomLevel.Standard], roomCounts[RoomLevel.Super], roomCounts[RoomLevel.Extreme], roomCounts[RoomLevel.Ultimate])}
            {string.Format("{0:P},{1:P},{2:P},{3:P}", roomCounts[RoomLevel.Standard] / roomSum, roomCounts[RoomLevel.Super] / roomSum, roomCounts[RoomLevel.Extreme] / roomSum, roomCounts[RoomLevel.Ultimate] / roomSum)}
            """
        );
    }

    // Helps write results to CSV
    private class StrategyWriter : IDisposable
    {
        private StreamWriter _stream;

        public StrategyWriter(string name)
        {
            _stream = new StreamWriter($"{name.Replace(" ", "")}.csv", append: false);
            _stream.WriteLine("Name,Standard,Super,Extreme,Ultimate");
        }

        public void WriteResult(string name, int standardCount, int superCount, int extremeCount, int ultimateCount)
        {
            _stream.WriteLine($"{name},{standardCount},{superCount},{extremeCount},{ultimateCount}");
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
