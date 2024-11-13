using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Equipment
{
    public int maxDurability = 3; // Maximum durability of the equipment
    public int remainingDurability = 3; // Current durability of the equipment

    public string name = ""; // Name of the equipment
    public string description = ""; // Description of the equipment
    public string spriteName = ""; // Name of the sprite used to represent the equipment

    public float basePrice = 10f; // Base price of the equipment, modified by the market
    public float mass = 0f; // Mass in kilograms
    public float activationPower = 0f; // Instantaneous power cost to activate
    public float continuousPower = 0f; // Continuous power cost per second

    public virtual Equipment Copy()
    {
        return (Equipment)MemberwiseClone();
    }

    public virtual float AddedMass()
    {
        return mass;
    }
}

public class Submarine : Equipment
{

}