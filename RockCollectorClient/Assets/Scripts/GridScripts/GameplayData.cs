using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Equipment
{
    public int remainingDurability = 3; // Current durability of the equipment

    public virtual int MaxDurability()
    {
        // Maximum durability of the equipment
        return 3;
    }

    public abstract string Name(); // Name of the equipment
    public abstract string Description(); // Description of the equipment
    public abstract string SpriteName(); // Name of the sprite used to represent the equipment

    public virtual float BasePrice()
    {
        // Base price of the equipment, modified by the market
        return 10f;
    }

    public abstract float Mass(); // Mass in kilograms

    public virtual float ActivationPower()
    {
        // Instantaneous power cost to activate
        return 0f;
    }
    public virtual float ContinuousPower()
    {
        // Continuous power cost per turn
        return 0f;
    }

    public virtual Equipment Copy()
    {
        return (Equipment)MemberwiseClone();
    }
}

//public class Submarine : Equipment
//{
    
//}