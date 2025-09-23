namespace CarFactoryArchitect.Source.Core
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public enum OreType
    {
        Iron,
        Copper,
        Sand,
        Rubber,
        Chassis,
        ECU,
        Wheel,
        Engine,
        Car
    }

    public enum OreState
    {
        Tile,
        Raw,
        Smelted,
        Plate,
        Wire,
        Manufactured
    }

    public enum MachineType
    {
        Smelter,
        Forge,
        Cutter,
        Assembler,
        Extractor,
        Seller
    }

    public enum ConveyorType
    {
        Basic,
        Fast,
        Underground,
        Express
    }

    public enum BuildMode
    {
        Conveyor,
        Machine
    }
}