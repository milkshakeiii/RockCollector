using System;
using System.Collections.Generic;
using UnityEngine;

public static class ThinkingQueue
{
    private static Queue<ThinkingTicket> queuedActions = new ();

    public static void EnqueueAction(ThinkingTicket action)
    {
        queuedActions.Enqueue(action);
    }

    /// <summary>
    /// Perform up to count actions from the queue, or all of them if there
    /// are less than count.
    /// </summary>
    /// <param name="count"></param>
    public static void PerformActions(int count, Map map)
    {
        int performed = 0;
        while (performed < count && queuedActions.Count > 0)
        {
            ThinkingTicket ticket = queuedActions.Dequeue();
            if (ticket.IsAlive())
            {
                ticket.Invoke(map);
                performed++;
            }
        }
    }
}

public class ThinkingTicket
{
    private Action<Map> action;
    private bool alive = true;

    public ThinkingTicket(Action<Map> action)
    {
        this.action = action;
    }

    public void Invoke(Map map)
    {
        if (!alive)
        {
            throw new InvalidOperationException("Ticket is not alive");
        }
        action.Invoke(map);
        alive = false;
    }

    public void Abort()
    {
        alive = false;
    }

    public bool IsAlive()
    {
        return alive;
    }
}