using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UnityEngine;

public static class Search
{
    public static Activity CraftSearch(Map map, Creature creature)
    {
        return map.GetActivities(creature)[0];
    }
}