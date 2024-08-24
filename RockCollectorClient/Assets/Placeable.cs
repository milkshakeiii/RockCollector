using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Placeable
{

}

public class Obstacle : Placeable
{
    
}

public class Powerup : Placeable
{
    public virtual bool RequireDoubleInput()
    {
        return false;
    }
}

public class Pogostick : Powerup
{
    public override bool RequireDoubleInput()
    {
        return true;
    }
}

public class Rollerblades : Powerup
{
    public override bool RequireDoubleInput()
    {
        return true;
    }
}

public class Pick : Powerup
{
    public int miningStrength;

    public Pick(int miningStrength)
    {
        this.miningStrength = miningStrength;
    }
}

public class Item : Placeable
{
    
}

public class Feature : Placeable
{
    
}

public class Ice : Feature
{
    
}

public class Exit : Feature
{
    
}

public class Player : Placeable
{
    public Item heldItem;
    public List<Powerup> powerups;

    public int InputsRequiredCount()
    {
        foreach (Powerup powerup in powerups)
        {
            if (powerup.RequireDoubleInput())
            {
                return 2;
            }
        }
        return 1;
    }
}
