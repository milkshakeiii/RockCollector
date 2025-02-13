using NUnit.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class Simulation 
{
    public static event WeaponStrike OnWeaponStrike;
    public delegate void WeaponStrike(Creature actor, Vector2Int target, Item weapon, Map map);

    public static event NonweaponStrike OnNonweaponStrike;
    public delegate void NonweaponStrike(Creature actor, Vector2Int target, TypeAbility ability, Map map);

    public static event ConditionApplied OnConditionApplied;
    public delegate void ConditionApplied(Creature actor, Creature target, ConditionType condition, Map map);

    public static event GameWin OnGameWin;
    public delegate void GameWin(Gamestate gamestate);

    public static event GameLose OnGameLose;
    public delegate void GameLose(Gamestate gamestate);

    public static void AdvanceTick(Gamestate gamestate, List<MapCommand> inputCommands)
    {
        // first advance the map, including update methods for all placeables
        gamestate.map.AdvanceTick(inputCommands);

        // then check for win/lose conditions
        if (gamestate.scenario.CheckWinCondition(gamestate.map))
        {
            OnGameWin?.Invoke(gamestate);
        }
        else if (gamestate.scenario.CheckLoseCondition(gamestate.map))
        {
            OnGameLose?.Invoke(gamestate);
        }

        // perform computations from the thinking queue
        ThinkingQueue.PerformActions(1, gamestate.map);
    }

    public static void AbilityEffect(TypeAbility ability, Creature actor, Map map)
    {
        if (ability.GetEnemyTargets() > 0)
        {
            List<string> weaponSkills = ability.GetWeaponSkills();
            // if the ability uses weapon skills, it is a weapon attack
            if (weaponSkills.Count > 0)
            {
                WeaponAttack(ability, actor, map, weaponSkills);
            }

            DieRoll damage = ability.GetDamage();
            // if the ability has a damage value, it is a non-weapon attack
            // alternatively if the abilities does no damage but applies conditions, it is a non-weapon attack
            // note that it is possible to be both a weapon and non-weapon attack
            if (damage != null || ability.GetConditionsInflicted().Count > 0)
            {
                NonweaponAttack(ability, actor, map);
            }
        }

        if (ability.GetAllyTargets() > 0)
        {
            if (ability.GetConditionsInflicted().Count > 0)
            {
                AllyEffect(ability, actor, map);
            }
        }
    }

    private static void WeaponAttack(TypeAbility ability, Creature actor, Map map, List<string> weaponSkills)
    {
        DieRoll damage = null;
        int range = 0;
        string chosenWeaponSkill = null;
        // get the damage and range based on the actor's preferred weapon skills
        foreach (string preferredWeaponSkill in actor.GetPreferredWeaponSkills())
        {
            if (weaponSkills.Contains(preferredWeaponSkill))
            {
                (damage, range) = actor.WeaponBaseDamangeAndRange(preferredWeaponSkill, map);
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
                        if (range == 1)
                        {
                            actor.MeleeStrike(target, damage, chosenWeaponSkill, map);
                        }
                        else
                        {
                            actor.RangedStrike(target, damage, chosenWeaponSkill, map);
                        }
                        OnWeaponStrike?.Invoke(actor, position, actor.GetWeapon(chosenWeaponSkill, map), map);
                        InflictConditionsFromAbility(ability, actor, target, map);
                        InflictConditionsFromWeapon(chosenWeaponSkill, actor, target, map);
                        targetsStruck++;
                        if (targetsStruck >= maxTargets)
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

        // if we still haven't struck the maximum number of targets, try attacking buildings
        if (targetsStruck < maxTargets)
        {
            foreach (Placeable placeable in map.UnheldPlaceables())
            {
                if (placeable is Building building && building.teamNumber != actor.teamNumber)
                {
                    if (map.DistanceBetween(actor, building) <= range)
                    {
                        actor.AttackBuilding(ability, building, map);
                        targetsStruck++;
                        if (targetsStruck >= maxTargets)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private static void NonweaponAttack(TypeAbility ability, Creature actor, Map map)
    {
        DieRoll damage = ability.GetDamage();
        int range = ability.GetRange();
        int radius = ability.GetEffectRadius();
        string skill = ability.GetSkill();
        int maxTargets = ability.GetEnemyTargets();

        RectInt actorRect = map.ExtentsOf(actor);
        int xMin = actorRect.xMin - range;
        int xMax = actorRect.xMax + range;
        int yMin = actorRect.yMin - range;
        int yMax = actorRect.yMax + range;

        int targetsStruck = 0;
        List<Vector2Int> affectedPositions = new();
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
                        affectedPositions.Add(position);
                        targetsStruck++;
                        if (targetsStruck >= maxTargets)
                        {
                            break;
                        }
                    }
                }
                if (targetsStruck >= maxTargets)
                {
                    break;
                }
            }
            if (targetsStruck >= maxTargets)
            {
                break;
            }
        }

        foreach (Vector2Int position in affectedPositions)
        {
            for (int x = position.x - radius; x <= position.x + radius; x++)
            {
                for (int y = position.y - radius; y <= position.y + radius; y++)
                {
                    Vector2Int blastSquare = new(x, y);
                    List<Placeable> placeables = map.PlaceablesAt(blastSquare);
                    foreach (Placeable placeable in placeables)
                    {
                        if (placeable is Creature target && target.teamNumber != actor.teamNumber)
                        {
                            if (damage != null)
                            {
                                actor.NonweaponStrike(target, damage, skill, map);
                            }
                            InflictConditionsFromAbility(ability, actor, target, map);
                        }
                    }
                }
            }
            OnNonweaponStrike?.Invoke(actor, position, ability, map);
        }

        // if we still haven't struck the maximum number of targets, try attacking buildings
        if (targetsStruck < maxTargets)
        {
            foreach (Placeable placeable in map.UnheldPlaceables())
            {
                if (placeable is Building building && building.teamNumber != actor.teamNumber)
                {
                    if (map.DistanceBetween(actor, building) <= range)
                    {
                        actor.AttackBuilding(ability, building, map);
                        targetsStruck++;
                        if (targetsStruck >= maxTargets)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private static void AllyEffect(TypeAbility ability, Creature actor, Map map)
    {
        int range = ability.GetRange();
        int radius = ability.GetEffectRadius();
        int maxTargets = ability.GetAllyTargets();

        RectInt actorRect = map.ExtentsOf(actor);
        int xMin = actorRect.xMin - range;
        int xMax = actorRect.xMax + range;
        int yMin = actorRect.yMin - range;
        int yMax = actorRect.yMax + range;

        List<Vector2Int> possibleTargets = new();
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                Vector2Int position = new(x, y);
                List<Placeable> placeables = map.PlaceablesAt(position);
                foreach (Placeable placeable in placeables)
                {
                    if (placeable is Creature target && target.teamNumber == actor.teamNumber)
                    {
                        possibleTargets.Add(position);
                    }
                }
            }
        }

        if (possibleTargets.Count == 0)
        {
            return;
        }
        int targetsStruck = 0;
        // start at a random index to avoid always hitting the same target
        int startindex = UnityEngine.Random.Range(0, possibleTargets.Count);
        int i = startindex;
        do
        {
            Vector2Int position = possibleTargets[i];
            for (int x = position.x - radius; x <= position.x + radius; x++)
            {
                for (int y = position.y - radius; y <= position.y + radius; y++)
                {
                    Vector2Int blastSquare = new(x, y);
                    List<Placeable> placeables = map.PlaceablesAt(blastSquare);
                    foreach (Placeable placeable in placeables)
                    {
                        if (placeable is Creature target && target.teamNumber == actor.teamNumber)
                        {
                            InflictConditionsFromAbility(ability, actor, target, map);
                        }
                    }
                }
            }
            targetsStruck++;
            if (targetsStruck >= maxTargets)
            {
                break;
            }
            i = (i + 1) % possibleTargets.Count;
        }
        while (i != startindex);

    }

    private static void InflictConditionsFromAbility(TypeAbility ability, Creature actor, Creature target, Map map)
    {
        List<ConditionType> conditions = ability.GetConditionsInflicted();
        List<int> baseStacks = ability.GetConditionsInflictedBaseStacks();
        List<int> maxStacks = ability.GetConditionsInflictedMaxStacks();
        for (int i = 0; i < conditions.Count; i++)
        {
            ConditionType condition = conditions[i];
            int baseStack = baseStacks[i];
            int maxStack = maxStacks[i];
            actor.InflictConditionOn(target, ability.GetSkill(), condition, baseStack, maxStack, map);
            OnConditionApplied?.Invoke(actor, target, condition, map);
        }
    }

    private static void InflictConditionsFromWeapon(string weaponSkill, Creature actor, Creature target, Map map)
    {
        // TODO
    }
}

public class Gamestate 
{
    public Gamestate(Map map, Scenario scenario)
    {
        this.map = map;
        this.scenario = scenario;
    }

    public Map map;
    public Scenario scenario;
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

    private HashSet<(Building, Building)> transportRoutes = new();

    private int currentTick = 0;

    public Map()
    {

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
        if (placeable is Creature outfitCreature && removeHeldItems)
        {
            if (creatureOutfits.ContainsKey(outfitCreature))
            {
                creatureOutfits.Remove(outfitCreature);
            }
        }

        // If this is a building, remove all transport routes to/from it
        if (placeable is Building building)
        {
            List<(Building, Building)> routesToRemove = new();
            foreach ((Building, Building) route in transportRoutes)
            {
                if (route.Item1 == building || route.Item2 == building)
                {
                    routesToRemove.Add(route);
                }
            }
            foreach ((Building, Building) route in routesToRemove)
            {
                transportRoutes.Remove(route);
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
            throw new System.Exception("Placeable does not exist in map: " + placeable.ToString());
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

    public bool IsPathable(Vector2Int position)
    {
        List<Placeable> currentOccupants = PlaceablesAt(position);
        foreach (Placeable occupant in currentOccupants)
        {
            if ((occupant is Building building && !building.buildingType.GetIsPathable()) ||
                (occupant is Prop prop && !prop.propType.GetIsPathable()) ||
                (occupant is Creature))
            {
                return false;
            }
        }
        return true;
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
        for (int x = -1; x < size + 1; x++)
        {
            for (int y = -1; y < size + 1; y++)
            {
                Vector2Int position = new(buildingPosition.x + x, buildingPosition.y + y);
                List<Placeable> placeablesAtPosition = PlaceablesAt(position);
                foreach (Placeable placeable in placeablesAtPosition)
                {
                    if (placeable is Building || placeable is Prop ||
                        (x != -1 && x != size && y != -1 && y != size && (placeable is Item || placeable is Creature)))
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
        if (!HolderOf(item).Equals(creature))
        {
            throw new System.Exception("Cannot add item to outfit that is not held by creature.");
        }
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

    public HashSet<Item> OutfitOf(Creature creature)
    {
        if (!creatureOutfits.ContainsKey(creature))
        {
            return new();
        }
        return new(creatureOutfits[creature]);
    }

    public void AddTransportRoute(Building sourceBuilding, Building destinationBuilding)
    {
        transportRoutes.Add((sourceBuilding, destinationBuilding));
    }

    public bool TransportRouteExists(Building sourceBuilding, Building destinationBuilding)
    {
        return transportRoutes.Contains((sourceBuilding, destinationBuilding));
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