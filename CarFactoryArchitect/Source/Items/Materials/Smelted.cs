using MonoGameLibrary.Graphics;
using System;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Items.Materials
{
    public class SmeltedMaterial : BaseItem
    {
        public SmeltedMaterial(OreType materialType, TextureAtlas atlas, float scale)
            : base(materialType, OreState.Smelted, atlas, scale)
        {
            ValidateMaterialType(materialType);
        }

        protected override string GetSpriteName()
        {
            // Special case for sand smelted -> glass
            if (Type == OreType.Sand)
            {
                return "glass";
            }

            string baseName = Type switch
            {
                OreType.Iron => "iron",
                OreType.Copper => "copper",
                _ => "iron"
            };

            return baseName + "-smelted";
        }

        private static void ValidateMaterialType(OreType type)
        {
            if (type is not (OreType.Iron or OreType.Copper or OreType.Sand))
            {
                throw new ArgumentException($"Invalid smelted material type: {type}");
            }
        }
    }
}