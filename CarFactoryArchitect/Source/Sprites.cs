using MonoGameLibrary.Graphics;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CarFactoryArchitect.Source;

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
    Rubber
}

public enum OreState
{
    Tile,
    Raw,
    Smelted,
    Plate,
    Wire
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
public class Conveyor
{
    public Direction Direction { get; private set; }
    public AnimatedSprite Sprite { get; private set; }

    public Conveyor(TextureAtlas atlas, Direction direction, Single scale)
    {
        Direction = direction;

        string animationName = direction switch
        {
            Direction.Up => "conveyor-animation-up",
            Direction.Right => "conveyor-animation-right",
            Direction.Down => "conveyor-animation-down",
            Direction.Left => "conveyor-animation-left",
            _ => "conveyor-animation-up"
        };

        Sprite = atlas.CreateAnimatedSprite(animationName);
        Sprite.Scale = new Vector2(scale, scale);
    }
    public void Update(GameTime gameTime)
    {
        Sprite?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        Sprite?.Draw(spriteBatch, position);
    }
}

public class Machine
{
    public MachineType Type { get; private set; }
    public Direction Direction { get; set; }
    public Sprite MachineSprite { get; set; }

    public Machine(MachineType machineType, Direction direction, TextureAtlas atlas, Single scale)
    {
        Type = machineType;
        Direction = direction;

        string baseName = machineType switch
        {
            MachineType.Smelter => "smelter",
            MachineType.Forge => "forge",
            MachineType.Cutter => "cutter",
            MachineType.Assembler => "assembler",
            MachineType.Extractor => "extractor",
            MachineType.Seller => "seller",
            _ => "smelter"
        };

        string directionSuffix = direction switch
        {
            Direction.Up => "-up",
            Direction.Right => "-right",
            Direction.Down => "-down",
            Direction.Left => "-left",
            _ => "-up"
        };

        string spriteName = machineType == MachineType.Seller ? baseName : baseName + GetDirectionSuffix(direction);

        MachineSprite = atlas.CreateSprite(spriteName);
        MachineSprite.Scale = new Vector2(scale, scale);
    }

    private string GetDirectionSuffix(Direction direction)
    {
        return direction switch
        {
            Direction.Up => "-up",
            Direction.Right => "-right",
            Direction.Down => "-down",
            Direction.Left => "-left",
            _ => "-up"
        };
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position)
    {
        MachineSprite?.Draw(spriteBatch, position);
    }
}

public class Ore
{
    public OreType Type { get; private set; }
    public Sprite OreSprite { get; set; }
    public OreState State { get; set; }

    public Ore(OreType oreType, OreState oreState, TextureAtlas atlas, Single scale)
    {
        Type = oreType;

        string spriteName = oreType switch
        {
            OreType.Iron => "iron",
            OreType.Copper => "copper",
            OreType.Sand => "sand",
            OreType.Rubber => "rubber",
            _ => "iron"
        };

        State = oreState;

        string spriteState = oreState switch
        {
            OreState.Tile => "-tile",
            OreState.Raw => "-raw",
            OreState.Smelted => "-smelted",
            OreState.Plate => "-plate",
            OreState.Wire => "-wire",
            _ => "-tile"
        };

        try
        {
            OreSprite = atlas.CreateSprite(spriteName + spriteState);
            OreSprite.Scale = new Vector2(scale, scale);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating sprite for {spriteName}{spriteState}: {ex.Message}");
            throw;
        }
    }
}

public class Seller
{
    public Sprite SellPoint { get; set; }
    public Seller(TextureAtlas atlas, Single scale)
    {
        SellPoint = atlas.CreateSprite("seller");
        SellPoint.Scale = new Vector2(scale, scale);
    }
}