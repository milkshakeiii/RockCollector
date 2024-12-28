using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Feat
{

}

public class TypeAbility
{

}

public class CreatureType
{
    public int startingHealth;
    public int healthPerLevel;

    public int strengthBonus;
    public int dexterityBonus;
    public int constitutionBonus;
    public int intelligenceBonus;
    public int wisdomBonus;
    public int charismaBonus;
}

public class Creature
{
    public int currentHealth;
}