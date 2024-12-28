using NUnit.Framework;
using Steamworks;
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

    public static void StoreEntity(string identifier, Entity entity)
    {
        if (identifier == "feats")
        {
            feats[entity.attributes["name"]] = (Feat)entity;
        }
        else if (identifier == "type_abilities")
        {
            typeAbilities[entity.attributes["name"]] = (TypeAbility)entity;
        }
        else if (identifier == "conditions")
        {
            conditions[entity.attributes["name"]] = (Condition)entity;
        }
        else if (identifier == "props")
        {
            props[entity.attributes["name"]] = (Prop)entity;
        }
        else if (identifier == "items")
        {
            items[entity.attributes["name"]] = (Item)entity;
        }
        else if (identifier == "creature_types")
        {
            creatureTypes[entity.attributes["name"]] = (CreatureType)entity;
        }
        else
        {
            throw new System.Exception("Unrecognized entity identifier");
        }
    }

    public static Entity CreateEntity(string identifier, Dictionary<string, string> attributes)
    {
        if (identifier == "feats")
        {
            return new Feat { attributes = attributes };
        }
        else if (identifier == "type_abilities")
        {
            return new TypeAbility { attributes = attributes };
        }
        else if (identifier == "conditions")
        {
            return new Condition { attributes = attributes };
        }
        else if (identifier == "props")
        {
            return new Prop { attributes = attributes };
        }
        else if (identifier == "items")
        {
            return new Item { attributes = attributes };
        }
        else if (identifier == "creature_types")
        {
            return new CreatureType { attributes = attributes };
        }
        else
        {
            throw new System.Exception("Unrecognized entity identifier");
        }
    }

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
        ReadEntity("feats");
        ReadEntity("type_abilities");
        ReadEntity("conditions");
        ReadEntity("creature_types");
        ReadEntity("props");
        ReadEntity("items");
    }

    public static void ReadEntity(string identifier)
    {
        items = new();
        TextAsset textAsset = Resources.Load<TextAsset>(identifier);
        string[] lines = textAsset.text.Split('\n');
        Dictionary<string, string> currentItem = new();
        foreach (string line in lines)
        {
            if (line.Length==0 || line.Length==1)
            {
                StoreEntity(identifier, EntityManager.CreateEntity(identifier, currentItem));
                currentItem = new();
            }
            else
            {
                string[] parts = line.Split(':');
                currentItem[parts[0]] = parts[1][..^1];
            }
        }
    }
}

public class Entity
{
    public Dictionary<string, string> attributes;
}

public class Feat : Entity
{
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

public class TypeAbility : Entity
{
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

public class Condition : Entity
{
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

public class CreatureType : Entity
{
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

public class Prop : Entity
{
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

public class Item : Entity
{
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