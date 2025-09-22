using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Items; 

namespace CarFactoryArchitect.Source.Machines
{
    public class ExtractorMachine : BaseMachine
    {
        private float _extractionTimer = 0f;
        private const float ExtractionInterval = 2.0f; // Extract every 2 seconds

        public ExtractorMachine(Direction direction, TextureAtlas atlas, float scale)
            : base(MachineType.Extractor, direction, atlas, scale)
        {
        }

        protected override string GetSpriteName()
        {
            return "extractor" + GetDirectionSuffix(Direction);
        }

        protected override void SetProcessingDuration()
        {
            ProcessingDuration = 2.0f;
        }

        public override bool CanAcceptInput(IItem item)
        {
            // Extractors don't accept input - they extract from ore tiles
            return false;
        }

        public override bool TryAcceptInput(IItem item)
        {
            // Extractors don't accept input
            return false;
        }

        public override void Update(GameTime gameTime, TextureAtlas atlas, float scale)
        {
            _extractionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // The extraction logic will be handled by the ConveyorSystem/World
            // This is just for timing

            base.Update(gameTime, atlas, scale);
        }

        protected override void CompleteProcessing(TextureAtlas atlas, float scale)
        {
            // Extractors handle their own extraction logic
            IsProcessing = false;
            ProcessingTime = 0f;
        }

        public bool CanExtract()
        {
            return OutputSlot == null && _extractionTimer >= ExtractionInterval;
        }

        public void ResetExtractionTimer()
        {
            _extractionTimer = 0f;
        }

        public void SetOutput(IItem ore)
        {
            OutputSlot = ore;
            ResetExtractionTimer();
        }
    }
}