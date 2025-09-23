using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using CarFactoryArchitect.Source.Items;

namespace CarFactoryArchitect.Source.WorldComponents;

public class ConveyorItemManager
{
    private readonly Dictionary<Point, IItem> _itemsOnConveyors;
    private readonly Dictionary<Point, Queue<IItem>> _conveyorInputQueues;
    private readonly TileManager _tileManager;
    private const int MaxQueueSize = 0;

    public ConveyorItemManager(TileManager tileManager)
    {
        _tileManager = tileManager;
        _itemsOnConveyors = new Dictionary<Point, IItem>();
        _conveyorInputQueues = new Dictionary<Point, Queue<IItem>>();
    }

    public IItem GetItemOnConveyor(int x, int y)
    {
        _itemsOnConveyors.TryGetValue(new Point(x, y), out IItem item);
        return item;
    }

    public bool HasSpaceOnConveyor(int x, int y)
    {
        return GetItemOnConveyor(x, y) == null;
    }

    public bool HasSpaceInQueue(int x, int y)
    {
        if (MaxQueueSize == 0) return false;

        var point = new Point(x, y);
        if (_conveyorInputQueues.TryGetValue(point, out Queue<IItem> queue))
        {
            return queue.Count < MaxQueueSize;
        }
        return true;
    }

    public bool TryPlaceItemOnConveyor(int x, int y, IItem item)
    {
        var point = new Point(x, y);
        var tile = _tileManager.GetTile(x, y);

        if (!(tile is Conveyor)) return false;

        if (HasSpaceOnConveyor(x, y))
        {
            if (_itemsOnConveyors.ContainsKey(point)) return false;

            _itemsOnConveyors[point] = item;
            return true;
        }

        return false;
    }

    public IItem RemoveItemFromConveyor(int x, int y)
    {
        var point = new Point(x, y);
        _itemsOnConveyors.TryGetValue(point, out IItem item);
        _itemsOnConveyors.Remove(point);
        ProcessConveyorQueue(x, y);
        return item;
    }

    private void ProcessConveyorQueue(int x, int y)
    {
        var point = new Point(x, y);

        if (!_itemsOnConveyors.ContainsKey(point) &&
            _conveyorInputQueues.TryGetValue(point, out Queue<IItem> queue) &&
            queue.Count > 0)
        {
            var nextItem = queue.Dequeue();
            _itemsOnConveyors[point] = nextItem;

            if (queue.Count == 0)
            {
                _conveyorInputQueues.Remove(point);
            }
        }
    }

    public void ProcessAllConveyorQueues()
    {
        var pointsToProcess = _conveyorInputQueues.Keys.ToList();
        foreach (var point in pointsToProcess)
        {
            ProcessConveyorQueue(point.X, point.Y);
        }
    }

    public int GetQueueCount(int x, int y)
    {
        var point = new Point(x, y);
        if (_conveyorInputQueues.TryGetValue(point, out Queue<IItem> queue))
        {
            return queue.Count;
        }
        return 0;
    }

    public void CleanupConveyorData(int x, int y)
    {
        var point = new Point(x, y);
        _itemsOnConveyors.Remove(point);
        _conveyorInputQueues.Remove(point);
    }
}