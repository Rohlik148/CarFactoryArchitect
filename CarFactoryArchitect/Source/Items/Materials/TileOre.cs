using MonoGameLibrary.Graphics;

namespace CarFactoryArchitect.Source.Items
{
    public class TileOre : BaseItem
    {
        public TileOre(OreType oreType, TextureAtlas atlas, float scale)
            : base(oreType, OreState.Tile, atlas, scale)
        {
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

            return baseName + "-tile";
        }
    }
}