using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Core;
using CarFactoryArchitect.Source.Machines;

namespace CarFactoryArchitect.Source.UI.Components;

public class BuildModePanel : UIComponent
{
    private readonly UIRenderer _renderer;
    private readonly TextureAtlas _atlas;
    private readonly float _scale;

    public BuildMode CurrentBuildMode { get; private set; } = BuildMode.Conveyor;
    public Direction ConveyorDirection { get; private set; } = Direction.Up;
    public Direction MachineDirection { get; private set; } = Direction.Up;
    public MachineType SelectedMachineType { get; private set; } = MachineType.Smelter;

    private readonly MachineType[] _machineTypes = {
        MachineType.Smelter, MachineType.Forge, MachineType.Cutter,
        MachineType.Assembler, MachineType.Extractor, MachineType.Seller
    };

    private int _selectedMachineIndex = 0;

    public BuildModePanel(UIRenderer renderer, TextureAtlas atlas, float scale)
    {
        _renderer = renderer;
        _atlas = atlas;
        _scale = scale;
    }

    public override void Update(GameTime gameTime) { }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // Draw panel background
        _renderer.DrawPanel(spriteBatch, Bounds, UITheme.PanelBackground, UITheme.PanelBorder);

        // Draw indicators
        var conveyorBounds = new Rectangle(Bounds.X + 20, Bounds.Y + 20, 80, 80);
        var machineBounds = new Rectangle(conveyorBounds.Right + 20, Bounds.Y + 20, 80, 80);

        DrawConveyorIndicator(spriteBatch, conveyorBounds);
        DrawMachineIndicator(spriteBatch, machineBounds);
    }

    private void DrawConveyorIndicator(SpriteBatch spriteBatch, Rectangle bounds)
    {
        var isSelected = CurrentBuildMode == BuildMode.Conveyor;
        _renderer.DrawIndicator(spriteBatch, bounds, isSelected);

        try
        {
            var conveyor = new Conveyor(_atlas, ConveyorDirection, _scale * 0.8f);
            var spritePos = bounds.Center.ToVector2() - new Vector2(conveyor.Sprite.Width, conveyor.Sprite.Height) / 2;
            conveyor.Sprite.Draw(spriteBatch, spritePos);
        }
        catch
        {
            _renderer.DrawFallback(spriteBatch, bounds, Color.Blue);
        }
    }

    private void DrawMachineIndicator(SpriteBatch spriteBatch, Rectangle bounds)
    {
        var isSelected = CurrentBuildMode == BuildMode.Machine;
        _renderer.DrawIndicator(spriteBatch, bounds, isSelected);

        try
        {
            var machine = MachineFactory.CreateMachine(SelectedMachineType, MachineDirection, _atlas, _scale * 0.8f);
            var spritePos = bounds.Center.ToVector2() - new Vector2(machine.MachineSprite.Width, machine.MachineSprite.Height) / 2;
            machine.MachineSprite.Draw(spriteBatch, spritePos);
        }
        catch
        {
            _renderer.DrawFallback(spriteBatch, bounds, Color.Red);
        }
    }

    // Public methods for input handling
    public void SetBuildMode(BuildMode mode) => CurrentBuildMode = mode;
    public void RotateConveyor() => ConveyorDirection = (Direction)(((int)ConveyorDirection + 1) % 4);
    public void RotateMachine() => MachineDirection = (Direction)(((int)MachineDirection + 1) % 4);

    public void CycleMachineType()
    {
        _selectedMachineIndex = (_selectedMachineIndex + 1) % _machineTypes.Length;
        SelectedMachineType = _machineTypes[_selectedMachineIndex];
    }

    public Conveyor CreateSelectedConveyor() => new Conveyor(_atlas, ConveyorDirection, _scale);
    public IMachine CreateSelectedMachine() => MachineFactory.CreateMachine(SelectedMachineType, MachineDirection, _atlas, _scale);
}