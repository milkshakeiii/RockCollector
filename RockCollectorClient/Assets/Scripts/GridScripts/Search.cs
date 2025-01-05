using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UnityEngine;

public static class Search
{
    public class SearchNode
    {
        public Map map;
        public List<Activity> activitiesCompleted;
        public int estimatedTicks;
        public int depth;

        public SearchNode Copy()
        {
            return new SearchNode()
            {
                map = map.ShallowCopy(),
                activitiesCompleted = new List<Activity>(activitiesCompleted),
                estimatedTicks = estimatedTicks,
                depth = depth
            };
        }
    }

    public static Activity GoalSearch(Map map, Creature creature)
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

        //List<Activity> activities = map.GetActivities(creature);
        //
        //return null;
    }

    public static Activity BFSGoalSearch(Map startingMap, Creature creature, int maxDepth)
    {
        Queue<SearchNode> queue = new ();
        SearchNode start = new ()
        {
            map = startingMap,
            activitiesCompleted = new List<Activity>(),
            estimatedTicks = 0,
            depth = 0
        };
        queue.Enqueue(start);

        SearchNode current = start;
        SearchNode best = start;
        int bestEvaluation = int.MaxValue;

        while (queue.Count > 0 && current.depth < maxDepth)
        {
            current = queue.Dequeue();
            
            List<Activity> activities = current.map.GetActivities(creature);
            foreach (Activity activity in activities)
            {
                SearchNode next = current.Copy();
                next.estimatedTicks += creature.MoveAndEstimate(activity, next.map);
                //next.estimatedTicks += activity.EffectAndEstimate(creature, next.map);
                next.activitiesCompleted.Add(activity);
                next.depth++;
                queue.Enqueue(next);

                int evaluation = creature.GetGoal().EvaluateMap(next.map);
                if (evaluation < bestEvaluation)
                {
                    best = next;
                    bestEvaluation = evaluation;
                }
            }
        }

        return best.activitiesCompleted.Count > 0 ? best.activitiesCompleted[0] : null;
    }
}