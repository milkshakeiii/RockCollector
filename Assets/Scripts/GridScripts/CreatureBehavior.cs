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

    public (Activity, string) NextActivity(Map map, Creature actor)
    {
        foreach (string behaviorPriorityName in creatureBehaviorType.GetPriorities())
        {
            BehaviorPriority behaviorPriority = BehaviorPriority.FromName(behaviorPriorityName);
            Activity activity = behaviorPriority.ChosenActivityOrNull(map, actor, creatureBehaviorType);
            if (activity != null)
            {
                string ingVerb = behaviorPriority.GetIngVerb();
                return (activity, ingVerb);
            }
        }
        return (null, "");
    }

    public bool CheckInterrupts(MapView map, CreatureSelf creature)
    {
        int dangerRating = GetDangerRating(map, creature);
        int selfEncounterLevel = creature.EncounterLevel();
        if (dangerRating > selfEncounterLevel * creatureBehaviorType.GetInterruptDangerThreshold())
        {
            return true;
        }
        return false;
    }

    private int GetDangerRating(MapView map, CreatureSelf self)
    {
        int dangerRating = 0;
        foreach (PlaceableView placeable in map.UnheldPlaceables())
        {

            if (placeable is CreatureView creature2
                && map.DistanceBetween(self.View(), creature2) <= creatureBehaviorType.GetDangerLookDistance())
            {
                if (creature2.GetTeamNumber() != self.GetTeamNumber())
                {
                    dangerRating += creature2.EncounterLevel();
                }
                else
                {
                    dangerRating -= creature2.EncounterLevel();
                }
            }
        }
        return dangerRating;
    }
}

public abstract class BehaviorPriority 
{
    public static BehaviorPriority FromName(string name)
    {
        return name switch
        {
            "Repair Damaged Buildings" => new RepairDamagedBuildingsPriority(),
            "Deliver Requested Items" => new DeliverRequestedItemsPriority(),
            "Harvest Requested Items" => new HarvestRequestedItemsPriority(),
            "Craft Items" => new CraftItemsPriority(),
            "Rest" => new RestPriority(),
            "Idle" => new IdlePriority(),
            "Wander" => new WanderPriority(),
            "Hunt" => new HuntPriority(),
            "Equip" => new EquipPriority(),
            "Flee" => new FleePriority(),
            "Fight" => new FightPriority(),
            "Plant Props" => new PlantPriority(),
            "Raid" => new RaidPriority(),
            _ => throw new Exception("Unknown behavior priority: " + name),
        };
    }

    public abstract string GetIngVerb();

    /// <summary>
    /// Check this priority level to see if the actor should perform a chosen activity.
    /// If the actor should perform an activity for this priority, return the activity.
    /// If the actor should not perform the activity, return null.
    /// </summary>
    /// <param name="map"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    public abstract Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType);

    protected int GetDangerRating(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        int dangerRating = 0;
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Creature creature2
                               && map.DistanceBetween(actor, creature2) <= behaviorType.GetDangerLookDistance())
            {
                if (creature2.teamNumber != actor.teamNumber)
                {
                    dangerRating += creature2.DifficultyEstimate();
                }
                else
                {
                    dangerRating -= creature2.DifficultyEstimate();
                }
            }
        }
        return dangerRating;
    }
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

    public override string GetIngVerb()
    {
        return "Repairing";
    }
}

public class DeliverRequestedItemsPriority : BehaviorPriority
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
            if (placeable is Building requestorBuilding && map.DistanceBetween(placeable, homeBuilding) <= behaviorType.GetPickUpRange())
            {
                foreach (Placeable unheldPlaceable in map.UnheldPlaceables())
                {
                    if (unheldPlaceable is Item item)
                    {
                        if (requestorBuilding.GetMissingItemAmount(item.itemType, map) > 0)
                        {
                            return new DeliverActivity(item, map.PositionOf(requestorBuilding));
                        }
                    }
                }
                foreach (Placeable heldPlaceable in map.HeldPlaceables())
                {
                    // we may still want to pick up an item if it is in a building that
                    // has a transport route to the requesting building or if the item is held
                    // by the actor and not part of the actor's outfit
                    if (heldPlaceable is Item item && map.HolderOf(item) is Building building)
                    {
                        if (building != requestorBuilding && map.TransportRouteExists(building, requestorBuilding) && requestorBuilding.GetMissingItemAmount(item.itemType, map) > 0)
                        {
                            return new DeliverActivity(item, map.PositionOf(requestorBuilding));
                        }
                    }
                    if (heldPlaceable is Item item2 && map.HolderOf(item2) is Creature creature)
                    {
                        if (creature == actor && !actor.OutfitContains(item2, map) && requestorBuilding.GetMissingItemAmount(item2.itemType, map) > 0)
                        {
                            return new DeliverActivity(item2, map.PositionOf(requestorBuilding));
                        }
                    }
                }
            }
        }
        return null;
    }

    public override string GetIngVerb()
    {
        return "Delivering";
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
                        continue;
                    }
                    // make sure you are holding the correct tool
                    if (prop.propType.GetRequiresImplement() && actor.HarvestingCooldownAndAmount(prop, map).Item1 == 0)
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

    public override string GetIngVerb()
    {
        return "Harvesting";
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
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Building craftBuilding && craftBuilding.teamNumber == actor.teamNumber && map.DistanceBetween(homeBuilding, craftBuilding) <= behaviorType.GetCraftRange())
            {
                foreach (ItemType itemType in craftBuilding.GetRequestableItemTypes())
                {
                    if (craftBuilding.GetMissingItemAmount(itemType, map) <= 0)
                    {
                        continue;
                    }

                    // craft the first item that is missing that can be crafted
                    List<ItemType> inputItemTypes = itemType.GetCraftingInputs();
                    Dictionary<ItemType, int> neededCount = new Dictionary<ItemType, int>();
                    foreach (ItemType inputItemType in inputItemTypes)
                    {
                        if (!neededCount.ContainsKey(inputItemType))
                        {
                            neededCount[inputItemType] = 0;
                        }
                        neededCount[inputItemType]++;
                    }
                    bool canCraft = true;
                    foreach (ItemType inputItemType in inputItemTypes)
                    {
                        if (craftBuilding.GetItemCount(inputItemType, map) < neededCount[inputItemType])
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
                        return new CraftActivity(itemType, craftBuilding);
                    }
                }
            }
        }
        return null;
    }

    public override string GetIngVerb()
    {
        return "Crafting";
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
        if (actor.GetDamageTaken() > 0 || map.DistanceBetween(actor, homeBuilding) > behaviorType.GetWanderRange())
        {
            return new RestActivity(homeBuilding);
        }
        return null;
    }

    public override string GetIngVerb()
    {
        return "Resting at";
    }
}

public class IdlePriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        return new IdleActivity(map.PositionOf(actor));
    }

    public override string GetIngVerb()
    {
        return "Idling";
    }
}

public class WanderPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        return new WanderActivity(actor.GetHomeBuilding(map), behaviorType.GetWanderRange());
    }

    public override string GetIngVerb()
    {
        return "Wandering";
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
                && creature.DifficultyEstimate() - actor.DifficultyEstimate() <= behaviorType.GetHuntMaximumLevelDifference()
                && creature.DifficultyEstimate() - actor.DifficultyEstimate() >= behaviorType.GetHuntMinimumLevelDifference())
            {
                return new HuntActivity(creature);
            }
        }
        return null;
    }

    public override string GetIngVerb()
    {
        return "Hunting";
    }
}

public class RaidPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        Building homeBuilding = actor.GetHomeBuilding(map);
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Building building && building.teamNumber != actor.teamNumber
                && map.DistanceBetween(homeBuilding, building) <= behaviorType.GetRaidRange()
                && map.DistanceBetween(actor, building) <= behaviorType.GetRaidLookDistance()
                && building.DifficultyEstimate() - actor.DifficultyEstimate() <= behaviorType.GetRaidMaximumLevelDifference()
                && building.DifficultyEstimate() - actor.DifficultyEstimate() >= behaviorType.GetRaidMinimumLevelDifference())
            {
                return new RaidActivity(building);
            }
        }
        return null;
    }

    public override string GetIngVerb()
    {
        return "Raiding";
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

    public override string GetIngVerb()
    {
        return "Equipping";
    }
}

public class FleePriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        int dangerRating = GetDangerRating(map, actor, behaviorType);
        if (dangerRating < behaviorType.GetFleeDangerThreshold() * actor.DifficultyEstimate())
        {
            return null;
        }
        Building homeBuilding = actor.GetHomeBuilding(map);
        if (homeBuilding == null)
        {
            return null;
        }
        return new RestActivity(homeBuilding);
    }

    public override string GetIngVerb()
    {
        return "Fleeing to";
    }
}

public class FightPriority : BehaviorPriority
{
    public override Activity ChosenActivityOrNull(Map map, Creature actor, CreatureBehaviorType behaviorType)
    {
        int dangerRating = GetDangerRating(map, actor, behaviorType);
        if (dangerRating < behaviorType.GetFightDangerThreshold() * actor.DifficultyEstimate())
        {
            return null;
        }
        Creature mostDangerousCreature = MostDangerousCreatureInLookRange(map, actor, behaviorType);
        if (mostDangerousCreature != null)
        {
            return new HuntActivity(mostDangerousCreature);
        }
        return null;
    }

    private Creature MostDangerousCreatureInLookRange(Map map, Creature self, CreatureBehaviorType creatureBehaviorType)
    {
        Creature mostDangerousCreature = null;
        int highestDangerRating = 0;
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Creature creature2
                               && map.DistanceBetween(self, creature2) <= creatureBehaviorType.GetDangerLookDistance())
            {
                if (creature2.teamNumber != self.teamNumber)
                {
                    int dangerRating = creature2.DifficultyEstimate();
                    if (dangerRating > highestDangerRating)
                    {
                        highestDangerRating = dangerRating;
                        mostDangerousCreature = creature2;
                    }
                }
            }
        }
        return mostDangerousCreature;
    }

    public override string GetIngVerb()
    {
        return "Fighting";
    }
}

public class PlantPriority : BehaviorPriority
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
            if (placeable is Building building && building.buildingType.GetPlantablePropTypes().Count > 0
                && map.DistanceBetween(homeBuilding, building) <= behaviorType.GetPlantRange())
            {
                foreach (PropType propType in building.buildingType.GetPlantablePropTypes())
                {
                    // ensure we have the correct skill level
                    string neededSkill = propType.GetPlantingSkill();
                    int skillLevel = actor.PlantingSkillModifier(neededSkill, map);
                    if (skillLevel < propType.GetPlantingLevel())
                    {
                        continue;
                    }

                    Vector2Int buildingPosition = map.PositionOf(building);
                    int xOffsetMin = building.buildingType.GetPlantingZoneXMin();
                    int xOffsetMax = building.buildingType.GetPlantingZoneXMax();
                    int yOffsetMin = building.buildingType.GetPlantingZoneYMin();
                    int yOffsetMax = building.buildingType.GetPlantingZoneYMax();
                    for (int xOffset = xOffsetMin; xOffset <= xOffsetMax; xOffset++)
                    {
                        for (int yOffset = yOffsetMin; yOffset <= yOffsetMax; yOffset++)
                        {
                            Vector2Int position = new Vector2Int(buildingPosition.x + xOffset, buildingPosition.y + yOffset);
                            // only plant when position x and y are both even
                            if (position.x % 2 != 0 || position.y % 2 != 0)
                            {
                                continue;
                            }
                            // don't plant if there is already a prop or building there
                            List<Placeable> placeables = map.PlaceablesAt(position);
                            bool alreadyOccupied = false;
                            foreach (Placeable placeable2 in placeables)
                            {
                                if (placeable2 is Prop || placeable2 is Building)
                                {
                                    alreadyOccupied = true;
                                }
                            }
                            if (alreadyOccupied)
                            {
                                continue;
                            }
                            return new PlantActivity(position, propType);
                        }
                    }
                }
            }
        }
        return null;
    }

    public override string GetIngVerb()
    {
        return "Planting";
    }
}