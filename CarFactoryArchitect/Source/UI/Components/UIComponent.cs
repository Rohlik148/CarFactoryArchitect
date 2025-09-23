using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CarFactoryArchitect.Source.UI.Components;

public abstract class UIComponent
{
    public Rectangle Bounds { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
}