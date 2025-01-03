using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    private readonly List<Activity> activities = new();

    private int currentTick = 0;

    public Map()
    {

    }

    public void AddActivity(Activity activity)
    {
        activities.Add(activity);
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

        placeable.OnAdd(this);
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

    public Dictionary<Placeable, List<Vector2Int>>.KeyCollection AllPlaceables()
    {
        return placeableToCells.Keys;
    }

    public List<Activity> GetActivities()
    {
        return activities;
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

public class Activity
{
    public int encounterLevel;

    public List<ItemType> craftingInputItems;
    public List<ItemType> craftingOutputItems;
    public List<ItemType> droppedItems;

    // One of sourcePlaceable and position must be non-null
    public static Vector2Int NULL_POSITION = new (int.MinValue, int.MinValue);
    private Placeable sourcePlaceable;
    private Vector2Int position;

    public Activity(int encounterLevel,
                    List<ItemType> craftingInputItems,
                    List<ItemType> craftingOutputItems,
                    List<ItemType> droppedItems,
                    Placeable sourcePlaceable,
                    Vector2Int position) // use Activity.NULL_POSITION for null position
    {
        this.encounterLevel = encounterLevel;
        this.craftingInputItems = craftingInputItems;
        this.craftingOutputItems = craftingOutputItems;
        this.droppedItems = droppedItems;
        this.sourcePlaceable = sourcePlaceable;
        this.position = position;
    }

    public Vector2Int GetLocation(Map map)
    {
        if (sourcePlaceable != null)
        {
            return map.PositionOf(sourcePlaceable);
        }
        if (position != Vector2Int.zero)
        {
            return position;
        }
        throw new Exception("Unable to determine location of activity");
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

    public virtual void ObserveAndFeel(Map map)
    {
        
    }

    public virtual void ThinkAndPlan(Map map)
    {
        
    }

    public virtual void Act(Map map)
    {
        
    }

    public virtual void OnAdd(Map map)
    {
        
    }
}

public abstract class Destructable : Placeable
{
    public int damageTaken;

    public Destructable(int sizeCategory) : base(sizeCategory)
    {
        this.damageTaken = 0;
    }

    public void TakeDamage(int damage)
    {
        damageTaken += damage;
    }

    public virtual bool IsDestroyed()
    {
        return HealthFraction() <= 0;
    }

    public abstract float HealthFraction();
}

public class Creature : Destructable
{
    public string name;

    public int level;
    public List<Feat> feats = new();
    public List<TypeAbility> abilities = new();

    public CreatureType creatureType;
    public Goal pursuingGoal;

    public static Creature NewCreatureOfType(CreatureType type)
    {
        return new Creature("Random Name", type);
    }

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

    public int GetMaxHealth()
    {
        return creatureType.GetStartingHealth() + level * creatureType.GetHealthPerLevel();
    }

    public override float HealthFraction()
    {
        return (float)(GetMaxHealth() - damageTaken) / GetMaxHealth();
    }
}

public class Building : Destructable
{
    public BuildingType buildingType;
    
    private Dictionary<string, List<Creature>> creaturesByType = new();
    private int spawnTicksRemaining = 0;
    private Creature spawningCreature; // Null if not spawning

    public Building(BuildingType buildingType) : base(buildingType.GetSize())
    {
        this.buildingType = buildingType;
    }

    public override float HealthFraction()
    {
        return (float)(100 - damageTaken) / 100;
    }

    public void StartSpawnCreature(CreatureType type)
    {
        if (spawningCreature != null)
        {
            return; // Already spawning
        }
        if (!creaturesByType.ContainsKey(type.GetName()))
        {
            throw new System.Exception("Creature type not supported");
        }
        if (GetCurrentCreaturesSupported(type) >= GetMaxCreaturesSupported(type))
        {
            return; // Max creatures already supported
        }

        spawningCreature = Creature.NewCreatureOfType(type);
        creaturesByType[type.GetName()].Add(spawningCreature);
        spawnTicksRemaining = GetSpawnTime(type);
    }

    public List<CreatureType> GetCreatureTypesAvailable()
    {
        return buildingType.GetSupportedCreatureTypes();
    }

    public int GetCurrentCreaturesSupported(CreatureType type)
    {
        if (!creaturesByType.ContainsKey(type.GetName()))
        {
            throw new System.Exception("Creature type not supported");
        }
        return creaturesByType[type.GetName()].Count;
    }

    public int GetMaxCreaturesSupported(CreatureType type)
    {
        int index = IndexOfCreatureType(type);
        return buildingType.GetSupportedCreatureCounts()[index];
    }

    public int GetSpawnTime(CreatureType type)
    {
        int index = IndexOfCreatureType(type);
        return buildingType.GetSupportedCreatureSpawnTimes()[index];
    }

    private int IndexOfCreatureType(CreatureType type)
    {
        List<CreatureType> supportedTypes = GetCreatureTypesAvailable();
        int index = supportedTypes.IndexOf(type);
        if (index == -1)
        {
            throw new System.Exception("Creature type not supported");
        }
        return index;
    }

    public override void ObserveAndFeel(Map map)
    {
        spawnTicksRemaining--;
        if (spawnTicksRemaining == 0)
        {
            spawningCreature = null;
            // Place the creature
            map.Add(spawningCreature, map.PositionOf(this) - new Vector2Int(1, 1));
        }
    }
}

public class Prop : Destructable
{
    public PropType propType;

    private int harvestedAmount = 0;

    public Prop(PropType propType) : base(propType.GetSize())
    {
        this.propType = propType;
    }

    public override float HealthFraction()
    {
        return (float)(100 - damageTaken) / 100;
    }

    public float HarvestedFraction()
    {
        return (float)harvestedAmount / propType.GetHarvestingRequired();
    }

    public override bool IsDestroyed()
    {
        return damageTaken >= 100 || HarvestedFraction() <= 0;
    }

    public override void OnAdd(Map map)
    {
        List<ItemType> droppedItems = propType.GetProducedItems();
        if (droppedItems.Count == 0)
        {
            return;
        }
        Activity harvestActivity = new (0, new(), new(), droppedItems, this, Activity.NULL_POSITION);
        map.AddActivity(harvestActivity);
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