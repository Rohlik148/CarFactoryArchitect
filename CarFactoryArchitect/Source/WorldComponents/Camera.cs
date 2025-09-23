using Microsoft.Xna.Framework;
using MonoGameLibrary;

namespace CarFactoryArchitect.Source.WorldComponents;

public class Camera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; } = 1.0f;

    private const float MinZoom = 0.6f;
    private const float MaxZoom = 3.0f;

    public Camera(Vector2 initialPosition)
    {
        Position = initialPosition;
    }

    public void SetZoom(float zoom)
    {
        Zoom = MathHelper.Clamp(zoom, MinZoom, MaxZoom);
    }

    public void Move(Vector2 delta)
    {
        Position += delta;
    }

    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return (screenPosition / Zoom) + Position;
    }

    public Point ScreenToGrid(Vector2 screenPosition, int tileSize)
    {
        Vector2 worldPos = ScreenToWorld(screenPosition);
        return new Point(
            (int)(worldPos.X / tileSize),
            (int)(worldPos.Y / tileSize)
        );
    }

    public Vector2 GridToWorld(int gridX, int gridY, int tileSize)
    {
        return new Vector2(gridX * tileSize, gridY * tileSize);
    }

    public Matrix GetTransformMatrix()
    {
        return Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
               Matrix.CreateScale(Zoom);
    }
}