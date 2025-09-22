using MonoGameLibrary.Graphics;
using System;

namespace CarFactoryArchitect.Source.Items.Materials
{
    public class ForgedMaterial : BaseItem
    {
        public ForgedMaterial(OreType materialType, TextureAtlas atlas, float scale)
            : base(materialType, OreState.Plate, atlas, scale)
        {
            ValidateForgedType(materialType);
        }

        protected override string GetSpriteName()
        {
            string baseName = Type switch
            {
                OreType.Iron => "iron",
                _ => "iron"
            };

            return baseName + "-plate";
        }

        private static void ValidateForgedType(OreType type)
        {
            if (type is not OreType.Iron)
            {
                throw new ArgumentException($"Invalid forged material type: {type}. Only Iron can be forged into plates.");
            }
        }
    }
}