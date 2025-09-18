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

public class Machine
{
    public MachineType Type { get; private set; }
    public Direction Direction { get; set; }
    public Sprite MachineSprite { get; set; }

    // Properties for input/output slots
    public Ore InputSlot { get; set; }
    public Ore InputSlotA { get; set; }
    public Ore InputSlotB { get; set; }
    public Ore OutputSlot { get; set; }

    // Processing state
    public bool IsProcessing { get; private set; }
    public float ProcessingTime { get; private set; }
    public float ProcessingDuration { get; private set; }

    // Recipes for assembler
    public Dictionary<Direction, Ore> InputSlots { get; set; }
    public Recipe CurrentRecipe { get; set; }


    public Machine(MachineType machineType, Direction direction, TextureAtlas atlas, Single scale)
    {
        Type = machineType;
        Direction = direction;

        ProcessingDuration = machineType switch
        {
            MachineType.Smelter => 3.0f,    // 3 seconds
            MachineType.Forge => 2.5f,      // 2.5 seconds
            MachineType.Cutter => 2.0f,     // 2 seconds
            MachineType.Assembler => 4.0f,  // 4 seconds
            MachineType.Extractor => 2.0f,  // 2 seconds
            _ => 1.0f
        };

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

        string spriteName = machineType == MachineType.Seller ? baseName : baseName + GetDirectionSuffix(direction);
        MachineSprite = atlas.CreateSprite(spriteName);
        MachineSprite.Scale = new Vector2(scale, scale);

        if (machineType == MachineType.Assembler)
        {
            InputSlots = new Dictionary<Direction, Ore>();
        }
    }

    public void Update(GameTime gameTime, TextureAtlas atlas, float scale)
    {
        if (IsProcessing)
        {
            ProcessingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (ProcessingTime >= ProcessingDuration)
            {
                CompleteProcessing(atlas, scale);
            }
        }
    }

    public bool CanAcceptInput(Ore item)
    {
        // Machine can't accept input if it's processing, has output waiting, or input slot is occupied
        if (IsProcessing || OutputSlot != null || InputSlot != null)
            return false;

        return Type switch
        {
            MachineType.Smelter => CanSmelterAcceptInput(item),
            MachineType.Forge => CanForgeAcceptInput(item),
            MachineType.Cutter => CanCutterAcceptInput(item),
            MachineType.Assembler => CanAssemblerAcceptInput(item),
            MachineType.Seller => true, // Seller accepts anything
            _ => false
        };
    }

    private bool CouldBePartOfRecipe(OreType oreType, OreState oreState)
    {
        return RecipeManager.CouldBePartOfRecipe(oreType, oreState);
    }

    public bool CanAcceptInput(Ore item, Direction fromDirection)
    {
        if (Type != MachineType.Assembler)
        {
            return CanAcceptInput(item);
        }

        System.Diagnostics.Debug.WriteLine($"Assembler CanAcceptInput: {item.Type} {item.State} from {fromDirection}");

        // Assembler-specific logic
        if (IsProcessing)
        {
            System.Diagnostics.Debug.WriteLine("  Rejected: Already processing");
            return false;
        }

        if (OutputSlot != null)
        {
            System.Diagnostics.Debug.WriteLine("  Rejected: Output slot occupied");
            return false;
        }

        // Can't accept input from output direction
        if (fromDirection == Direction)
        {
            System.Diagnostics.Debug.WriteLine($"  Rejected: Input from output direction (machine faces {Direction})");
            return false;
        }

        // Check if this item could be part of any recipe
        bool couldBePartOfRecipe = CouldBePartOfRecipe(item.Type, item.State);
        if (!couldBePartOfRecipe)
        {
            System.Diagnostics.Debug.WriteLine($"  Rejected: {item.Type} {item.State} not part of any recipe");
            return false;
        }

        if (InputSlots.Count >= 3) // Maximum 3 inputs for any recipe
        {
            System.Diagnostics.Debug.WriteLine("  Rejected: Already has maximum number of inputs (3)");
            return false;
        }

        bool alreadyHasThisItem = InputSlots.Values.Any(ore => ore.Type == item.Type && ore.State == item.State);
        if (alreadyHasThisItem)
        {
            System.Diagnostics.Debug.WriteLine($"  Rejected: Already has {item.Type} {item.State}");
            return false;
        }

        System.Diagnostics.Debug.WriteLine($"  Accepted: {item.Type} {item.State} from {fromDirection}");
        return true;
    }

    private bool CanSmelterAcceptInput(Ore item)
    {
        if (item.State != OreState.Raw) return false;

        return item.Type switch
        {
            OreType.Iron => true,    // iron-raw > iron-smelted
            OreType.Copper => true,  // copper-raw > copper-smelted
            OreType.Sand => true,    // sand-raw > glass
            OreType.Rubber => false, // rubber-raw > no smelted version
            _ => false
        };
    }

    private bool CanForgeAcceptInput(Ore item)
    {
        if (item.State != OreState.Smelted) return false;

        return item.Type switch
        {
            OreType.Iron => true,    // iron-smelted > iron-plate
            OreType.Copper => false, // copper can't be forged
            OreType.Sand => false,   // glass can't be forged
            OreType.Rubber => false, // rubber can't be forged
            _ => false
        };
    }

    private bool CanCutterAcceptInput(Ore item)
    {
        return item.Type == OreType.Copper && item.State == OreState.Smelted;
    }

    private bool CanAssemblerAcceptInput(Ore item)
    {
        return CouldBePartOfRecipe(item.Type, item.State);
    }

    public bool TryAcceptInput(Ore item)
    {
        if (!CanAcceptInput(item))
            return false;

        // For non-assembler machines
        if (Type != MachineType.Assembler)
        {
            InputSlot = item;
            StartProcessing();
            return true;
        }

        return false;
    }

    public bool TryAcceptInput(Ore item, Direction fromDirection)
    {
        if (Type != MachineType.Assembler)
        {
            return TryAcceptInput(item);
        }

        System.Diagnostics.Debug.WriteLine($"Assembler: Trying to accept {item.Type} {item.State} from direction {fromDirection}");

        if (!CanAcceptInput(item, fromDirection))
        {
            System.Diagnostics.Debug.WriteLine($"Assembler: Cannot accept {item.Type} {item.State} from direction {fromDirection}");
            return false;
        }

        // Store item using direction as key
        InputSlots[fromDirection] = item;
        System.Diagnostics.Debug.WriteLine($"Assembler: Accepted {item.Type} {item.State} from direction {fromDirection}. Total inputs: {InputSlots.Count}");

        // List all current inputs
        foreach (var slot in InputSlots)
        {
            System.Diagnostics.Debug.WriteLine($"  - Input from {slot.Key}: {slot.Value.Type} {slot.Value.State}");
        }

        // Check if we can start processing
        TryStartAssemblerProcessing();

        return true;
    }

    private void TryStartAssemblerProcessing()
    {
        if (IsProcessing)
        {
            System.Diagnostics.Debug.WriteLine("Assembler: Already processing, cannot start new recipe");
            return;
        }

        var availableInputs = InputSlots.Values.Select(ore => (ore.Type, ore.State)).ToList();
        System.Diagnostics.Debug.WriteLine($"Assembler: Available inputs: {string.Join(", ", availableInputs.Select(x => $"{x.Type} {x.State}"))}");

        if (availableInputs.Count == 3)
        {
            System.Diagnostics.Debug.WriteLine("Assembler: Checking 3-input recipe (Car)...");
            var hasChassisManufactured = availableInputs.Contains((OreType.Chassis, OreState.Manufactured));
            var hasEngineManufactured = availableInputs.Contains((OreType.Engine, OreState.Manufactured));
            var hasGlass = availableInputs.Contains((OreType.Sand, OreState.Smelted));

            System.Diagnostics.Debug.WriteLine($"  Has Chassis Manufactured: {hasChassisManufactured}");
            System.Diagnostics.Debug.WriteLine($"  Has Engine Manufactured: {hasEngineManufactured}");
            System.Diagnostics.Debug.WriteLine($"  Has Glass (Sand Smelted): {hasGlass}");
        }

        var recipe = RecipeManager.FindRecipe(availableInputs);

        if (recipe != null)
        {
            System.Diagnostics.Debug.WriteLine($"Assembler: Found recipe! Output: {recipe.Output.type} {recipe.Output.state}");
            CurrentRecipe = recipe;
            ProcessingDuration = recipe.ProcessingTime;
            StartProcessing();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Assembler: No recipe found for current inputs");

            foreach (var testRecipe in RecipeManager._recipes)
            {
                System.Diagnostics.Debug.WriteLine($"  Checking recipe for {testRecipe.Output.type}: requires {string.Join(", ", testRecipe.Inputs.Select(x => $"{x.type} {x.state}"))}");

                if (HasRequiredInputs(availableInputs, testRecipe.Inputs))
                {
                    System.Diagnostics.Debug.WriteLine($"    ✓ This recipe SHOULD match!");
                }
            }
        }
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

    private void StartProcessing()
    {
        IsProcessing = true;
        ProcessingTime = 0f;
    }

    private void CompleteProcessing(TextureAtlas atlas, float scale)
    {
        IsProcessing = false;
        ProcessingTime = 0f;

        if (Type == MachineType.Assembler)
        {
            CompleteAssemblerProcessing(atlas, scale);
            return;
        }

        if (InputSlot == null)
        {
            return;
        }

        OutputSlot = Type switch
        {
            MachineType.Smelter => CreateSmelterOutput(InputSlot, atlas, scale),
            MachineType.Forge => CreateForgeOutput(InputSlot, atlas, scale),
            MachineType.Cutter => CreateCutterOutput(InputSlot, atlas, scale),
            MachineType.Seller => null, // Seller consumes items
            _ => null
        };

        InputSlot = null;
    }

    private Ore CreateSmelterOutput(Ore input, TextureAtlas atlas, float scale)
    {
        return input.Type switch
        {
            OreType.Iron => CreateOreIfSpriteExists("iron-smelted", OreType.Iron, OreState.Smelted, atlas, scale),
            OreType.Copper => CreateOreIfSpriteExists("copper-smelted", OreType.Copper, OreState.Smelted, atlas, scale),
            OreType.Sand => CreateOreIfSpriteExists("glass", OreType.Sand, OreState.Smelted, atlas, scale),
            _ => null // Rubber and others can't be smelted
        };
    }

    private Ore CreateForgeOutput(Ore input, TextureAtlas atlas, float scale)
    {
        return input.Type switch
        {
            OreType.Iron when input.State == OreState.Smelted =>
                CreateOreIfSpriteExists("iron-plate", OreType.Iron, OreState.Plate, atlas, scale),
            _ => null
        };
    }

    private Ore CreateCutterOutput(Ore input, TextureAtlas atlas, float scale)
    {
        // Only copper smelted can be turned into wire
        return input.Type switch
        {
            OreType.Copper when input.State == OreState.Smelted =>
                CreateOreIfSpriteExists("copper-wire", OreType.Copper, OreState.Wire, atlas, scale),
            _ => null
        };
    }

    private Ore CreateOreIfSpriteExists(string spriteName, OreType oreType, OreState oreState, TextureAtlas atlas, float scale)
    {
        try
        {
            // Try to create the sprite to see if it exists
            var testSprite = atlas.CreateSprite(spriteName);
            if (testSprite != null)
            {
                return new Ore(oreType, oreState, atlas, scale);
            }
        }
        catch (Exception)
        {
            // Sprite doesn't exist
            System.Diagnostics.Debug.WriteLine($"Sprite '{spriteName}' not found in atlas");
        }
        return null;
    }

    private void CompleteAssemblerProcessing(TextureAtlas atlas, float scale)
    {
        if (CurrentRecipe == null) return;

        // Create output item
        OutputSlot = new Ore(CurrentRecipe.Output.type, CurrentRecipe.Output.state, atlas, scale);

        // Clear used input slots (remove items used in recipe)
        var usedInputs = new List<(OreType, OreState)>(CurrentRecipe.Inputs);
        var slotsToRemove = new List<Direction>();

        foreach (var slot in InputSlots)
        {
            var itemKey = (slot.Value.Type, slot.Value.State);
            if (usedInputs.Contains(itemKey))
            {
                usedInputs.Remove(itemKey);
                slotsToRemove.Add(slot.Key);
            }
        }

        foreach (var direction in slotsToRemove)
        {
            InputSlots.Remove(direction);
        }

        CurrentRecipe = null;
    }

    public Ore TryExtractOutput()
    {
        if (OutputSlot != null)
        {
            var output = OutputSlot;
            OutputSlot = null;
            return output;
        }
        return null;
    }

    public bool HasOutput()
    {
        return OutputSlot != null;
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
        State = oreState;

        string spriteName = GetSpriteName(oreType, oreState);

        try
        {
            OreSprite = atlas.CreateSprite(spriteName);
            OreSprite.Scale = new Vector2(scale, scale);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating sprite for {spriteName}: {ex.Message}");
            throw;
        }
    }

    private string GetSpriteName(OreType oreType, OreState oreState)
    {
        // Special case for sand smelted -> glass
        if (oreType == OreType.Sand && oreState == OreState.Smelted)
        {
            return "glass";
        }

        if (oreState == OreState.Manufactured)
        {
            return oreType switch
            {
                OreType.Chassis => "chassis",
                OreType.ECU => "ecu",
                OreType.Wheel => "wheel",
                OreType.Engine => "engine",
                OreType.Car => "car",
                _ => "iron-raw"
            };
        }

        string baseName = oreType switch
        {
            OreType.Iron => "iron",
            OreType.Copper => "copper",
            OreType.Sand => "sand",
            OreType.Rubber => "rubber",
            _ => "iron"
        };

        string stateSuffix = oreState switch
        {
            OreState.Tile => "-tile",
            OreState.Raw => "-raw",
            OreState.Smelted => "-smelted",
            OreState.Plate => "-plate",
            OreState.Wire => "-wire",
            _ => "-tile"
        };

        return baseName + stateSuffix;
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