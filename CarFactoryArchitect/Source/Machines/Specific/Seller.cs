using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Items;

namespace CarFactoryArchitect.Source.Machines
{
    public class SellerMachine : BaseMachine
    {
        public SellerMachine(Direction direction, TextureAtlas atlas, float scale)
            : base(MachineType.Seller, direction, atlas, scale)
        {
        }

        protected override string GetSpriteName()
        {
            // Seller doesn't have directional sprites
            return "seller";
        }

        protected override void SetProcessingDuration()
        {
            ProcessingDuration = 1.0f; // Fast selling
        }

        public override bool CanAcceptInput(IItem item)
        {
            // Seller accepts anything and always has space (consumes items)
            return !IsProcessing;
        }

        public override bool CanAcceptInput(IItem item, Direction fromDirection)
        {
            // Seller accepts from all directions
            return CanAcceptInput(item);
        }

        public override bool TryAcceptInput(IItem item)
        {
            if (!CanAcceptInput(item)) return false;

            InputSlot = item;
            StartProcessing();
            return true;
        }

        public override bool TryAcceptInput(IItem item, Direction fromDirection)
        {
            // Seller accepts from all directions
            return TryAcceptInput(item);
        }

        protected override void CompleteProcessing(TextureAtlas atlas, float scale)
        {
            IsProcessing = false;
            ProcessingTime = 0f;

            if (InputSlot != null)
            {
                // Seller consumes the item (sells it)
                // Could add score/money logic here
                System.Diagnostics.Debug.WriteLine($"Sold: {InputSlot.Type} {InputSlot.State}");
                InputSlot = null;
                // Note: OutputSlot remains null because seller consumes items
            }
        }
    }
}