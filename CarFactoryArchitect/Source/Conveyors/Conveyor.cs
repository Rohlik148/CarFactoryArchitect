using CarFactoryArchitect.Source.Conveyors;

namespace CarFactoryArchitect.Source
{
    public class Conveyor : BasicConveyor
    {
        public Conveyor(MonoGameLibrary.Graphics.TextureAtlas atlas, Core.Direction direction, float scale)
            : base(direction, atlas, scale)
        {
        }
    }
}