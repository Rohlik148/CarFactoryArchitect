using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;

namespace CarFactoryArchitect.Source.Systems
{
    public class WorldSystem
    {
        private readonly ConveyorSystem _conveyorSystem;
        private readonly ExtractorSystem _extractorSystem;
        private readonly MachineSystem _machineSystem;

        public WorldSystem(World world, TextureAtlas atlas, float scale)
        {
            _conveyorSystem = new ConveyorSystem(world);
            _extractorSystem = new ExtractorSystem(world);
            _machineSystem = new MachineSystem(world, atlas, scale);
        }

        public void Update(GameTime gameTime)
        {
            _extractorSystem.Update(gameTime);    // Extract raw materials first
            _machineSystem.Update(gameTime);      // Process materials
            _conveyorSystem.Update(gameTime);     // Move items
        }

        public ConveyorSystem ConveyorSystem => _conveyorSystem;
        public ExtractorSystem ExtractorSystem => _extractorSystem;
        public MachineSystem MachineSystem => _machineSystem;
    }
}