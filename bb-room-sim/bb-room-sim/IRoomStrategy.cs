namespace bbsurvivor;
interface IRoomStrategy
{
    string Name { get; }
    bool ShouldAdvance(RoomQuality currentRoom, RoomQuality nextRoom);
}

class RoomOneRetreat : IRoomStrategy
{
    public string Name => "R1R";

    public bool ShouldAdvance(RoomQuality currentRoom, RoomQuality nextRoom) => false;
}

class QuickTurnaround : IRoomStrategy {
    public string Name => "Leave if current and next room are <= Super";

    public bool ShouldAdvance(RoomQuality currentRoom, RoomQuality nextRoom)
    {
        return currentRoom <= RoomQuality.Super && nextRoom <= RoomQuality.Super;
    }
}

class BailOnLower : IRoomStrategy
{
    public string Name => "Leave if next is lower floor";

    public bool ShouldAdvance(RoomQuality currentRoom, RoomQuality nextRoom)
    {
        return currentRoom > nextRoom;
    }
}

class CoastTilExtremeOrBetter : IRoomStrategy
{
    public string Name => "Retreat on Extreme or Ultimate";

    public bool ShouldAdvance(RoomQuality currentRoom, RoomQuality nextRoom)
    {
        return currentRoom < RoomQuality.Extreme;
    }
}

class CoastTilUltimate : IRoomStrategy
{
    public string Name => "Retreat on Ultimate";

    public bool ShouldAdvance(RoomQuality currentRoom, RoomQuality nextRoom)
    {
        return currentRoom < RoomQuality.Ultimate;
    }
}
