using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using System;

namespace CarFactoryArchitect.Source.Items
{
    public abstract class BaseItem : IItem
    {
        public OreType Type { get; protected set; }
        public OreState State { get; protected set; }
        public Sprite ItemSprite { get; protected set; }

        protected BaseItem(OreType oreType, OreState oreState, TextureAtlas atlas, float scale)
        {
            Type = oreType;
            State = oreState;
            SetupSprite(atlas, scale);
        }

        protected virtual void SetupSprite(TextureAtlas atlas, float scale)
        {
            string spriteName = GetSpriteName();
            try
            {
                ItemSprite = atlas.CreateSprite(spriteName);
                ItemSprite.Scale = new Vector2(scale, scale);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating sprite for {spriteName}: {ex.Message}");
                throw;
            }
        }

        protected abstract string GetSpriteName();

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            ItemSprite?.Draw(spriteBatch, position);
        }
    }
}