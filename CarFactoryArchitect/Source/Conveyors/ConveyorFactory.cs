using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;
using System;

namespace CarFactoryArchitect.Source.Conveyors
{
    public static class ConveyorFactory
    {
        public static IConveyor CreateConveyor(ConveyorType conveyorType, Direction direction, TextureAtlas atlas, float scale)
        {
            return conveyorType switch
            {
                ConveyorType.Basic => new BasicConveyor(direction, atlas, scale),
                _ => throw new ArgumentException($"Unknown conveyor type: {conveyorType}")
            };
        }

        public static IConveyor CreateBasicConveyor(Direction direction, TextureAtlas atlas, float scale)
        {
            return new BasicConveyor(direction, atlas, scale);
        }
    }
}