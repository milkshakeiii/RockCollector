using System;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class CreatureBehavior
{
    public CreatureBehaviorType creatureBehaviorType;

    public static CreatureBehavior FromName(string name)
    {
        return new CreatureBehavior(EntityManager.creatureBehaviors[name]);
    }

    public CreatureBehavior(CreatureBehaviorType creatureBehaviorType)
    {
        this.creatureBehaviorType = creatureBehaviorType;
    }

    public Activity NextActivity(Map map, Creature actor)
    {
        foreach (string behaviorPriorityName in creatureBehaviorType.GetPriorities())
        {
            BehaviorPriority behaviorPriority = BehaviorPriority.FromName(behaviorPriorityName);
            Activity activity = behaviorPriority.ChosenActivityOrNull(map, actor, creatureBehaviorType);
            if (activity != null)
            {
                return activity;
            }
        }
        return null;
    }

    public void CheckInterrupts(MapView map, CreatureSelf creature)
    {

    }
}

public abstract class BehaviorPriority 
{
    public static BehaviorPriority FromName(string name)
    {
        switch (name)
        {
            case "Repair Damaged Buildings":
                return new RepairDamagedBuildingsPriority();
            case "Drop Off Requested Items":
                return new DropOffRequestedItemsPriority();
            case "Pick Up Requested Items":
                return new PickUpRequestedItemsPriority();
            case "Harvest Requested Items":
                return new HarvestRequestedItemsPriority();
            case "Craft Items":
                return new CraftItemsPriority();
            case "Rest":
                return new RestPriority();
            case "Idle":
                return new IdlePriority();
            case "Wander":
                return new WanderPriority();
            case "Hunt":
                return new HuntPriority();
            case "Equip":
                return new EquipPriority();
            default:
                throw new Exception("Unknown behavior priority: " + name);
        }
    }

    /// <summary>
    /// Check this priority level to see if the actor should perform a chosen activity.
    /// If the actor should perform an activity for this priority, return the activity.
    /// If the actor should not perform the activity, return null.
    /// </summary>
    /// <param name="map"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public abstract Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType);
}

public class RepairDamagedBuildingsPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        if (!actor.HasRepairAbility())
        {
            return null;
        }

        // make sure you are holding a repair implement
        if (actor.RepairCooldownAndAmount(map).Item1 == 0)
        {
            // we are not holding a repair implement
            // so find a repair implement
            foreach (Placeable placeable in map.AllPlaceables())
            {
                if (placeable is Item item && item.itemType.GetRepairAmount() > 0 && (!map.IsHeld(item) || map.HolderOf(item) is Building))
                {
                    return new PickUpActivity(item, true);
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
            if (placeable is Building building && building.teamNumber == actor.teamNumber)
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
}

public class DropOffRequestedItemsPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
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
                if (homeBuilding.GetMissingItemAmount(item.itemType, map) > 0 && !actor.OutfitContains(item, map))
                {
                    return new DropOffActivity(homeBuilding);
                }
            }
        }
        return null;
    }
}

public class PickUpRequestedItemsPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
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
                    return new PickUpActivity(item, false);
                }
            }
        }
        return null;
    }
}

public class HarvestRequestedItemsPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
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
                                return new PickUpActivity(item, true);
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
}

public class CraftItemsPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
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

            // craft the first item that is missing that can be crafted
            List<ItemType> inputItemTypes = itemType.GetCraftingInputs();
            bool canCraft = true;
            foreach (ItemType inputItemType in inputItemTypes)
            {
                if (homeBuilding.GetItemCount(inputItemType, map) <= 0)
                {
                    canCraft = false;
                    break;
                }
            }

            // make sure you have a high enough skill level
            string neededSkill = itemType.GetCraftingSkill();
            canCraft = canCraft && actor.CraftingSkillModifier(neededSkill, map) >= itemType.GetCraftingLevel();

            if (canCraft)
            {
                return new CraftActivity(itemType, homeBuilding);
            }
        }
        return null;
    }
}

public class RestPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        Building homeBuilding = actor.GetHomeBuilding(map);
        if (homeBuilding == null)
        {
            return null;
        }
        if (actor.GetDamageTaken() > 0 || map.DistanceBetween(actor, homeBuilding) > 1)
        {
            return new RestActivity(homeBuilding);
        }
        return null;
    }
}

public class IdlePriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        return new IdleActivity(map.PositionOf(actor));
    }
}

public class WanderPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        return new WanderActivity(actor.GetHomeBuilding(map), behaviorType.GetWanderRange());
    }
}

public class HuntPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        Building homeBuilding = actor.GetHomeBuilding(map);
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Creature creature && creature.teamNumber != actor.teamNumber
                && map.DistanceBetween(homeBuilding, creature) <= behaviorType.GetHuntRange()
                && map.DistanceBetween(actor, creature) <= behaviorType.GetHuntLookDistance()
                && creature.EncounterLevel() - actor.EncounterLevel() <= behaviorType.GetHuntMaximumLevelDifference()
                && creature.EncounterLevel() - actor.EncounterLevel() >= behaviorType.GetHuntMinimumLevelDifference())
            {
                return new HuntActivity(creature);
            }
        }
        return null;
    }
}

public class EquipPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        Building homeBuilding = actor.GetHomeBuilding(map);
        if (homeBuilding == null)
        {
            return null;
        }
        int range = behaviorType.GetEquipRange();
        foreach (string equipmentCategory in behaviorType.GetEquipCategories())
        {
            int currentEquipmentLevel = actor.EquipmentLevel(equipmentCategory, map);
            foreach (Placeable placeable in map.AllPlaceables())
            {
                if (placeable is Item item && item.itemType.GetEquipmentCategory() == equipmentCategory
                    && map.DistanceBetween(homeBuilding, item) <= range
                    && !item.IsClaimed()
                    && (!map.IsHeld(item) || map.HolderOf(item) is Building))
                {
                    int itemEquipmentLevel = item.itemType.GetEquipmentLevel();
                    if (itemEquipmentLevel > currentEquipmentLevel)
                    {
                        return new PickUpActivity(item, true);
                    }
                }
            }
        }
        return null;
    }
}