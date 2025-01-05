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
        List<Activity> activities = map.GetActivities(creature);
        // testing. return harvest activity, pick up activity, drop off activity, craft activity in that order of priority
        foreach (Activity activity in activities)
        {
            if (activity is HarvestActivity)
            {
                return activity;
            }
        }
        foreach (Activity activity in activities)
        {
            if (activity is PickUpActivity)
            {
                return activity;
            }
        }
        foreach (Activity activity in activities)
        {
            if (activity is DropOffActivity)
            {
                return activity;
            }
        }
        foreach (Activity activity in activities)
        {
            if (activity is CraftActivity)
            {
                return activity;
            }
        }
        return null;
    }
}