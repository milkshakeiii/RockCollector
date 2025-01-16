using System;
using UnityEngine;

public abstract class CreatureBehavior
{
    public abstract Activity NextActivity(Map mapCopy, Creature newMe);
}

public class PeasantBehavior : CreatureBehavior
{
    public override Activity NextActivity(Map mapCopy, Creature newMe)
    {
        return Search.GoalSearch(mapCopy, newMe);
    }
}