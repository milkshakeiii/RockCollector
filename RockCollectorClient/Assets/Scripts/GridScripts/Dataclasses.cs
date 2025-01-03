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
    public static Dictionary<string, PropType> propTypes = new();
    public static Dictionary<string, ItemType> itemTypes = new();
    public static Dictionary<string, BuildingType> buildingTypes = new();

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
        else if (identifier == "prop_types")
        {
            propTypes[entity.attributes["name"]] = (PropType)entity;
        }
        else if (identifier == "item_types")
        {
            itemTypes[entity.attributes["name"]] = (ItemType)entity;
        }
        else if (identifier == "creature_types")
        {
            creatureTypes[entity.attributes["name"]] = (CreatureType)entity;
        }
        else if (identifier == "building_types")
        {
            buildingTypes[entity.attributes["name"]] = (BuildingType)entity;
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
        else if (identifier == "prop_types")
        {
            return new PropType { attributes = attributes };
        }
        else if (identifier == "item_types")
        {
            return new ItemType { attributes = attributes };
        }
        else if (identifier == "creature_types")
        {
            return new CreatureType { attributes = attributes };
        }
        else if (identifier == "building_types")
        {
            return new BuildingType { attributes = attributes };
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

    public static List<ItemType> GetItemListAttribute(Dictionary<string, string> attributes, string key, List<ItemType> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<ItemType>(System.Array.ConvertAll(attributes[key].Split(','), x => itemTypes[x]));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static List<CreatureType> GetCreatureTypeListAttribute(Dictionary<string, string> attributes, string key, List<CreatureType> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<CreatureType>(System.Array.ConvertAll(attributes[key].Split(','), x => creatureTypes[x]));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static List<Goal> GetGoalListAttribute(Dictionary<string, string> attributes, string key, List<Goal> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<Goal>(System.Array.ConvertAll(attributes[key].Split(','), x => Goal.NameToGoal(x)));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static List<TypeAbility> GetTypeAbilityListAttribute(Dictionary<string, string> attributes, string key, List<TypeAbility> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<TypeAbility>(System.Array.ConvertAll(attributes[key].Split(','), x => typeAbilities[x]));
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
        ReadEntity("prop_types");
        ReadEntity("item_types");
        ReadEntity("building_types");
    }

    public static void ReadEntity(string identifier)
    {
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
    public string GetHarvestingType()
    {
        return EntityManager.GetStringAttribute(attributes, "harvestingType");
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
    public int GetSizeCategory()
    {
        return EntityManager.GetIntAttribute(attributes, "sizeCategory", defaultValue: 1);
    }
    public List<TypeAbility> GetAbilities()
    {
        return EntityManager.GetTypeAbilityListAttribute(attributes, "abilities");
    }
    public List<int> GetAbilityLevels()
    {
        return EntityManager.GetIntListAttribute(attributes, "abilityLevels");
    }
    public List<Goal> GetGoals()
    {
        return EntityManager.GetGoalListAttribute(attributes, "goals");
    }
}

public class PropType : Entity
{
    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public int GetSize()
    {
        return EntityManager.GetIntAttribute(attributes, "size");
    }
    public string GetHarvestingType()
    {
        return EntityManager.GetStringAttribute(attributes, "harvestingType");
    }
    public int GetHarvestingRequired()
    {
        return EntityManager.GetIntAttribute(attributes, "harvestingRequired");
    }
    public List<ItemType> GetProducedItems()
    {
        return EntityManager.GetItemListAttribute(attributes, "producedItems", new List<ItemType>());
    }
    public List<int> GetProducedItemsProbabilities()
    {
        return EntityManager.GetIntListAttribute(attributes, "producedItemsProbabilities", new List<int>());
    }
}

public class ItemType : Entity
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

public class BuildingType : Entity
{
    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public List<CreatureType> GetSupportedCreatureTypes()
    {
        return EntityManager.GetCreatureTypeListAttribute(attributes, "supportedCreatureTypes", new List<CreatureType>());
    }
    public List<int> GetSupportedCreatureCounts()
    {
        return EntityManager.GetIntListAttribute(attributes, "supportedCreatureCounts", new List<int>());
    }
    public List<int> GetSupportedCreatureSpawnTimes()
    {
        return EntityManager.GetIntListAttribute(attributes, "supportedCreatureSpawnTimes", new List<int>());
    }
    public int GetSize()
    {
        return EntityManager.GetIntAttribute(attributes, "size");
    }
    public List<ItemType> GetRequestableItemTypes()
    {
        return EntityManager.GetItemListAttribute(attributes, "requestableItemTypes", new List<ItemType>());
    }
    public List<int> GetRequestableItemCounts()
    {
        return EntityManager.GetIntListAttribute(attributes, "requestableItemCounts", new List<int>());
    }
}