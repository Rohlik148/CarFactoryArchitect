using MonoGameLibrary.Graphics;
using System;

namespace CarFactoryArchitect.Source.Items.Materials
{
    public class CutMaterial : BaseItem
    {
        public CutMaterial(OreType materialType, TextureAtlas atlas, float scale)
            : base(materialType, OreState.Wire, atlas, scale)
        {
            ValidateCutType(materialType);
        }

        protected override string GetSpriteName()
        {
            string baseName = Type switch
            {
                OreType.Copper => "copper",
                _ => "copper"
            };

            return baseName + "-wire";
        }

        private static void ValidateCutType(OreType type)
        {
            if (type is not OreType.Copper)
            {
                throw new ArgumentException($"Invalid cut material type: {type}. Only Copper can be cut into wire.");
            }
        }
    }
}