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

    public virtual bool ClaimsSourcePlaceable()
    {
        return true;
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
}

public class HuntActivity : Activity
{
    public HuntActivity(Creature target) : base(target.EncounterLevel(),
        new(), null, target.GetCreatureType().GetDroppedItems(), target.GetCreatureType().GetDroppedItemsProbabilities(), target, Activity.NULL_POSITION)
    {

    }

    public override bool ClaimsSourcePlaceable()
    {
        return false;
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

    public override void Perform(Creature performer, Map map)
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

        HashSet<Item> consumedItems = new();
        foreach (ItemType inputType in countsRequired.Keys)
        {
            // consume input items until we run out of items or we reach the expected values demanded
            int amountCollected = 0;
            foreach (Placeable availableItem in availableItems)
            {
                if (availableItem is Item item && !consumedItems.Contains(item) && item.itemType.GetName() == inputType.GetName())
                {
                    consumedItems.Add(item);

                    amountCollected += 1;
                    if (amountCollected >= countsRequired[inputType])
                    {
                        break;
                    }
                }
            }
        }

        // consume the items
        foreach (Item consumedItem in consumedItems)
        {
            consumedItem.Consume();
        }

        // create the output item
        Item outputItem = new(craftingOutputItem);
        map.AddHeld(Workshop(), outputItem);
        performer.AddCooldown(outputItem.itemType.GetCraftingTime());
    }
}

public class PickUpActivity : Activity
{
    bool addToOutfit;

    public PickUpActivity(Item item, bool addToOutfit) : base(0,
        new(), null, new(), new(), item, Activity.NULL_POSITION)
    {
        this.addToOutfit = addToOutfit;
    }

    public Item Item()
    {
        return (Item)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return Item().IsConsumed() || (map.HolderOf(Item()) is Creature);
    }

    public override void Perform(Creature performer, Map map)
    {
        if (map.IsHeld(Item()))
        {
            map.Transfer(Item(), performer);
        }
        else
        {
            map.PickUp(performer, Item());
        }
        if (addToOutfit)
        {
            performer.AddToOutfit(Item(), map);
        }
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
                if (Building().GetMissingItemAmount(item.itemType, map) > 0 && !performer.OutfitContains(item, map))
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
        List<Placeable> heldPlaceables = new(map.HeldPlaceablesOf(performer));
        foreach (Placeable heldItem in heldPlaceables)
        {
            if (heldItem is Item item)
            {
                // only transfer items that are requested by the building
                if (Building().GetMissingItemAmount(item.itemType, map) > 0 && !performer.OutfitContains(item, map))
                {
                    map.Transfer(heldItem, Building());
                }
            }
        }
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

    public override bool ClaimsSourcePlaceable()
    {
        return false;
    }

    public Building Building()
    {
        return (Building)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return Building().IsDestroyed() || (performer.GetDamageTaken() == 0 && map.DistanceBetween(performer, Building()) <= 1);
    }

    public override void Perform(Creature performer, Map map)
    {
        int healAmount = Building().buildingType.GetRestHealAmount();
        performer.TakeDamage(-healAmount);
        int cooldown = Building().buildingType.GetRestHealCooldown();
        performer.AddCooldown(cooldown);
    }
}

public class IdleActivity : Activity
{
    private int firstPerformedTick = -1;

    public IdleActivity(Vector2Int position) : base(0,
        new(), null, new(), new(), null, position)
    {

    }

    public override bool ClaimsSourcePlaceable()
    {
        return false;
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
}

public class WanderActivity : Activity
{
    private int firstPerformedTick = -1;
    private int range;

    public WanderActivity(Building homeBuilding, int range) : base(0,
        new(), null, new(), new(), homeBuilding, Activity.NULL_POSITION)
    {
        this.range = range;
    }

    public override bool ClaimsSourcePlaceable()
    {
        return false;
    }

    public override int ProximityRequirement(Creature forCreature, Map map)
    {
        return int.MaxValue;
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
        // 1% chance of moving to a new location
        if (UnityEngine.Random.Range(0, 100) != 0)
        {
            return;
        }
        Vector2Int randomDirection = new(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
        Vector2Int newPosition = map.PositionOf(performer) + randomDirection;
        // if this would not take us out of range, move to the new position
        if (map.DistanceTo(newPosition, sourcePlaceable) <= range)
        {
            performer.MoveInDirection(randomDirection, map);
        }
    }
}
