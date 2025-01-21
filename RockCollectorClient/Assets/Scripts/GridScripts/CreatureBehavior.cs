using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatureBehavior
{
    public static CreatureBehavior FromName(string name)
    {
        if (name == "PeasantBehavior")
        {
            return new PeasantBehavior();
        }
        else if (name == "SkeletonBehavior")
        {
            return new SkeletonBehavior();
        }
        else
        {
            throw new Exception("Unknown behavior name: " + name);
        }
    }

    public abstract string Name();

    public abstract Activity NextActivity(Map map, Creature actor);

    public abstract void CheckInterrupts(MapView map, CreatureSelf creature);
}

public class PeasantBehavior : CreatureBehavior
{
    public override string Name()
    {
        return "PeasantBehavior";
    }

    public override Activity NextActivity(Map map, Creature actor)
    {
        // priority 1: repair damaged buildings within a maximum range
        // -> make sure you are holding a repair implement
        // -> repair closer buildings first
        // priority 2: supply 
        return Search.GoalSearch(map, actor);
    }

    public override void CheckInterrupts(MapView map, CreatureSelf creature)
    {
        
    }
}

public class SkeletonBehavior : CreatureBehavior
{
    public override string Name()
    {
        return "SkeletonBehavior";
    }

    public override Activity NextActivity(Map map, Creature actor)
    {
        // if there is a creature within 4 squares, hunt it
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                List<Placeable> targets = map.PlaceablesAt(map.PositionOf(actor) + new Vector2Int(x, y));
                foreach (Placeable target in targets)
                {
                    if (target is Creature creature && creature.teamNumber != actor.teamNumber)
                    {
                        return new HuntActivity(creature);
                    }
                }
            }
        }
        return null;
    }

    public override void CheckInterrupts(MapView map, CreatureSelf self)
    {
        if (self.HasCurrentActivity())
        {
            return;
        }

        // with a 1 in 100 chance, move to a random adjacent square
        if (UnityEngine.Random.Range(0, 100) == 0)
        {
            Vector2Int target = new (UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
            self.MoveInDirection(target);
        }
    }
}