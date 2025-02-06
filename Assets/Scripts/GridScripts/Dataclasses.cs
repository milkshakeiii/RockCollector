using NUnit.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public static class EntityManager 
{
    public static Dictionary<string, Feat> feats = new();
    public static Dictionary<string, TypeAbility> typeAbilities = new();
    public static Dictionary<string, ConditionType> conditions = new();
    public static Dictionary<string, CreatureType> creatureTypes = new();
    public static Dictionary<string, PropType> propTypes = new();
    public static Dictionary<string, ItemType> itemTypes = new();
    public static Dictionary<string, BuildingType> buildingTypes = new();
    public static Dictionary<string, CreatureBehaviorType> creatureBehaviors = new();

    public static Dictionary<string, string> skillsToAttributeScores = new();

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
        else if (identifier == "condition_types")
        {
            conditions[entity.attributes["name"]] = (ConditionType)entity;
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
        else if (identifier == "creature_behaviors")
        {
            creatureBehaviors[entity.attributes["name"]] = (CreatureBehaviorType)entity;
        }
        else
        {
            throw new System.Exception("Unrecognized entity identifier");
        }
    }

    public static void StoreMapEntry(string identifier, string key, string value)
    {
        if (identifier == "skills")
        {
            skillsToAttributeScores[key] = value;
        }
        else
        {
            throw new System.Exception("Unrecognized map identifier");
        }
    }

    public static AttributeScores ParseAttributeScore(string name)
    {
        bool parseSuccess = Enum.TryParse(name, true, out AttributeScores score);
        if (!parseSuccess)
        {
            throw new System.ArgumentException("Invalid attribute score name: " + name);
        }
        return score;
    }

    public static Entity CreateEntity(string identifier, string teamName, Dictionary<string, string> attributes)
    {
        if (identifier == "feats")
        {
            return new Feat (attributes, teamName);
        }
        else if (identifier == "type_abilities")
        {
            return new TypeAbility (attributes, teamName);
        }
        else if (identifier == "condition_types")
        {
            return new ConditionType (attributes, teamName);
        }
        else if (identifier == "prop_types")
        {
            return new PropType (attributes, teamName);
        }
        else if (identifier == "item_types")
        {
            return new ItemType (attributes, teamName);
        }
        else if (identifier == "creature_types")
        {
            return new CreatureType (attributes, teamName);
        }
        else if (identifier == "building_types")
        {
            return new BuildingType (attributes, teamName);
        }
        else if (identifier == "creature_behaviors")
        {
            return new CreatureBehaviorType (attributes, teamName);
        }
        else
        {
            throw new System.Exception("Unrecognized entity identifier");
        }
    }
        
    public static string GetStringAttribute(ReadOnlyDictionary<string, string> attributes, string key, string defaultValue = null)
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

    public static int GetIntAttribute(ReadOnlyDictionary<string, string> attributes, string key, int defaultValue = -1)
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

    public static float GetFloatAttribute(ReadOnlyDictionary<string, string> attributes, string key, float defaultValue = -1)
    {
        if (attributes.ContainsKey(key))
        {
            return float.Parse(attributes[key]);
        }
        if (defaultValue == -1)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static bool GetBoolAttribute(ReadOnlyDictionary<string, string> attributes, string key, bool defaultValue = false)
    {
        if (attributes.ContainsKey(key))
        {
            return bool.Parse(attributes[key]);
        }
        return defaultValue;
    }

    public static List<int> GetIntListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<int> defaultValue = null)
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

    public static List<string> GetStringListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<string> defaultValue = null)
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

    public static bool HasAttribute(ReadOnlyDictionary<string, string> attributes, string key)
    {
        return attributes.ContainsKey(key);
    }

    public static List<ItemType> GetItemListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<ItemType> defaultValue = null)
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

    public static List<ConditionType> GetConditionListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<ConditionType> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<ConditionType>(System.Array.ConvertAll(attributes[key].Split(','), x => conditions[x]));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static List<CreatureType> GetCreatureTypeListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<CreatureType> defaultValue = null)
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

    public static List<TypeAbility> GetTypeAbilityListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<TypeAbility> defaultValue = null)
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

    public static List<BuildingType> GetBuildingTypeListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<BuildingType> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<BuildingType>(System.Array.ConvertAll(attributes[key].Split(','), x => buildingTypes[x]));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static PropType GetPropTypeAttribute(ReadOnlyDictionary<string, string> attributes, string key, PropType defaultValue = null)
    {
        if (!attributes.ContainsKey(key))
        {
            return defaultValue;
        }
        return propTypes[attributes[key]];
    }

    public static List<PropType> GetPropTypeListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<PropType> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<PropType>(System.Array.ConvertAll(attributes[key].Split(','), x => propTypes[x]));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static DieRoll GetDieRollAttribute(ReadOnlyDictionary<string, string> attributes, string key, DieRoll defaultValue = null)
    {
        if (!attributes.ContainsKey(key))
        {
            return defaultValue;
        }
        string[] parts = attributes[key].Split('d');
        return new DieRoll(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    public static AttributeScores GetAttributeScoreAttribute(ReadOnlyDictionary<string, string> attributes, string key)
    {
        string name = GetStringAttribute(attributes, key);
        return ParseAttributeScore(name);
    }

    public static List<AttributeScores> GetAttributeScoresListAttribute(ReadOnlyDictionary<string, string> attributes, string key, List<AttributeScores> defaultValue = null)
    {
        if (attributes.ContainsKey(key))
        {
            return new List<AttributeScores>(System.Array.ConvertAll(attributes[key].Split(','), x => ParseAttributeScore(x)));
        }
        if (defaultValue == null)
        {
            throw new System.ArgumentNullException("No default value");
        }
        return defaultValue;
    }

    public static void ReadTeam(string teamName)
    {
        ReadEntity("feats", teamName);
        ReadEntity("type_abilities", teamName);
        ReadEntity("condition_types", teamName);
        ReadEntity("creature_types", teamName);
        ReadEntity("prop_types", teamName);
        ReadEntity("item_types", teamName);
        ReadEntity("building_types", teamName);
        ReadEntity("creature_behaviors", teamName);
        ReadMap("skills", teamName);
    }

    private static void ReadEntity(string identifier, string teamName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Teams/" + teamName + "/" + identifier);
        string[] lines = textAsset.text.Split('\n');
        Dictionary<string, string> currentItem = new();
        foreach (string line in lines)
        {
            if (line.Length==0 || line.Length==1)
            {
                StoreEntity(identifier, EntityManager.CreateEntity(identifier, teamName, currentItem));
                currentItem = new();
            }
            else
            {
                string[] parts = line.Split(':');
                currentItem[parts[0]] = parts[1][..^1];
            }
        }
    }

    private static void ReadMap(string identifier, string teamName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>("Teams/" + teamName + "/" + identifier);
        string[] lines = textAsset.text.Split('\n');
        foreach (string line in lines)
        {
            if (line.Length==0)
            {
                continue;
            }
            string[] parts = line.Split(':');
            StoreMapEntry(identifier, parts[0], parts[1][..^1]);
        }
    }
}

public class Entity 
{
    public readonly ReadOnlyDictionary<string, string> attributes;
    public string teamName;

    public string GetTeamFolder()
    {
        return "Teams/" + teamName + "/";
    }

    public Entity(Dictionary<string, string> attributes, string teamName)
    {
        this.attributes = new(attributes);
        this.teamName = teamName;
    }
}

public class Feat : Entity
{
    public Feat(Dictionary<string, string> attributes, string teamName) : base(attributes, teamName) {}

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public string GetSkillBonusName()
    {
        return EntityManager.GetStringAttribute(attributes, "skillBonusName", "");
    }
    public int GetSkillBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "skillBonus", 0);
    }
}

public class TypeAbility : Entity
{
    public TypeAbility(Dictionary<string, string> attributes, string teamName) : base(attributes, teamName) {}

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public int GetRange()
    {
        // in squares
        return EntityManager.GetIntAttribute(attributes, "range", 1);
    }
    public int GetEffectRadius()
    {
        return EntityManager.GetIntAttribute(attributes, "effectRadius", 0);
    }
    public int GetCooldown()
    {
        // in ticks
        return EntityManager.GetIntAttribute(attributes, "cooldown", 0);
    }
    public int GetRechargeTicks()
    {
        return EntityManager.GetIntAttribute(attributes, "rechargeTicks", 0);
    }
    public int GetDuration()
    {
        // in ticks
        return EntityManager.GetIntAttribute(attributes, "duration", 0);
    }
    public DieRoll GetDamage()
    {
        return EntityManager.GetDieRollAttribute(attributes, "damage", null);
    }
    public string GetSkill()
    {
        return EntityManager.GetStringAttribute(attributes, "skill", "none");
    }
    public int GetHeal()
    {
        // in health points
        return EntityManager.GetIntAttribute(attributes, "heal", 0);
    }
    public string GetHarvestingSkill()
    {
        return EntityManager.GetStringAttribute(attributes, "harvestingSkill", "none");
    }
    public int GetHarvestingAmount()
    {
        return EntityManager.GetIntAttribute(attributes, "harvestingAmount", 0);
    }
    public int GetEnemyTargets()
    {
        return EntityManager.GetIntAttribute(attributes, "enemyTargets", 0);
    }
    public List<string> GetWeaponSkills()
    {
        return EntityManager.GetStringListAttribute(attributes, "weaponSkills", new List<string>());
    }
    public List<string> GetRepairImplementSkills()
    {
        return EntityManager.GetStringListAttribute(attributes, "repairImplementSkills", new List<string>());
    }
    public string GetCraftingSkill()
    {
        return EntityManager.GetStringAttribute(attributes, "craftingSkill", "none");
    }
    public int GetCraftingSkillBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "craftingSkillBonus", 0);
    }
    public string GetPlantingSkill()
    {
        return EntityManager.GetStringAttribute(attributes, "plantingSkill", "none");
    }
    public int GetPlantingSkillBonus()
    {
        return EntityManager.GetIntAttribute(attributes, "plantingSkillBonus", 0);
    }
    public List<ConditionType> GetConditionsInflicted()
    {
        return EntityManager.GetConditionListAttribute(attributes, "conditionsInflicted", new List<ConditionType>());
    }
    public List<int> GetConditionsInflictedBaseStacks()
    {
        return EntityManager.GetIntListAttribute(attributes, "baseStacks", new List<int>());
    }
    public List<int> GetConditionsInflictedMaxStacks()
    {
        int conditionsCount = GetConditionsInflicted().Count;
        List<int> defaultList = new();
        for (int i = 0; i < conditionsCount; i++)
        {
            defaultList.Add(999);
        }
        return EntityManager.GetIntListAttribute(attributes, "maxStacks", defaultList);
    }
    public int GetAllyTargets()
    {
        return EntityManager.GetIntAttribute(attributes, "allyTargets", 0);
    }
    public string GetEffectSpriteName()
    {
        return EntityManager.GetStringAttribute(attributes, "effectSpritePath", "nosprite");
    }
    public string GetEffectSpritePath()
    {
        string folder = GetTeamFolder();
        string name = GetEffectSpriteName();
        return System.IO.Path.Combine(folder, name);
    }
}

public class ConditionType : Entity
{
    public ConditionType(Dictionary<string, string> attributes, string teamName) : base(attributes, teamName) {}

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
    public AttributeScores GetProtectionAttribute()
    {
        return EntityManager.GetAttributeScoreAttribute(attributes, "protectionAttribute");
    }
    public bool GetIsHarmful()
    {
        return EntityManager.GetBoolAttribute(attributes, "isHarmful", true);
    }

    public List<string> GetSkillBonusNames()
    {
        return EntityManager.GetStringListAttribute(attributes, "skillBonusNames", new List<string>());
    }

    public List<int> GetSkillBonusAmounts()
    {
        return EntityManager.GetIntListAttribute(attributes, "skillBonusAmounts", new List<int>());
    }

    public float GetSpeedModifier()
    {
        return EntityManager.GetFloatAttribute(attributes, "speedModifier", 0);
    }

    public int GetMoveSpeedModifier()
    {
        return EntityManager.GetIntAttribute(attributes, "moveSpeedModifier", 0);
    }

    public List<AttributeScores> GetModifiedAttributeScores()
    {
        return EntityManager.GetAttributeScoresListAttribute(attributes, "modifiedAttributeScores", new List<AttributeScores>());
    }

    public List<int> GetAttributeModifierAmounts()
    {
        return EntityManager.GetIntListAttribute(attributes, "attributeModifierAmounts", new List<int>());
    }

    public int GetMaxHealthModifier()
    {
        return EntityManager.GetIntAttribute(attributes, "maxHealthModifier", 0);
    }

    public int GetConditionProtection()
    {
        return EntityManager.GetIntAttribute(attributes, "conditionProtection", 0);
    }

    public int GetDuration()
    {
        return EntityManager.GetIntAttribute(attributes, "duration", 0);
    }

    public string GetSpriteName()
    {
        return EntityManager.GetStringAttribute(attributes, "spriteName", "nosprite");
    }

    public string GetSpritePath()
    {
        string folder = GetTeamFolder();
        string name = GetSpriteName();
        return System.IO.Path.Combine(folder, name);
    }
}

public class CreatureType : Entity
{
    public CreatureType(Dictionary<string, string> attributes, string teamName) : base(attributes, teamName) {}

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public string GetSpriteName()
    {
        return EntityManager.GetStringAttribute(attributes, "spriteName", "nosprite");
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
        return EntityManager.GetTypeAbilityListAttribute(attributes, "abilities", new List<TypeAbility>());
    }
    public List<TypeAbility> GetStartingAbilities()
    {
        return EntityManager.GetTypeAbilityListAttribute(attributes, "startingAbilities", new List<TypeAbility>());
    }
    public List<int> GetAbilityLevels()
    {
        return EntityManager.GetIntListAttribute(attributes, "abilityLevels");
    }
    public List<ItemType> GetDroppedItems()
    {
        return EntityManager.GetItemListAttribute(attributes, "droppedItems", new List<ItemType>());
    }
    public List<int> GetDroppedItemsProbabilities()
    {
        return EntityManager.GetIntListAttribute(attributes, "droppedItemsProbabilities", new List<int>());
    }
    public string GetBehavior()
    {
        return EntityManager.GetStringAttribute(attributes, "behavior");
    }
    public List<ItemType> GetStartingEquipment()
    {
        return EntityManager.GetItemListAttribute(attributes, "startingEquipment", new List<ItemType>());
    }
}

public class PropType : Entity
{
    public PropType(Dictionary<string, string> attributes, string teamName) : base(attributes, teamName) {}

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public int GetMaxHealth()
    {
        return EntityManager.GetIntAttribute(attributes, "maxHealth", 100);
    }
    public int GetSize()
    {
        return EntityManager.GetIntAttribute(attributes, "size");
    }
    public string GetHarvestingSkill()
    {
        return EntityManager.GetStringAttribute(attributes, "harvestingSkill");
    }
    public int GetHarvestingDifficulty()
    {
        return EntityManager.GetIntAttribute(attributes, "harvestingDifficulty");
    }
    public int GetHarvestingRequired()
    {
        return EntityManager.GetIntAttribute(attributes, "harvestingRequired", 0);
    }
    public List<ItemType> GetProducedItems()
    {
        return EntityManager.GetItemListAttribute(attributes, "producedItems", new List<ItemType>());
    }
    public List<int> GetProducedItemsProbabilities()
    {
        return EntityManager.GetIntListAttribute(attributes, "producedItemsProbabilities", new List<int>());
    }
    public int GetPlantingDifficulty()
    {
        return EntityManager.GetIntAttribute(attributes, "plantingDifficulty");
    }
    public int GetPlantingLevel()
    {
        return EntityManager.GetIntAttribute(attributes, "plantingLevel");
    }
    public string GetPlantingSkill()
    {
        return EntityManager.GetStringAttribute(attributes, "plantingSkill");
    }
    public int GetGrowthTicks()
    {
        return EntityManager.GetIntAttribute(attributes, "growthTicks", 0);
    }
    public PropType GetGrowsInto()
    {
        return EntityManager.GetPropTypeAttribute(attributes, "growsInto");
    }
    public PropType GetReplacedBy()
    {
        return EntityManager.GetPropTypeAttribute(attributes, "replacedBy");
    }
    public bool GetRequiresImplement()
    {
        return EntityManager.GetBoolAttribute(attributes, "requiresImplement", true);
    }
    public string GetSpriteName()
    {
        return EntityManager.GetStringAttribute(attributes, "spriteName", "nosprite");
    }
}

public class ItemType : Entity
{
    public ItemType(Dictionary<string, string> attributes, string teamName) : base(attributes, teamName) {}

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public string GetEquipmentCategory()
    {
        return EntityManager.GetStringAttribute(attributes, "equipmentCategory", "none");
    }
    public List<string> GetHarvestingSkills()
    {
        return EntityManager.GetStringListAttribute(attributes, "harvestingSkill", new List<string>());
    }
    public int GetHarvestingAmount()
    {
        return EntityManager.GetIntAttribute(attributes, "harvestingAmount", 0);
    }
    public int GetSize()
    {
        return EntityManager.GetIntAttribute(attributes, "size", 1);
    }
    public List<ItemType> GetCraftingInputs()
    {
        return EntityManager.GetItemListAttribute(attributes, "craftingInputs", new List<ItemType>());
    }
    public string GetCraftingSkill()
    {
        return EntityManager.GetStringAttribute(attributes, "craftingSkill", "none");
    }
    public int GetCraftingLevel()
    {
        return EntityManager.GetIntAttribute(attributes, "craftingLevel", 0);
    }
    public int GetCraftingTime()
    {
        return EntityManager.GetIntAttribute(attributes, "craftingTime", 0);
    }
    public DieRoll GetWeaponDamage()
    {
        return EntityManager.GetDieRollAttribute(attributes, "weaponDamage");
    }
    public int GetWeaponRange()
    {
        return EntityManager.GetIntAttribute(attributes, "weaponRange", 1);
    }
    public string GetWeaponSkill()
    {
        return EntityManager.GetStringAttribute(attributes, "weaponSkill", "");
    }
    public int GetRepairAmount()
    {
        return EntityManager.GetIntAttribute(attributes, "repairAmount", 0);
    }
    public List<string> GetSkillBonusNames()
    {
        return EntityManager.GetStringListAttribute(attributes, "skillBonusNames", new List<string>());
    }
    public List<int> GetSkillBonusAmounts()
    {
        return EntityManager.GetIntListAttribute(attributes, "skillBonusAmounts", new List<int>());
    }
    public int GetEquipmentLevel()
    {
        return EntityManager.GetIntAttribute(attributes, "equipmentLevel", 0);
    }
    public int GetConditionProtection()
    {
        return EntityManager.GetIntAttribute(attributes, "conditionProtection", 0);
    }
    public int GetConditionInfliction()
    {
        return EntityManager.GetIntAttribute(attributes, "conditionInfliction", 0);
    }
    public string GetSpriteName()
    {
        return EntityManager.GetStringAttribute(attributes, "spriteName", "nosprite");
    }
    public string GetSpritePath()
    {
        string folder = GetTeamFolder();
        string name = GetSpriteName();
        return System.IO.Path.Combine(folder, name);
    }
}

public class BuildingType : Entity
{
    public BuildingType(Dictionary<string, string> attributes, string teamName) : base(attributes, teamName) {}

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public int GetMaxHealth()
    {
        return EntityManager.GetIntAttribute(attributes, "maxHealth");
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
    public List<ItemType> GetCraftedItemTypes()
    {
        return EntityManager.GetItemListAttribute(attributes, "craftedItemTypes", new List<ItemType>());
    }
    public int GetRestHealAmount()
    {
        return EntityManager.GetIntAttribute(attributes, "restHealAmount", 1);
    }
    public int GetRestHealCooldown()
    {
        return EntityManager.GetIntAttribute(attributes, "restHealCooldown", 10);
    }
    public List<BuildingType> GetBuildableBuildingTypes()
    {
        return EntityManager.GetBuildingTypeListAttribute(attributes, "buildableBuildingTypes", new List<BuildingType>());
    }
    public List<ItemType> GetConstructionItemTypes()
    {
        return EntityManager.GetItemListAttribute(attributes, "constructionItemTypes", new List<ItemType>());
    }
    public List<int> GetConstructionItemAmounts()
    {
        return EntityManager.GetIntListAttribute(attributes, "constructionItemAmounts", new List<int>());
    }
    public List<PropType> GetPlantablePropTypes()
    {
        return EntityManager.GetPropTypeListAttribute(attributes, "plantablePropTypes", new List<PropType>());
    }
    public int GetPlantingZoneXMin()
    {
        return EntityManager.GetIntAttribute(attributes, "plantingZoneXMin", 0);
    }
    public int GetPlantingZoneXMax()
    {
        return EntityManager.GetIntAttribute(attributes, "plantingZoneXMax", 0);
    }
    public int GetPlantingZoneYMin()
    {
        return EntityManager.GetIntAttribute(attributes, "plantingZoneYMin", 0);
    }
    public int GetPlantingZoneYMax()
    {
        return EntityManager.GetIntAttribute(attributes, "plantingZoneYMax", 0);
    }
    public List<PropType> GetSupportedBonusProps()
    {
        return EntityManager.GetPropTypeListAttribute(attributes, "supportedBonusProps", new List<PropType>());
    }
    public List<int> GetSupportedBonusPropXs()
    {
        return EntityManager.GetIntListAttribute(attributes, "supportedBonusPropXs", new List<int>());
    }
    public List<int> GetSupportedBonusPropYs()
    {
        return EntityManager.GetIntListAttribute(attributes, "supportedBonusPropYs", new List<int>());
    }
    public List<int> GetSupportedBonusPropRespawnTicks()
    {
        return EntityManager.GetIntListAttribute(attributes, "supportedBonusPropRespawnTicks", new List<int>());
    }
    public string GetSpriteName()
    {
        return EntityManager.GetStringAttribute(attributes, "spriteName", "nosprite");
    }
    public int GetDifficultyEstimate()
    {
        return EntityManager.GetIntAttribute(attributes, "difficultyEstimate", 10);
    }
    public int GetDemolitionDifficulty()
    {
        return EntityManager.GetIntAttribute(attributes, "demolitionDifficulty", 10);
    }
}

public class CreatureBehaviorType : Entity
{
    public CreatureBehaviorType(Dictionary<string, string> attributes, string teamName) : base(attributes, teamName) { }

    public string GetName()
    {
        return EntityManager.GetStringAttribute(attributes, "name");
    }
    public List<string> GetPriorities()
    {
        return EntityManager.GetStringListAttribute(attributes, "priorities", new List<string>());
    }
    public int GetRepairRange()
    {
        return EntityManager.GetIntAttribute(attributes, "repairRange");
    }
    public int GetDropOffRange()
    {
        return EntityManager.GetIntAttribute(attributes, "dropOffRange");
    }
    public int GetPickUpRange()
    {
        return EntityManager.GetIntAttribute(attributes, "pickUpRange");
    }
    public int GetHarvestRange()
    {
        return EntityManager.GetIntAttribute(attributes, "harvestRange");
    }
    public int GetCraftRange()
    {
        return EntityManager.GetIntAttribute(attributes, "craftRange");
    }
    public int GetRestRange()
    {
        return EntityManager.GetIntAttribute(attributes, "restRange");
    }
    public int GetWanderRange()
    {
        return EntityManager.GetIntAttribute(attributes, "wanderRange", 1);
    }
    public int GetHuntRange()
    {
        return EntityManager.GetIntAttribute(attributes, "huntRange");
    }
    public int GetHuntLookDistance()
    {
        return EntityManager.GetIntAttribute(attributes, "huntLookDistance");
    }
    public int GetHuntMaximumLevelDifference()
    {
        return EntityManager.GetIntAttribute(attributes, "huntMaximumLevelDifference");
    }
    public int GetHuntMinimumLevelDifference()
    {
        return EntityManager.GetIntAttribute(attributes, "huntMinimumLevelDifference");
    }
    public List<string> GetEquipCategories()
    {
        return EntityManager.GetStringListAttribute(attributes, "equipCategories");
    }
    public int GetEquipRange()
    {
        return EntityManager.GetIntAttribute(attributes, "equipRange");
    }
    public int GetDangerLookDistance()
    {
        return EntityManager.GetIntAttribute(attributes, "dangerLookDistance", 0);
    }
    public int GetInterruptDangerThreshold()
    {
        return EntityManager.GetIntAttribute(attributes, "interruptDangerThreshold", 999);
    }
    public int GetFleeDangerThreshold()
    {
        return EntityManager.GetIntAttribute(attributes, "fleeDangerThreshold", 999);
    }
    public int GetFightDangerThreshold()
    {
        return EntityManager.GetIntAttribute(attributes, "fightDangerThreshold", 999);
    }
    public int GetPlantRange()
    {
        return EntityManager.GetIntAttribute(attributes, "plantRange");
    }
    public int GetRaidLookDistance()
    {
        return EntityManager.GetIntAttribute(attributes, "raidLookDistance");
    }

    public int GetRaidMaximumLevelDifference()
    {
        return EntityManager.GetIntAttribute(attributes, "raidMaximumLevelDifference");
    }

    public int GetRaidMinimumLevelDifference()
    {
        return EntityManager.GetIntAttribute(attributes, "raidMinimumLevelDifference");
    }

    public int GetRaidRange()
    {
        return EntityManager.GetIntAttribute(attributes, "raidRange");
    }
}

public class DieRoll 
{
    public int sides;
    public int rolls;

    public DieRoll(int sides, int rolls)
    {
        this.sides = sides;
        this.rolls = rolls;
    }

    public override string ToString()
    {
        return rolls + "d" + sides;
    }

    public float ExpectedValue()
    {
        return (sides + 1) * rolls / 2;
    }

    public int Roll()
    {
        System.Random random = new();
        return random.Next(sides, rolls * sides + 1);
    }
}