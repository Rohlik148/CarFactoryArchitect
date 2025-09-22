using CarFactoryArchitect.Source.Items;
using MonoGameLibrary.Graphics;

namespace CarFactoryArchitect.Source.Machines
{
    public class ForgeMachine : BaseMachine
    {
        public ForgeMachine(Direction direction, TextureAtlas atlas, float scale)
            : base(MachineType.Forge, direction, atlas, scale)
        {
        }

        protected override string GetSpriteName()
        {
            return "forge" + GetDirectionSuffix(Direction);
        }

        protected override void SetProcessingDuration()
        {
            ProcessingDuration = 2.5f;
        }

        public override bool CanAcceptInput(IItem item)
        {
            if (IsProcessing || OutputSlot != null || InputSlot != null)
                return false;

            return item.State == OreState.Smelted && item.Type == OreType.Iron;
        }

        public override bool TryAcceptInput(IItem item)
        {
            if (!CanAcceptInput(item)) return false;

            InputSlot = item;
            StartProcessing();
            return true;
        }

        protected override void CompleteProcessing(TextureAtlas atlas, float scale)
        {
            IsProcessing = false;
            ProcessingTime = 0f;

            if (InputSlot != null)
            {
                OutputSlot = ItemFactory.CreateForgedMaterial(OreType.Iron, atlas, scale);
                InputSlot = null;
            }
        }
    }
}