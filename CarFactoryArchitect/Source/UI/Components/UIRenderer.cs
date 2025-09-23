using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;

namespace CarFactoryArchitect.Source.UI.Components;

public class UIRenderer
{
    private Texture2D _pixelTexture;
    private bool _isInitialized = false;

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        if (graphicsDevice == null)
        {
            Console.WriteLine("Warning: GraphicsDevice is null, skipping UI texture creation");
            return;
        }

        CreatePixelTexture(graphicsDevice);
        _isInitialized = true;
    }

    private void CreatePixelTexture(GraphicsDevice graphicsDevice)
    {
        try
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
            Console.WriteLine("UIRenderer pixel texture created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create UI pixel texture: {ex.Message}");
            _pixelTexture = null;
        }
    }

    private void EnsurePixelTexture()
    {
        if (_pixelTexture == null && !_isInitialized && GameEngine.GraphicsDevice != null)
        {
            Initialize(GameEngine.GraphicsDevice);
        }
    }

    public void DrawPanel(SpriteBatch spriteBatch, Rectangle bounds, Color background, Color border)
    {
        EnsurePixelTexture();
        if (_pixelTexture == null) return;

        DrawRectangle(spriteBatch, bounds, background);
        DrawRectangleBorder(spriteBatch, bounds, border, 2);
    }

    public void DrawIndicator(SpriteBatch spriteBatch, Rectangle bounds, bool isSelected)
    {
        EnsurePixelTexture();
        if (_pixelTexture == null) return;

        DrawRectangle(spriteBatch, bounds, Color.DarkGray);
        if (isSelected)
        {
            DrawRectangleBorder(spriteBatch, bounds, UITheme.SelectionColor, 4);
        }
    }

    public void DrawFallback(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        EnsurePixelTexture();
        if (_pixelTexture == null) return;

        var innerBounds = new Rectangle(bounds.X + 10, bounds.Y + 10, bounds.Width - 20, bounds.Height - 20);
        DrawRectangle(spriteBatch, innerBounds, color);
    }

    public void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        if (_pixelTexture != null)
        {
            spriteBatch.Draw(_pixelTexture, rectangle, color);
        }
    }

    public void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int borderWidth)
    {
        if (_pixelTexture == null) return;

        // Top, Bottom, Left, Right borders
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, borderWidth), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y + rectangle.Height - borderWidth, rectangle.Width, borderWidth), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X, rectangle.Y, borderWidth, rectangle.Height), color);
        spriteBatch.Draw(_pixelTexture, new Rectangle(rectangle.X + rectangle.Width - borderWidth, rectangle.Y, borderWidth, rectangle.Height), color);
    }
}