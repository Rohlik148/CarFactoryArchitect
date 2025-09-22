using System;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Items.Materials;
using CarFactoryArchitect.Source.Items.Products;

namespace CarFactoryArchitect.Source.Items
{
    public static class ItemFactory
    {
        public static IItem CreateItem(OreType oreType, OreState oreState, TextureAtlas atlas, float scale)
        {
            return oreState switch
            {
                OreState.Tile => new TileOre(oreType, atlas, scale),
                OreState.Raw => new RawMaterial(oreType, atlas, scale),
                OreState.Smelted => new SmeltedMaterial(oreType, atlas, scale),
                OreState.Plate => new ForgedMaterial(oreType, atlas, scale),      // New
                OreState.Wire => new CutMaterial(oreType, atlas, scale),         // New
                OreState.Manufactured => new ManufacturedProduct(oreType, atlas, scale),
                _ => throw new ArgumentException($"Unknown ore state: {oreState}")
            };
        }

        public static IItem CreateRawMaterial(OreType materialType, TextureAtlas atlas, float scale)
        {
            return new RawMaterial(materialType, atlas, scale);
        }

        public static IItem CreateTileOre(OreType oreType, TextureAtlas atlas, float scale)
        {
            return new TileOre(oreType, atlas, scale);
        }

        public static IItem CreateManufacturedProduct(OreType productType, TextureAtlas atlas, float scale)
        {
            return new ManufacturedProduct(productType, atlas, scale);
        }

        // New specific factory methods
        public static IItem CreateForgedMaterial(OreType materialType, TextureAtlas atlas, float scale)
        {
            return new ForgedMaterial(materialType, atlas, scale);
        }

        public static IItem CreateCutMaterial(OreType materialType, TextureAtlas atlas, float scale)
        {
            return new CutMaterial(materialType, atlas, scale);
        }

        // Validation method to check if a combination is valid
        public static bool IsValidCombination(OreType oreType, OreState oreState)
        {
            return oreState switch
            {
                OreState.Tile => oreType is OreType.Iron or OreType.Copper or OreType.Sand or OreType.Rubber,
                OreState.Raw => oreType is OreType.Iron or OreType.Copper or OreType.Sand or OreType.Rubber,
                OreState.Smelted => oreType is OreType.Iron or OreType.Copper or OreType.Sand,
                OreState.Plate => oreType == OreType.Iron,          // Only Iron can be forged
                OreState.Wire => oreType == OreType.Copper,         // Only Copper can be cut
                OreState.Manufactured => oreType is OreType.Chassis or OreType.ECU or OreType.Wheel or OreType.Engine or OreType.Car,
                _ => false
            };
        }
    }
}