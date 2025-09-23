using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Core;
using CarFactoryArchitect.Source.Conveyors;
using CarFactoryArchitect.Source.Machines;

namespace CarFactoryArchitect.Source.Systems
{
    public class ConveyorSystem
    {
        private readonly World _world;
        private float _moveTimer = 0f;
        private const float MoveInterval = 1.0f;

        public ConveyorSystem(World world)
        {
            _world = world;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateConveyorAnimations(gameTime);

            _moveTimer += deltaTime;

            if (_moveTimer >= MoveInterval)
            {
                ProcessConveyorMovement();
                _moveTimer = 0f;
            }
        }

        private void UpdateConveyorAnimations(GameTime gameTime)
        {
            // Update conveyor animations separately from movement
            foreach (var position in _world.GetAllConveyorPositions())
            {
                var conveyor = _world.GetTile(position.X, position.Y) as Conveyor;
                conveyor?.Update(gameTime);
            }
        }

        private void ProcessConveyorMovement()
        {
            var potentialMoves = CollectPotentialMoves();
            var executedMoves = ResolveMoveConflicts(potentialMoves);
            ExecuteMoves(executedMoves);
        }

        private List<ConveyorMove> CollectPotentialMoves()
        {
            var potentialMoves = new List<ConveyorMove>();

            foreach (var position in _world.GetAllConveyorPositions())
            {
                var conveyor = _world.GetTile(position.X, position.Y) as IConveyor;
                var itemOnConveyor = _world.GetItemOnConveyor(position.X, position.Y);

                if (conveyor != null && itemOnConveyor != null)
                {
                    Point nextPosition = GetNextPosition(position, conveyor.Direction);

                    if (CanMoveToPosition(nextPosition, itemOnConveyor))
                    {
                        potentialMoves.Add(new ConveyorMove
                        {
                            From = position,
                            To = nextPosition,
                            Item = itemOnConveyor,
                            ConveyorSpeed = conveyor.Speed
                        });
                    }
                }
            }

            return potentialMoves;
        }

        private List<ConveyorMove> ResolveMoveConflicts(List<ConveyorMove> potentialMoves)
        {
            var executedMoves = new List<ConveyorMove>();
            var movesByTarget = potentialMoves.GroupBy(move => move.To).ToList();

            foreach (var targetGroup in movesByTarget)
            {
                Point targetPos = targetGroup.Key;
                var movesToTarget = targetGroup.ToList();

                var targetTile = _world.GetTile(targetPos.X, targetPos.Y);
                if (targetTile == null) continue;

                if (movesToTarget.Count == 1)
                {
                    var move = movesToTarget[0];
                    if (ValidateMove(move))
                    {
                        executedMoves.Add(move);
                    }
                }
                else
                {
                    HandleMoveConflict(targetPos, movesToTarget, executedMoves);
                }
            }

            return executedMoves;
        }

        private void HandleMoveConflict(Point targetPos, List<ConveyorMove> movesToTarget, List<ConveyorMove> executedMoves)
        {
            if (_world.HasSpaceOnConveyor(targetPos.X, targetPos.Y))
            {
                var chosenMove = SelectMoveWithSpeedPriority(movesToTarget);
                if (ValidateMove(chosenMove))
                {
                    executedMoves.Add(chosenMove);
                }
            }
            else if (_world.HasSpaceInQueue(targetPos.X, targetPos.Y))
            {
                var chosenMove = SelectMoveWithSpeedPriority(movesToTarget);
                if (ValidateMove(chosenMove))
                {
                    executedMoves.Add(chosenMove);
                }
            }
        }

        private void ExecuteMoves(List<ConveyorMove> moves)
        {
            System.Diagnostics.Debug.WriteLine($"[ConveyorSystem] Global timer triggered - executing {moves.Count} moves");

            foreach (var move in moves)
            {
                if (ExecuteMove(move))
                {
                    System.Diagnostics.Debug.WriteLine($"[ConveyorSystem] ✓ Moved {move.Item.Type} from {move.From} to {move.To}");
                }
            }
        }

        private bool ExecuteMove(ConveyorMove move)
        {
            try
            {
                var sourceItem = _world.GetItemOnConveyor(move.From.X, move.From.Y);
                if (sourceItem == null || !AreSameItem(sourceItem, move.Item))
                    return false;

                _world.MoveItem(move.From, move.To, move.Item);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Conveyor move execution exception: {ex.Message}");
                return false;
            }
        }

        private ConveyorMove SelectMoveWithSpeedPriority(List<ConveyorMove> moves)
        {
            return moves.OrderByDescending(move => move.ConveyorSpeed)
                       .ThenBy(move => move.From.X + move.From.Y * 100)
                       .First();
        }

        private bool ValidateMove(ConveyorMove move)
        {
            var currentItem = _world.GetItemOnConveyor(move.From.X, move.From.Y);
            if (currentItem == null || !AreSameItem(currentItem, move.Item))
                return false;

            return CanMoveToPosition(move.To, move.Item);
        }

        private bool CanMoveToPosition(Point position, IItem item)
        {
            if (!_world.IsInBounds(position.X, position.Y))
                return false;

            var targetTile = _world.GetTile(position.X, position.Y);

            return targetTile switch
            {
                IConveyor => _world.HasSpaceOnConveyor(position.X, position.Y) ||
                            (_world.GetItemOnConveyor(position.X, position.Y) != null && _world.HasSpaceInQueue(position.X, position.Y)),

                IMachine machine => CanMachineAcceptItem(machine, item, position),

                null => false,
                _ => false
            };
        }

        private bool CanMachineAcceptItem(IMachine machine, IItem item, Point machinePosition)
        {
            if (machine == null || item == null) return false;

            if (machine.Type == MachineType.Assembler)
            {
                return machine.CanAcceptInput(item, Direction.Up) ||
                       machine.CanAcceptInput(item, Direction.Right) ||
                       machine.CanAcceptInput(item, Direction.Down) ||
                       machine.CanAcceptInput(item, Direction.Left);
            }

            return machine.CanAcceptInput(item);
        }

        private bool AreSameItem(IItem item1, IItem item2)
        {
            if (item1 == null || item2 == null) return false;
            return item1.Type == item2.Type && item1.State == item2.State;
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
    }
}