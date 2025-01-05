using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UnityEngine;

public static class Search
{
    public class CraftModelMap
    {
        public int encounterLevel = 0;
        public int estimatedTicks = 0;

        public List<ItemType> heldItems = new();

        public Dictionary<Vector2Int, List<ItemType>> expectedItems = new();
        // because items dropped from props and monsters are not guaranteed, and crafting is not guaranteed to succeed,
        // itemWeights contains the probability that each expectedItem will actually exist at the location.
        public Dictionary<Vector2Int, List<float>> itemWeights = new();

        // demandedItems are the items that each building is requesting.
        // the goal of craft search is to satisfy as many of these demands as possible.
        public Dictionary<Vector2Int, List<ItemType>> demandedItems = new();

        public List<Activity> availableActions = new();

        public void UpdateAvailableActions(Creature actor, Map sourceMap)
        {

        }

        public CraftModelMap Copy()
        {
            CraftModelMap copy = new()
            {
                encounterLevel = encounterLevel,
                estimatedTicks = estimatedTicks,
                heldItems = new List<ItemType>(heldItems),
                expectedItems = new Dictionary<Vector2Int, List<ItemType>>(expectedItems),
                itemWeights = new Dictionary<Vector2Int, List<float>>(itemWeights),
                demandedItems = new Dictionary<Vector2Int, List<ItemType>>(demandedItems),
                availableActions = new List<Activity>(availableActions)
            };
            return copy;
        }
    }

    public static Activity CraftSearch(Map map, Creature creature)
    {
        CraftModelMap model = new();
        // populate model with expectedItems, itemWeights, and demandedItems
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            // if this is an item, add it to expectedItems with a weight of 1
            if (placeable is Item item)
            {
                Vector2Int position = map.PositionOf(item);
                if (!model.expectedItems.ContainsKey(position))
                {
                    model.expectedItems[position] = new List<ItemType>();
                    model.itemWeights[position] = new List<float>();
                }
                model.expectedItems[position].Add(item.itemType);
                model.itemWeights[position].Add(1);
            }

            // if this is a building, add its requestableItemTypes to demandedItems
            // duplicate itemTypes as many times as the building requests them (requestableItemCounts)
            if (placeable is Building building)
            {
                Vector2Int position = map.PositionOf(building);
                if (!model.demandedItems.ContainsKey(position))
                {
                    model.demandedItems[position] = new List<ItemType>();
                }
                for (int i = 0; i < building.buildingType.GetRequestableItemTypes().Count; i++)
                {
                    ItemType itemType = building.buildingType.GetRequestableItemTypes()[i];
                    int count = building.buildingType.GetRequestableItemCounts()[i];
                    for (int j = 0; j < count; j++)
                    {
                        model.demandedItems[position].Add(itemType);
                    }
                }
            }
        }

        List<Activity> firstPossibleActivities = map.GetActivities(creature);


        return CraftBFS(map, creature, model, firstPossibleActivities, 10);
    }

    public class CraftBFSNode
    {
        public CraftModelMap model;
        public List<Activity> possibleActivities;
        public List<Activity> activitiesTaken = new();
        
        public int depth = 0;

        // the value of a node is satisfiedDemandsValue over ticksTaken
        public int satisfiedDemandsValue = 0;
        public int ticksTaken = 1;

        public CraftBFSNode Copy()
        {
            CraftBFSNode copy = new()
            {
                model = model,
                possibleActivities = new List<Activity>(possibleActivities),
                activitiesTaken = new List<Activity>(activitiesTaken),
                depth = depth,
                satisfiedDemandsValue = satisfiedDemandsValue,
                ticksTaken = ticksTaken
            };
            return copy;
        }
    }

    public static Activity CraftBFS(Map map, Creature creature, CraftModelMap model, List<Activity> firstPossibleActivities, int maxDepth)
    {
        Queue<CraftBFSNode> queue = new();
        CraftBFSNode firstNode = new CraftBFSNode();
        firstNode.model = model;
        firstNode.possibleActivities = firstPossibleActivities;
        queue.Enqueue(firstNode);

        CraftBFSNode bestNode = null;
        CraftBFSNode currentNode = null;
        while ((currentNode == null || currentNode.depth < maxDepth) && queue.Count > 0)
        {
            currentNode = queue.Dequeue();

        }
        
        return null;
    }
}