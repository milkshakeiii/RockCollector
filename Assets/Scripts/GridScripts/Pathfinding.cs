using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    public static Vector2Int StepTowards(Vector2Int currentPosition, Vector2Int target, Map map)
    {
        // Use A* to find the next step towards the target

        // Initialize the frontier with the starting position
        Utils.PriorityQueue<Vector2Int, int> frontier = new();
        frontier.Enqueue(currentPosition, 0);
        
        // Initialize the cameFrom dictionary
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        cameFrom[currentPosition] = currentPosition;

        // Initialize the costSoFar dictionary
        Dictionary<Vector2Int, int> costSoFar = new();
        costSoFar[currentPosition] = 0;

        for (int i = 0; i < 1000; i++)
        {
            if (frontier.Count == 0)
            {
                break;
            }
            // Get the current position
            Vector2Int current = frontier.Dequeue();

            // If the current position is the target,
            // retrace the path and return the next step
            if (current == target)
            {
                while (cameFrom[current] != currentPosition)
                {
                    current = cameFrom[current];
                }
                return current;
            }

            // Get the neighbors of the current position
            List<Vector2Int> neighbors = GetNeighbors(current, map);
            foreach (Vector2Int next in neighbors)
            {
                // If the neighbor is not pathable, skip it unless it is the target
                if (next != target && !map.IsPathable(next))
                {
                    continue;
                }

                int newCost = costSoFar[current] + 1;

                // If the neighbor is not in the costSoFar dictionary or the new cost is less than the current cost
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    // Update the costSoFar dictionary
                    costSoFar[next] = newCost;

                    // Calculate the priority
                    int priority = newCost + ManhattanDistance(next, target);

                    // Add the neighbor to the frontier
                    frontier.Enqueue(next, priority);

                    // Update the cameFrom dictionary
                    cameFrom[next] = current;
                }
            }
        }

        return Map.NULL_POSITION;
    }

    private static int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int position, Map map)
    {
        return new ()
        {
            new (position.x, position.y + 1),
            new (position.x, position.y - 1),
            new (position.x + 1, position.y),
            new (position.x - 1, position.y),
            new (position.x + 1, position.y + 1),
            new (position.x + 1, position.y - 1),
            new (position.x - 1, position.y + 1),
            new (position.x -1, position.y - 1),
        };
    }
}
