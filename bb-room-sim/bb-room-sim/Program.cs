using KaimiraGames;

namespace bbsurvivor;

internal class Program
{
    private static async Task Main(string[] args)
    {
        int numRuns = 10000;
        var runner = new RoomRunner(
            numRuns,
            [
                new RoomOneRetreat(), // Leave on first room. This should prove that the room visits == room distribution
                new QuickTurnaround(),
                new BailOnLower(),
                new CoastTilExtremeOrBetter(),
                new CoastTilUltimate(),
            ],
            [
                // numbers are just guesses, not accurate
                new DefaultRoomDistribution(), // 4, 3, 2, 1
                new GreatHallRoomDistribution(), // 45, 35, 15, 5
                new KeyedGreatHallRoomDistribution(), // 0, 70, 20, 10
            ]
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
            //writer.WriteResult("Room Distribution",rooms.GetWeightOf(RoomLevel.Standard),rooms.GetWeightOf(RoomLevel.Super),rooms.GetWeightOf(RoomLevel.Extreme),rooms.GetWeightOf(RoomLevel.Ultimate));
            await Console.Out.WriteLineAsync(
                $"""
                {string.Format("{0}: {1:P},{2:P},{3:P},{4:P}", distribution.Name,rooms.GetWeightOf(RoomQuality.Standard) / (double)rooms.TotalWeight,rooms.GetWeightOf(RoomQuality.Super) / (double)rooms.TotalWeight,rooms.GetWeightOf(RoomQuality.Extreme) / (double)rooms.TotalWeight,rooms.GetWeightOf(RoomQuality.Ultimate) / (double)rooms.TotalWeight)}
                """);

            // queue all the strategies for parallel execution
            foreach (IRoomStrategy strategy in _roomStrategies)
            {
                await RunStrategy(writer, distribution, strategy);
                //tasks.Add(Task.Run(async () => await RunStrategy(writer, distribution, strategy)));
            }
        }

        //await Task.WhenAll(tasks);
        foreach (var writer in writers)
        {
            writer.Dispose();
        }
    }

    private async Task RunStrategy(StrategyWriter writer, IRoomDistributionProvider roomDistributionProvider, IRoomStrategy strategy)
    {
        
        var roomCounts = new Dictionary<RoomQuality, int>
        {
            {RoomQuality.Standard, 0},
            {RoomQuality.Super, 0},
            {RoomQuality.Extreme, 0},
            {RoomQuality.Ultimate, 0},
        };
        WeightedList<RoomQuality> roomDistributions = roomDistributionProvider.GetRoomDistributions();

        for (int i = 1; i <= _numRuns; i++)
        {
            RoomQuality currentRoom = roomDistributions.Next();
            RoomQuality nextRoom = roomDistributions.Next();
            roomCounts[currentRoom]++;

            while (strategy.ShouldAdvance(currentRoom, nextRoom))
            {
                currentRoom = nextRoom;
                nextRoom = roomDistributions.Next();

                roomCounts[nextRoom]++;
            }
        }

        writer.WriteResult(strategy.Name, roomCounts[RoomQuality.Standard], roomCounts[RoomQuality.Super], roomCounts[RoomQuality.Extreme], roomCounts[RoomQuality.Ultimate]);

        double roomSum = roomCounts.Sum(kvp => kvp.Value);
        await Console.Out.WriteLineAsync(
            $"""
            '{roomDistributionProvider.Name}', Strategy: '{strategy.Name}';
            {string.Format("{0:P},{1:P},{2:P},{3:P}", roomCounts[RoomQuality.Standard] / roomSum, roomCounts[RoomQuality.Super] / roomSum, roomCounts[RoomQuality.Extreme] / roomSum, roomCounts[RoomQuality.Ultimate] / roomSum)}
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
