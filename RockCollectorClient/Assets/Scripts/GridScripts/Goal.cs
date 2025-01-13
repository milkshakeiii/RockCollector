using NUnit.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using UnityEngine;

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

    public abstract float EvaluateMap(Map map);

    public abstract void Initialize(int tickBegun);
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

    public override float EvaluateMap(Map map)
    {
        float evaluation = 0;
        // for each building, add the value of the satisfied item requests
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Building building)
            {
                List<ItemType> requestedItemTypes = building.GetRequestedItemTypes();
                List<int> requestedItemAmounts = building.GetRequestedItemAmounts();
                Dictionary<ItemType, int> itemsRequestedByType = new();
                for (int i = 0; i < requestedItemTypes.Count; i++)
                {
                    itemsRequestedByType[requestedItemTypes[i]] = requestedItemAmounts[i];
                }

                List<Placeable> itemsPresent = map.HeldPlaceablesOf(building);
                Dictionary<ItemType, int> itemsPresentByType = new();
                foreach (Placeable heldPlaceable in itemsPresent)
                {
                    if (heldPlaceable is Item item)
                    {
                        if (!itemsPresentByType.ContainsKey(item.itemType))
                        {
                            itemsPresentByType[item.itemType] = 0;
                        }
                        if (!itemsRequestedByType.ContainsKey(item.itemType))
                        {
                            continue;
                        }
                        if (itemsPresentByType[item.itemType] >= itemsRequestedByType[item.itemType])
                        {
                            // only score the first items of the requested type
                            continue;
                        }
                        itemsPresentByType[item.itemType]++;
                        evaluation += item.Value();
                    }
                }
            }
        }
        return evaluation;
    }
}