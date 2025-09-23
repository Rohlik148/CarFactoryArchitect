using Microsoft.Xna.Framework;
using CarFactoryArchitect.Source.Machines;
using CarFactoryArchitect.Source.Core;
using CarFactoryArchitect.Source.Conveyors;

namespace CarFactoryArchitect.Source.Systems
{
    public class ExtractorSystem
    {
        private readonly World _world;
        private float _extractorTimer = 0f;
        private const float ExtractorInterval = 2.0f;

        public ExtractorSystem(World world)
        {
            _world = world;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _extractorTimer += deltaTime;

            if (_extractorTimer >= ExtractorInterval)
            {
                ProcessExtractors();
                _extractorTimer = 0f;
            }
        }

        private void ProcessExtractors()
        {
            foreach (var position in _world.GetAllExtractorPositions())
            {
                var extractor = _world.GetTile(position.X, position.Y) as IMachine;

                if (extractor?.Type == MachineType.Extractor)
                {
                    ProcessExtractor(extractor, position);
                }
            }
        }

        private void ProcessExtractor(IMachine extractor, Point position)
        {
            if (!_world.HasUnderlyingOre(position.X, position.Y))
                return;

            var underlyingOre = _world.GetUnderlyingOre(position.X, position.Y);
            Point outputPosition = GetNextPosition(position, extractor.Direction);

            if (!_world.IsInBounds(outputPosition.X, outputPosition.Y))
                return;

            var targetTile = _world.GetTile(outputPosition.X, outputPosition.Y);

            if (targetTile is IConveyor && _world.HasSpaceOnConveyor(outputPosition.X, outputPosition.Y))
            {
                var extractedOre = _world.CreateRawOre(underlyingOre.Type);
                _world.TryPlaceItemOnConveyor(outputPosition.X, outputPosition.Y, extractedOre);

                System.Diagnostics.Debug.WriteLine($"Extractor at {position} extracted {underlyingOre.Type}");
            }
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