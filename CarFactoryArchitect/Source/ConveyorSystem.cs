using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using CarFactoryArchitect.Source.Machines;
using CarFactoryArchitect.Source.Items;

namespace CarFactoryArchitect.Source;
public class ConveyorSystem
{
    private World _world;
    private float _moveTimer = 0f;
    private float _extractorTimer = 0f;
    private const float MoveInterval = 1.0f;
    private const float ExtractorInterval = 2.0f;

    public ConveyorSystem(World world)
    {
        _world = world;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _moveTimer += deltaTime;
        _extractorTimer += deltaTime;

        if (_moveTimer >= MoveInterval)
        {
            ProcessConveyorMovement();
            _moveTimer = 0f;
        }

        if (_extractorTimer >= ExtractorInterval)
        {
            ProcessExtractors();
            _extractorTimer = 0f;
        }
    }

    private void ProcessExtractors()
    {
        foreach (var position in _world.GetAllExtractorPositions())
        {
            var extractor = _world.GetTile(position.X, position.Y) as IMachine;

            if (extractor != null && extractor.Type == MachineType.Extractor)
            {
                if (_world.HasUnderlyingOre(position.X, position.Y))
                {
                    var underlyingOre = _world.GetUnderlyingOre(position.X, position.Y);
                    Point outputPosition = GetNextPosition(position, extractor.Direction);

                    if (_world.IsInBounds(outputPosition.X, outputPosition.Y))
                    {
                        var targetTile = _world.GetTile(outputPosition.X, outputPosition.Y);

                        if (targetTile is Conveyor && _world.HasSpaceOnConveyor(outputPosition.X, outputPosition.Y))
                        {
                            var extractedOre = _world.CreateRawOre(underlyingOre.Type);
                            _world.TryPlaceItemOnConveyor(outputPosition.X, outputPosition.Y, extractedOre);
                        }
                    }
                }
            }
        }
    }

    private void ProcessConveyorMovement()
    {
        var potentialMoves = new List<(Point from, Point to, IItem item)>();

        // Collect all potential moves first
        foreach (var position in _world.GetAllConveyorPositions())
        {
            var conveyor = _world.GetTile(position.X, position.Y) as Conveyor;
            var itemOnConveyor = _world.GetItemOnConveyor(position.X, position.Y);

            if (conveyor != null && itemOnConveyor != null)
            {
                Point nextPosition = GetNextPosition(position, conveyor.Direction);

                if (CanMoveToPosition(nextPosition, itemOnConveyor))
                {
                    potentialMoves.Add((position, nextPosition, itemOnConveyor));
                }
            }
        }

        var executedMoves = new List<(Point from, Point to, IItem item)>();
        var movesByTarget = potentialMoves.GroupBy(move => move.to).ToList();

        // Process moves grouped by target position to handle conflicts
        foreach (var targetGroup in movesByTarget)
        {
            Point targetPos = targetGroup.Key;
            var movesToTarget = targetGroup.ToList();

            var targetTile = _world.GetTile(targetPos.X, targetPos.Y);
            if (targetTile == null)
            {
                // Target was deleted - skip all moves to this position
                continue;
            }

            if (movesToTarget.Count == 1)
            {
                var move = movesToTarget[0];
                if (ValidateMoveRobust(move.from, move.to, move.item))
                {
                    executedMoves.Add(move);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Move validation failed for {move.item.Type} from {move.from} to {move.to}");
                }
            }
            else
            {
                // Handle conflicts based on target type
                if (targetTile is Conveyor)
                {
                    HandleConveyorConflict(targetPos, movesToTarget, executedMoves);
                }
                else if (targetTile is IMachine machine)
                {
                    HandleMachineConflict(machine, movesToTarget, executedMoves);
                }
            }
        }

        // Execute the valid moves with final validation
        var successfulMoves = 0;
        foreach (var (from, to, item) in executedMoves)
        {
            if (ExecuteMoveWithValidation(from, to, item))
            {
                successfulMoves++;
            }
        }

        if (successfulMoves != executedMoves.Count)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: {executedMoves.Count - successfulMoves} moves failed during execution");
        }
    }

    private bool ExecuteMoveWithValidation(Point from, Point to, IItem item)
    {
        // Final validation before executing the move
        var sourceItem = _world.GetItemOnConveyor(from.X, from.Y);
        if (sourceItem == null || !AreSameItem(sourceItem, item))
        {
            // System.Diagnostics.Debug.WriteLine($"Move execution failed: source item mismatch at {from}");
            return false;
        }

        var targetTile = _world.GetTile(to.X, to.Y);
        if (targetTile == null)
        {
            // System.Diagnostics.Debug.WriteLine($"Move execution failed: target tile removed at {to}");
            return false;
        }

        // Execute the move
        try
        {
            _world.MoveItem(from, to, item);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Move execution exception: {ex.Message}");
            return false;
        }
    }

    private void HandleConveyorConflict(Point targetPos, List<(Point from, Point to, IItem item)> movesToTarget, List<(Point from, Point to, IItem item)> executedMoves)
    {
        // Check if target conveyor has space
        if (_world.HasSpaceOnConveyor(targetPos.X, targetPos.Y))
        {
            // Choose one item to move to the conveyor
            var chosenMove = SelectMoveWithPriority(movesToTarget);
            if (ValidateMoveRobust(chosenMove.from, chosenMove.to, chosenMove.item))
            {
                executedMoves.Add(chosenMove);
            }
        }
        else if (_world.HasSpaceInQueue(targetPos.X, targetPos.Y))
        {
            var chosenMove = SelectMoveWithPriority(movesToTarget);
            if (ValidateMoveRobust(chosenMove.from, chosenMove.to, chosenMove.item))
            {
                executedMoves.Add(chosenMove);
            }
        }
    }

    private void HandleMachineConflict(IMachine machine, List<(Point from, Point to, IItem item)> movesToTarget, List<(Point from, Point to, IItem item)> executedMoves)
    {
        // Find all items that the machine can accept
        var acceptableMoves = movesToTarget.Where(move => CanMachineAcceptItem(machine, move.item)).ToList();

        if (acceptableMoves.Count > 0)
        {
            // Choose one acceptable item
            var chosenMove = SelectMachineMove(acceptableMoves);
            if (ValidateMoveRobust(chosenMove.from, chosenMove.to, chosenMove.item))
            {
                executedMoves.Add(chosenMove);
            }
        }
    }

    private (Point from, Point to, IItem item) SelectMachineMove(List<(Point from, Point to, IItem item)> acceptableMoves)
    {
        return acceptableMoves.First();
    }

    private bool ValidateMoveRobust(Point from, Point to, IItem item)
    {
        try
        {
            // Check if source still has the item
            var currentItem = _world.GetItemOnConveyor(from.X, from.Y);
            if (currentItem == null || !AreSameItem(currentItem, item))
            {
                return false;
            }

            // Check if destination still exists and can accept the item
            var targetTile = _world.GetTile(to.X, to.Y);
            if (targetTile == null)
            {
                return false;
            }

            // Check if destination can still accept the item
            if (!CanMoveToPosition(to, item))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Move validation exception: {ex.Message}");
            return false;
        }
    }

    private bool AreSameItem(IItem item1, IItem item2)
    {
        if (item1 == null || item2 == null) return false;
        return item1.Type == item2.Type && item1.State == item2.State;
    }

    private (Point from, Point to, IItem item) SelectMoveWithPriority(List<(Point from, Point to, IItem item)> conflictingMoves)
    {
        var priorityOrder = conflictingMoves.OrderBy(move =>
        {
            int positionPriority = move.from.X + move.from.Y * 100;
            int oreTypePriority = (int)move.item.Type * 10000;
            int oreStatePriority = (int)move.item.State * 1000000;

            return positionPriority + oreTypePriority + oreStatePriority;
        }).ToList();

        return priorityOrder.First();
    }

    private Point GetNextPosition(Point current, Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Point(current.X, current.Y - 1),
            Direction.Right => new Point(current.X + 1, current.Y),
            Direction.Down => new Point(current.X, current.Y + 1),
            Direction.Left => new Point(current.X - 1, current.Y),
            _ => current
        };
    }

    private bool CanMoveToPosition(Point position, IItem item)
    {
        if (!_world.IsInBounds(position.X, position.Y))
            return false;

        var targetTile = _world.GetTile(position.X, position.Y);

        return targetTile switch
        {
            Conveyor conveyor =>
                _world.HasSpaceOnConveyor(position.X, position.Y) ||
                (_world.GetItemOnConveyor(position.X, position.Y) != null && _world.HasSpaceInQueue(position.X, position.Y)),

            IMachine machine => CanMachineAcceptItem(machine, item),
            null => false,
            _ => false
        };
    }

    private bool CanMachineAcceptItem(IMachine machine, IItem item)
    {
        if (machine == null || item == null) return false;

        if (machine.Type == MachineType.Assembler)
        {
            var possibleDirections = new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left }
                .Where(dir => dir != machine.Direction);

            return possibleDirections.Any(dir => machine.CanAcceptInput(item, dir));
        }

        return machine.CanAcceptInput(item);
    }
}