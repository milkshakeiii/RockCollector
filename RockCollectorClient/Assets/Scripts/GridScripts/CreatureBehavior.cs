using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class CreatureBehavior
{
    public static CreatureBehavior FromName(string name)
    {
        if (name == "PeasantBehavior")
        {
            return new PeasantBehavior();
        }
        else if (name == "SkeletonBehavior")
        {
            return new SkeletonBehavior();
        }
        else
        {
            throw new Exception("Unknown behavior name: " + name);
        }
    }

    public abstract string Name();

    public abstract Activity NextActivity(Map map, Creature actor);

    public abstract void CheckInterrupts(MapView map, CreatureSelf creature);
}

public class PeasantBehavior : CreatureBehavior
{
    public override string Name()
    {
        return "PeasantBehavior";
    }

    public override Activity NextActivity(Map map, Creature actor)
    {
        // priority 1: repair damaged buildings within a maximum range
        // -> make sure you are holding a repair implement
        // -> repair closer buildings first
        // priority 2: drop off requested items at home building (from inventory)
        // priority 3: pick up items that are requested by home building (from the ground)
        // priority 4: harvest requested items
        // -> make sure you are holding the correct tool
        // -> harvest props (within range) that have a chance of dropping the requested item
        // priority 5: craft items at home building
        // -> craft items using materials in the home building
        // priority 6: rest at home building
        // priority 7: idle
        Activity nextActivity = RepairDamagedBuildings(map, actor);
        if (nextActivity != null)
        {
            return nextActivity;
        }
        nextActivity = DropOffRequestedItems(map, actor);
        if (nextActivity != null)
        {
            return nextActivity;
        }
        nextActivity = PickUpRequestedItems(map, actor);
        if (nextActivity != null)
        {
            return nextActivity;
        }
        nextActivity = HarvestRequestedItems(map, actor);
        if (nextActivity != null)
        {
            return nextActivity;
        }
        //nextActivity = CraftItems(map, actor);
        if (nextActivity != null)
        {
            return nextActivity;
        }
        //nextActivity = RestAtHomeBuilding(map, actor);
        if (nextActivity != null)
        {
            return nextActivity;
        }
        return new IdleActivity(map.PositionOf(actor));
    }

    private Activity RepairDamagedBuildings(Map map, Creature actor)
    {
        // make sure you are holding a repair implement
        if (actor.RepairCooldownAndAmount(map).Item1 == 0)
        {
            // we are not holding a repair implement
            // so find a repair implement
            foreach (Placeable placeable in map.AllPlaceables())
            {
                if (placeable is Item item && item.itemType.GetRepairAmount() > 0)
                {
                    return new PickUpActivity(item);
                }
            }
            // we were unable to find a repair implement
            return null;
        }

        // repair nearest damaged building
        Building nearestDamagedBuilding = null;
        int nearestDistance = int.MaxValue;
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Building building)
            {
                if (building.GetDamageTaken() > 0)
                {
                    int distance = map.DistanceBetween(actor, building);
                    if (distance < nearestDistance)
                    {
                        nearestDamagedBuilding = building;
                        nearestDistance = distance;
                    }
                }
            }
        }
        if (nearestDamagedBuilding != null)
        {
            return new RepairActivity(nearestDamagedBuilding);
        }
        // no damaged buildings found
        return null;
    }

    private Activity DropOffRequestedItems(Map map, Creature actor)
    {
        Building homeBuilding = actor.GetHomeBuilding(map);
        if (homeBuilding == null)
        {
            return null;
        }
        foreach (Placeable placeable in map.HeldPlaceablesOf(actor))
        {
            if (placeable is Item item)
            {
                if (homeBuilding.GetMissingItemAmount(item.itemType, map) > 0)
                {
                    return new DropOffActivity(homeBuilding);
                }
            }
        }
        return null;
    }

    private Activity PickUpRequestedItems(Map map, Creature actor)
    {
        Building homeBuilding = actor.GetHomeBuilding(map);
        if (homeBuilding == null)
        {
            return null;
        }
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Item item)
            {
                if (homeBuilding.GetMissingItemAmount(item.itemType, map) > 0)
                {
                    return new PickUpActivity(item);
                }
            }
        }
        return null;
    }

    private Activity HarvestRequestedItems(Map map, Creature actor)
    {
        Building homeBuilding = actor.GetHomeBuilding(map);
        if (homeBuilding == null)
        {
            return null;
        }
        foreach (ItemType itemType in homeBuilding.GetRequestableItemTypes())
        {
            if (homeBuilding.GetMissingItemAmount(itemType, map) <= 0)
            {
                continue;
            }
            // for now, just harvest the first prop that has a chance of dropping the requested item
            // in the future, we should at least prioritize props that are closer to the home building
            foreach (Placeable placeable in map.UnheldPlaceables())
            {
                if (placeable is Prop prop && prop.ChanceOfItemDrop(itemType) > 0)
                {
                    string neededSkill = prop.propType.GetHarvestingSkill();
                    if (actor.BestHarvestingAbility(neededSkill) == null)
                    {
                        Debug.Log("No harvesting ability for skill: " + neededSkill);
                        continue;
                    }
                    // make sure you are holding the correct tool
                    if (actor.HarvestingCooldownAndAmount(prop, map).Item1 == 0)
                    {
                        // we are not holding the correct tool
                        // so find the correct tool
                        foreach (Placeable placeable2 in map.AllPlaceables())
                        {
                            if (map.HolderOf(placeable2) is Creature)
                            {
                                continue;
                            }
                            if (placeable2 is Item item && item.itemType.GetHarvestingSkills().Contains(neededSkill))
                            {
                                return new PickUpActivity(item);
                            }
                        }
                        // we were unable to find the correct tool
                        continue;
                    }
                    // harvest the prop
                    return new HarvestActivity(prop);
                }
            }
        }
        return null;
    }

    public override void CheckInterrupts(MapView map, CreatureSelf creature)
    {
        
    }
}

public class SkeletonBehavior : CreatureBehavior
{
    public override string Name()
    {
        return "SkeletonBehavior";
    }

    public override Activity NextActivity(Map map, Creature actor)
    {
        // if there is a creature within 4 squares, hunt it
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                List<Placeable> targets = map.PlaceablesAt(map.PositionOf(actor) + new Vector2Int(x, y));
                foreach (Placeable target in targets)
                {
                    if (target is Creature creature && creature.teamNumber != actor.teamNumber)
                    {
                        return new HuntActivity(creature);
                    }
                }
            }
        }
        return null;
    }

    public override void CheckInterrupts(MapView map, CreatureSelf self)
    {
        if (self.HasCurrentActivity())
        {
            return;
        }

        // with a 1 in 100 chance, move to a random adjacent square
        if (UnityEngine.Random.Range(0, 100) == 0)
        {
            Vector2Int target = new (UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
            self.MoveInDirection(target);
        }
    }
}