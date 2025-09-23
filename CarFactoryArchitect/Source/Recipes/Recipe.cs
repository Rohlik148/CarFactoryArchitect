using System.Collections.Generic;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Recipes
{
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
}