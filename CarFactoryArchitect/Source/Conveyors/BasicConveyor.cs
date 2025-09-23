using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Conveyors
{
    public class BasicConveyor : BaseConveyor
    {
        public override float Speed => 1.0f;

        public BasicConveyor(Direction direction, TextureAtlas atlas, float scale)
            : base(ConveyorType.Basic, direction, atlas, scale)
        {
        }

        protected override string GetAnimationName()
        {
            return Direction switch
            {
                Direction.Up => "conveyor-animation-up",
                Direction.Right => "conveyor-animation-right",
                Direction.Down => "conveyor-animation-down",
                Direction.Left => "conveyor-animation-left",
                _ => "conveyor-animation-up"
            };
        }
    }
}