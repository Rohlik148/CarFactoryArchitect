using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.UI.Components;

namespace CarFactoryArchitect.Source.UI
{
    public class UIManager
    {
        private readonly List<UIComponent> _components;
        private readonly UIRenderer _renderer;
        private readonly BuildModePanel _buildModePanel;

        public BuildModePanel BuildPanel => _buildModePanel;

        public UIManager(TextureAtlas atlas, World world, float scale)
        {
            _components = new List<UIComponent>();
            _renderer = new UIRenderer();

            _buildModePanel = new BuildModePanel(_renderer, atlas, scale);
            _components.Add(_buildModePanel);
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _renderer.Initialize(graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var component in _components)
            {
                component.Update(gameTime);
            }

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            var screenWidth = GameEngine.Graphics.PreferredBackBufferWidth;
            var screenHeight = GameEngine.Graphics.PreferredBackBufferHeight;

            int panelWidth = (UITheme.Layout.IndicatorSize + UITheme.Layout.IndicatorSpacing) * 2 + UITheme.Layout.IndicatorSpacing;
            int panelHeight = UITheme.Layout.IndicatorSize + UITheme.Layout.IndicatorSpacing * 2;
            int panelX = (screenWidth - panelWidth) / 2;
            int panelY = screenHeight - panelHeight - UITheme.Layout.BottomMargin;

            _buildModePanel.Bounds = new Rectangle(panelX, panelY, panelWidth, panelHeight);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            foreach (var component in _components)
            {
                component.Draw(spriteBatch);
            }

            spriteBatch.End();
        }
    }
}