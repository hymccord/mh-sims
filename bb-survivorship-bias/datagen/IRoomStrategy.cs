namespace bbsurvivor;
interface IRoomStrategy
{
    string Name { get; }
    bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom);
}

class NoobStrat : IRoomStrategy
{
    public string Name => "Take First Room";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom) => false;
}

class QuickTurnaround : IRoomStrategy {
    public string Name => "Leave if current and next room are <= Super";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom)
    {
        return currentRoom <= RoomLevel.Super && nextRoom <= RoomLevel.Super;
    }
}

class BailOnLower : IRoomStrategy
{
    public string Name => "Leave if next is lower floor";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom)
    {
        return currentRoom > nextRoom;
    }
}

class CoastTilExtremeOrBetter : IRoomStrategy
{
    public string Name => "Extreme is good enough.";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom)
    {
        return currentRoom < RoomLevel.Extreme;
    }
}

class CoastTilUltimate : IRoomStrategy
{
    public string Name => "Ultimate! <<Insert 4 chevrons here>>";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom)
    {
        return currentRoom < RoomLevel.Ultimate;
    }
}

class CoastTilUltimateRnd : IRoomStrategy
{
    public string Name => "Ultimate! 1% chance of room failing";

    public bool ShouldAdvance(RoomLevel currentRoom, RoomLevel nextRoom)
    {
        if (Random.Shared.NextDouble() < 0.01)
        {
            // _ = Console.Out.WriteLineAsync("kicked out!");
            return false;
        }

        return currentRoom < RoomLevel.Ultimate;
    }
}
