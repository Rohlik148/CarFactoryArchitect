using MonoGameLibrary.Graphics;
using System;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Items.Materials
{
    public class RawMaterial : BaseItem
    {
        public RawMaterial(OreType materialType, TextureAtlas atlas, float scale)
            : base(materialType, OreState.Raw, atlas, scale)
        {
            ValidateMaterialType(materialType);
        }

        protected override string GetSpriteName()
        {
            string baseName = Type switch
            {
                OreType.Iron => "iron",
                OreType.Copper => "copper",
                OreType.Sand => "sand",
                OreType.Rubber => "rubber",
                _ => "iron"
            };

            return baseName + "-raw";
        }

        private static void ValidateMaterialType(OreType type)
        {
            if (type is not (OreType.Iron or OreType.Copper or OreType.Sand or OreType.Rubber))
            {
                throw new ArgumentException($"Invalid raw material type: {type}");
            }
        }
    }
}