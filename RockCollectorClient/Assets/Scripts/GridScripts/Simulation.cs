using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Simulation
{
    public static void MovePlaceable(Placeable placeable, Map map, Vector2Int destination)
    {
        map.Remove(placeable);
        map.Add(placeable, destination);
    }

    public static void AdvanceTick(Gamestate gamestate)
    {
        gamestate.maps.ForEach(map =>
        {
            
        });
    }
}

public class Gamestate
{
    public List<Map> maps = new();
}

public class Map
{
    // We keep both a dictionary of cells to placeables and a dictionary of placeables to cells
    // We keep them in sync in the Add and Remove methods
    private Dictionary<Vector2Int, List<Placeable>> cells = new();
    private Dictionary<Placeable, List<Vector2Int>> placeableToCells = new();

    public Map()
    {

    }

    public void Add(Placeable placeable, Vector2Int position)
    {
        if (placeableToCells.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable already exists in map");
        }

        List<Vector2Int> occupiedCells = new();
        for (int x = 0; x < placeable.SquaresMinimumOne(); x++)
        {
            for (int y = 0; y < placeable.SquaresMinimumOne(); y++)
            {
                Vector2Int cell = new(position.x + x, position.y + y);
                if (!cells.ContainsKey(cell))
                {
                    cells[cell] = new();
                }
                cells[cell].Add(placeable);
                occupiedCells.Add(cell);
            }
        }
        placeableToCells[placeable] = occupiedCells;
    }

    public void Remove(Placeable placeable)
    {
        if (!placeableToCells.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable does not exist in map");
        }

        foreach (Vector2Int cell in placeableToCells[placeable])
        {
            cells[cell].Remove(placeable);
            if (cells[cell].Count == 0)
            {
                cells.Remove(cell);
            }
        }
        placeableToCells.Remove(placeable);
    }

    public List<Placeable> PlaceablesAt(Vector2Int position)
    {
        if (!cells.ContainsKey(position))
        {
            return new();
        }
        return cells[position];
    }

    public Vector2Int PositionOf(Placeable placeable)
    {
        if (!placeableToCells.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable does not exist in map");
        }
        return placeableToCells[placeable][0];
    }

    public int CountAllPlaceables()
    {
        return placeableToCells.Count;
    }

    public int CountOccupiedSquares()
    {
        return cells.Count;
    }
}

public class Placeable
{
    // sizeCategory is the width and height in cells if positive
    // if negative, the placeable takes up (1/sizeCategory) of a cell
    // sizeCategory 0 placeables take up no space
    private int sizeCategory;

    public Placeable(int sizeCategory)
    {
        this.sizeCategory = sizeCategory;
    }

    public int SquaresMinimumOne()
    {
        return Mathf.Max(1, sizeCategory);
    }
}