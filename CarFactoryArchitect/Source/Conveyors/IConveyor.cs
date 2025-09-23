using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Conveyors
{
    public interface IConveyor
    {
        Direction Direction { get; }
        ConveyorType Type { get; }
        AnimatedSprite Sprite { get; }
        float Speed { get; }

        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch, Vector2 position);
    }
}