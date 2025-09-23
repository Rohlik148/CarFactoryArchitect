using System.Collections.Generic;
using System.Linq;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Core;
using CarFactoryArchitect.Source.Recipes;

namespace CarFactoryArchitect.Source.Machines
{
    public class AssemblerMachine : BaseMachine
    {
        public Dictionary<Direction, IItem> InputSlots { get; private set; }
        public Recipe CurrentRecipe { get; private set; }

        public AssemblerMachine(Direction direction, TextureAtlas atlas, float scale)
            : base(MachineType.Assembler, direction, atlas, scale)
        {
            InputSlots = new Dictionary<Direction, IItem>();
        }

        protected override string GetSpriteName()
        {
            return "assembler" + GetDirectionSuffix(Direction);
        }

        protected override void SetProcessingDuration()
        {
            ProcessingDuration = 4.0f; // Default, later overriden by a recipe
        }

        public override bool CanAcceptInput(IItem item)
        {
            return false;
        }

        public override bool CanAcceptInput(IItem item, Direction fromDirection)
        {
            if (IsProcessing || OutputSlot != null || fromDirection == Direction)
                return false;

            if (InputSlots.Count >= 3 || !RecipeManager.CouldBePartOfRecipe(item.Type, item.State))
                return false;

            if (InputSlots.Values.Any(ore => ore.Type == item.Type && ore.State == item.State))
                return false;

            if (InputSlots.ContainsKey(fromDirection))
                return false;

            return true;
        }

        public override bool TryAcceptInput(IItem item)
        {
            return false;
        }

        public override bool TryAcceptInput(IItem item, Direction fromDirection)
        {
            if (!CanAcceptInput(item, fromDirection)) return false;

            InputSlots[fromDirection] = item;
            TryStartProcessing();
            return true;
        }

        private void TryStartProcessing()
        {
            if (IsProcessing) return;

            var availableInputs = InputSlots.Values.Select(ore => (ore.Type, ore.State)).ToList();
            var recipe = RecipeManager.FindRecipe(availableInputs);

            if (recipe != null)
            {
                CurrentRecipe = recipe;
                ProcessingDuration = recipe.ProcessingTime;
                StartProcessing();
            }
        }

        protected override void CompleteProcessing(TextureAtlas atlas, float scale)
        {
            IsProcessing = false;
            ProcessingTime = 0f;

            if (CurrentRecipe != null)
            {
                OutputSlot = ItemFactory.CreateManufacturedProduct(CurrentRecipe.Output.type, atlas, scale);

                var usedInputs = new List<(OreType, OreState)>(CurrentRecipe.Inputs);
                var slotsToRemove = new List<Direction>();

                foreach (var slot in InputSlots)
                {
                    var itemKey = (slot.Value.Type, slot.Value.State);
                    if (usedInputs.Contains(itemKey))
                    {
                        usedInputs.Remove(itemKey);
                        slotsToRemove.Add(slot.Key);
                    }
                }

                foreach (var direction in slotsToRemove)
                {
                    InputSlots.Remove(direction);
                }

                CurrentRecipe = null;
            }
        }
    }
}