using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.OpenGL;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

namespace CarFactoryArchitect;

public class Game1 : Core
{
    private Conveyor _conveyor;

    private const Single SizeScale = 3.0f;

    public Game1() : base("Car Factory Architect", 1920, 1080, true)
    {

    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        TextureAtlas myAtlas = TextureAtlas.FromFile(Content, "textures/my-atlas.xml");

        _conveyor = new Conveyor(myAtlas, SizeScale);
    }

    protected override void Update(GameTime gameTime)
    {
        _conveyor.ConveyorUp.Update(gameTime);

        // Create a bounding rectangle for the screen.
        Rectangle screenBounds = new Rectangle(
            0,
            0,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight
        );

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        GraphicsDevice.Clear(Color.WhiteSmoke);

        // Begin the sprite batch to prepare for rendering.
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _conveyor.ConveyorUp.Draw(SpriteBatch, new Vector2(0, 0));

        // Always end the sprite batch when finished.
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
