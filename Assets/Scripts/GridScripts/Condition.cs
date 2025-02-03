using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionType conditionType;
    private float stacks;

    // a measure of how long the condition has been active
    // increased by stacks every tick, decrementing stacks when
    // it reaches duration
    private int elapsedTicks;

    public Condition(ConditionType conditionType, float stacks)
    {
        this.conditionType = conditionType;
        this.stacks = stacks;
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
        List<string> skillNames = conditionType.GetSkillBonusNames();
        List<int> skillBonusAmounts = conditionType.GetSkillBonusAmounts();
        for (int i = 0; i < skillNames.Count; i++)
        {
            if (skillNames[i] == skillName)
            {
                return skillBonusAmounts[i];
            }
        }
        return 0;
    }

    public float GetSpeedModifier()
    {
        return conditionType.GetSpeedModifier() * stacks;
    }

    public int GetMoveSpeedModifier()
    {
        return conditionType.GetMoveSpeedModifier();
    }

    public int GetAttributeModifier(AttributeScores score)
    {
        List<AttributeScores> modifiedAttributeScores = conditionType.GetModifiedAttributeScores();
        List<int> attributeModifiers = conditionType.GetAttributeModifierAmounts();
        for (int i = 0; i < modifiedAttributeScores.Count; i++)
        {
            if (modifiedAttributeScores[i] == score)
            {
                return attributeModifiers[i];
            }
        }
        return 0;
    }

    public int GetMaxHealthModifier()
    {
        return conditionType.GetMaxHealthModifier();
    }

    public void Tick(Creature creature, Map map)
    {
        elapsedTicks += Mathf.FloorToInt(stacks);
        int duration = conditionType.GetDuration();
        int stacksToRemove = elapsedTicks / duration;
        stacks -= stacksToRemove;
    }

    /// <summary>
    /// The protection this condition gives against other conditions.
    /// </summary>
    public int GetConditionProtection()
    {
        return conditionType.GetConditionProtection();
    }
}