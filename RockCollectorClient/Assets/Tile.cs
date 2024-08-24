using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public Placeable placeable;

    public static Tile RandomTile()
    {
        int random = Random.Range(0, 7);

        return random switch
        {
            0 => new Rock(),
            1 => new Ore(),
            2 => new Gems(),
            3 => new Lava(),
            4 => new Water(),
            5 => new Ice(),
            _ => new Tile(),
        };
    }

    public virtual bool IsBlocking()
    {
        return false;
    }
}

public class Rock : Tile
{
    public int hardnessPoints;

    public Rock()
    {
        hardnessPoints = Random.Range(1, 10);
    }

    public override bool IsBlocking()
    {
        return true;
    }
}

public class Ore : Tile
{
    public override bool IsBlocking()
    {
        return true;
    
    }
}

public class Gems : Tile
{
    public override bool IsBlocking()
    {
        return true;
    }
}

public class Lava : Tile
{
    public override bool IsBlocking()
    {
        return true;
    }
}

public class Water : Tile
{
    public override bool IsBlocking()
    {
        return true;
    }
}