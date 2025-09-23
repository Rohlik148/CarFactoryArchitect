using CarFactoryArchitect.Source.Items;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Machines
{
    public class CutterMachine : BaseMachine
    {
        public CutterMachine(Direction direction, TextureAtlas atlas, float scale)
            : base(MachineType.Cutter, direction, atlas, scale)
        {
        }

        protected override string GetSpriteName()
        {
            return "cutter" + GetDirectionSuffix(Direction);
        }

        protected override void SetProcessingDuration()
        {
            ProcessingDuration = 2.0f;
        }

        public override bool CanAcceptInput(IItem item)
        {
            if (IsProcessing || OutputSlot != null || InputSlot != null)
                return false;

            // Cutter only accepts copper smelted to make wire
            return item.Type == OreType.Copper && item.State == OreState.Smelted;
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
                // Use the new ItemFactory instead of direct instantiation
                OutputSlot = ItemFactory.CreateCutMaterial(OreType.Copper, atlas, scale);
                InputSlot = null;
            }
        }
    }
}