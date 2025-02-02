using System;
using UnityEngine;

public class Condition
{
    public ConditionType conditionType;
    private float stacks;

    public Condition(ConditionType conditionType, float stacks)
    {
        this.conditionType = conditionType;
        this.stacks = stacks;
    }

    public string GetName()
    {
        return conditionType.ToString();
    }

    public int GetStacks()
    {
        return Mathf.FloorToInt(stacks);
    }

    public void AddStacks(float stacks)
    {
        this.stacks += stacks;
    }

    public int GetSkillModifier(string skillName)
    {
        throw new NotImplementedException();
    }

    public float GetSpeedModifier()
    {
        throw new NotImplementedException();
    }

    public int GetMoveSpeedModifier()
    {
        throw new NotImplementedException();
    }

    public int GetAttributeModifier(AttributeScores score)
    {
        throw new NotImplementedException();
    }

    public int GetMaxHealthModifier()
    {
        throw new NotImplementedException();
    }

    internal void Tick(Creature creature, Map map)
    {
        throw new NotImplementedException();
    }

    internal int GetProtectionModifier()
    {
        throw new NotImplementedException();
    }

    internal int GetInflictorModifier()
    {
        throw new NotImplementedException();
    }
}