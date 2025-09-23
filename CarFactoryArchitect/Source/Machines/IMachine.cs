using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Machines
{
    public interface IMachine
    {
        MachineType Type { get; }
        Direction Direction { get; set; }
        Sprite MachineSprite { get; }
        bool IsProcessing { get; }
        bool HasOutput { get; }
        bool CanAcceptInput(IItem item);
        bool CanAcceptInput(IItem item, Direction fromDirection);
        bool TryAcceptInput(IItem item);
        bool TryAcceptInput(IItem item, Direction fromDirection);
        IItem TryExtractOutput();
        void SetOutputSlot(IItem item);
        void Update(GameTime gameTime, TextureAtlas atlas, float scale);
        void Draw(SpriteBatch spriteBatch, Vector2 position);
    }
}