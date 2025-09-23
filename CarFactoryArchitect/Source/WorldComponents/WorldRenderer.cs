using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Machines;

namespace CarFactoryArchitect.Source.WorldComponents;

public class WorldRenderer
{
    private Texture2D _gridLineTexture;
    private readonly Color _gridLineColor = Color.Gray * 0.3f;
    private const int GridLineThickness = 2;
    private bool _isInitialized = false;

    private readonly int _tileSize;
    private readonly int _gridSize;

    public WorldRenderer(int tileSize, int gridSize)
    {
        _tileSize = tileSize;
        _gridSize = gridSize;
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        if (graphicsDevice == null)
        {
            Console.WriteLine("Warning: GraphicsDevice is null, skipping grid texture creation");
            return;
        }

        CreateGridLineTexture(graphicsDevice);
        _isInitialized = true;
    }

    private void CreateGridLineTexture(GraphicsDevice graphicsDevice)
    {
        try
        {
            _gridLineTexture = new Texture2D(graphicsDevice, 1, 1);
            _gridLineTexture.SetData(new[] { Color.White });
            Console.WriteLine("Grid texture created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create grid texture: {ex.Message}");
            _gridLineTexture = null;
        }
    }

    private void EnsureGridTexture()
    {
        if (_gridLineTexture == null && !_isInitialized && GameEngine.GraphicsDevice != null)
        {
            Initialize(GameEngine.GraphicsDevice);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera, TileManager tileManager, ConveyorItemManager conveyorItemManager)
    {
        var transform = camera.GetTransformMatrix();

        spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: transform
        );

        DrawGrid(spriteBatch, camera);
        DrawTiles(spriteBatch, camera, tileManager, conveyorItemManager);

        spriteBatch.End();
    }

    private void DrawGrid(SpriteBatch spriteBatch, Camera camera)
    {
        EnsureGridTexture();
        if (_gridLineTexture == null) return;
        if (camera.Zoom < 0.8f) return;

        Vector2 topLeft = camera.ScreenToWorld(Vector2.Zero);
        Vector2 bottomRight = camera.ScreenToWorld(new Vector2(
            GameEngine.Graphics.PreferredBackBufferWidth,
            GameEngine.Graphics.PreferredBackBufferHeight));

        int startX = Math.Max(0, (int)(topLeft.X / _tileSize) - 1);
        int endX = Math.Min(_gridSize, (int)(bottomRight.X / _tileSize) + 2);
        int startY = Math.Max(0, (int)(topLeft.Y / _tileSize) - 1);
        int endY = Math.Min(_gridSize, (int)(bottomRight.Y / _tileSize) + 2);

        // Draw vertical lines
        for (int x = startX; x <= endX; x++)
        {
            if (x >= 0 && x <= _gridSize)
            {
                float worldX = x * _tileSize;
                Rectangle lineRect = new Rectangle((int)worldX, 0, GridLineThickness, _gridSize * _tileSize);
                spriteBatch.Draw(_gridLineTexture, lineRect, _gridLineColor);
            }
        }

        // Draw horizontal lines
        for (int y = startY; y <= endY; y++)
        {
            if (y >= 0 && y <= _gridSize)
            {
                float worldY = y * _tileSize;
                Rectangle lineRect = new Rectangle(0, (int)worldY, _gridSize * _tileSize, GridLineThickness);
                spriteBatch.Draw(_gridLineTexture, lineRect, _gridLineColor);
            }
        }
    }

    private void DrawTiles(SpriteBatch spriteBatch, Camera camera, TileManager tileManager, ConveyorItemManager conveyorItemManager)
    {
        foreach (var kvp in tileManager.GetAllTiles())
        {
            Point gridPos = kvp.Key;
            object tile = kvp.Value;
            Vector2 worldPos = camera.GridToWorld(gridPos.X, gridPos.Y, _tileSize);

            DrawTile(spriteBatch, tile, worldPos, gridPos, conveyorItemManager);
        }
    }

    private void DrawTile(SpriteBatch spriteBatch, object tile, Vector2 position, Point gridPos, ConveyorItemManager conveyorItemManager)
    {
        switch (tile)
        {
            case Conveyor conveyor:
                conveyor.Draw(spriteBatch, position);

                var itemOnConveyor = conveyorItemManager.GetItemOnConveyor(gridPos.X, gridPos.Y);
                if (itemOnConveyor != null)
                {
                    itemOnConveyor.ItemSprite.Draw(spriteBatch, position);
                }

                int queueCount = conveyorItemManager.GetQueueCount(gridPos.X, gridPos.Y);
                if (queueCount > 0)
                {
                    for (int i = 0; i < Math.Min(queueCount, 3); i++)
                    {
                        Vector2 queueIndicatorPos = position + new Vector2(2 + i * 4, 2);
                    }
                }
                break;

            case IMachine machine:
                machine.Draw(spriteBatch, position);
                break;
            case IItem item:
                item.ItemSprite.Draw(spriteBatch, position);
                break;
        }
    }
}