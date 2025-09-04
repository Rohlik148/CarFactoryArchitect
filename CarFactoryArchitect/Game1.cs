using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.OpenGL;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Input;

using CarFactoryArchitect.Source;

namespace CarFactoryArchitect;

public class Game1 : Core
{
    private World _world;
    private UI _ui;
    private TextureAtlas _atlas;

    private Machine _ma;

    private const Single SizeScale = 3.0f;

    public Game1() : base("Car Factory Architect", 1920, 1080, false)
    {

    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _atlas = TextureAtlas.FromFile(Content, "textures/my-atlas.xml");

        _world = new World();
        _world.Initialize(GraphicsDevice);

        _ui = new UI(_atlas, SizeScale);
        _ui.Initialize(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        _world.Update(gameTime);
        _ui.Update(gameTime);

        // Handle tile placement with bounds checking
        if (Input.Mouse.WasButtonJustPressed(MouseButton.Left))
        {
            Point gridPos = _world.ScreenToGrid(new Vector2(Input.Mouse.X, Input.Mouse.Y));

            // Check if the position is within grid bounds
            if (_world.IsInBounds(gridPos.X, gridPos.Y))
            {
                // Check if the tile is already occupied
                var existingTile = _world.GetTile(gridPos.X, gridPos.Y);
                if (existingTile == null)
                {
                    // Place tile based on UI selection
                    switch (_ui.CurrentBuildMode)
                    {
                        case BuildMode.Conveyor:
                            var conveyor = _ui.CreateSelectedConveyor();
                            _world.PlaceConveyor(gridPos.X, gridPos.Y, conveyor);
                            break;
                        case BuildMode.Machine:
                            var machine = _ui.CreateSelectedMachine();
                            _world.PlaceMachine(gridPos.X, gridPos.Y, machine);
                            break;
                    }
                }
            }
        }

        // Handle tile deletion on right-click
        if (Input.Mouse.WasButtonJustPressed(MouseButton.Right))
        {
            Point gridPos = _world.ScreenToGrid(new Vector2(Input.Mouse.X, Input.Mouse.Y));
            if (_world.IsInBounds(gridPos.X, gridPos.Y))
            {
                _world.RemoveTile(gridPos.X, gridPos.Y);
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.WhiteSmoke);
        _world.Draw(SpriteBatch);
        _ui.Draw(SpriteBatch);
        base.Draw(gameTime);
    }
}
