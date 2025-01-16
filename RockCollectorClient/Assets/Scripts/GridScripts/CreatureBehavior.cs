using System;
using UnityEngine;

public abstract class CreatureBehavior
{
    public abstract Activity NextActivity(Map map, Creature actor);

    public abstract void CheckInterrupts(MapView map, CreatureSelf creature);
}

public class PeasantBehavior : CreatureBehavior
{
    public override Activity NextActivity(Map map, Creature actor)
    {
        return Search.GoalSearch(map, actor);
    }

    public override void CheckInterrupts(MapView map, CreatureSelf creature)
    {
        
    }
}