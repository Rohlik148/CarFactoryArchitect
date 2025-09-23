namespace CarFactoryArchitect.Source.Controls
{
    public enum InputAction
    {
        // Camera Controls
        CameraMoveUp,
        CameraMoveDown,
        CameraMoveLeft,
        CameraMoveRight,
        CameraZoomIn,
        CameraZoomOut,

        // Building Controls
        PlaceTile,
        DeleteTile,

        // UI Controls
        SelectConveyorMode,
        SelectMachineMode,
        RotateBuilding,
        CycleMachineType,

        // System Controls
        LoadMap,
        SaveMap,
        Exit
    }

    public enum MouseWheelDirection
    {
        Up,
        Down
    }
}