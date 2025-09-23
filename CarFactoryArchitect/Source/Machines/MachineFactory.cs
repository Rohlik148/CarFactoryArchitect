using System;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Machines
{
    public static class MachineFactory
    {
        public static IMachine CreateMachine(MachineType machineType, Direction direction, TextureAtlas atlas, float scale)
        {
            return machineType switch
            {
                MachineType.Smelter => new SmelterMachine(direction, atlas, scale),
                MachineType.Forge => new ForgeMachine(direction, atlas, scale),
                MachineType.Cutter => new CutterMachine(direction, atlas, scale),
                MachineType.Assembler => new AssemblerMachine(direction, atlas, scale),
                MachineType.Extractor => new ExtractorMachine(direction, atlas, scale),
                MachineType.Seller => new SellerMachine(direction, atlas, scale),
                _ => throw new ArgumentException($"Unknown machine type: {machineType}")
            };
        }
    }
}