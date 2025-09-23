using System;
using System.IO;
using MonoGameLibrary.Graphics;
using CarFactoryArchitect.Source.Items;
using CarFactoryArchitect.Source.Core;

namespace CarFactoryArchitect.Source.Maps;

public enum TileType
{
    Empty = 0,
    IronOre = 1,
    CopperOre = 2,
    SandOre = 3,
    RubberOre = 4
}

public static class MapLoader
{
    public static void LoadMap(string mapFileName, World world, TextureAtlas atlas, float scale)
    {
        try
        {
            string filePath = Path.Combine("Content/maps", mapFileName);
            string[] lines = File.ReadAllLines(filePath);

            Console.WriteLine($"Loading map: {mapFileName}");

            for (int y = 0; y < lines.Length; y++)
            {
                string line = lines[y].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                for (int x = 0; x < values.Length; x++)
                {
                    if (int.TryParse(values[x], out int tileValue))
                    {
                        CreateTileFromValue(tileValue, x, y, world, atlas, scale);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading map {mapFileName}: {ex.Message}");
        }
    }

    private static void CreateTileFromValue(int value, int x, int y, World world, TextureAtlas atlas, float scale)
    {
        TileType tileType = (TileType)value;
        IItem item = null;

        switch (tileType)
        {
            case TileType.Empty:
                // Do nothing for empty tiles
                break;

            case TileType.IronOre:
                item = ItemFactory.CreateItem(OreType.Iron, OreState.Tile, atlas, scale);
                break;

            case TileType.CopperOre:
                item = ItemFactory.CreateItem(OreType.Copper, OreState.Tile, atlas, scale);
                break;

            case TileType.SandOre:
                item = ItemFactory.CreateItem(OreType.Sand, OreState.Tile, atlas, scale);
                break;

            case TileType.RubberOre:
                item = ItemFactory.CreateItem(OreType.Rubber, OreState.Tile, atlas, scale);
                break;

            default:
                Console.WriteLine($"Unknown tile type: {value} at position ({x}, {y})");
                break;
        }

        if (item != null)
        {
            world.PlaceOre(x, y, item);
        }
    }

    public static void SaveMap(string mapFileName, World world)
    {
        try
        {
            string filePath = Path.Combine("Content/maps", mapFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int y = 0; y < world.GridSize; y++)
                {
                    string line = "";
                    for (int x = 0; x < world.GridSize; x++)
                    {
                        var tile = world.GetTile(x, y);
                        int tileValue = GetTileValue(tile);

                        line += tileValue.ToString();
                        if (x < world.GridSize - 1) line += ",";
                    }
                    writer.WriteLine(line);
                }
            }

            Console.WriteLine($"Map saved: {mapFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving map {mapFileName}: {ex.Message}");
        }
    }

    private static int GetTileValue(object tile)
    {
        return tile switch
        {
            IItem item => item.Type switch
            {
                OreType.Iron => (int)TileType.IronOre,
                OreType.Copper => (int)TileType.CopperOre,
                OreType.Sand => (int)TileType.SandOre,
                OreType.Rubber => (int)TileType.RubberOre,
                _ => (int)TileType.Empty
            },
            null => (int)TileType.Empty,
            _ => (int)TileType.Empty
        };
    }
}