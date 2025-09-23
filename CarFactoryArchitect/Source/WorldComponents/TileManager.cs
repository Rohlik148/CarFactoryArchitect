using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Machines;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.WorldComponents;

public class TileManager
{
    private readonly Dictionary<Point, object> _tiles;
    private readonly Dictionary<Point, IItem> _underlyingOres;
    private readonly int _gridSize;

    public TileManager(int gridSize)
    {
        _gridSize = gridSize;
        _tiles = new Dictionary<Point, object>();
        _underlyingOres = new Dictionary<Point, IItem>();
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < _gridSize && y >= 0 && y < _gridSize;
    }

    public void PlaceTile(int x, int y, object tile)
    {
        if (!IsInBounds(x, y)) return;

        Point point = new Point(x, y);
        var existingTile = GetTile(x, y);

        if (existingTile is IItem ore)
        {
            _underlyingOres[point] = ore;
        }

        _tiles[point] = tile;
    }

    public void RemoveTile(int x, int y, Action<int, int> onConveyorRemoved = null)
    {
        if (!IsInBounds(x, y)) return;

        Point point = new Point(x, y);
        var existingTile = GetTile(x, y);

        // Prevent deletion of ore tiles
        if (existingTile is IItem item && item.State == OreState.Tile)
        {
            return;
        }

        if (existingTile is Conveyor && onConveyorRemoved != null)
        {
            onConveyorRemoved(x, y);
        }

        _tiles.Remove(point);

        // If there's an underlying ore, restore it
        if (_underlyingOres.TryGetValue(point, out IItem underlyingOre))
        {
            _tiles[point] = underlyingOre;
            _underlyingOres.Remove(point);
        }
    }

    public object GetTile(int x, int y)
    {
        if (IsInBounds(x, y))
        {
            _tiles.TryGetValue(new Point(x, y), out object tile);
            return tile;
        }
        return null;
    }

    public bool HasUnderlyingOre(int x, int y)
    {
        return _underlyingOres.ContainsKey(new Point(x, y));
    }

    public IItem GetUnderlyingOre(int x, int y)
    {
        _underlyingOres.TryGetValue(new Point(x, y), out IItem ore);
        return ore;
    }

    public IEnumerable<Point> GetAllConveyorPositions()
    {
        return _tiles.Where(kvp => kvp.Value is Conveyor).Select(kvp => kvp.Key);
    }

    public IEnumerable<Point> GetAllMachinePositions()
    {
        return _tiles.Where(kvp => kvp.Value is IMachine).Select(kvp => kvp.Key);
    }

    public IEnumerable<Point> GetAllExtractorPositions()
    {
        return _tiles.Where(kvp => kvp.Value is IMachine machine && machine.Type == MachineType.Extractor)
                     .Select(kvp => kvp.Key);
    }

    public IEnumerable<KeyValuePair<Point, object>> GetAllTiles()
    {
        return _tiles;
    }
}