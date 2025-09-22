using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;

namespace CarFactoryArchitect.Source.Items
{
    public interface IItem
    {
        OreType Type { get; }
        OreState State { get; }
        Sprite ItemSprite { get; }
        void Draw(SpriteBatch spriteBatch, Vector2 position);
    }
}