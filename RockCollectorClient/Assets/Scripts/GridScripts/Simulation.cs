using NUnit.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using UnityEngine;

public class Simulation 
{
    public static void AdvanceTick(Gamestate gamestate)
    {
        gamestate.maps.ForEach(map =>
        {
            map.AdvanceTick(new());
        });
    }

    public static void AbilityEffect(TypeAbility ability, Creature actor, Map map)
    {
        if (ability.GetEnemyTargets() > 0)
        {
            List<string> weaponSkills = ability.GetWeaponSkills();
            // if the ability uses weapon skills, it is a basic weapon attack
            if (weaponSkills.Count > 0)
            {
                BasicWeaponAttack(ability, actor, map, weaponSkills);
                return;
            }
        }
    }

    private static void BasicWeaponAttack(TypeAbility ability, Creature actor, Map map, List<string> weaponSkills)
    {
        DieRoll damage = null;
        int range = 0;
        string chosenWeaponSkill = null;
        // get the damage and range based on the actor's preferred weapon skills
        foreach (string preferredWeaponSkill in actor.GetPreferredWeaponSkills())
        {
            if (weaponSkills.Contains(preferredWeaponSkill))
            {
                (damage, range) = actor.WeaponDamangeAndRange(preferredWeaponSkill, map);
                if (damage != null)
                {
                    chosenWeaponSkill = preferredWeaponSkill;
                    break;
                }
            }
        }
        if (damage == null)
        {
            throw new System.Exception("No weapon to use with ability.");
        }
        int chosenWeaponModifier = actor.SkillModifier(chosenWeaponSkill);
        // we have also now set the chosenWeaponSkill and chosenWeaponModifier

        // get the range of possible targets
        RectInt actorRect = map.ExtentsOf(actor);
        int xMin = actorRect.xMin - range;
        int xMax = actorRect.xMax + range;
        int yMin = actorRect.yMin - range;
        int yMax = actorRect.yMax + range;

        // strike up to the maximum number of targets
        int targetsStruck = 0;
        int maxTargets = ability.GetEnemyTargets();
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                Vector2Int position = new(x, y);
                List<Placeable> placeables = map.PlaceablesAt(position);
                foreach (Placeable placeable in placeables)
                {
                    if (placeable is Creature target && target.teamNumber != actor.teamNumber)
                    {
                        actor.Strike(target, damage, chosenWeaponModifier, chosenWeaponSkill, map);
                        targetsStruck++;
                        if (targetsStruck >= ability.GetEnemyTargets())
                        {
                            break;
                        }
                    }
                }
            }
            if (targetsStruck >= ability.GetEnemyTargets())
            {
                break;
            }
        }
    }
}

public class Gamestate 
{
    public List<Map> maps = new();
}

public class Map 
{
    public static Vector2Int NULL_POSITION = new(int.MinValue, int.MinValue);

    // We keep both a dictionary of cells to placeables and a dictionary of placeables to cells
    // We keep them in sync in the Add and Remove methods
    private Dictionary<Vector2Int, List<Placeable>> cells = new();
    private Dictionary<Placeable, List<Vector2Int>> placeableToCells = new();
    
    private Dictionary<Placeable, Placeable> heldToHolder = new();
    private Dictionary<Placeable, List<Placeable>> holderToHeld = new();

    private Dictionary<Creature, HashSet<Item>> creatureOutfits = new();

    private int currentTick = 0;

    public Map()
    {

    }

    public (Map, Dictionary<Placeable, Placeable>, Creature) DeepCopy(Creature caller)
    {
        Map deepCopy = new Map();

        // This is a deep copy. We need to create new instances of all the placeables.
        Dictionary<Placeable, Placeable> placeableCopies = new();
        foreach (Placeable placeable in AllPlaceables())
        {
            Placeable placeableCopy = placeable.DeepCopy();
            placeableCopies[placeable] = placeableCopy;
        }

        deepCopy.cells = new Dictionary<Vector2Int, List<Placeable>>();
        foreach (Vector2Int cell in cells.Keys)
        {
            deepCopy.cells[cell] = new List<Placeable>();
            foreach (Placeable placeable in cells[cell])
            {
                deepCopy.cells[cell].Add(placeableCopies[placeable]);
            }
        }

        deepCopy.placeableToCells = new Dictionary<Placeable, List<Vector2Int>>();
        foreach (Placeable placeable in placeableToCells.Keys)
        {
            deepCopy.placeableToCells[placeableCopies[placeable]] = new List<Vector2Int>(placeableToCells[placeable]);
        }

        deepCopy.heldToHolder = new Dictionary<Placeable, Placeable>();
        foreach (Placeable held in heldToHolder.Keys)
        {
            deepCopy.heldToHolder[placeableCopies[held]] = placeableCopies[heldToHolder[held]];
        }

        deepCopy.holderToHeld = new Dictionary<Placeable, List<Placeable>>();
        foreach (Placeable holder in holderToHeld.Keys)
        {
            deepCopy.holderToHeld[placeableCopies[holder]] = new List<Placeable>();
            foreach (Placeable held in holderToHeld[holder])
            {
                deepCopy.holderToHeld[placeableCopies[holder]].Add(placeableCopies[held]);
            }
        }

        deepCopy.creatureOutfits = new Dictionary<Creature, HashSet<Item>>();
        foreach (Creature creature in creatureOutfits.Keys)
        {
            deepCopy.creatureOutfits[(Creature)placeableCopies[creature]] = new HashSet<Item>();
            foreach (Item item in creatureOutfits[creature])
            {
                deepCopy.creatureOutfits[(Creature)placeableCopies[creature]].Add((Item)placeableCopies[item]);
            }
        }

        deepCopy.currentTick = currentTick;

        Dictionary<Placeable, Placeable> backDictionary = new();
        foreach (Placeable key in placeableCopies.Keys)
        {
            backDictionary[placeableCopies[key]] = key;
        }
        return (deepCopy, backDictionary, (Creature)placeableCopies[caller]);
    }

    public void PickUp(Placeable holder, Placeable held)
    {
        if (heldToHolder.ContainsKey(held))
        {
            throw new System.Exception("Placeable is already held");
        }
        int distance = DistanceBetween(holder, held);
        if (distance > 1)
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
        Placeable previousHolder = heldToHolder[placeable];
        if (previousHolder is Creature creature && placeable is Item item && creature.OutfitContains(item, this))
        {
            throw new System.Exception("Cannot transfer item that is part of a creature's outfit.");
        }

        // Remove from held dicts
        heldToHolder.Remove(placeable);
        holderToHeld[previousHolder].Remove(placeable);
        if (holderToHeld[previousHolder].Count == 0)
        {
            holderToHeld.Remove(previousHolder);
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
        if (heldToHolder.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable already exists (being held)");
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

    public void AddHeld(Placeable holder, Placeable held)
    {
        if (placeableToCells.ContainsKey(held))
        {
            throw new System.Exception("Placeable already exists in map");
        }
        if (heldToHolder.ContainsKey(held))
        {
            throw new System.Exception("Placeable already exists (being held)");
        }
        if (!holderToHeld.ContainsKey(holder))
        {
            holderToHeld[holder] = new();
        }
        holderToHeld[holder].Add(held);
        heldToHolder[held] = holder;
    }

    // Warning: not removing held items can cause dangling references
    public void Remove(Placeable placeable, bool removeHeldItems = true)
    {
        if (!placeableToCells.ContainsKey(placeable) && !heldToHolder.ContainsKey(placeable))
        {
            throw new System.Exception("Placeable does not exist in map");
        }

        // if this is a creature, remove it from the outfit dict
        if (placeable is Creature outfitCreature)
        {
            if (creatureOutfits.ContainsKey(outfitCreature))
            {
                creatureOutfits.Remove(outfitCreature);
            }
        }

        // If this is a held item, remove it from held dicts
        if (heldToHolder.ContainsKey(placeable))
        {
            Placeable holder = heldToHolder[placeable];
            if (placeable is Item item && holder is Creature creature && creature.OutfitContains(item, this))
            {
                throw new System.Exception("Cannot remove placeable that is part of a creature's outfit.");
            }
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

    // Returns the position of the minimum (bottom-leftmost) cell occupied by the placeable
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

    public RectInt ExtentsOf(Placeable placeable)
    {
        Vector2Int position = PositionOf(placeable);
        return new RectInt(position.x, position.y, placeable.SquaresMinimumOne()-1, placeable.SquaresMinimumOne()-1);
    }

    public int DistanceBetween(Placeable placeable1, Placeable placeable2)
    {
        RectInt extents1 = ExtentsOf(placeable1);
        RectInt extents2 = ExtentsOf(placeable2);
        // find the closest x distance by subtracting the rightmost left edge from the leftmost right
        int xDistance = 0;
        if (extents1.xMax < extents2.xMin)
        {
            xDistance = extents2.xMin - extents1.xMax;
        }
        else if (extents2.xMax < extents1.xMin)
        {
            xDistance = extents1.xMin - extents2.xMax;
        }
        // find the closest y distance by subtracting the topmost bottom edge from the bottommost top
        int yDistance = 0;
        if (extents1.yMax < extents2.yMin)
        {
            yDistance = extents2.yMin - extents1.yMax;
        }
        else if (extents2.yMax < extents1.yMin)
        {
            yDistance = extents1.yMin - extents2.yMax;
        }
        return Math.Max(xDistance, yDistance);
    }

    public int DistanceTo(Vector2Int position, Placeable forPlaceable)
    {
        RectInt placeableRect = ExtentsOf(forPlaceable);
        int xDistance = 0;
        if (position.x < placeableRect.xMin)
        {
            xDistance = placeableRect.xMin - position.x;
        }
        else if (position.x > placeableRect.xMax)
        {
            xDistance = position.x - placeableRect.xMax;
        }
        int yDistance = 0;
        if (position.y < placeableRect.yMin)
        {
            yDistance = placeableRect.yMin - position.y;
        }
        else if (position.y > placeableRect.yMax)
        {
            yDistance = position.y - placeableRect.yMax;
        }
        return Math.Max(xDistance, yDistance);
    }

    public Dictionary<Placeable, List<Vector2Int>>.KeyCollection UnheldPlaceables()
    {
        return placeableToCells.Keys;
    }

    public Dictionary<Placeable, Placeable>.KeyCollection HeldPlaceables()
    {
        return heldToHolder.Keys;
    }

    public List<Placeable> AllPlaceables()
    {
        List<Placeable> placeables = new();
        placeables.AddRange(UnheldPlaceables());
        placeables.AddRange(HeldPlaceables());
        return placeables;
    }

    public bool PlaceableExists(Placeable placeable)
    {
        return placeableToCells.ContainsKey(placeable) || heldToHolder.ContainsKey(placeable);
    }

    public bool IsHeld(Placeable placeable)
    {
        return heldToHolder.ContainsKey(placeable);
    }

    public Placeable HolderOf(Placeable held)
    {
        if (!heldToHolder.ContainsKey(held))
        {
            return null;
        }
        return heldToHolder[held];
    }

    public List<Placeable> HeldPlaceablesOf(Placeable holder)
    {
        if (!holderToHeld.ContainsKey(holder))
        {
            return new();
        }
        return holderToHeld[holder];
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

    public void AdvanceTick(List<MapCommand> inputCommands)
    {
        foreach (MapCommand command in inputCommands)
        {
            if (command.CheckStillValid(this))
            {
                command.Execute(this);
            }
            else 
            {
                Debug.Log("Command " + command + " is no longer valid.");
            }
        }

        List<Placeable> placeables = AllPlaceables();

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

    public MapView GetView()
    {
        return MapView.GetMapView(this);
    }

    public bool IsBuildable(Vector2Int buildingPosition, int size)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int position = new(buildingPosition.x + x, buildingPosition.y + y);
                List<Placeable> placeablesAtPosition = PlaceablesAt(position);
                foreach (Placeable placeable in placeablesAtPosition)
                {
                    if (placeable is Building || placeable is Prop)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void AddToOutfit(Creature creature, Item item)
    {
        if (!creatureOutfits.ContainsKey(creature))
        {
            creatureOutfits[creature] = new();
        }
        creatureOutfits[creature].Add(item);
    }

    public bool IsInOutfit(Creature creature, Item item)
    {
        if (!creatureOutfits.ContainsKey(creature))
        {
            return false;
        }
        return creatureOutfits[creature].Contains(item);
    }
}

public class MapView 
{
    public static Dictionary<Map, MapView> mapViews = new();

    public static MapView GetMapView(Map map)
    {
        if (!mapViews.ContainsKey(map))
        {
            mapViews[map] = new MapView(map);
        }
        return mapViews[map];
    }

    private readonly Map map;

    public MapView(Map map)
    {
        this.map = map;
    }

    public List<PlaceableView> PlaceablesAt(Vector2Int position) 
    {
        List<Placeable> placeables = map.PlaceablesAt(position);
        List<PlaceableView> placeableViews = new();
        foreach (Placeable placeable in placeables)
        {
            placeableViews.Add(PlaceableView.GetPlaceableView(placeable, map));
        }
        return placeableViews;
    }

    public Vector2Int PositionOf(PlaceableView placeable)
    {
        return map.PositionOf(PlaceableView.GetPlaceable(placeable));
    }

    public RectInt ExtentsOf(PlaceableView placeable)
    {
        return map.ExtentsOf(PlaceableView.GetPlaceable(placeable));
    }

    public int DistanceBetween(PlaceableView placeable1, PlaceableView placeable2)
    {
        return map.DistanceBetween(PlaceableView.GetPlaceable(placeable1), PlaceableView.GetPlaceable(placeable2));
    }

    public int DistanceTo(Vector2Int position, PlaceableView forPlaceable)
    {
        return map.DistanceTo(position, PlaceableView.GetPlaceable(forPlaceable));
    }

    public List<PlaceableView> UnheldPlaceables()
    {
        List<PlaceableView> placeableViews = new();
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            placeableViews.Add(PlaceableView.GetPlaceableView(placeable, map));
        }
        return placeableViews;
    }

    public List<PlaceableView> HeldPlaceables()
    {
        List<PlaceableView> placeableViews = new();
        foreach (Placeable placeable in map.HeldPlaceables())
        {
            placeableViews.Add(PlaceableView.GetPlaceableView(placeable, map));
        }
        return placeableViews;
    }

    public List<PlaceableView> AllPlaceables()
    {
        List<PlaceableView> placeableViews = new();
        foreach (Placeable placeable in map.AllPlaceables())
        {
            placeableViews.Add(PlaceableView.GetPlaceableView(placeable, map));
        }
        return placeableViews;
    }

    public bool PlaceableExists(PlaceableView placeable)
    {
        return map.PlaceableExists(PlaceableView.GetPlaceable(placeable));
    }

    public bool IsHeld(PlaceableView placeable)
    {
        return map.IsHeld(PlaceableView.GetPlaceable(placeable));
    }

    public List<PlaceableView> HeldPlaceablesOf(PlaceableView holder)
    {
        List<PlaceableView> placeableViews = new();
        foreach (Placeable placeable in map.HeldPlaceablesOf(PlaceableView.GetPlaceable(holder)))
        {
            placeableViews.Add(PlaceableView.GetPlaceableView(placeable, map));
        }
        return placeableViews;
    }

    public int CurrentTick()
    {
        return map.CurrentTick();
    }
}