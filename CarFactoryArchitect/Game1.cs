using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

using CarFactoryArchitect.Source;
using CarFactoryArchitect.Source.Controls;
using CarFactoryArchitect.Source.UI;
using CarFactoryArchitect.Source.Maps;

namespace CarFactoryArchitect;

public class Game1 : GameEngine
{
    private World _world;
    private UIManager _ui;
    private TextureAtlas _atlas;
    private GameInputManager _inputManager;

    private const float SizeScale = 3.0f;

    public Game1() : base("Car Factory Architect", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _atlas = TextureAtlas.FromFile(Content, "textures/my-atlas.xml");

        _world = new World(_atlas, SizeScale);
        _world.Initialize(GraphicsDevice);

        _ui = new UIManager(_atlas, _world, SizeScale);
        _ui.Initialize(GraphicsDevice);

        _inputManager = new GameInputManager(_world, _ui.BuildPanel, _atlas, SizeScale);

        MapLoader.LoadMap("level1.txt", _world, _atlas, SizeScale);
    }

    protected override void Update(GameTime gameTime)
    {
        _inputManager.Update(gameTime);
        _world.Update(gameTime);
        _ui.Update(gameTime);

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
