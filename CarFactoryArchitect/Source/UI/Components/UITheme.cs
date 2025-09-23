using Microsoft.Xna.Framework;

namespace CarFactoryArchitect.Source.UI.Components;

public static class UITheme
{
    public static readonly Color PanelBackground = Color.Black * 0.7f;
    public static readonly Color PanelBorder = Color.White;
    public static readonly Color SelectionColor = Color.Green;
    public static readonly Color IndicatorBackground = Color.DarkGray;

    public static class Layout
    {
        public const int IndicatorSize = 80;
        public const int IndicatorSpacing = 20;
        public const int BottomMargin = 20;
        public const int SelectionBorderWidth = 4;
    }
}