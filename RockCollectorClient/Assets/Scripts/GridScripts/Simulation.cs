using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            map.AdvanceTick();
        });
    }
}

public class Action
{

}

public class Gamestate
{
    public List<Map> maps = new();
}

public class Map
{
    // We keep both a dictionary of cells to placeables and a dictionary of placeables to cells
    // We keep them in sync in the Add and Remove methods
    private readonly Dictionary<Vector2Int, List<Placeable>> cells = new();
    private readonly Dictionary<Placeable, List<Vector2Int>> placeableToCells = new();

    private int currentTick = 0;

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

    public int CurrentTick()
    {
        return currentTick;
    }

    public void AdvanceTick()
    {
        foreach (Placeable placeable in placeableToCells.Keys)
        {
            placeable.ObserveAndFeel(this);
        }


        foreach (Placeable placeable in placeableToCells.Keys)
        {
            placeable.ThinkAndPlan(this);
        }


        foreach (Placeable placeable in placeableToCells.Keys)
        {
            placeable.Act(this);
        }

        currentTick++;
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

    public uint SquaresMinimumOne()
    {
        return (uint)Mathf.Max(1, sizeCategory);
    }

    public virtual void ObserveAndFeel(Map map)
    {
        
    }

    public virtual void ThinkAndPlan(Map map)
    {
        
    }

    public virtual void Act(Map map)
    {
        
    }
}

public class Creature : Placeable
{
    public string name;

    public int level;
    public int currentHealth;
    public List<Feat> feats = new();
    public List<TypeAbility> abilities = new();

    public CreatureType creatureType;
    public Goal pursuingGoal;

    public Creature(string name, CreatureType creatureType) : base(creatureType.GetSizeCategory())
    {
        this.name = name;
        this.creatureType = creatureType;
    }

    public override void ObserveAndFeel(Map map)
    {
        
    }

    public override void ThinkAndPlan(Map map)
    {
        if (ShouldGetNewGoal(map))
        {
            pursuingGoal = GetNewGoal(map);
        }
    }

    public override void Act(Map map)
    {
        
    }

    private bool ShouldGetNewGoal(Map map)
    {
        return pursuingGoal == null || pursuingGoal.IsAchieved();
    }

    private Goal GetNewGoal(Map map)
    {
        Goal newGoal = creatureType.GetGoals()[0];
        newGoal.Initialize(map.CurrentTick());
        return newGoal;
    }
}

public abstract class Goal
{
    public static Goal NameToGoal(string name)
    {
        if (name == "PersonalWealth")
        {
            return new PersonalWealth();
        }
        throw new System.Exception("Goal not found");
    }

    public abstract bool IsAchieved();

    public abstract int EvaluateMap(Map map);

    public abstract void Initialize(int tickBegun);
}

public class PersonalWealth : Goal
{
    private int achievedAmount;
    private int targetAmount;

    private int tickBegun;

    public override void Initialize(int tickBegun)
    {
        this.tickBegun = tickBegun;
        this.achievedAmount = 0;
        this.targetAmount = 100;
    }

    public override bool IsAchieved()
    {
        return achievedAmount >= targetAmount;
    }

    public override int EvaluateMap(Map map)
    {
        throw new System.NotImplementedException();
    }
}