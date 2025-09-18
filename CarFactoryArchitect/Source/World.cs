using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<Point, Ore> _underlyingOres;

        // Conveyors
        private Dictionary<Point, Ore> _itemsOnConveyors;
        private Dictionary<Point, Queue<Ore>> _conveyorInputQueues;
        private const int MaxQueueSize = 0;
        private ConveyorSystem _conveyorSystem;

        // General
        private TextureAtlas _atlas;
        private float _scale;

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

        public World(TextureAtlas atlas, float scale)
        {
            _atlas = atlas;
            _scale = scale;

            _tiles = new Dictionary<Point, object>();
            _underlyingOres = new Dictionary<Point, Ore>();
            _itemsOnConveyors = new Dictionary<Point, Ore>();
            _conveyorInputQueues = new Dictionary<Point, Queue<Ore>>(); // Queue for waiting items
            _conveyorSystem = new ConveyorSystem(this);

            float worldWidth = GridSize * TileSize;
            float worldHeight = GridSize * TileSize;
            float screenWidth = Core.Graphics.PreferredBackBufferWidth;
            float screenHeight = Core.Graphics.PreferredBackBufferHeight;

            CameraPosition = new Vector2(
                (worldWidth - screenWidth) / 2,
                (worldHeight - screenHeight) / 2
            );
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
            if (!IsInBounds(x, y)) return;

            Point point = new Point(x, y);
            var existingTile = GetTile(x, y);

            // If there's an ore tile, store it as underlying ore
            if (existingTile is Ore ore)
            {
                _underlyingOres[point] = ore;
            }

            // Place the new tile
            _tiles[point] = tile;
        }

        public void RemoveTile(int x, int y)
        {
            if (!IsInBounds(x, y)) return;

            Point point = new Point(x, y);
            var existingTile = GetTile(x, y);

            // Prevent deletion of ore tiles
            if (existingTile is Ore ore && ore.State == OreState.Tile)
            {
                return;
            }

            // If removing a conveyor, clean up all associated items and queues
            if (existingTile is Conveyor)
            {
                CleanupConveyorData(x, y);
            }

            // Remove the current tile
            _tiles.Remove(point);

            // If there's an underlying ore, restore it
            if (_underlyingOres.TryGetValue(point, out Ore underlyingOre))
            {
                _tiles[point] = underlyingOre;
                _underlyingOres.Remove(point);
            }
        }

        private void CleanupConveyorData(int x, int y)
        {
            var point = new Point(x, y);

            // Remove any item currently on the conveyor
            _itemsOnConveyors.Remove(point);

            // Remove any queued items for this conveyor
            _conveyorInputQueues.Remove(point);

            // System.Diagnostics.Debug.WriteLine($"Cleaned up conveyor data at ({x}, {y})");
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

        public bool HasUnderlyingOre(int x, int y)
        {
            return _underlyingOres.ContainsKey(new Point(x, y));
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
            ProcessAllConveyorQueues();
            _conveyorSystem.Update(gameTime);
        }

        private void ProcessAllConveyorQueues()
        {
            // Process all conveyor queues to move items from queue to conveyor
            var pointsToProcess = _conveyorInputQueues.Keys.ToList();
            foreach (var point in pointsToProcess)
            {
                ProcessConveyorQueue(point.X, point.Y);
            }
        }

        public Ore GetItemOnConveyor(int x, int y)
        {
            _itemsOnConveyors.TryGetValue(new Point(x, y), out Ore item);
            return item;
        }

        public bool HasSpaceOnConveyor(int x, int y)
        {
            // Conveyor has space if there is no item currently on it
            return GetItemOnConveyor(x, y) == null;
        }

        public bool HasSpaceInQueue(int x, int y)
        {
            if (MaxQueueSize == 0)
            {
                return false;
            }

            // If Queue exists
            var point = new Point(x, y);
            if (_conveyorInputQueues.TryGetValue(point, out Queue<Ore> queue))
            {
                return queue.Count < MaxQueueSize;
            }
            return true;
        }

        public bool TryPlaceItemOnConveyor(int x, int y, Ore item)
        {
            var point = new Point(x, y);

            var tile = GetTile(x, y);
            if (!(tile is Conveyor))
            {
                return false;
            }

            // Check if conveyor is completely empty
            if (HasSpaceOnConveyor(x, y))
            {
                // Double-check before placing to prevent race conditions
                if (_itemsOnConveyors.ContainsKey(point))
                {
                    return false;
                }

                _itemsOnConveyors[point] = item;
                // System.Diagnostics.Debug.WriteLine($"Placed {item.Type} {item.State} on conveyor at ({x}, {y})");
                return true;
            }

            // If MaxQueueSize is 0, don't allow queuing at all
            if (MaxQueueSize == 0)
            {
                return false;
            }

            // If conveyor is occupied, try to add to queue
            if (HasSpaceInQueue(x, y))
            {
                if (!_conveyorInputQueues.ContainsKey(point))
                {
                    _conveyorInputQueues[point] = new Queue<Ore>();
                }
                _conveyorInputQueues[point].Enqueue(item);
                // System.Diagnostics.Debug.WriteLine($"Queued {item.Type} {item.State} for conveyor at ({x}, {y})");
                return true;
            }

            return false;
        }

        public void PlaceItemOnConveyor(int x, int y, Ore item)
        {
            TryPlaceItemOnConveyor(x, y, item);
        }

        public Ore RemoveItemFromConveyor(int x, int y)
        {
            var point = new Point(x, y);

            // Remove the item currently on the conveyor
            _itemsOnConveyors.TryGetValue(point, out Ore item);
            _itemsOnConveyors.Remove(point);

            // Move next item from queue to conveyor (if any)
            ProcessConveyorQueue(x, y);

            return item;
        }

        private void ProcessConveyorQueue(int x, int y)
        {
            var point = new Point(x, y);

            // If conveyor is now empty and there's a queue, move next item to conveyor
            if (!_itemsOnConveyors.ContainsKey(point) &&
                _conveyorInputQueues.TryGetValue(point, out Queue<Ore> queue) &&
                queue.Count > 0)
            {
                var nextItem = queue.Dequeue();
                _itemsOnConveyors[point] = nextItem;

                // Clean up empty queues
                if (queue.Count == 0)
                {
                    _conveyorInputQueues.Remove(point);
                }
            }
        }

        public int GetQueueCount(int x, int y)
        {
            var point = new Point(x, y);
            if (_conveyorInputQueues.TryGetValue(point, out Queue<Ore> queue))
            {
                return queue.Count;
            }
            return 0;
        }

        public void MoveItem(Point from, Point to, Ore item)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"MoveItem: Moving {item.Type} {item.State} from {from} to {to}");

                // Remove item from source
                var removedItem = RemoveItemFromConveyor(from.X, from.Y);
                if (removedItem == null)
                {
                    System.Diagnostics.Debug.WriteLine($"  No item found at source {from}");
                    return;
                }

                var targetTile = GetTile(to.X, to.Y);
                if (targetTile == null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Target tile at {to} is null, returning item to source");
                    TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                    return;
                }

                if (targetTile is Conveyor)
                {
                    System.Diagnostics.Debug.WriteLine($"  Target is conveyor at {to}");
                    // Try to place on target conveyor
                    if (!TryPlaceItemOnConveyor(to.X, to.Y, removedItem))
                    {
                        System.Diagnostics.Debug.WriteLine($"  Failed to place on target conveyor, returning to source");
                        TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  Successfully moved to conveyor at {to}");
                    }
                }
                else if (targetTile is Machine machine)
                {
                    System.Diagnostics.Debug.WriteLine($"  Target is {machine.Type} machine at {to}");
                    if (machine.Type == MachineType.Assembler)
                    {
                        // Calculate the direction from which the item is coming
                        Direction fromDirection = GetDirectionFromPoints(from, to);
                        if (!machine.TryAcceptInput(removedItem, fromDirection))
                        {
                            System.Diagnostics.Debug.WriteLine($"  Machine rejected {removedItem.Type}, returning to source");
                            // Machine rejected the item, put it back on source conveyor
                            TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  Machine accepted {removedItem.Type} from {fromDirection}");
                        }
                    }
                    else
                    {
                        // Try to give the item directly to the machine
                        if (!machine.TryAcceptInput(removedItem))
                        {
                            System.Diagnostics.Debug.WriteLine($"  Machine rejected {removedItem.Type}, returning to source");
                            // Machine rejected the item, put it back on source conveyor
                            TryPlaceItemOnConveyor(from.X, from.Y, removedItem);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"  Machine accepted {removedItem.Type}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  Unknown target type, returning to source");
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

            System.Diagnostics.Debug.WriteLine($"GetDirectionFromPoints: from={from}, to={to}, deltaX={deltaX}, deltaY={deltaY}");

            Direction result;
            if (deltaX > 0)
            {
                result = Direction.Left;  // Item moved left->right, so coming from Left
                System.Diagnostics.Debug.WriteLine($"  -> deltaX > 0: returning Direction.Left");
            }
            else if (deltaX < 0)
            {
                result = Direction.Right; // Item moved right->left, so coming from Right
                System.Diagnostics.Debug.WriteLine($"  -> deltaX < 0: returning Direction.Right");
            }
            else if (deltaY > 0)
            {
                result = Direction.Up;    // Item moved up->down, so coming from Up
                System.Diagnostics.Debug.WriteLine($"  -> deltaY > 0: returning Direction.Up");
            }
            else if (deltaY < 0)
            {
                result = Direction.Down;  // Item moved down->up, so coming from Down
                System.Diagnostics.Debug.WriteLine($"  -> deltaY < 0: returning Direction.Down");
            }
            else
            {
                result = Direction.Up; // Default
                System.Diagnostics.Debug.WriteLine($"  -> Default: returning Direction.Up");
            }

            System.Diagnostics.Debug.WriteLine($"  -> Final result: {result}");
            return result;
        }

        private void OutputProcessedItem(Machine machine, Ore item, Point position)
        {
            // Try to output in the machine's facing direction
            Point outputPos = GetNextPosition(position, machine.Direction);

            if (IsInBounds(outputPos.X, outputPos.Y))
            {
                var outputTile = GetTile(outputPos.X, outputPos.Y);
                if (outputTile is Conveyor)
                {
                    // Use the new queue-based placement
                    TryPlaceItemOnConveyor(outputPos.X, outputPos.Y, item);
                }
            }
        }

        private Point GetNextPosition(Point current, Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Point(current.X, current.Y - 1),
                Direction.Right => new Point(current.X + 1, current.Y),
                Direction.Down => new Point(current.X, current.Y + 1),
                Direction.Left => new Point(current.X - 1, current.Y),
                _ => current
            };
        }

        public IEnumerable<Point> GetAllConveyorPositions()
        {
            return _tiles.Where(kvp => kvp.Value is Conveyor).Select(kvp => kvp.Key);
        }

        public IEnumerable<Point> GetAllExtractorPositions()
        {
            return _tiles.Where(kvp => kvp.Value is Machine machine && machine.Type == MachineType.Extractor)
                         .Select(kvp => kvp.Key);
        }

        public Ore GetUnderlyingOre(int x, int y)
        {
            _underlyingOres.TryGetValue(new Point(x, y), out Ore ore);
            return ore;
        }

        public Ore CreateRawOre(OreType oreType)
        {
            return new Ore(oreType, OreState.Raw, _atlas, _scale);
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
                        conveyor.Update(gameTime);
                        break;
                    case Machine machine:
                        machine.Update(gameTime, _atlas, _scale);

                        // Try to output processed items
                        if (machine.HasOutput())
                        {
                            TryOutputMachineItem(machine, kvp.Key);
                        }
                        break;
                }
            }
        }

        private void TryOutputMachineItem(Machine machine, Point machinePos)
        {
            var outputItem = machine.TryExtractOutput();
            if (outputItem != null)
            {
                Point outputPos = GetNextPosition(machinePos, machine.Direction);

                if (IsInBounds(outputPos.X, outputPos.Y))
                {
                    var outputTile = GetTile(outputPos.X, outputPos.Y);
                    if (outputTile is Conveyor)
                    {
                        if (!TryPlaceItemOnConveyor(outputPos.X, outputPos.Y, outputItem))
                        {
                            machine.OutputSlot = outputItem;
                        }
                    }
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

                    // Draw the single item on the conveyor
                    var gridPos = new Point((int)(position.X / TileSize), (int)(position.Y / TileSize));
                    var itemOnConveyor = GetItemOnConveyor(gridPos.X, gridPos.Y);

                    if (itemOnConveyor != null)
                    {
                        // Center the item in the tile
                        itemOnConveyor.OreSprite.Draw(spriteBatch, position);
                    }

                    // Draw queue indicator if there are items waiting
                    int queueCount = GetQueueCount(gridPos.X, gridPos.Y);
                    if (queueCount > 0)
                    {
                        // Draw a small indicator showing queue count
                        for (int i = 0; i < Math.Min(queueCount, 3); i++)
                        {
                            Vector2 queueIndicatorPos = position + new Vector2(2 + i * 4, 2);
                        }
                    }
                    break;

                case Machine machine:
                    machine.Draw(spriteBatch, position);
                    break;
                case Ore ore:
                    ore.OreSprite.Draw(spriteBatch, position);
                    break;
            }
        }

        // Helper methods to place different types of tiles easily
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