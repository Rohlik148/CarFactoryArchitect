using MonoGameLibrary.Graphics;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

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

public class Recipe
{
    public List<(OreType type, OreState state)> Inputs { get; set; }
    public (OreType type, OreState state) Output { get; set; }
    public float ProcessingTime { get; set; }

    public Recipe(List<(OreType, OreState)> inputs, (OreType, OreState) output, float processingTime = 5.0f)
    {
        Inputs = inputs;
        Output = output;
        ProcessingTime = processingTime;
    }
}

public static class RecipeManager
{
    public static readonly List<Recipe> _recipes = new()
    {
        // Wheel = Iron Smelted + Rubber Raw
        new Recipe(
            new List<(OreType, OreState)> { (OreType.Iron, OreState.Smelted), (OreType.Rubber, OreState.Raw) },
            (OreType.Wheel, OreState.Manufactured),
            4.0f),

        // ECU = Iron Plate + Copper Wire
        new Recipe(
            new List<(OreType, OreState)> { (OreType.Iron, OreState.Plate), (OreType.Copper, OreState.Wire) },
            (OreType.ECU, OreState.Manufactured),
            5.0f),

        // Chassis = Iron Smelted + Wheel
        new Recipe(
            new List<(OreType, OreState)> { (OreType.Iron, OreState.Smelted), (OreType.Wheel, OreState.Manufactured) },
            (OreType.Chassis, OreState.Manufactured),
            6.0f),

        // Engine = ECU + Iron Smelted
        new Recipe(
            new List<(OreType, OreState)> { (OreType.ECU, OreState.Manufactured), (OreType.Iron, OreState.Smelted) },
            (OreType.Engine, OreState.Manufactured),
            7.0f),

        // Car = Chassis + Engine + Glass
        new Recipe(
            new List<(OreType, OreState)> { (OreType.Chassis, OreState.Manufactured), (OreType.Engine, OreState.Manufactured), (OreType.Sand, OreState.Smelted) },
            (OreType.Car, OreState.Manufactured),
            10.0f)
    };

    public static Recipe FindRecipe(List<(OreType type, OreState state)> availableInputs)
    {
        System.Diagnostics.Debug.WriteLine($"RecipeManager: Looking for recipe with inputs: {string.Join(", ", availableInputs.Select(x => $"{x.type} {x.state}"))}");

        foreach (var recipe in _recipes)
        {
            System.Diagnostics.Debug.WriteLine($"  Checking recipe for {recipe.Output.type}: requires {string.Join(", ", recipe.Inputs.Select(x => $"{x.type} {x.state}"))}");

            if (HasRequiredInputs(availableInputs, recipe.Inputs))
            {
                System.Diagnostics.Debug.WriteLine($"  Recipe found: {recipe.Output.type}");
                return recipe;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"  Recipe doesn't match");
            }
        }

        System.Diagnostics.Debug.WriteLine("  No matching recipe found");
        return null;
    }

    private static bool HasRequiredInputs(List<(OreType type, OreState state)> available, List<(OreType type, OreState state)> required)
    {
        var availableCopy = new List<(OreType, OreState)>(available);

        foreach (var requiredItem in required)
        {
            if (availableCopy.Contains(requiredItem))
            {
                availableCopy.Remove(requiredItem);
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public static bool CouldBePartOfRecipe(OreType oreType, OreState oreState)
    {
        return _recipes.Any(recipe => recipe.Inputs.Contains((oreType, oreState)));
    }
}