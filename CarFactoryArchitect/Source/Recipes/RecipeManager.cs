using System.Collections.Generic;
using System.Linq;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Recipes
{
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
}