using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;
using MonoGameLibrary;
using System.Threading;

namespace CarFactoryArchitect.Source.Controls
{
    public class InputHandler
    {
        private readonly Dictionary<InputAction, List<InputBinding>> _bindings;
        private readonly Dictionary<InputAction, Action<InputContext>> _handlers;

        public InputHandler()
        {
            _bindings = new Dictionary<InputAction, List<InputBinding>>();
            _handlers = new Dictionary<InputAction, Action<InputContext>>();
            SetupDefaultBindings();
        }

        private void SetupDefaultBindings()
        {
            // Camera Controls
            Bind(InputAction.CameraMoveUp, Keys.W, false);
            Bind(InputAction.CameraMoveDown, Keys.S, false);
            Bind(InputAction.CameraMoveLeft, Keys.A, false);
            Bind(InputAction.CameraMoveRight, Keys.D, false);
            Bind(InputAction.CameraZoomIn, MouseWheelDirection.Up);
            Bind(InputAction.CameraZoomOut, MouseWheelDirection.Down);

            // Building Controls  
            Bind(InputAction.PlaceTile, MouseButton.Left);
            Bind(InputAction.DeleteTile, MouseButton.Right);

            // UI Controls
            Bind(InputAction.SelectConveyorMode, Keys.D1);
            Bind(InputAction.SelectMachineMode, Keys.D2);
            Bind(InputAction.RotateBuilding, Keys.R);
            Bind(InputAction.CycleMachineType, Keys.T);

            // System Controls
            Bind(InputAction.LoadMap, Keys.F1);
            Bind(InputAction.SaveMap, Keys.F5);
            Bind(InputAction.Exit, Keys.Escape);
        }

        public void Bind(InputAction action, Keys key, bool requireJustPressed = true)
        {
            if (!_bindings.ContainsKey(action))
                _bindings[action] = new List<InputBinding>();

            _bindings[action].Add(new KeyboardBinding(key, requireJustPressed));
        }

        public void Bind(InputAction action, MouseButton button)
        {
            if (!_bindings.ContainsKey(action))
                _bindings[action] = new List<InputBinding>();

            _bindings[action].Add(new MouseButtonBinding(button));
        }

        public void Bind(InputAction action, MouseWheelDirection direction)
        {
            if (!_bindings.ContainsKey(action))
                _bindings[action] = new List<InputBinding>();

            _bindings[action].Add(new MouseWheelBinding(direction));
        }

        public void RegisterHandler(InputAction action, Action<InputContext> handler)
        {
            _handlers[action] = handler;
        }

        public void Update(GameTime gameTime)
        {
            var inputManager = GameEngine.Input;
            var context = new InputContext
            {
                GameTime = gameTime,
                MousePosition = new Vector2(inputManager.Mouse.X, inputManager.Mouse.Y),
                MouseScrollDelta = inputManager.Mouse.ScrollWheelDelta
            };

            foreach (var actionBinding in _bindings)
            {
                var action = actionBinding.Key;
                var bindings = actionBinding.Value;

                foreach (var binding in bindings)
                {
                    if (binding.IsTriggered(inputManager, context))
                    {
                        if (_handlers.TryGetValue(action, out var handler))
                        {
                            handler(context);
                        }
                        break;
                    }
                }
            }
        }
    }

    public class InputContext
    {
        public GameTime GameTime { get; set; }
        public Vector2 MousePosition { get; set; }
        public int MouseScrollDelta { get; set; }
    }
}