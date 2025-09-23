using Microsoft.Xna.Framework;
using System.Linq;
using CarFactoryArchitect.Source.Machines;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Core;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Conveyors;

namespace CarFactoryArchitect.Source.Systems
{
    public class MachineSystem
    {
        private readonly World _world;
        private readonly TextureAtlas _atlas;
        private readonly float _scale;

        public MachineSystem(World world, TextureAtlas atlas, float scale)
        {
            _world = world;
            _atlas = atlas;
            _scale = scale;
        }

        public void Update(GameTime gameTime)
        {
            ProcessAllMachines(gameTime);
            ProcessMachineOutputs();
        }

        private void ProcessAllMachines(GameTime gameTime)
        {
            foreach (var position in _world.GetAllMachinePositions())
            {
                var machine = _world.GetTile(position.X, position.Y) as IMachine;
                if (machine != null)
                {
                    machine.Update(gameTime, _atlas, _scale);
                }
            }
        }

        private void ProcessMachineOutputs()
        {
            foreach (var position in _world.GetAllMachinePositions())
            {
                var machine = _world.GetTile(position.X, position.Y) as IMachine;

                if (machine?.HasOutput == true)
                {
                    TryOutputMachineItem(machine, position);
                }
            }
        }

        private void TryOutputMachineItem(IMachine machine, Point machinePos)
        {
            var outputItem = machine.TryExtractOutput();
            if (outputItem == null) return;

            Point outputPos = GetNextPosition(machinePos, machine.Direction);

            if (!_world.IsInBounds(outputPos.X, outputPos.Y))
            {
                // Put item back if output is out of bounds
                machine.SetOutputSlot(outputItem);
                return;
            }

            var outputTile = _world.GetTile(outputPos.X, outputPos.Y);

            if (outputTile is IConveyor)
            {
                if (!_world.TryPlaceItemOnConveyor(outputPos.X, outputPos.Y, outputItem))
                {
                    // Put the item back in the machine's output slot
                    machine.SetOutputSlot(outputItem);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"{machine.Type} at {machinePos} output {outputItem.Type}");
                }
            }
            else
            {
                // Put item back if no valid output target
                machine.SetOutputSlot(outputItem);
            }
        }

        public bool CanMachineAcceptItem(IMachine machine, IItem item)
        {
            if (machine == null || item == null) return false;

            if (machine.Type == MachineType.Assembler)
            {
                var possibleDirections = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left }
                    .Where(dir => dir != machine.Direction);

                return possibleDirections.Any(dir => machine.CanAcceptInput(item, dir));
            }

            return machine.CanAcceptInput(item);
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
    }
}