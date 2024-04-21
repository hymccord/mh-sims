using KaimiraGames;

namespace bbsurvivor;

interface IRoomDistributionProvider
{
    string Name { get; }
    WeightedList<RoomLevel> GetRoomDistributions();
}

class DefaultRoomDistribution : IRoomDistributionProvider
{
    public string Name => nameof(DefaultRoomDistribution);

    private readonly Lazy<WeightedList<RoomLevel>> _cache = new(() =>
    {
        return new WeightedList<RoomLevel>(new WeightedListItem<RoomLevel>[]
        {
            new WeightedListItem<RoomLevel>(RoomLevel.Standard, 40),
            new WeightedListItem<RoomLevel>(RoomLevel.Super, 30),
            new WeightedListItem<RoomLevel>(RoomLevel.Extreme, 20),
            new WeightedListItem<RoomLevel>(RoomLevel.Ultimate, 10),
        });
    });

    public WeightedList<RoomLevel> GetRoomDistributions() => _cache.Value;
}

class GreatHallRoomDistribution : IRoomDistributionProvider
{
    public string Name => nameof(GreatHallRoomDistribution);
    private readonly Lazy<WeightedList<RoomLevel>> _cache = new(() =>
    {
        return new WeightedList<RoomLevel>(new WeightedListItem<RoomLevel>[]
        {
            new WeightedListItem<RoomLevel>(RoomLevel.Standard, 45),
            new WeightedListItem<RoomLevel>(RoomLevel.Super, 35),
            new WeightedListItem<RoomLevel>(RoomLevel.Extreme, 15),
            new WeightedListItem<RoomLevel>(RoomLevel.Ultimate, 5),
        });
    });

    public WeightedList<RoomLevel> GetRoomDistributions() => _cache.Value;
}

class KeyedGreatHallRoomDistribution : IRoomDistributionProvider
{
    public string Name => nameof(KeyedGreatHallRoomDistribution);
    private readonly Lazy<WeightedList<RoomLevel>> _cache = new(() =>
    {
        return new WeightedList<RoomLevel>(new WeightedListItem<RoomLevel>[]
        {
            new WeightedListItem<RoomLevel>(RoomLevel.Standard, 0),
            new WeightedListItem<RoomLevel>(RoomLevel.Super, 70),
            new WeightedListItem<RoomLevel>(RoomLevel.Extreme, 20),
            new WeightedListItem<RoomLevel>(RoomLevel.Ultimate, 10),
        });
    });

    public WeightedList<RoomLevel> GetRoomDistributions() => _cache.Value;
}
