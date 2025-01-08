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
        return BFSGoalSearch(map, creature, 8);
    }

    public static Activity BFSGoalSearch(Map startingMap, Creature creature, int maxDepth)
    {
        static void PrintBestInfo(SearchNode best, float bestEvaluation)
        {
            Debug.Log("Best evaluation: " + bestEvaluation);
            Debug.Log("Ticks: " + best.estimatedTicks);
            Debug.Log("Best path:");
            foreach (Activity a in best.activitiesCompleted)
            {
                if (a is CraftActivity craft)
                {
                    Debug.Log("Craft " + craft.craftingOutputItem.GetName());
                }
                else if (a is DropOffActivity dropoff)
                {
                    Debug.Log("Drop off " + dropoff.Building().buildingType.GetName());
                }
                else if (a is HarvestActivity harvest)
                {
                    Debug.Log("Harvest " + harvest.SourceProp().propType.GetName());
                }
                else if (a is HuntActivity hunt)
                {
                    Debug.Log("Hunt ");
                }
                else if (a is PickUpActivity pickup)
                {
                    Debug.Log("Pick up " + pickup.Item().itemType.GetName());
                }
            }
        }

        Queue<SearchNode> queue = new ();
        SearchNode start = new ()
        {
            map = startingMap.ShallowCopy(),
            activitiesCompleted = new List<Activity>(),
            estimatedTicks = 0,
            depth = 0
        };
        queue.Enqueue(start);

        SearchNode current = start;
        SearchNode best = start;
        float bestEvaluation = creature.GetGoal().EvaluateMap(start.map);

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
                if (queue.Count > 100000)
                {
                    Debug.Log("Next path length: " + next.activitiesCompleted.Count);
                    PrintBestInfo(best, bestEvaluation);
                    throw new Exception("Queue too long, bailing out.");
                }

                float evaluation = creature.GetGoal().EvaluateMap(next.map);// / next.estimatedTicks;
                if (evaluation > bestEvaluation)
                {
                    best = next;
                    bestEvaluation = evaluation;
                }
            }
        }

        //PrintBestInfo(best, bestEvaluation);
        return best.activitiesCompleted.Count > 0 ? best.activitiesCompleted[0] : null;
    }
}