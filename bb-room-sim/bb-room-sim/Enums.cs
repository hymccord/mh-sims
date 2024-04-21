namespace bbsurvivor;

enum FloorType
{
    Dungeon,
    Ballroom,
    GreatHall,
}

enum RoomType
{
    Bean,
    Lavish,
    Ruby,
    Harp,
    Mystery,
    Egg
}

enum RoomQuality
{
    Standard,
    Super,
    Extreme,
    Ultimate
}

[Flags]
enum Embellishments
{
    GoldenKey =     0b0001,
    RubyRemover =   0b0010,
}
