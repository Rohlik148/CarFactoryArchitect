using Microsoft.Xna.Framework.Input;
using MonoGameLibrary.Input;

namespace CarFactoryArchitect.Source.Controls
{
    public abstract class InputBinding
    {
        public abstract bool IsTriggered(InputManager inputManager, InputContext context);
    }

    public class KeyboardBinding : InputBinding
    {
        private readonly Keys _key;
        private readonly bool _requireJustPressed;

        public KeyboardBinding(Keys key, bool requireJustPressed = true)
        {
            _key = key;
            _requireJustPressed = requireJustPressed;
        }

        public override bool IsTriggered(InputManager inputManager, InputContext context)
        {
            return _requireJustPressed
                ? inputManager.Keyboard.WasKeyJustPressed(_key)
                : inputManager.Keyboard.IsKeyDown(_key);
        }
    }

    public class MouseButtonBinding : InputBinding
    {
        private readonly MouseButton _button;

        public MouseButtonBinding(MouseButton button)
        {
            _button = button;
        }

        public override bool IsTriggered(InputManager inputManager, InputContext context)
        {
            return inputManager.Mouse.WasButtonJustPressed(_button);
        }
    }

    public class MouseWheelBinding : InputBinding
    {
        private readonly MouseWheelDirection _direction;

        public MouseWheelBinding(MouseWheelDirection direction)
        {
            _direction = direction;
        }

        public override bool IsTriggered(InputManager inputManager, InputContext context)
        {
            return _direction switch
            {
                MouseWheelDirection.Up => inputManager.Mouse.ScrollWheelDelta > 0,
                MouseWheelDirection.Down => inputManager.Mouse.ScrollWheelDelta < 0,
                _ => false
            };
        }
    }
}