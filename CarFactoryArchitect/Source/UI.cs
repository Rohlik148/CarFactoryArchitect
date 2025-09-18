using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace CarFactoryArchitect.Source
{
    public enum BuildMode
    {
        Conveyor,
        Machine
    }

    public class UI
    {
        // UI textures
        private Texture2D _pixelTexture;
        private TextureAtlas _atlas;
        private World _world;
        private bool _isInitialized = false;

        // UI state
        public BuildMode CurrentBuildMode { get; private set; } = BuildMode.Conveyor;
        public Direction ConveyorDirection { get; private set; } = Direction.Up;
        public Direction MachineDirection { get; private set; } = Direction.Up;
        public MachineType SelectedMachineType { get; private set; } = MachineType.Smelter;
        public int SelectedMachineIndex { get; private set; } = 0;

        // UI layout constants
        private const int IndicatorSize = 80;
        private const int IndicatorSpacing = 20;
        private const int BottomMargin = 20;
        private const int SelectionBorderWidth = 4;

        // UI colors
        private readonly Color _backgroundColor = Color.Black * 0.7f;
        private readonly Color _selectionColor = Color.Green;
        private readonly Color _borderColor = Color.White;

        // Available machine types for cycling
        private readonly MachineType[] _machineTypes = {
            MachineType.Smelter,
            MachineType.Forge,
            MachineType.Cutter,
            MachineType.Assembler,
            MachineType.Extractor,
            MachineType.Seller
        };

        private Single _scale;

        public UI(TextureAtlas atlas, World world, Single scale = 1.0f)
        {
            _atlas = atlas;
            _world = world;
            _scale = scale;
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
            {
                Console.WriteLine("Warning: GraphicsDevice is null, skipping UI texture creation");
                return;
            }

            CreatePixelTexture(graphicsDevice);
            _isInitialized = true;
        }

        private void CreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            try
            {
                // Create a 1x1 white pixel texture for drawing UI elements
                _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create UI pixel texture: {ex.Message}");
                _pixelTexture = null;
            }
        }

        // Create texture when first needed
        private void EnsurePixelTexture()
        {
            if (_pixelTexture == null && !_isInitialized && Core.GraphicsDevice != null)
            {
                Initialize(Core.GraphicsDevice);
            }
        }

        public void Update(GameTime gameTime)
        {
            HandleInput();
        }

        private void HandleInput()
        {
            var input = Core.Input;

            // Handle number key selection (1, 2)
            if (input.Keyboard.WasKeyJustPressed(Keys.D1))
            {
                CurrentBuildMode = BuildMode.Conveyor;
            }
            else if (input.Keyboard.WasKeyJustPressed(Keys.D2))
            {
                CurrentBuildMode = BuildMode.Machine;
            }

            // Handle rotation with R key
            if (input.Keyboard.WasKeyJustPressed(Keys.R))
            {
                switch (CurrentBuildMode)
                {
                    case BuildMode.Conveyor:
                        RotateConveyorDirection();
                        break;
                    case BuildMode.Machine:
                        if (SelectedMachineType != MachineType.Seller)
                        {
                            RotateMachineDirection();
                        }
                        break;
                }
            }

            // Handle cycling machine types with T key
            if (input.Keyboard.WasKeyJustPressed(Keys.T) && CurrentBuildMode == BuildMode.Machine)
            {
                CycleMachineType();
            }

            // WIP
            if (input.Keyboard.WasKeyJustPressed(Keys.F1))
            {
                // Clear current world and load new map
                MapLoader.LoadMap("level1.txt", _world, _atlas, _scale);
            }

            // WIP
            if (input.Keyboard.WasKeyJustPressed(Keys.F5))
            {
                // Save current map
                MapLoader.SaveMap("saved_map.txt", _world);
            }
        }

        private void RotateConveyorDirection()
        {
            ConveyorDirection = (Direction)(((int)ConveyorDirection + 1) % 4);
        }

        private void RotateMachineDirection()
        {
            MachineDirection = (Direction)(((int)MachineDirection + 1) % 4);
        }

        private void CycleMachineType()
        {
            SelectedMachineIndex = (SelectedMachineIndex + 1) % _machineTypes.Length;
            SelectedMachineType = _machineTypes[SelectedMachineIndex];
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Ensure pixel texture exists
            EnsurePixelTexture();

            if (_pixelTexture == null) return;

            int screenWidth = Core.Graphics.PreferredBackBufferWidth;
            int screenHeight = Core.Graphics.PreferredBackBufferHeight;

            // Calculate UI panel dimensions
            int panelWidth = (IndicatorSize + IndicatorSpacing) * 2 + IndicatorSpacing;
            int panelHeight = IndicatorSize + IndicatorSpacing * 2;
            int panelX = (screenWidth - panelWidth) / 2;
            int panelY = screenHeight - panelHeight - BottomMargin;

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw background panel
            DrawRectangle(spriteBatch,
                new Rectangle(panelX, panelY, panelWidth, panelHeight),
                _backgroundColor);

            // Draw panel border
            DrawRectangleBorder(spriteBatch,
                new Rectangle(panelX, panelY, panelWidth, panelHeight),
                _borderColor, 2);

            // Draw indicators
            DrawConveyorIndicator(spriteBatch, panelX + IndicatorSpacing, panelY + IndicatorSpacing);
            DrawMachineIndicator(spriteBatch, panelX + IndicatorSpacing * 2 + IndicatorSize, panelY + IndicatorSpacing);

            spriteBatch.End();
        }

        private void DrawConveyorIndicator(SpriteBatch spriteBatch, int x, int y)
        {
            Rectangle indicatorRect = new Rectangle(x, y, IndicatorSize, IndicatorSize);

            DrawRectangle(spriteBatch, indicatorRect, Color.DarkGray);

            if (CurrentBuildMode == BuildMode.Conveyor)
            {
                DrawRectangleBorder(spriteBatch, indicatorRect, _selectionColor, SelectionBorderWidth);
            }

            try
            {
                var conveyor = new Conveyor(_atlas, ConveyorDirection, _scale * 0.8f);

                Vector2 spritePos = new Vector2(
                    x + IndicatorSize / 2 - conveyor.Sprite.Width / 2,
                    y + IndicatorSize / 2 - conveyor.Sprite.Height / 2
                );

                conveyor.Sprite.Draw(spriteBatch, spritePos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error drawing conveyor sprite: {ex.Message}");
                DrawRectangle(spriteBatch, new Rectangle(x + 10, y + 10, IndicatorSize - 20, IndicatorSize - 20), Color.Blue);
            }
        }

        private void DrawMachineIndicator(SpriteBatch spriteBatch, int x, int y)
        {
            Rectangle indicatorRect = new Rectangle(x, y, IndicatorSize, IndicatorSize);

            // Draw background
            DrawRectangle(spriteBatch, indicatorRect, Color.DarkGray);

            // Draw selection highlight
            if (CurrentBuildMode == BuildMode.Machine)
            {
                DrawRectangleBorder(spriteBatch, indicatorRect, _selectionColor, SelectionBorderWidth);
            }

            // Draw current machine sprite
            try
            {
                var machine = new Machine(SelectedMachineType, MachineDirection, _atlas, _scale * 0.8f);

                Vector2 spritePos = new Vector2(
                    x + IndicatorSize / 2 - machine.MachineSprite.Width / 2,
                    y + IndicatorSize / 2 - machine.MachineSprite.Height / 2
                );

                machine.MachineSprite.Draw(spriteBatch, spritePos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error drawing machine sprite: {ex.Message}");
                // Fallback: draw simple colored rectangle
                DrawRectangle(spriteBatch, new Rectangle(x + 10, y + 10, IndicatorSize - 20, IndicatorSize - 20), Color.Red);
            }
        }

        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            if (_pixelTexture != null)
            {
                spriteBatch.Draw(_pixelTexture, rectangle, color);
            }
        }

        private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int borderWidth)
        {
            if (_pixelTexture == null) return;

            // Top border
            spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, borderWidth), color);
            // Bottom border
            spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - borderWidth, rectangle.Width, borderWidth), color);
            // Left border
            spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, borderWidth, rectangle.Height), color);
            // Right border
            spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X + rectangle.Width - borderWidth, rectangle.Y, borderWidth, rectangle.Height), color);
        }

        // Public methods to get current selection for placing tiles
        public Conveyor CreateSelectedConveyor()
        {
            return new Conveyor(_atlas, ConveyorDirection, _scale);
        }

        public Machine CreateSelectedMachine()
        {
            return new Machine(SelectedMachineType, MachineDirection, _atlas, _scale);
        }
    }
}