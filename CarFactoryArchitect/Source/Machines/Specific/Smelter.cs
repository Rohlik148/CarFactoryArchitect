using CarFactoryArchitect.Source.Items;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Machines
{
    public class SmelterMachine : BaseMachine
    {
        public SmelterMachine(Direction direction, TextureAtlas atlas, float scale)
            : base(MachineType.Smelter, direction, atlas, scale)
        {
        }

        protected override string GetSpriteName()
        {
            return "smelter" + GetDirectionSuffix(Direction);
        }

        protected override void SetProcessingDuration()
        {
            ProcessingDuration = 3.0f;
        }

        public override bool CanAcceptInput(IItem item)
        {
            if (IsProcessing || OutputSlot != null || InputSlot != null)
                return false;

            return item.State == OreState.Raw && item.Type switch
            {
                OreType.Iron => true,
                OreType.Copper => true,
                OreType.Sand => true,
                _ => false
            };
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
                OutputSlot = CreateSmelterOutput(InputSlot, atlas, scale);
                InputSlot = null;
            }
        }

        private IItem CreateSmelterOutput(IItem input, TextureAtlas atlas, float scale)
        {
            return input.Type switch
            {
                OreType.Iron => ItemFactory.CreateItem(OreType.Iron, OreState.Smelted, atlas, scale),
                OreType.Copper => ItemFactory.CreateItem(OreType.Copper, OreState.Smelted, atlas, scale),
                OreType.Sand => ItemFactory.CreateItem(OreType.Sand, OreState.Smelted, atlas, scale), // Glass
                _ => null
            };
        }
    }
}