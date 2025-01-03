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
        public Dictionary<Vector2Int, List<ItemType>> expectedItems = new();
        // because items dropped from props and monsters are not guaranteed, and crafting is not guaranteed to succeed,
        // itemWeights contains the probability that each expectedItem will actually exist at the location.
        public Dictionary<Vector2Int, List<float>> itemWeights = new();

        // demandedItems are the items that each building is requesting.
        // the goal of craft search is to satisfy as many of these demands as possible.
        public Dictionary<Vector2Int, List<ItemType>> demandedItems = new();
    }

    public static Activity CraftSearch(Map map, Creature creature)
    {


        return null;
    }
}