using System;
using System.Collections.Generic;

public abstract class Equipment
{
    public int remainingDurability = 3; // Current durability of the equipment

    public static Equipment EquipmentFromName(string name)
    {
        // declare a list of all the leaf node equipment classes
        List<Type> types = new() { typeof(CapsuleSubmarine), typeof(Scoop) };

        foreach (Type type in types)
        {
            Equipment instance = (Equipment)Activator.CreateInstance(type);
            if (name == instance.Name())
            {
                return instance;
            }
        }

        throw new Exception("Equipment name not found: " + name);
    }

    public virtual int MaxDurability()
    {
        // Maximum durability of the equipment
        return 3;
    }

    public abstract string Name(); // Name of the equipment
    public abstract string Description(); // Description of the equipment
    public abstract string SpritePath(); // Filepath (within resources) to the sprite used to represent the equipment

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

public abstract class Submarine : Equipment
{

}

public class CapsuleSubmarine : Submarine
{
    public override string Name()
    {
        return "capsule_submarine";
    }

    public override string Description()
    {
        return "The smallest submarine.";
    }

    public override string SpritePath()
    {
        return "Art/Characters/Submarines/Submarine_Small";
    }

    public override float Mass()
    {
        return 1000; // kg
    }
}

public class Scoop : Equipment
{
    public override string Name()
    {
        return "scoop";
    }

    public override string Description()
    {
        return "A scoop for collecting small creatures.";
    }

    public override string SpritePath()
    {
        return "Art/Characters/Submarines/Scoop_open";
    }

    public override float Mass()
    {
        return 100; // kg
    }
}