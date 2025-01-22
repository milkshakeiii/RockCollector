using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UnityEngine;

public abstract class Activity 
{
    public int encounterLevel;

    public List<ItemType> craftingInputItems;
    public ItemType craftingOutputItem;
    public List<ItemType> droppedItems;
    public List<int> droppedItemsProbabilities;

    // One of sourcePlaceable and position must be non-null
    public static Vector2Int NULL_POSITION = new (int.MinValue, int.MinValue);
    protected Placeable sourcePlaceable;
    protected Vector2Int position;

    private Dictionary<Placeable, Placeable> backDictionary = null;

    protected Activity(int encounterLevel,
                       List<ItemType> craftingInputItems,
                       ItemType craftingOutputItem,
                       List<ItemType> droppedItems,
                       List<int> droppedItemsProbabilities,
                       Placeable sourcePlaceable,
                       Vector2Int position) // use Activity.NULL_POSITION for null position
    {
        this.encounterLevel = encounterLevel;
        this.craftingInputItems = craftingInputItems;
        this.craftingOutputItem = craftingOutputItem;
        this.droppedItems = droppedItems;
        this.droppedItemsProbabilities = droppedItemsProbabilities;
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

    public virtual int ProximityRequirement(Creature forCreature, Map map)
    {
        return 1;
    }

    public abstract bool IsCompletedOrImpossible(Map map, Creature performer);

    public abstract void Perform(Creature performer, Map map);

    public int DistanceTo(Creature creature, Map map)
    {
        if (sourcePlaceable != null)
        {
            return map.DistanceBetween(creature, sourcePlaceable);
        }
        if (position != NULL_POSITION)
        {
            return map.DistanceTo(position, creature);
        }
        throw new Exception("Unable to determine location of activity");
    }

    /// <summary>
    /// Mutate map (probabilistic version). Return the estimate for the number of ticks.
    /// </summary>
    /// <param name="creature"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public abstract int EffectAndEstimate(Creature creature, Map map);

    public void MarkForBackConversion(Dictionary<Placeable, Placeable> newBackDictionary)
    {
        if (this.backDictionary != null)
        {
            throw new Exception("Already marked for back conversion");
        }
        this.backDictionary = newBackDictionary;
    }

    public bool SuccessfulBackConversion(Map map)
    {
        if (backDictionary == null)
        {
            throw new Exception("Not marked for back conversion");
        }
        if (sourcePlaceable == null)
        {
            return true;
        }
        Placeable originalPlaceable = backDictionary[sourcePlaceable];
        sourcePlaceable = originalPlaceable;
        return map.PlaceableExists(originalPlaceable);
    }

    public bool IsSourcePlaceableClaimed()
    {
        return sourcePlaceable?.IsClaimed() ?? false;
    }

    public void MarkSourcePlaceableClaimed()
    {
        sourcePlaceable?.Claim();
    }

    public void MarkSourcePlaceableUnclaimed()
    {
        sourcePlaceable?.Unclaim();
    }
}

public class HarvestActivity : Activity
{
    public HarvestActivity(Prop prop) : base(0,
        new(), null, prop.propType.GetProducedItems(), prop.propType.GetProducedItemsProbabilities(), prop, Activity.NULL_POSITION)
    {

    }

    public Prop SourceProp()
    {
        return (Prop)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return SourceProp().IsDestroyed() || performer.HarvestingCooldownAndAmount(SourceProp(), map).Item1 == 0;
    }

    public override void Perform(Creature performer, Map map)
    {
        performer.HarvestProp(SourceProp(), map);
    }

    public override int EffectAndEstimate(Creature creature, Map map)
    {
        // estimate the number of ticks required to harvest the prop
        (int harvestCooldown, int harvestAmount) = creature.HarvestingCooldownAndAmount(SourceProp(), map);
        int propHarvestRequired = SourceProp().propType.GetHarvestingRequired();   
        int estimatedTicks = Mathf.CeilToInt((float)harvestAmount / (float)propHarvestRequired) * harvestCooldown;

        // mutate the map
        SourceProp().MakeImaginaryDrops(map);
        map.Remove(SourceProp());

        return estimatedTicks;
    }
}

public class HuntActivity : Activity
{
    public HuntActivity(Creature target) : base(target.EncounterLevel(),
        new(), null, target.GetCreatureType().GetDroppedItems(), target.GetCreatureType().GetDroppedItemsProbabilities(), target, Activity.NULL_POSITION)
    {

    }

    public override int ProximityRequirement(Creature forCreature, Map map)
    {
        int myMaxRangeUp = -1;
        int myMaxRangeEver = 1;
        foreach (TypeAbility ability in forCreature.ListAbilities())
        {
            if (ability.GetEnemyTargets() > 0 && forCreature.AbilityIsUp(ability, map))
            {
                myMaxRangeUp = Math.Max(myMaxRangeUp, ability.GetRange());
            }
            if (ability.GetEnemyTargets() > 0)
            {
                myMaxRangeEver = Math.Max(myMaxRangeEver, ability.GetRange());
            }
        }
        if (myMaxRangeUp == -1)
        {
            return myMaxRangeEver;
        }
        return myMaxRangeUp;
    }

    private Creature SourceCreature()
    {
        return (Creature)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return SourceCreature().IsDestroyed();
    }

    public override void Perform(Creature performer, Map map)
    {
        performer.UseAnyAbility(map);
    }

    public override int EffectAndEstimate(Creature creature, Map map)
    {
        return 0; // TODO: Implement
    }
}

public class CraftActivity : Activity
{
    public CraftActivity(ItemType producedItemType, Building workshop) : base(0,
        producedItemType.GetCraftingInputs(), producedItemType, new(), new(), workshop, Activity.NULL_POSITION)
    {

    }

    private Building Workshop()
    {
        return (Building)sourcePlaceable;
    }

    public override int ProximityRequirement(Creature forCreature, Map map)
    {
        return 0;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        if (Workshop().IsDestroyed())
        {
            return true;
        }

        foreach (ItemType inputItem in craftingOutputItem.GetCraftingInputs())
        {
            bool itemFound = false;

            // check if the workshop is holding the input item
            List<Placeable> heldItemsWorkshop = new(map.HeldPlaceablesOf(Workshop()));
            foreach (Placeable heldItem in heldItemsWorkshop)
            {
                if (heldItem is Item item && item.itemType.GetName() == inputItem.GetName() && !item.IsConsumed())
                {
                    itemFound = true;
                    break;
                }
            }

            // check if the performer is holding the input item
            if (!itemFound)
            {
                List<Placeable> heldItems = new(map.HeldPlaceablesOf(performer));
                foreach (Placeable heldItem in heldItems)
                {
                    if (heldItem is Item item && item.itemType.GetName() == inputItem.GetName() && !item.IsConsumed())
                    {
                        itemFound = true;
                        break;
                    }
                }
            }

            if (!itemFound)
            {
                // if an input item is missing, the crafting activity is completed (cannot continue)
                return true;
            }
        }

        return false;
    }

    private void PerformWithOptions(Creature performer, Map map, bool successRoll, bool immediateConsume)
    {
        // how many of each input item are demanded
        Dictionary<ItemType, int> countsRequired = new();
        foreach (ItemType inputItem in craftingInputItems)
        {
            if (countsRequired.ContainsKey(inputItem))
            {
                countsRequired[inputItem] += 1;
            }
            else
            {
                countsRequired[inputItem] = 1;
            }
        }

        // consume held items first before consuming workshop items
        List<Placeable> availableItems = new(map.HeldPlaceablesOf(performer));
        availableItems.AddRange(map.HeldPlaceablesOf(Workshop()));

        Dictionary<ItemType, List<float>> expectedValuesAvailable = new();
        HashSet<Item> consumedItems = new();
        foreach (ItemType inputType in countsRequired.Keys)
        {
            // consume input items until we run out of items or we reach the expected values demanded
            float expectedAmountCollected = 0;
            foreach (Placeable availableItem in availableItems)
            {
                if (availableItem is Item item && !consumedItems.Contains(item) && item.itemType.GetName() == inputType.GetName())
                {
                    consumedItems.Add(item);

                    float itemProbability = item.GetProbability();
                    if (expectedValuesAvailable.ContainsKey(inputType))
                    {
                        expectedValuesAvailable[inputType].Add(itemProbability);
                    }
                    else
                    {
                        expectedValuesAvailable[inputType] = new List<float> { itemProbability };
                    }

                    expectedAmountCollected += itemProbability;
                    if (expectedAmountCollected >= countsRequired[inputType])
                    {
                        break;
                    }
                }
            }
        }

        if (!immediateConsume)
        {
            // consume the items
            foreach (Item consumedItem in consumedItems)
            {
                consumedItem.Consume();
            }
        }
        else
        {
            // directly remove the items from the map
            foreach (Item consumedItem in consumedItems)
            {
                map.Remove(consumedItem);
            }
        }

        // create the output item
        float finalSuccessProbability = 1;
        foreach (ItemType inputType in countsRequired.Keys)
        {
            if (!expectedValuesAvailable.ContainsKey(inputType))
            {
                throw new Exception("Expected values for input type " + inputType.GetName() + " not found");
            }
            if (expectedValuesAvailable[inputType].Count < countsRequired[inputType])
            {
                throw new Exception("Not enough items of type " + inputType.GetName() + ". Expected " + countsRequired[inputType] + " but only found " + expectedValuesAvailable[inputType].Count);
            }
            float expectedValue = 0;
            for (int i = 0; i < expectedValuesAvailable[inputType].Count; i++)
            {
                float availableProbability = expectedValuesAvailable[inputType][i];
                expectedValue += availableProbability;
            }
            // if the expected value is greater than the count required, we consider it guaranteed success
            // and we only will consume as many items as are needed to reach that expected value.
            float outputProbability;
            if (expectedValue > countsRequired[inputType])
            {
                outputProbability = 1;
            }
            // otherwise, we sum the probabilities and divide by the counts required to determine the chance of success.
            // Note: this is not how math actually works.
            else
            {
                outputProbability = expectedValue / countsRequired[inputType];
            }
            finalSuccessProbability *= outputProbability;
        }
        Item outputItem = new(craftingOutputItem, finalSuccessProbability);
        map.AddHeld(Workshop(), outputItem);
        //Debug.Log(Workshop() + " holds " + outputItem.itemType.GetName() + " with probability " + outputItem.GetProbability());
        performer.AddCooldown(outputItem.itemType.GetCraftingTime());
    }

    public override void Perform(Creature performer, Map map)
    {
        PerformWithOptions(performer, map, true, false);
    }

    public override int EffectAndEstimate(Creature creature, Map map)
    {
        PerformWithOptions(creature, map, false, true);

        return craftingOutputItem.GetCraftingTime();
    }
}

public class PickUpActivity : Activity
{
    public PickUpActivity(Item item) : base(0,
        new(), null, new(), new(), item, Activity.NULL_POSITION)
    {

    }

    public Item Item()
    {
        return (Item)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return Item().IsConsumed() || map.IsHeld(Item());
    }

    public override void Perform(Creature performer, Map map)
    {
        map.PickUp(performer, Item());
    }

    public override int EffectAndEstimate(Creature creature, Map map)
    {
        Perform(creature, map);

        return 1; // Picking up an item takes 1 tick
    }
}

public class DropOffActivity : Activity
{
    public DropOffActivity(Building building) : base(0,
        new(), null, new(), new(), building, Activity.NULL_POSITION)
    {

    }

    public Building Building()
    {
        return (Building)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        bool noItemsRequested = true;
        List<Placeable> heldItems = new(map.HeldPlaceablesOf(performer));
        foreach (Placeable heldItem in heldItems)
        {
            if (heldItem is Item item)
            {
                // only transfer items that are requested by the building
                if (Building().GetMissingItemAmount(item.itemType, map) > 0)
                {
                    noItemsRequested = false;
                    break;
                }
            }
        }
        return Building().IsDestroyed() || map.HeldPlaceablesOf(performer).Count == 0 || noItemsRequested;
    }

    public override void Perform(Creature performer, Map map)
    {
        List<Placeable> heldItems = new(map.HeldPlaceablesOf(performer));
        foreach (Placeable heldItem in heldItems)
        {
            if (heldItem is Item item)
            {
                // only transfer items that are requested by the building
                if (Building().GetMissingItemAmount(item.itemType, map) > 0)
                {
                    map.Transfer(heldItem, Building());
                }
            }
        }
    }

    public override int EffectAndEstimate(Creature creature, Map map)
    {
        Perform(creature, map);

        return 1; // Dropping off an item takes 1 tick
    }
}

public class RepairActivity : Activity
{
    public RepairActivity(Building building) : base(0,
        new(), null, new(), new(), building, Activity.NULL_POSITION)
    {

    }

    public Building Building()
    {
        return (Building)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return Building().GetDamageTaken() == 0 || Building().IsDestroyed() || performer.RepairCooldownAndAmount(map) == (0, 0);
    }

    public override void Perform(Creature performer, Map map)
    {
        performer.RepairBuilding(Building(), map);
    }

    public override int EffectAndEstimate(Creature creature, Map map)
    {
        Perform(creature, map);

        return 1;
    }
}

public class RestActivity : Activity
{
    /// <summary>
    /// Building is the building to rest in. Creatures can always rest
    /// at home but may also be able to rest in other buildings.
    /// </summary>
    /// <param name="building"></param>
    public RestActivity(Building building) : base(0,
        new(), null, new(), new(), building, Activity.NULL_POSITION)
    {

    }

    public Building Building()
    {
        return (Building)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return Building().IsDestroyed() || performer.GetDamageTaken() == 0;
    }

    public override void Perform(Creature performer, Map map)
    {
        int healAmount = Building().buildingType.GetRestHealAmount();
        performer.TakeDamage(-healAmount);
        int cooldown = Building().buildingType.GetRestHealCooldown();
        performer.AddCooldown(cooldown);
    }

    public override int EffectAndEstimate(Creature creature, Map map)
    {
        Perform(creature, map);

        return 1;
    }
}

public class IdleActivity : Activity
{
    private int firstPerformedTick = -1;

    public IdleActivity(Vector2Int position) : base(0,
        new(), null, new(), new(), null, position)
    {

    }

    public override int ProximityRequirement(Creature forCreature, Map map)
    {
        return 0;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return firstPerformedTick != -1 && map.CurrentTick() - firstPerformedTick >= 150;
    }

    public override void Perform(Creature performer, Map map)
    {
        if (firstPerformedTick == -1)
        {
            firstPerformedTick = map.CurrentTick();
        }
    }

    public override int EffectAndEstimate(Creature creature, Map map)
    {
        return 150;
    }
}
