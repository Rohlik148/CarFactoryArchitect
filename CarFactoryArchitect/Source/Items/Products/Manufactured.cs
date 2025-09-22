using MonoGameLibrary.Graphics;
using System;

namespace CarFactoryArchitect.Source.Items.Products
{
    public class ManufacturedProduct : BaseItem
    {
        public ManufacturedProduct(OreType productType, TextureAtlas atlas, float scale)
            : base(productType, OreState.Manufactured, atlas, scale)
        {
            ValidateProductType(productType);
        }

        protected override string GetSpriteName()
        {
            return Type switch
            {
                OreType.Chassis => "chassis",
                OreType.ECU => "ecu",
                OreType.Wheel => "wheel",
                OreType.Engine => "engine",
                OreType.Car => "car",
                _ => "chassis"
            };
        }

        private static void ValidateProductType(OreType type)
        {
            if (type is not (OreType.Chassis or OreType.ECU or OreType.Wheel or OreType.Engine or OreType.Car))
            {
                throw new ArgumentException($"Invalid manufactured product type: {type}");
            }
        }
    }
}