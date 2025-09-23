using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using MonoGameLibrary;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Machines;
using CarFactoryArchitect.Source.Core;
using CarFactoryArchitect.Source.Systems;
using CarFactoryArchitect.Source.WorldComponents;

namespace CarFactoryArchitect.Source
{
    public class World
    {
        // Core components
        private readonly TileManager _tileManager;
        private readonly ConveyorItemManager _conveyorItemManager;
        private readonly Camera _camera;
        private readonly WorldRenderer _renderer;
        private readonly WorldSystem _worldSystem;

        // Resources
        private readonly TextureAtlas _atlas;
        private readonly float _scale;

        // World settings
        public int TileSize { get; } = 48;
        public int GridSize { get; } = 16;

        // Expose camera properties
        public Vector2 CameraPosition
        {
            get => _camera.Position;
            set => _camera.Position = value;
        }

        public float Zoom
        {
            get => _camera.Zoom;
            set => _camera.SetZoom(value);
        }

        public World(TextureAtlas atlas, float scale)
        {
            _atlas = atlas;
            _scale = scale;

            // Initialize components
            _tileManager = new TileManager(GridSize);
            _conveyorItemManager = new ConveyorItemManager(_tileManager);
            _renderer = new WorldRenderer(TileSize, GridSize);

            // Initialize camera
            float worldWidth = GridSize * TileSize;
            float worldHeight = GridSize * TileSize;
            float screenWidth = GameEngine.Graphics.PreferredBackBufferWidth;
            float screenHeight = GameEngine.Graphics.PreferredBackBufferHeight;

            Vector2 initialCameraPos = new Vector2(
                (worldWidth - screenWidth) / 2,
                (worldHeight - screenHeight) / 2
            );
            _camera = new Camera(initialCameraPos);

            // Initialize world system
            _worldSystem = new WorldSystem(this, atlas, scale);
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _renderer.Initialize(graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            _conveyorItemManager.ProcessAllConveyorQueues();
            _worldSystem.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _renderer.Draw(spriteBatch, _camera, _tileManager, _conveyorItemManager);
        }

        // Coordinate conversion methods
        public bool IsInBounds(int x, int y) => _tileManager.IsInBounds(x, y);
        public Point ScreenToGrid(Vector2 screenPosition) => _camera.ScreenToGrid(screenPosition, TileSize);
        public Vector2 ScreenToWorld(Vector2 screenPosition) => _camera.ScreenToWorld(screenPosition);
        public Vector2 GridToWorld(int gridX, int gridY) => _camera.GridToWorld(gridX, gridY, TileSize);

        // Tile management
        public void PlaceTile(int x, int y, object tile) => _tileManager.PlaceTile(x, y, tile);
        public void RemoveTile(int x, int y) => _tileManager.RemoveTile(x, y, _conveyorItemManager.CleanupConveyorData);
        public object GetTile(int x, int y) => _tileManager.GetTile(x, y);

        // Conveyor item management
        public IItem GetItemOnConveyor(int x, int y) => _conveyorItemManager.GetItemOnConveyor(x, y);
        public bool HasSpaceOnConveyor(int x, int y) => _conveyorItemManager.HasSpaceOnConveyor(x, y);
        public bool HasSpaceInQueue(int x, int y) => _conveyorItemManager.HasSpaceInQueue(x, y);
        public bool TryPlaceItemOnConveyor(int x, int y, IItem item) => _conveyorItemManager.TryPlaceItemOnConveyor(x, y, item);
        public void PlaceItemOnConveyor(int x, int y, IItem item) => _conveyorItemManager.TryPlaceItemOnConveyor(x, y, item);
        public IItem RemoveItemFromConveyor(int x, int y) => _conveyorItemManager.RemoveItemFromConveyor(x, y);
        public int GetQueueCount(int x, int y) => _conveyorItemManager.GetQueueCount(x, y);

        // Ore management
        public bool HasUnderlyingOre(int x, int y) => _tileManager.HasUnderlyingOre(x, y);
        public IItem GetUnderlyingOre(int x, int y) => _tileManager.GetUnderlyingOre(x, y);
        public IItem CreateRawOre(OreType oreType) => ItemFactory.CreateRawMaterial(oreType, _atlas, _scale);

        // Position queries
        public System.Collections.Generic.IEnumerable<Point> GetAllConveyorPositions() => _tileManager.GetAllConveyorPositions();
        public System.Collections.Generic.IEnumerable<Point> GetAllMachinePositions() => _tileManager.GetAllMachinePositions();
        public System.Collections.Generic.IEnumerable<Point> GetAllExtractorPositions() => _tileManager.GetAllExtractorPositions();

        public void MoveItem(Point from, Point to, IItem item)
        {
            try
            {
                var removedItem = RemoveItemFromConveyor(from.X, from.Y);
                if (removedItem == null)
                {
                    return;
                }

                var targetTile = GetTile(to.X, to.Y);
                if (targetTile == null)
                {
                    TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                    return;
                }

                if (targetTile is Conveyor)
                {
                    if (!TryPlaceItemOnConveyor(to.X, to.Y, removedItem))
                    {
                        TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                    }
                }
                else if (targetTile is IMachine machine)
                {
                    if (machine.Type == MachineType.Assembler)
                    {
                        Direction fromDirection = GetDirectionFromPoints(from, to);
                        if (!machine.TryAcceptInput(removedItem, fromDirection))
                        {
                            TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                        }
                    }
                    else
                    {
                        if (!machine.TryAcceptInput(removedItem))
                        {
                            TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                        }
                    }
                }
                else
                {
                    TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MoveItem exception: {ex.Message}");
            }
        }

        private Direction GetDirectionFromPoints(Point from, Point to)
        {
            int deltaX = to.X - from.X;
            int deltaY = to.Y - from.Y;

            if (deltaX > 0) return Direction.Left;
            if (deltaX < 0) return Direction.Right;
            if (deltaY > 0) return Direction.Up;
            if (deltaY < 0) return Direction.Down;
            return Direction.Up;
        }

        // Helper methods
        public void PlaceConveyor(int x, int y, Conveyor conveyor) => PlaceTile(x, y, conveyor);
        public void PlaceMachine(int x, int y, IMachine machine) => PlaceTile(x, y, machine);
        public void PlaceOre(int x, int y, IItem item) => PlaceTile(x, y, item);
        public Rectangle GetGridBounds() => new Rectangle(0, 0, GridSize * TileSize, GridSize * TileSize);
    }
}