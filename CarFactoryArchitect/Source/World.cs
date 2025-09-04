using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace CarFactoryArchitect.Source
{
    public class World
    {
        // Tile storage
        private Dictionary<Point, object> _tiles;

        // Camera properties
        public Vector2 CameraPosition { get; set; }
        public float Zoom { get; set; } = 1.0f;

        // World settings
        public int TileSize { get; set; } = 48; // Size of each tile in pixels
        public int GridSize { get; set; } = 16; // Number of tiles in each direction

        // Grid appearance
        private Texture2D _gridLineTexture;
        private Color _gridLineColor = Color.Gray * 0.3f;
        private const int GridLineThickness = 2;
        private bool _isInitialized = false;

        // Zoom limits
        private const float MinZoom = 0.6f;
        private const float MaxZoom = 3.0f;
        private const float ZoomSpeed = 0.2f;

        public World()
        {
            _tiles = new Dictionary<Point, object>();
            CameraPosition = Vector2.Zero;
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
                // Create a 1x1 white pixel texture for drawing grid lines
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
            if (_gridLineTexture == null && !_isInitialized && Core.GraphicsDevice != null)
            {
                Initialize(Core.GraphicsDevice);
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < GridSize && y >= 0 && y < GridSize;
        }

        // Place tiles in the world
        public void PlaceTile(int x, int y, object tile)
        {
            if (IsInBounds(x, y))
            {
                _tiles[new Point(x, y)] = tile;
            }
        }

        public void RemoveTile(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                _tiles.Remove(new Point(x, y));
            }
        }

        public object GetTile(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                _tiles.TryGetValue(new Point(x, y), out object tile);
                return tile;
            }
            return null;
        }

        // Convert screen position to world grid coordinates
        public Point ScreenToGrid(Vector2 screenPosition)
        {
            Vector2 worldPos = ScreenToWorld(screenPosition);
            return new Point(
                (int)(worldPos.X / TileSize),
                (int)(worldPos.Y / TileSize)
            );
        }

        // Convert screen position to world position
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return (screenPosition / Zoom) + CameraPosition;
        }

        // Convert grid coordinates to world position
        public Vector2 GridToWorld(int gridX, int gridY)
        {
            return new Vector2(gridX * TileSize, gridY * TileSize);
        }

        public void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            UpdateAnimatedTiles(gameTime);
        }

        private void HandleInput(GameTime gameTime)
        {
            var input = Core.Input;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float cameraSpeed = 400f * deltaTime / Zoom; // Slower when zoomed in

            // Camera movement with WASD
            if (input.Keyboard.IsKeyDown(Keys.W))
                CameraPosition += new Vector2(0, -cameraSpeed);
            if (input.Keyboard.IsKeyDown(Keys.S))
                CameraPosition += new Vector2(0, cameraSpeed);
            if (input.Keyboard.IsKeyDown(Keys.A))
                CameraPosition += new Vector2(-cameraSpeed, 0);
            if (input.Keyboard.IsKeyDown(Keys.D))
                CameraPosition += new Vector2(cameraSpeed, 0);

            // Zoom with mouse wheel
            if (input.Mouse.ScrollWheelDelta != 0)
            {
                Vector2 mousePos = new Vector2(input.Mouse.X, input.Mouse.Y);

                // Get world position before zoom
                Vector2 worldBeforeZoom = ScreenToWorld(mousePos);

                // Apply zoom
                float zoomChange = input.Mouse.ScrollWheelDelta > 0 ? ZoomSpeed : -ZoomSpeed;
                float oldZoom = Zoom;
                Zoom = MathHelper.Clamp(Zoom + zoomChange, MinZoom, MaxZoom);

                // Only adjust camera if zoom actually changed
                if (Zoom != oldZoom)
                {
                    // Get world position after zoom
                    Vector2 worldAfterZoom = ScreenToWorld(mousePos);

                    // Adjust camera to compensate
                    CameraPosition += worldBeforeZoom - worldAfterZoom;
                }
            }
        }
        private void UpdateAnimatedTiles(GameTime gameTime)
        {
            foreach (var kvp in _tiles)
            {
                object tile = kvp.Value;

                switch (tile)
                {
                    case Conveyor conveyor:
                        conveyor.Update(gameTime); // Use the new Update method
                        break;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Create transformation matrix for camera
            Matrix transform = Matrix.CreateTranslation(-CameraPosition.X, -CameraPosition.Y, 0) *
                              Matrix.CreateScale(Zoom);

            spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: transform
            );

            DrawGrid(spriteBatch);

            // Draw tiles
            foreach (var kvp in _tiles)
            {
                Point gridPos = kvp.Key;
                object tile = kvp.Value;
                Vector2 worldPos = GridToWorld(gridPos.X, gridPos.Y);

                DrawTile(spriteBatch, tile, worldPos);
            }

            spriteBatch.End();
        }

        private void DrawGrid(SpriteBatch spriteBatch)
        {
            // Ensure grid texture exists
            EnsureGridTexture();

            if (_gridLineTexture == null) return;

            // Only draw grid lines when zoomed in enough to see them clearly
            if (Zoom < 0.8f) return;

            // Calculate which grid lines are visible on screen
            Vector2 topLeft = ScreenToWorld(Vector2.Zero);
            Vector2 bottomRight = ScreenToWorld(new Vector2(Core.Graphics.PreferredBackBufferWidth, Core.Graphics.PreferredBackBufferHeight));

            int startX = Math.Max(0, (int)(topLeft.X / TileSize) - 1);
            int endX = Math.Min(GridSize, (int)(bottomRight.X / TileSize) + 2);
            int startY = Math.Max(0, (int)(topLeft.Y / TileSize) - 1);
            int endY = Math.Min(GridSize, (int)(bottomRight.Y / TileSize) + 2);

            // Draw vertical lines
            for (int x = startX; x <= endX; x++)
            {
                if (x >= 0 && x <= GridSize)
                {
                    float worldX = x * TileSize;
                    Rectangle lineRect = new Rectangle(
                        (int)worldX,
                        0,
                        GridLineThickness,
                        GridSize * TileSize
                    );
                    spriteBatch.Draw(_gridLineTexture, lineRect, _gridLineColor);
                }
            }

            // Draw horizontal lines
            for (int y = startY; y <= endY; y++)
            {
                if (y >= 0 && y <= GridSize)
                {
                    float worldY = y * TileSize;
                    Rectangle lineRect = new Rectangle(
                        0,
                        (int)worldY,
                        GridSize * TileSize,
                        GridLineThickness
                    );
                    spriteBatch.Draw(_gridLineTexture, lineRect, _gridLineColor);
                }
            }
        }

        private void DrawTile(SpriteBatch spriteBatch, object tile, Vector2 position)
        {
            switch (tile)
            {
                case Conveyor conveyor:
                    conveyor.Draw(spriteBatch, position);
                    break;
                case Machine machine:
                    machine.MachineSprite.Draw(spriteBatch, position);
                    break;
                case Ore ore:
                    ore.OreSprite.Draw(spriteBatch, position);
                    break;
                case Seller seller:
                    seller.SellPoint.Draw(spriteBatch, position);
                    break;
            }
        }

        // Helper method to place different types of tiles easily
        public void PlaceConveyor(int x, int y, Conveyor conveyor)
        {
            PlaceTile(x, y, conveyor);
        }

        public void PlaceMachine(int x, int y, Machine machine)
        {
            PlaceTile(x, y, machine);
        }

        public void PlaceOre(int x, int y, Ore ore)
        {
            PlaceTile(x, y, ore);
        }

        public Rectangle GetGridBounds()
        {
            return new Rectangle(0, 0, GridSize * TileSize, GridSize * TileSize);
        }
    }
}