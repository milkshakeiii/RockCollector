using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UnityEngine;

public class Simulation 
{
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
    private Dictionary<Vector2Int, List<Placeable>> cells = new();
    private Dictionary<Placeable, List<Vector2Int>> placeableToCells = new();
    
    private Dictionary<Placeable, Placeable> heldToHolder = new();
    private Dictionary<Placeable, List<Placeable>> holderToHeld = new();

    private int currentTick = 0;

    public Map()
    {

    }

    public Map ShallowCopy()
    {
        return new()
        {
            cells = new Dictionary<Vector2Int, List<Placeable>>(cells),
            placeableToCells = new Dictionary<Placeable, List<Vector2Int>>(placeableToCells),
            heldToHolder = new Dictionary<Placeable, Placeable>(heldToHolder),
            holderToHeld = new Dictionary<Placeable, List<Placeable>>(holderToHeld),
            currentTick = currentTick
        };
    }

    public void PickUp(Placeable holder, Placeable held)
    {
        if (heldToHolder.ContainsKey(held))
        {
            throw new System.Exception("Placeable is already held");
        }
        int xDiff = PositionOf(holder).x - PositionOf(held).x;
        int yDiff = PositionOf(holder).y - PositionOf(held).y;
        if (Math.Abs(xDiff) > 1 || Math.Abs(yDiff) > 1)
        {
            throw new System.Exception("Placeable is not adjacent to holder");
        }

        // Remove from cells dicts
        Remove(held);

        // Add to held dicts
        heldToHolder[held] = holder;
        if (!holderToHeld.ContainsKey(holder))
        {
            holderToHeld[holder] = new();
        }
        holderToHeld[holder].Add(held);
    }

    public void Transfer(Placeable placeable, Placeable newHolder)
    {
        if (!heldToHolder.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable is not held");
        }
        if (heldToHolder[placeable] == newHolder)
        {
            throw new System.Exception("Placeable is already held by new holder");
        }

        // Remove from held dicts
        Placeable holder = heldToHolder[placeable];
        heldToHolder.Remove(placeable);
        holderToHeld[holder].Remove(placeable);
        if (holderToHeld[holder].Count == 0)
        {
            holderToHeld.Remove(holder);
        }

        // Add to held dicts
        heldToHolder[placeable] = newHolder;
        if (!holderToHeld.ContainsKey(newHolder))
        {
            holderToHeld[newHolder] = new();
        }
        holderToHeld[newHolder].Add(placeable);
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

    public void Remove(Placeable placeable, bool removeHeldItems = true)
    {
        if (!placeableToCells.ContainsKey(placeable) && !heldToHolder.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable does not exist in map");
        }

        // If this is a held item, remove it from held dicts
        if (heldToHolder.ContainsKey(placeable))
        {
            Placeable holder = heldToHolder[placeable];
            holderToHeld[holder].Remove(placeable);
            if (holderToHeld[holder].Count == 0)
            {
                holderToHeld.Remove(holder);
            }
            heldToHolder.Remove(placeable);
            return;
        }

        // Remove unheld placeable
        foreach (Vector2Int cell in placeableToCells[placeable])
        {
            cells[cell].Remove(placeable);
            if (cells[cell].Count == 0)
            {
                cells.Remove(cell);
            }
        }
        placeableToCells.Remove(placeable);

        // Remove unheld placeable's held placeables if removeHeldItems is true
        if (removeHeldItems && holderToHeld.ContainsKey(placeable))
        {
            foreach (Placeable held in holderToHeld[placeable])
            {
                if (holderToHeld.ContainsKey(held))
                {
                    throw new System.Exception("Placeable is holding a placeable that is also holding");
                }
                // remove from heldToHolder
                heldToHolder.Remove(held);
            }
            holderToHeld.Remove(placeable);
        }
    }

    public void MovePlaceable(Placeable placeable, Vector2Int newPosition)
    {
        if (!placeableToCells.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable does not exist in any cell (is it being held?)");
        }

        Remove(placeable, removeHeldItems: false);
        Add(placeable, newPosition);
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
        if (heldToHolder.ContainsKey(placeable))
        {
            Placeable holder = heldToHolder[placeable];
            if (heldToHolder.ContainsKey(holder))
            {
                throw new System.Exception("Placeable is held by a placeable that is also held");
            }
            return PositionOf(holder);
        }
        if (!placeableToCells.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable does not exist in map");
        }
        return placeableToCells[placeable][0];
    }

    public Dictionary<Placeable, List<Vector2Int>>.KeyCollection UnheldPlaceables()
    {
        return placeableToCells.Keys;
    }

    public Dictionary<Placeable, Placeable>.KeyCollection HeldPlaceables()
    {
        return heldToHolder.Keys;
    }

    public bool IsHeld(Placeable placeable)
    {
        return heldToHolder.ContainsKey(placeable);
    }

    public List<Placeable> HeldPlaceablesOf(Placeable holder)
    {
        if (!holderToHeld.ContainsKey(holder))
        {
            return new();
        }
        return holderToHeld[holder];
    }

    public List<Activity> GetActivities(Creature forCreature)
    {
        List<Activity> candidateActivities = new();

        foreach (Placeable placeable in UnheldPlaceables())
        {
            if (placeable is Creature creature && creature.teamNumber != forCreature.teamNumber)
            {
                // creatures can be hunted
                candidateActivities.Add(new HuntActivity(creature));
            }
            if (placeable is Building building)
            {
                // buildings can be dropped off at
                candidateActivities.Add(new DropOffActivity(building));
                // and also crafted at
                foreach (ItemType itemType in building.buildingType.GetCraftedItemTypes())
                {
                    candidateActivities.Add(new CraftActivity(itemType, building));
                }
            }
            if (placeable is Prop prop)
            {
                // props can be harvested
                List<ItemType> droppedItems = prop.propType.GetProducedItems();
                if (droppedItems.Count == 0)
                {
                    continue;
                }
                HarvestActivity harvestActivity = new(prop);
                candidateActivities.Add(harvestActivity);
            }
            if (placeable is Item item)
            {
                // items can be picked up
                candidateActivities.Add(new PickUpActivity(item));
            }
        }

        List<Activity> activities = new();
        foreach (Activity activity in candidateActivities)
        {
            if (!activity.IsCompletedOrImpossible(this, forCreature))
            {
                activities.Add(activity);
            }
        }
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
        List<Placeable> placeables = new(HeldPlaceables());
        placeables.AddRange(UnheldPlaceables());

        foreach (Placeable placeable in placeables)
        {
            placeable.ObserveAndFeel(this);
        }

        foreach (Placeable placeable in placeables)
        {
            placeable.ThinkAndPlan(this);
        }

        foreach (Placeable placeable in placeables)
        {
            placeable.Act(this);
        }

        foreach (Placeable placeable in placeables)
        {
            if (placeable is Destructable destructable && destructable.IsDestroyed())
            {
                destructable.OnDestroyed(this);
                Remove(destructable);
            }
            if (placeable is Item item && item.IsConsumed())
            {
                Remove(item);
            }
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
    protected int damageTaken;

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

    public virtual void OnDestroyed(Map map)
    {
        
    }

    public abstract float HealthFraction();
}

public class Creature : Destructable
{
    private string name;
    public int teamNumber = 0;

    private int level = 0;
    private List<Feat> feats = new();
    private List<TypeAbility> abilities = new();

    private CreatureType creatureType;

    private Goal pursuingGoal;
    private Activity nextActivity;

    private int cooldownTicksRemaining = 0;

    public static Creature NewCreatureOfType(CreatureType type)
    {
        return new Creature("Random Name", 0, type);
    }

    public Creature(string name, int teamNumber, CreatureType creatureType) : base(creatureType.GetSizeCategory())
    {
        this.name = name;
        this.teamNumber = teamNumber;
        this.creatureType = creatureType;
        LevelUp();
    }

    public int EncounterLevel()
    {
        return level;
    }

    public void LevelUp()
    {
        level++;
        if (level == 1 || level % 3 == 0)
        {
            // Add a feat
        }
        if (level == 1 || level % 5 == 0)
        {
            // Add an ability
            TypeAbility ability = creatureType.GetAbilities()[0];
            abilities.Add(ability); 
        }
    }

    public void HarvestProp(Prop prop, Map map)
    {
        string neededSkill = prop.propType.GetHarvestingSkill();
        (TypeAbility bestAbility, int bestAmount) = BestHarvestingAmountAndAbility(neededSkill);
        if (bestAbility == null)
        {
            throw new System.Exception("No ability found to harvest prop");
        }
        prop.TakeHarvest(bestAmount);
        cooldownTicksRemaining = bestAbility.GetCooldown();
    }

    public (TypeAbility, int) BestHarvestingAmountAndAbility(string skill)
    {
        TypeAbility bestAbility = null;
        int bestAmount = 0;
        foreach (TypeAbility ability in abilities)
        {
            if (ability.GetHarvestingSkill() == skill)
            {
                if (ability.GetHarvestingAmount() > bestAmount)
                {
                    bestAbility = ability;
                    bestAmount = ability.GetHarvestingAmount();
                }
            }
        }
        return (bestAbility, bestAmount);
    }

    public CreatureType GetCreatureType()
    {
        return creatureType;
    }

    public Goal GetGoal()
    {
        return pursuingGoal;
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
        nextActivity ??= pursuingGoal.GetNextActivity(map, this);
    }

    public int MoveSpeed()
    {
        return 10;
    }

    public override void Act(Map map)
    {
        if (cooldownTicksRemaining > 0)
        {
            cooldownTicksRemaining--;
            return;
        }
        else if (nextActivity == null)
        {
            return;
        }
        else if (nextActivity.IsCompletedOrImpossible(map, this))
        {
            // Something else completed the activity this frame
            // or there is no activity assigned
            nextActivity = null;
            return;
        }
        else if (DistanceToNextActivity(map) > 1)
        {
            Vector2Int difference = DirectionToNextActivity(map);
            Vector2Int direction = new (Math.Sign(difference.x), Math.Sign(difference.y));
            Vector2Int newPosition = map.PositionOf(this) + direction;
            map.MovePlaceable(this, newPosition);
            cooldownTicksRemaining = MoveSpeed();
            return;
        }
        else // Perform the activity
        {
            Debug.Log(nextActivity);
            nextActivity.Perform(this, map);
            if (nextActivity.IsCompletedOrImpossible(map, this))
            {
                Debug.Log("Activity completed");
                nextActivity = null; // Otherwise, there would be a "stunned" frame
            }
            return;
        }
    }

    private int DistanceToNextActivity(Map map)
    {
        Vector2Int location = nextActivity.GetLocation(map);
        Vector2Int currentPosition = map.PositionOf(this);
        return Math.Min(Math.Abs(location.x - currentPosition.x), Math.Abs(location.y - currentPosition.y));
    }

    private Vector2Int DirectionToNextActivity(Map map)
    {
        Vector2Int location = nextActivity.GetLocation(map);
        Vector2Int currentPosition = map.PositionOf(this);
        return location - currentPosition;
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
        return damageTaken >= 100 || HarvestedFraction() >= 1;
    }

    public override void OnAdd(Map map)
    {
        
    }

    public override void OnDestroyed(Map map)
    {
        if (HarvestedFraction() >= 1)
        {
            List<ItemType> droppedItems = propType.GetProducedItems();
            ItemType droppedItem = droppedItems[0]; // TODO: Factor in difficulty and possibly limit to one produced item type
            map.Add(new Item(droppedItem), map.PositionOf(this));
        }
    }

    public void TakeHarvest(int amount)
    {
        harvestedAmount += amount;
    }
}

public class Item : Placeable
{
    public ItemType itemType;
    private bool consumed = false;

    public Item(ItemType itemType) : base(itemType.GetSize())
    {
        this.itemType = itemType;
    }

    public void Consume()
    {
        consumed = true;
    }

    public bool IsConsumed()
    {
        return consumed;
    }
}

public abstract class Goal 
{
    public static Goal NameToGoal(string name)
    {
        if (name == "Craft")
        {
            return new Craft();
        }
        throw new System.Exception("Goal not found");
    }

    public abstract bool IsAchieved();

    public abstract int EvaluateMap(Map map);

    public abstract void Initialize(int tickBegun);

    public abstract Activity GetNextActivity(Map map, Creature creature);
}

public class Craft : Goal
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
       foreach (Placeable placeable in map.UnheldPlaceables())
       {
           
       }
       return 0;
    }

    public override Activity GetNextActivity(Map map, Creature creature)
    {
        if (map.GetActivities(creature).Count == 0)
        {
            return null;
        }
        return Search.CraftSearch(map, creature);
    }
}