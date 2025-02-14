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

    public virtual Vector2Int GetLocation(Creature performer, Map map)
    {
        if (sourcePlaceable != null)
        {
            return map.PositionOf(sourcePlaceable);
        }
        if (position != NULL_POSITION)
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

    public virtual int DistanceTo(Creature creature, Map map)
    {
        if (sourcePlaceable != null && !map.PlaceableExists(sourcePlaceable))
        {
            throw new Exception("Source placeable does not exist in " + this);
        }
        if (sourcePlaceable != null)
        {
            return map.DistanceBetween(creature, sourcePlaceable);
        }
        if (position != NULL_POSITION)
        {
            return map.DistanceTo(position, creature);
        }
        throw new Exception("Unable to determine location of activity " + this);
    }

    public virtual bool TryClaimPlaceables(Creature performer, Map map)
    {
        if (sourcePlaceable == null)
        {
            return true;
        }
        if (sourcePlaceable.IsClaimed())
        {
            return false;
        }
        sourcePlaceable?.Claim();
        return true;
    }

    public virtual void UnclaimPlaceables(Creature performer, Map map)
    {
        sourcePlaceable?.Unclaim();
    }

    public abstract string TargetDescriptiveString();
}

public class HarvestActivity : Activity
{
    public HarvestActivity(Prop prop) : base(0,
        new(), null, prop.propType.GetProducedItems(), prop.propType.GetProducedItemsProbabilities(), prop, Activity.NULL_POSITION)
    {

    }

    public override string TargetDescriptiveString()
    {
        return SourceProp().propType.GetName();
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
    public HuntActivity(Creature target) : base(target.DifficultyEstimate(),
        new(), null, target.GetCreatureType().GetDroppedItems(), target.GetCreatureType().GetDroppedItemsProbabilities(), target, Activity.NULL_POSITION)
    {

    }

    public override string TargetDescriptiveString()
    {
        return SourceCreature().GetCreatureType().GetName();
    }

    public override bool TryClaimPlaceables(Creature performer, Map map)
    {
        // do nothing
        return true;
    }

    public override void UnclaimPlaceables(Creature performer, Map map)
    {
        // do nothing
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

public class RaidActivity : Activity
{
    public RaidActivity(Building targetBuilding) : base(0,
               new(), null, new(), new(), targetBuilding, Activity.NULL_POSITION)
    {

    }

    public override string TargetDescriptiveString()
    {
        return SourceBuilding().buildingType.GetName();
    }

    public Building SourceBuilding()
    {
        return (Building)sourcePlaceable;
    }

    public override bool TryClaimPlaceables(Creature performer, Map map)
    {
        // do nothing
        return true;
    }

    public override void UnclaimPlaceables(Creature performer, Map map)
    {
        // do nothing
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

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return SourceBuilding().IsDestroyed();
    }

    public override void Perform(Creature performer, Map map)
    {
        performer.UseAnyAbility(map);
    }
}

public class CraftActivity : Activity
{
    bool crafted = false;

    public CraftActivity(ItemType producedItemType, Building workshop) : base(0,
        producedItemType.GetCraftingInputs(), producedItemType, new(), new(), workshop, Activity.NULL_POSITION)
    {

    }

    public override string TargetDescriptiveString()
    {
        return craftingOutputItem.GetName();
    }

    private HashSet<Item> ConsumedItems(Creature performer, Map map)
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
        return consumedItems;
    }

    private Building Workshop()
    {
        return (Building)sourcePlaceable;
    }

    public override int ProximityRequirement(Creature forCreature, Map map)
    {
        return 1;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        // check if the item has already been crafted or the workshop is destroyed
        if (crafted || Workshop().IsDestroyed())
        {
            return true;
        }

        // check that the item is still requested
        if (Workshop().GetMissingItemAmount(craftingOutputItem, map) <= 0)
        {
            return true;
        }

        // check that the performer has the required skill level
        string craftingSkill = craftingOutputItem.GetCraftingSkill();
        if (performer.CraftingSkillModifier(craftingSkill, map) < craftingOutputItem.GetCraftingLevel())
        {
            return true;
        }

        // check that the performer or the workshop has the required input items
        if (ConsumedItems(performer, map).Count != craftingInputItems.Count)
        {
            return true;
        }
        
        return false;
    }

    public override void Perform(Creature performer, Map map)
    {
        HashSet<Item> consumedItems = ConsumedItems(performer, map);

        // consume the items
        foreach (Item consumedItem in consumedItems)
        {
            consumedItem.Consume();
        }

        // create the output item
        Item outputItem = new(craftingOutputItem);
        map.AddHeld(Workshop(), outputItem);
        crafted = true;
        performer.GainExperience(craftingOutputItem.GetCraftingLevel() + 10, craftingOutputItem.GetCraftingSkill());
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

    public override string TargetDescriptiveString()
    {
        return Item().itemType.GetName();
    }

    public Item Item()
    {
        return (Item)sourcePlaceable;
    }

    public override int DistanceTo(Creature creature, Map map)
    {
        if (map.HolderOf(Item()) != null)
        {
            return map.DistanceBetween(creature, map.HolderOf(Item()));
        }
        return map.DistanceTo(map.PositionOf(Item()), creature);
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return !map.PlaceableExists(Item()) || Item().IsConsumed() || (map.HolderOf(Item()) is Creature);
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

public class DeliverActivity : Activity
{
    private Vector2Int buildingLocation;
    private bool delivered = false;

    public DeliverActivity(Item item, Vector2Int buildingLocation) : base(0,
        new(), null, new(), new(), item, Activity.NULL_POSITION)
    {
        this.buildingLocation = buildingLocation;
    }

    public override string TargetDescriptiveString()
    {
        return Item().itemType.GetName();
    }

    public Item Item()
    {
        return (Item)sourcePlaceable;
    }

    public override Vector2Int GetLocation(Creature performer, Map map)
    {
        if (map.HolderOf(Item()) != performer)
        {
            return map.PositionOf(Item());
        }
        return buildingLocation;
    }

    public override int DistanceTo(Creature creature, Map map)
    {
        if (map.HolderOf(Item()) != creature && map.IsHeld(Item()))
        {
            return map.DistanceBetween(creature, map.HolderOf(Item()));
        }
        else if (map.HolderOf(Item()) != creature)
        {
            return map.DistanceTo(map.PositionOf(Item()), creature);
        }
        return map.DistanceBetween(creature, Building(map));
    }

    public Building Building(Map map)
    {
        List<Placeable> placeables = map.PlaceablesAt(buildingLocation);
        foreach (Placeable placeable in placeables)
        {
            if (placeable is Building building)
            {
                return building;
            }
        }
        return null;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        Building building = Building(map);
        if (building == null)
        {
            return true;
        }
        return building.IsDestroyed() || !map.PlaceableExists(Item()) || building.GetMissingItemAmount(Item().itemType, map) <= 0 || delivered;
    }

    public override void Perform(Creature performer, Map map)
    {
        Building building = Building(map);
        if (building == null)
        {
            throw new Exception("Building not found");
        }
        if (map.HolderOf(Item()) != performer)
        {
            if (map.IsHeld(Item()))
            {
                map.Transfer(Item(), performer);
            }
            else
            {
                map.PickUp(performer, Item());
            }
        }
        else
        {
            List<Placeable> heldPlaceables = new(map.HeldPlaceablesOf(performer));
            foreach (Placeable heldItem in heldPlaceables)
            {
                if (heldItem is Item item)
                {
                    // only transfer items that are requested by the building
                    if (building.GetMissingItemAmount(item.itemType, map) > 0 && !performer.OutfitContains(item, map))
                    {
                        Debug.Log("Delivering " + item.itemType.GetName() + " to " + building.buildingType.GetName());
                        map.Transfer(heldItem, building);
                        delivered = true;
                    }
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

    public override string TargetDescriptiveString()
    {
        return Building().buildingType.GetName();
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

    public override string TargetDescriptiveString()
    {
        return Building().buildingType.GetName();
    }

    public override bool TryClaimPlaceables(Creature performer, Map map)
    {
        // we don't need to claim the building
        return true;
    }

    public override void UnclaimPlaceables(Creature performer, Map map)
    {
        // do nothing
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

public class PlantActivity : Activity
{
    private PropType propTypeToPlant;

    public PlantActivity(Vector2Int plantPosition, PropType propTypeToPlant) : base(0,
        new(), null, new(), new(), null, plantPosition)
    {
        this.propTypeToPlant = propTypeToPlant;
    }

    public override string TargetDescriptiveString()
    {
        return propTypeToPlant.GetName();
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        // check if the performer has the required skill level
        string plantingSkill = propTypeToPlant.GetPlantingSkill();
        if (performer.PlantingSkillModifier(plantingSkill, map) < propTypeToPlant.GetPlantingLevel())
        {
            return true;
        }

        // check if there is a prop or building at the target location
        List<Placeable> placeables = map.PlaceablesAt(position);
        foreach (Placeable placeable in placeables)
        {
            if (placeable is Prop || placeable is Building)
            {
                return true;
            }
        }
        return false;
    }

    public override void Perform(Creature performer, Map map)
    {
        performer.PlantProp(propTypeToPlant, GetLocation(performer, map), map);
    }
}

public class IdleActivity : Activity
{
    private int firstPerformedTick = -1;

    public IdleActivity(Vector2Int position) : base(0,
        new(), null, new(), new(), null, position)
    {

    }

    public override string TargetDescriptiveString()
    {
        return "";
    }

    public override bool TryClaimPlaceables(Creature performer, Map map)
    {
        // do nothing
        return true;
    }

    public override void UnclaimPlaceables(Creature performer, Map map)
    {
        // do nothing
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
        if (homeBuilding == null)
        {
            throw new Exception("Home building must not be null");
        }
        this.range = range;
    }

    private Building HomeBuilding()
    {
        return (Building)sourcePlaceable;
    }

    public override string TargetDescriptiveString()
    {
        return "";
    }

    public override bool TryClaimPlaceables(Creature performer, Map map)
    {
        // do nothing
        return true;
    }

    public override void UnclaimPlaceables(Creature performer, Map map)
    {
        // do nothing
    }

    public override int ProximityRequirement(Creature forCreature, Map map)
    {
        return int.MaxValue;
    }

    public override bool IsCompletedOrImpossible(Map map, Creature performer)
    {
        return HomeBuilding().IsDestroyed() || firstPerformedTick != -1 && map.CurrentTick() - firstPerformedTick >= 150;
    }

    private static readonly List<Vector2Int> clockwise = new () { new(0, 0), new(1, 0), new(1, -1), new(0, -1), new(-1, -1), new(-1, 0), new(-1, 1), new(0, 1), new(1, 1) };
    public override void Perform(Creature performer, Map map)
    {
        if (firstPerformedTick == -1)
        {
            firstPerformedTick = map.CurrentTick();
        }
        // 1% chance of moving to a new location
        if (map.CurrentTick() % 10 != 0 || UnityEngine.Random.Range(0,10) != 0)
        {
            return;
        }
        Vector2Int randomDirection = new(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
        // if this would not take us further out of range, move to the new position, otherwise try the next direction
        int clockwiseIndex = clockwise.IndexOf(randomDirection);
        for (int i = clockwiseIndex; i < clockwise.Count + clockwiseIndex; i++)
        {
            Vector2Int newDirection = clockwise[i % clockwise.Count];
            Vector2Int newPosition = map.PositionOf(performer) + randomDirection;
            if (!map.IsPathable(newPosition))
            {
                continue;
            }
            int newDistance = map.DistanceTo(newPosition, sourcePlaceable);
            int oldDistance = map.DistanceTo(map.PositionOf(performer), sourcePlaceable);
            if (newDistance <= range || newDistance < oldDistance)
            {
                performer.MoveInDirection(newDirection, map);
                return;
            }
        }
    }
}
