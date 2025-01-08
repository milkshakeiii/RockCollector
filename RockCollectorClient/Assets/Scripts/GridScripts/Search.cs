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
        return BFSGoalSearch(map, creature, 10);
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
        float bestEvaluation = float.MaxValue;

        while (queue.Count > 0 && current.depth < maxDepth)
        {
            current = queue.Dequeue();
            
            List<Activity> activities = current.map.GetActivities(creature);
            foreach (Activity activity in activities)
            {
                SearchNode next = current.Copy();
                next.estimatedTicks += creature.MoveAndEstimate(activity, next.map);
                next.estimatedTicks += activity.EffectAndEstimate(creature, next.map);
                next.activitiesCompleted.Add(activity);
                next.depth++;
                queue.Enqueue(next);

                float evaluation = creature.GetGoal().EvaluateMap(next.map);
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