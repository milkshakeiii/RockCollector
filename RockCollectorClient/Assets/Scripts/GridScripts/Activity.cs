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
    public List<ItemType> craftingOutputItems;
    public List<ItemType> droppedItems;
    public List<int> droppedItemsProbabilities;

    // One of sourcePlaceable and position must be non-null
    public static Vector2Int NULL_POSITION = new (int.MinValue, int.MinValue);
    protected Placeable sourcePlaceable;
    protected Vector2Int position;

    public Activity(int encounterLevel,
                    List<ItemType> craftingInputItems,
                    List<ItemType> craftingOutputItems,
                    List<ItemType> droppedItems,
                    List<int> droppedItemsProbabilities,
                    Placeable sourcePlaceable,
                    Vector2Int position) // use Activity.NULL_POSITION for null position
    {
        this.encounterLevel = encounterLevel;
        this.craftingInputItems = craftingInputItems;
        this.craftingOutputItems = craftingOutputItems;
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

    public virtual int ProximityRequirement()
    {
        return 1;
    }

    public abstract bool IsCompletedOrImpossible(Map map, Creature performer);

    public abstract void Perform(Creature performer, Map map);

    internal int DistanceTo(Creature creature, Map map)
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

    //internal abstract int EffectAndEstimate(Creature creature, Map map);
}

public class HarvestActivity : Activity
{
    public HarvestActivity(Prop prop) : base(0,
        new(), new(), prop.propType.GetProducedItems(), prop.propType.GetProducedItemsProbabilities(), prop, Activity.NULL_POSITION)
    {

    }

    private Prop SourceProp()
    {
        return (Prop)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return SourceProp().IsDestroyed();
    }

    public override void Perform(Creature performer, Map map)
    {
        performer.HarvestProp(SourceProp(), map);
    }
}

public class HuntActivity : Activity
{
    public HuntActivity(Creature target) : base(target.EncounterLevel(),
        new(), new(), target.GetCreatureType().GetDroppedItems(), target.GetCreatureType().GetDroppedItemsProbabilities(), target, Activity.NULL_POSITION)
    {

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
        // TODO: Implement
    }
}

public class CraftActivity : Activity
{
    public CraftActivity(ItemType producedItemType, Building workshop) : base(0,
        producedItemType.GetCraftingInputs(), new() { producedItemType }, new(), new(), workshop, Activity.NULL_POSITION)
    {

    }

    private Building Workshop()
    {
        return (Building)sourcePlaceable;
    }

    public override int ProximityRequirement()
    {
        return 0;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        if (Workshop().IsDestroyed())
        {
            return true;
        }

        foreach (ItemType outputItem in craftingOutputItems)
        {
            foreach (ItemType inputItem in outputItem.GetCraftingInputs())
            {
                bool itemFound = false;

                // check if the workshop is holding the input item
                List<Placeable> heldItemsWorkshop = new(map.HeldPlaceablesOf(Workshop()));
                foreach (Placeable heldItem in heldItemsWorkshop)
                {
                    if (heldItem is Item item && item.itemType.GetName() == inputItem.GetName())
                    {
                        itemFound = true;
                        break;
                    }
                }

                // check if the performer is holding the input item
                if (!itemFound)
                {
                    List<Placeable> heldItems = new(map.HeldPlaceablesOf(Workshop()));
                    foreach (Placeable heldItem in heldItems)
                    {
                        if (heldItem is Item item && item.itemType.GetName() == inputItem.GetName())
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
        }

        return false;
    }

    public override void Perform(Creature performer, Map map)
    {
        // consume the input items
        foreach (ItemType inputItem in craftingInputItems)
        {
            bool itemFound = false;

            // consume the first item of this type that the performer is holding
            List<Placeable> heldItems = new(map.HeldPlaceablesOf(performer));
            foreach (Placeable heldItem in heldItems)
            {
                if (heldItem is Item item && item.itemType.GetName() == inputItem.GetName())
                {
                    item.Consume();
                    itemFound = true;
                    break;
                }
            }

            // if the performer doesn't have the item, consume the first item of this type that the workshop is holding
            if (!itemFound)
            {
                List<Placeable> heldItemsWorkshop = new(map.HeldPlaceablesOf(Workshop()));
                foreach (Placeable heldItem in heldItemsWorkshop)
                {
                    if (heldItem is Item item && item.itemType.GetName() == inputItem.GetName())
                    {
                        Debug.Log("Consuming item " + item.itemType.GetName());
                        item.Consume();
                        itemFound = true;
                        break;
                    }
                }
            }

            if (!itemFound)
            {
                throw new System.Exception("Crafting input item not found");
            }
        }
    }
}

public class PickUpActivity : Activity
{
    public PickUpActivity(Item item) : base(0,
        new(), new(), new(), new(), item, Activity.NULL_POSITION)
    {

    }

    private Item Item()
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
}

public class DropOffActivity : Activity
{
    public DropOffActivity(Building building) : base(0,
        new(), new(), new(), new(), building, Activity.NULL_POSITION)
    {

    }

    private Building Building()
    {
        return (Building)sourcePlaceable;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return Building().IsDestroyed() || map.HeldPlaceablesOf(performer).Count == 0;
    }

    public override void Perform(Creature performer, Map map)
    {
        List<Placeable> heldItems = new(map.HeldPlaceablesOf(performer));
        foreach (Placeable heldItem in heldItems)
        {
            map.Transfer(heldItem, Building());
        }
    }
}
