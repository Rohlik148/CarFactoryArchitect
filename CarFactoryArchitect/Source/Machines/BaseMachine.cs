using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Machines
{
    public abstract class BaseMachine : IMachine
    {
        public MachineType Type { get; protected set; }
        public Direction Direction { get; set; }
        public Sprite MachineSprite { get; protected set; }

        public IItem InputSlot { get; protected set; }
        public IItem OutputSlot { get; protected set; }

        public bool IsProcessing { get; protected set; }
        public float ProcessingTime { get; protected set; }
        public float ProcessingDuration { get; protected set; }

        public virtual bool HasOutput => OutputSlot != null;

        protected BaseMachine(MachineType machineType, Direction direction, TextureAtlas atlas, float scale)
        {
            Type = machineType;
            Direction = direction;
            SetupSprite(atlas, scale);
            SetProcessingDuration();
        }

        protected virtual void SetupSprite(TextureAtlas atlas, float scale)
        {
            string spriteName = GetSpriteName();
            MachineSprite = atlas.CreateSprite(spriteName);
            MachineSprite.Scale = new Vector2(scale, scale);
        }

        protected abstract string GetSpriteName();
        protected abstract void SetProcessingDuration();

        public abstract bool CanAcceptInput(IItem item);

        public virtual bool CanAcceptInput(IItem item, Direction fromDirection)
        {
            // Can't accept input from output direction
            if (fromDirection == Direction)
                return false;

            return CanAcceptInput(item);
        }

        public abstract bool TryAcceptInput(IItem item);

        public virtual bool TryAcceptInput(IItem item, Direction fromDirection)
        {
            if (!CanAcceptInput(item, fromDirection))
                return false;

            return TryAcceptInput(item);
        }

        protected virtual void StartProcessing()
        {
            IsProcessing = true;
            ProcessingTime = 0f;
        }

        public virtual void Update(GameTime gameTime, TextureAtlas atlas, float scale)
        {
            if (IsProcessing)
            {
                ProcessingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (ProcessingTime >= ProcessingDuration)
                {
                    CompleteProcessing(atlas, scale);
                }
            }
        }

        protected abstract void CompleteProcessing(TextureAtlas atlas, float scale);

        public virtual IItem TryExtractOutput()
        {
            if (OutputSlot != null)
            {
                var output = OutputSlot;
                OutputSlot = null;
                return output;
            }
            return null;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            MachineSprite?.Draw(spriteBatch, position);
        }

        protected string GetDirectionSuffix(Direction direction)
        {
            return direction switch
            {
                Direction.Up => "-up",
                Direction.Right => "-right",
                Direction.Down => "-down",
                Direction.Left => "-left",
                _ => "-up"
            };
        }

        public void SetOutputSlot(IItem item)
        {
            OutputSlot = item;
        }
    }
}