using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.UI.Components;
using CarFactoryArchitect.Source.Maps;

namespace CarFactoryArchitect.Source.Controls
{
    public class GameInputManager
    {
        private readonly InputHandler _inputHandler;
        private readonly World _world;
        private readonly BuildModePanel _buildPanel;
        private readonly TextureAtlas _atlas;
        private readonly float _scale;

        public GameInputManager(World world, BuildModePanel buildPanel, TextureAtlas atlas, float scale)
        {
            _world = world;
            _buildPanel = buildPanel;
            _atlas = atlas;
            _scale = scale;
            _inputHandler = new InputHandler();
            SetupInputHandlers();
        }

        private void SetupInputHandlers()
        {
            // Camera Controls
            _inputHandler.RegisterHandler(InputAction.CameraMoveUp, ctx => HandleCameraMove(Vector2.UnitY * -1, ctx));
            _inputHandler.RegisterHandler(InputAction.CameraMoveDown, ctx => HandleCameraMove(Vector2.UnitY, ctx));
            _inputHandler.RegisterHandler(InputAction.CameraMoveLeft, ctx => HandleCameraMove(Vector2.UnitX * -1, ctx));
            _inputHandler.RegisterHandler(InputAction.CameraMoveRight, ctx => HandleCameraMove(Vector2.UnitX, ctx));
            _inputHandler.RegisterHandler(InputAction.CameraZoomIn, ctx => HandleZoom(true, ctx));
            _inputHandler.RegisterHandler(InputAction.CameraZoomOut, ctx => HandleZoom(false, ctx));

            // Building Controls
            _inputHandler.RegisterHandler(InputAction.PlaceTile, HandlePlaceTile);
            _inputHandler.RegisterHandler(InputAction.DeleteTile, HandleDeleteTile);

            // UI Controls
            _inputHandler.RegisterHandler(InputAction.SelectConveyorMode, ctx => _buildPanel.SetBuildMode(BuildMode.Conveyor));
            _inputHandler.RegisterHandler(InputAction.SelectMachineMode, ctx => _buildPanel.SetBuildMode(BuildMode.Machine));
            _inputHandler.RegisterHandler(InputAction.RotateBuilding, ctx => RotateCurrentBuilding());
            _inputHandler.RegisterHandler(InputAction.CycleMachineType, ctx => _buildPanel.CycleMachineType());

            // System Controls
            _inputHandler.RegisterHandler(InputAction.LoadMap, ctx => MapLoader.LoadMap("level1.txt", _world, _atlas, _scale));
            _inputHandler.RegisterHandler(InputAction.SaveMap, ctx => MapLoader.SaveMap("saved_map.txt", _world));
        }

        private void RotateCurrentBuilding()
        {
            switch (_buildPanel.CurrentBuildMode)
            {
                case BuildMode.Conveyor:
                    _buildPanel.RotateConveyor();
                    break;
                case BuildMode.Machine:
                    _buildPanel.RotateMachine();
                    break;
            }
        }

        public void Update(GameTime gameTime)
        {
            _inputHandler.Update(gameTime);
        }

        private void HandleCameraMove(Vector2 direction, InputContext context)
        {
            float deltaTime = (float)context.GameTime.ElapsedGameTime.TotalSeconds;
            float cameraSpeed = 400f * deltaTime / _world.Zoom;
            _world.CameraPosition += direction * cameraSpeed;
        }

        private void HandleZoom(bool zoomIn, InputContext context)
        {
            const float ZoomSpeed = 0.2f;
            const float MinZoom = 0.6f;
            const float MaxZoom = 3.0f;

            Vector2 mousePos = context.MousePosition;
            Vector2 worldBeforeZoom = _world.ScreenToWorld(mousePos);

            float zoomChange = zoomIn ? ZoomSpeed : -ZoomSpeed;
            float oldZoom = _world.Zoom;
            _world.Zoom = MathHelper.Clamp(_world.Zoom + zoomChange, MinZoom, MaxZoom);

            if (_world.Zoom != oldZoom)
            {
                Vector2 worldAfterZoom = _world.ScreenToWorld(mousePos);
                _world.CameraPosition += worldBeforeZoom - worldAfterZoom;
            }
        }

        private void HandlePlaceTile(InputContext context)
        {
            Point gridPos = _world.ScreenToGrid(context.MousePosition);

            if (_world.IsInBounds(gridPos.X, gridPos.Y))
            {
                var existingTile = _world.GetTile(gridPos.X, gridPos.Y);

                if (existingTile == null || (existingTile is IItem item && item.State == OreState.Tile))
                {
                    switch (_buildPanel.CurrentBuildMode)
                    {
                        case BuildMode.Conveyor:
                            var conveyor = _buildPanel.CreateSelectedConveyor();
                            _world.PlaceConveyor(gridPos.X, gridPos.Y, conveyor);
                            break;
                        case BuildMode.Machine:
                            var machine = _buildPanel.CreateSelectedMachine();
                            _world.PlaceMachine(gridPos.X, gridPos.Y, machine);
                            break;
                    }
                }
            }
        }

        private void HandleDeleteTile(InputContext context)
        {
            Point gridPos = _world.ScreenToGrid(context.MousePosition);
            if (_world.IsInBounds(gridPos.X, gridPos.Y))
            {
                _world.RemoveTile(gridPos.X, gridPos.Y);
            }
        }
    }
}