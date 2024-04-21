
using System.Collections.Immutable;

using ency_sim;


internal class Program
{
    private static async Task Main(string[] args)
    {
        var sim = new EncySim(new()
        {
            SilverQuill = true,
            GoldenQuill = true,
            GoldFoilPrinting = true,
            CondensedCreativity = true,
        });

        while (true)
        {
            //await sim.Run(0, 25, 51972, 91);
            Console.Clear();
            await sim.Run(4000, 24, 51972, 91);
        }
    }
}

internal class EncySim
{
    private static double s_mythWeaverAR = 60;
    private static double s_bitterGrammarianAR = 40;

    private readonly EncySimOptions _options;
    private readonly int _mythWeaverWords;
    private readonly int _grammarianWords;

    private WeightedList<string> _something;

    public EncySim(EncySimOptions options)
    {
        _options = options;

        _mythWeaverWords = (int)Math.Floor(1000 * (_options.SilverQuill ? 1.25 : 1) * (_options.GoldenQuill ? 1.5 : 1));
        _grammarianWords = (int)Math.Floor(250 * (_options.SilverQuill ? 1.25 : 1) * (_options.GoldenQuill ? 1.5 : 1));

        _mythWeaverWords *= _options.CondensedCreativity ? 2 : 1;
        _grammarianWords *= _options.CondensedCreativity ? 2 : 1;

    }

    internal async Task Run(int numWords, int remainingHunts, ulong power, ulong luck)
    {
        double mythCR = 0.25; // CatchRate(power, luck, 200_000, 7.5);
        double grammarianCR = 0;// CatchRate(power, luck, 90_000, 4.0);

        _something = new WeightedList<string>([
            new ("M", 100),
            new ("G", 0)
            ]);

        Dictionary<int, int> volumesWritten = new();
        for (int i = 0; i < 100_000; i++)
        {
            var words = await DoOneRun(numWords, remainingHunts, mythCR, grammarianCR);
            var volumes = words / 4000;

            if (volumesWritten.TryGetValue(volumes, out int value))
            {
                volumesWritten[volumes] = ++value;
            }
            else
            {
                volumesWritten.Add(volumes, 1);
            }
        }

        var final = volumesWritten.ToImmutableSortedDictionary();

        for (int i = 1; i < final.Keys.Max(); i++)
        {
            if (volumesWritten.TryGetValue(i, out int value))
            {
                await Console.Out.WriteLineAsync($"{i},{volumesWritten[i] / 100_000.0}");
            }
            else
            {
                await Console.Out.WriteLineAsync($"{i},{0:e}");
            }
        }

        Console.ReadLine();
    }

    private async Task<int> DoOneRun(int wordCount, int huntsRemaining, double mythCR, double grammarianCR)
    {
        do
        {
            double roll = Random.Shared.Next(0, 10000) / 10000.0;
            if (wordCount < 4000)
            {
                if (roll < grammarianCR)
                {
                    wordCount += _grammarianWords;
                }
            } else
            {
                var x = _something.Next();
                if (Random.Shared.Next(0, 100) <= 100)
                {
                    // myth
                    if (roll < mythCR)
                    {
                        wordCount += _mythWeaverWords;
                        huntsRemaining += 2;
                    }
                }
                else
                {
                    if (roll < grammarianCR)
                    {
                        wordCount += _grammarianWords;
                    }
                }
            }

            huntsRemaining--;
        } while (huntsRemaining > 0);

        return wordCount;
    }

    private double CatchRate(ulong power, ulong luck, ulong mousePower, double eff)
    {
        double num = (power * eff + 2 * Math.Floor(Math.Pow(luck * Math.Min(eff, 1.4), 2)));
        double divisor = mousePower + power * eff; 

        return Math.Min(1,  num / divisor);
    }
}

record EncySimOptions
{
    public bool SilverQuill { get; init; }
    public bool GoldenQuill { get; init; }
    public bool GoldFoilPrinting { get; init; }
    public bool CondensedCreativity { get; init; }
}
