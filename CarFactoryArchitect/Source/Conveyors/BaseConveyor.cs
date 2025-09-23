using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;
using System;

namespace CarFactoryArchitect.Source.Conveyors
{
    public abstract class BaseConveyor : IConveyor
    {
        public Direction Direction { get; protected set; }
        public ConveyorType Type { get; protected set; }
        public AnimatedSprite Sprite { get; protected set; }
        public abstract float Speed { get; }

        protected BaseConveyor(ConveyorType type, Direction direction, TextureAtlas atlas, float scale)
        {
            Type = type;
            Direction = direction;
            SetupSprite(atlas, scale);
        }

        protected virtual void SetupSprite(TextureAtlas atlas, float scale)
        {
            string animationName = GetAnimationName();
            try
            {
                Sprite = atlas.CreateAnimatedSprite(animationName);
                Sprite.Scale = new Vector2(scale, scale);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating conveyor sprite for {animationName}: {ex.Message}");
                throw;
            }
        }

        protected abstract string GetAnimationName();

        public virtual void Update(GameTime gameTime)
        {
            Sprite?.Update(gameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            Sprite?.Draw(spriteBatch, position);
        }
    }
}