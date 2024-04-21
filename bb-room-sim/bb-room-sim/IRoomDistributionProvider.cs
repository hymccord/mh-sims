using KaimiraGames;

namespace bbsurvivor;

interface IRoomDistributionProvider
{
    string Name { get; }
    WeightedList<RoomQuality> GetRoomDistributions();
}

class DefaultRoomDistribution : IRoomDistributionProvider
{
    public string Name => nameof(DefaultRoomDistribution);

    private readonly Lazy<WeightedList<RoomQuality>> _cache = new(() =>
    {
        return new WeightedList<RoomQuality>(new WeightedListItem<RoomQuality>[]
        {
            new(RoomQuality.Standard, 40),
            new(RoomQuality.Super, 30),
            new(RoomQuality.Extreme, 20),
            new(RoomQuality.Ultimate, 10),
        });
    });

    public WeightedList<RoomQuality> GetRoomDistributions() => _cache.Value;
}

class GreatHallRoomDistribution : IRoomDistributionProvider
{
    public string Name => nameof(GreatHallRoomDistribution);
    private readonly Lazy<WeightedList<RoomQuality>> _cache = new(() =>
    {
        return new WeightedList<RoomQuality>(new WeightedListItem<RoomQuality>[]
        {
            new(RoomQuality.Standard, 35),
            new(RoomQuality.Super, 40),
            new(RoomQuality.Extreme, 15),
            new(RoomQuality.Ultimate, 10),
        });
    });

    public WeightedList<RoomQuality> GetRoomDistributions() => _cache.Value;
}

class KeyedGreatHallRoomDistribution : IRoomDistributionProvider
{
    public string Name => nameof(KeyedGreatHallRoomDistribution);
    private readonly Lazy<WeightedList<RoomQuality>> _cache = new(() =>
    {
        return new WeightedList<RoomQuality>(new WeightedListItem<RoomQuality>[]
        {
            new(RoomQuality.Standard, 0),
            new(RoomQuality.Super, 60),
            new(RoomQuality.Extreme, 25),
            new(RoomQuality.Ultimate, 15),
        });
    });

    public WeightedList<RoomQuality> GetRoomDistributions() => _cache.Value;
}

internal static class DungeonDistribution
{
    private static readonly Func<KeyValuePair<(RoomType, RoomQuality), double>, bool> s_keyFilter = (kvp) => kvp.Key.Item2 > RoomQuality.Standard;
    public static readonly IReadOnlyDictionary<(RoomType type, RoomQuality quality), double> Rooms = new Dictionary<(RoomType, RoomQuality), double>()
    {
        { (RoomType.Bean, RoomQuality.Standard),    0.087 },
        { (RoomType.Bean, RoomQuality.Super),       0.045 },
        { (RoomType.Bean, RoomQuality.Extreme),     0.020 },
        { (RoomType.Bean, RoomQuality.Ultimate),    0.008 },
        { (RoomType.Lavish, RoomQuality.Standard),  0.080 },
        { (RoomType.Lavish, RoomQuality.Super),     0.220 },
        { (RoomType.Lavish, RoomQuality.Extreme),   0.337 },
        { (RoomType.Lavish, RoomQuality.Ultimate),  0.115 },
        { (RoomType.Mystery, RoomQuality.Standard), 0.045 },
        { (RoomType.Mystery, RoomQuality.Super),    0.025 },
        { (RoomType.Mystery, RoomQuality.Extreme),  0.013 },
        { (RoomType.Mystery, RoomQuality.Ultimate), 0.005 },
    };

    public static IReadOnlyCollection<KeyValuePair<(RoomType, RoomQuality), double>> GetEmbellishedRooms(Embellishments embellishments)
    {
        return embellishments switch
        {
            (Embellishments.GoldenKey) => Rooms.Where(kvp => kvp.Key.quality > RoomQuality.Standard),
            (Embellishments.RubyRemover) => Rooms.Where(kvp => kvp.Key.type != RoomType.Ruby),
            (Embellishments.GoldenKey & Embellishments.RubyRemover) => Rooms
                .Where(kvp => ),
            _ => throw new NotImplementedException()
        };
    }
}
