using Microsoft.Xna.Framework;
using CarFactoryArchitect.Source.Items;

namespace CarFactoryArchitect.Source.Systems
{
    public class ConveyorTimer
    {
        private float _timer;
        private readonly float _moveInterval;

        public ConveyorTimer(float conveyorSpeed)
        {
            _moveInterval = 1.0f / conveyorSpeed;
            _timer = 0f;
        }

        public void Update(float deltaTime)
        {
            _timer += deltaTime;
        }

        public bool IsReadyToMove()
        {
            return _timer >= _moveInterval;
        }

        public void ResetTimer()
        {
            _timer = 0f;
        }
    }

    public class ConveyorMove
    {
        public Point From { get; set; }
        public Point To { get; set; }
        public IItem Item { get; set; }
        public float ConveyorSpeed { get; set; }
        public ConveyorTimer Timer { get; set; }
    }
}