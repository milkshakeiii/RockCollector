using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class EntityManager
{
    public static Dictionary<string, Feat> feats = new();
    public static Dictionary<string, TypeAbility> typeAbilities = new();
    public static Dictionary<string, Condition> conditions = new();
    public static Dictionary<string, CreatureType> creatureTypes = new();
    public static Dictionary<string, Prop> props = new();
    public static Dictionary<string, Item> items = new();

    public static string GetStringAttribute(Dictionary<string, string> attributes, string key, string defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return attributes[key];
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static int GetIntAttribute(Dictionary<string, string> attributes, string key, int defaultValue = -1)
    {
        if (attributes.ContainsKey(key))
        {
            return int.Parse(attributes[key]);
        }
        if (defaultValue == -1)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static List<int> GetIntListAttribute(Dictionary<string, string> attributes, string key, List<int> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<int>(System.Array.ConvertAll(attributes[key].Split(','), int.Parse));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static List<string> GetStringListAttribute(Dictionary<string, string> attributes, string key, List<string> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<string>(attributes[key].Split(','));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static bool HasAttribute(Dictionary<string, string> attributes, string key)
    {
        return attributes.ContainsKey(key);
    }

    public static List<Item> GetItemListAttribute(Dictionary<string, string> attributes, string key, List<Item> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<Item>(System.Array.ConvertAll(attributes[key].Split(','), x => items[x]));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static void ReadAllEntities()
    {
        ReadFeats();
        ReadTypeAbilities();
        ReadConditions();
        ReadCreatureTypes();
        ReadProps();
        ReadItems();
    }
    public static void ReadFeats()
    {
        feats = new();
        TextAsset textAsset = Resources.Load<TextAsset>("Feats");
        string[] lines = textAsset.text.Split('\n');
        Dictionary<string, string> currentFeat = new();
        foreach (string line in lines)
        {
            if (line == "")
            {
                feats[currentFeat["name"]] = new Feat { attributes = currentFeat };
                currentFeat = new();
            }
            else
            {
                string[] parts = line.Split(':');
                currentFeat[parts[0]] = parts[1];
            }
        }
    }
    public static void ReadTypeAbilities()
    {
        typeAbilities = new();
        TextAsset textAsset = Resources.Load<TextAsset>("TypeAbilities");
        string[] lines = textAsset.text.Split('\n');
        Dictionary<string, string> currentTypeAbility = new();
        foreach (string line in lines)
        {
            if (line == "")
            {
                typeAbilities[currentTypeAbility["name"]] = new TypeAbility { attributes = currentTypeAbility };
                currentTypeAbility = new();
            }
            else
            {
                string[] parts = line.Split(':');
                currentTypeAbility[parts[0]] = parts[1];
            }
        }
    }
    public static void ReadConditions()
    {
        conditions = new();
        TextAsset textAsset = Resources.Load<TextAsset>("Conditions");
        string[] lines = textAsset.text.Split('\n');
        Dictionary<string, string> currentCondition = new();
        foreach (string line in lines)
        {
            if (line == "")
            {
                conditions[currentCondition["name"]] = new Condition { attributes = currentCondition };
                currentCondition = new();
            }
            else
            {
                string[] parts = line.Split(':');
                currentCondition[parts[0]] = parts[1];
            }
        }
    }
    public static void ReadCreatureTypes()
    {
        creatureTypes = new();
        TextAsset textAsset = Resources.Load<TextAsset>("CreatureTypes");
        string[] lines = textAsset.text.Split('\n');
        Dictionary<string, string> currentCreatureType = new();
        foreach (string line in lines)
        {
            if (line == "")
            {
                creatureTypes[currentCreatureType["name"]] = new CreatureType { attributes = currentCreatureType };
                currentCreatureType = new();
            }
            else
            {
                string[] parts = line.Split(':');
                currentCreatureType[parts[0]] = parts[1];
            }
        }
    }
    public static void ReadProps()
    {
        props = new();
        TextAsset textAsset = Resources.Load<TextAsset>("Props");
        string[] lines = textAsset.text.Split('\n');
        Dictionary<string, string> currentProp = new();
        foreach (string line in lines)
        {
            if (line == "")
            {
                props[currentProp["name"]] = new Prop { attributes = currentProp };
                currentProp = new();
            }
            else
            {
                string[] parts = line.Split(':');
                currentProp[parts[0]] = parts[1];
            }
        }
    }
    public static void ReadItems()
    {
        items = new();
        TextAsset textAsset = Resources.Load<TextAsset>("Items");
        string[] lines = textAsset.text.Split('\n');
        Dictionary<string, string> currentItem = new();
        foreach (string line in lines)
        {
            if (line == "")
            {
                items[currentItem["name"]] = new Item { attributes = currentItem };
                currentItem = new();
            }
            else
            {
                string[] parts = line.Split(':');
                currentItem[parts[0]] = parts[1];
            }
        }
    }
}

public class Feat
{
    public Dictionary<string, string> attributes;

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public string GetSkillBonusName()
    {
        return EntityManager.GetStringAttribute(attributes, "skillBonusName");
    }
    public int GetSkillBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "skillBonus");
    }
}

public class TypeAbility
{
    public Dictionary<string, string> attributes;

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public int GetRange()
    {
        // in squares
        return EntityManager.GetIntAttribute(attributes, "range", 1);
    }
    public int GetEffectArea()
    {
        // in square diameter
        return EntityManager.GetIntAttribute(attributes, "effectArea", 1);
    }
    public int GetCooldown()
    {
        // in ticks
        return EntityManager.GetIntAttribute(attributes, "cooldown", 0);
    }
    public int GetDuration()
    {
        // in ticks
        return EntityManager.GetIntAttribute(attributes, "duration", 0);
    }
    public int GetDamage()
    {
        // in health points
        return EntityManager.GetIntAttribute(attributes, "damage", 0);
    }
    public int GetHeal()
    {
        // in health points
        return EntityManager.GetIntAttribute(attributes, "heal", 0);
    }
    public int GetHarvestingAmount()
    {
        // in ticks
        return EntityManager.GetIntAttribute(attributes, "harvestingAmount");
    }
}

public class Condition
{
    public Dictionary<string, string> attributes;

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public int GetAttackPenalty()
    {
        return EntityManager.GetIntAttribute(attributes, "attackPenalty", 0);
    }
    public int GetTicksPerDamage()
    {
        return EntityManager.GetIntAttribute(attributes, "ticksPerDamage", 0);
    }
    public int GetDamage()
    {
        return EntityManager.GetIntAttribute(attributes, "damage", 0);
    }
}

public class CreatureType
{
    public Dictionary<string, string> attributes;
    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public int GetStartingHealth()
    {
        return EntityManager.GetIntAttribute(attributes, "startingHealth");
    }
    public int GetHealthPerLevel()
    {
        return EntityManager.GetIntAttribute(attributes, "healthPerLevel");
    }
    public int GetStrengthBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "strengthBonus");
    }
    public int GetDexterityBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "dexterityBonus");
    }
    public int GetConstitutionBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "constitutionBonus");
    }
    public int GetIntelligenceBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "intelligenceBonus");
    }
    public int GetWisdomBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "wisdomBonus");
    }
    public int GetCharismaBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "charismaBonus");
    }
    public List<string> GetAbilities()
    {
        return EntityManager.GetStringListAttribute(attributes, "abilities");
    }
    public List<int> GetAbilityLevels()
    {
        return EntityManager.GetIntListAttribute(attributes, "abilityLevels");
    }
}

public class Prop
{
    public Dictionary<string, string> attributes;

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public string GetHarvestingType()
    {
        return EntityManager.GetStringAttribute(attributes, "harvestingType");
    }
    public int GetHarvestingRequired()
    {
        return EntityManager.GetIntAttribute(attributes, "harvestingRequired");
    }
    public List<Item> GetProducedItems()
    {
        return EntityManager.GetItemListAttribute(attributes, "producedItems", new List<Item>());
    }
    public List<int> GetProducedItemsProbabilities()
    {
        return EntityManager.GetIntListAttribute(attributes, "producedItemsProbabilities", new List<int>());
    }
}

public class Item
{
    public Dictionary<string, string> attributes;

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public List<string> GetHarvestingTypes()
    {
        return EntityManager.GetStringListAttribute(attributes, "harvestingTypes", new List<string>());
    }
    public int GetHarvestingAmount()
    {
        return EntityManager.GetIntAttribute(attributes, "harvestingAmount", 0);
    }
}

public class Creature
{
    public string name;

    public int currentHealth;
    public List<Feat> feats = new();
    public Dictionary<string, int> skillRanks = new();
}