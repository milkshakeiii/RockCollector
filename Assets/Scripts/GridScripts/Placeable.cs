using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class Placeable 
{
    // sizeCategory is the width and height in cells if positive
    // if negative, the placeable takes up (1/sizeCategory) of a cell
    // sizeCategory 0 placeables take up no space
    protected int sizeCategory;

    protected bool claimed = false;

    public Placeable(int sizeCategory, bool claimed = false)
    {
        this.sizeCategory = sizeCategory;
        this.claimed = claimed;
    }

    public abstract string GetSpritePath();

    public int SquaresMinimumOne()
    {
        return Mathf.Max(1, sizeCategory);
    }

    public virtual void ObserveAndFeel(Map map)
    {
        
    }

    public virtual void ThinkAndPlan(Map map)
    {
        
    }

    public virtual void Act(Map map)
    {
        
    }

    public virtual void OnAdd(Map map)
    {
        
    }

    public void Claim()
    {
        claimed = true;
    }

    public void Unclaim()
    {
        claimed = false;
    }

    public bool IsClaimed()
    {
        return claimed;
    }

    public virtual Placeable DeepCopy()
    {
        throw new System.NotImplementedException("Deep copy on placeable parent class not implemented");
    }
}

public class PlaceableView 
{
    public static Dictionary<Placeable, PlaceableView> placeableViews = new();
    public static Dictionary<PlaceableView, Placeable> placeables = new();

    public static PlaceableView GetPlaceableView(Placeable placeable, Map map)
    {
        if (!placeableViews.ContainsKey(placeable))
        {
            if (placeable is Creature creature)
            {
                placeableViews[placeable] = new CreatureView(creature, map);
            }
            else if (placeable is Building building)
            {
                placeableViews[placeable] = new BuildingView(building, map);
            }
            else if (placeable is Prop prop)
            {
                placeableViews[placeable] = new PropView(prop, map);
            }
            else if (placeable is Item item)
            {
                placeableViews[placeable] = new ItemView(item, map);
            }
            else
            {
                throw new System.Exception("Placeable type not supported");
            }
            placeables[placeableViews[placeable]] = placeable;
        }
        return placeableViews[placeable];
    }

    public static Placeable GetPlaceable(PlaceableView placeableView)
    {
        if (!placeables.ContainsKey(placeableView))
        {
            throw new System.Exception("Placeable view not found");
        }
        return placeables[placeableView];
    }

    protected PlaceableView(Placeable placeable, Map map)
    {
        this.placeable = placeable;
        this.map = map;
    }

    protected readonly Placeable placeable;
    protected readonly Map map;

    public int SquaresMinimumOne()
    {
        return placeable.SquaresMinimumOne();
    }
}

public abstract class Destructable : Placeable
{
    protected int damageTaken;

    public Destructable(int sizeCategory) : base(sizeCategory)
    {
        this.damageTaken = 0;
    }

    public void TakeDamage(int damage)
    {
        damageTaken += damage;
        damageTaken = Mathf.Max(0, damageTaken);
    }

    public int GetDamageTaken()
    {
        return damageTaken;
    }

    public virtual bool IsDestroyed()
    {
        return HealthFraction() <= 0;
    }

    public virtual void OnDestroyed(Map map)
    {
        
    }

    public abstract float HealthFraction();

    public abstract int GetMaxHealth();
}

public abstract class DestructableView : PlaceableView
{
    public DestructableView(Destructable destructable, Map map) : base(destructable, map)
    {

    }

    public int GetDamageTaken()
    {
        return (placeable as Destructable).GetDamageTaken();
    }

    public bool IsDestroyed()
    {
        return (placeable as Destructable).IsDestroyed();
    }

    public float HealthFraction()
    {
        return (placeable as Destructable).HealthFraction();
    }
}

public enum AttributeScores
{
    STRENGTH,
    DEXTERITY,
    CONSTITUTION,
    INTELLIGENCE,
    WISDOM,
    CHARISMA
}

public class Creature : Destructable
{
    private string name;
    public int teamNumber = 0; // negative numbers denote CPU teams, positive denote human player teams

    private CreatureType creatureType;

    private Dictionary<AttributeScores, int> attributeScores;

    private int level = 0;
    private int experience = 0; // reset to 0 on level up
    private Dictionary<string, float> skillIncreases = new();
    private List<Feat> feats = new();
    private List<TypeAbility> abilities = new();
    private Dictionary<TypeAbility, int> ticksLastUsed = new();
    private List<Condition> conditions = new();

    private Vector2Int homePosition;

    private CreatureBehavior behavior;
    private Activity currentActivity;
    private Activity stagedActivity;
    private Thread newActivityComputation;

    private float cooldownTicksRemaining = 0;
    private int lastInterruptTick = 0;

    private string ingVerb = "";
    private string stagedIngVerb = "";

    public static Creature NewCreatureOfType(CreatureType type, int teamNumber, Vector2Int homePosition)
    {
        return new Creature("Random Name", teamNumber, type, homePosition);
    }

    public Creature(string name, int teamNumber, CreatureType creatureType, Vector2Int homePosition) : base(creatureType.GetSizeCategory())
    {
        this.name = name;
        this.teamNumber = teamNumber;
        this.creatureType = creatureType;
        this.homePosition = homePosition;

        this.behavior = CreatureBehavior.FromName(creatureType.GetBehavior());

        this.attributeScores = new Dictionary<AttributeScores, int>();
        foreach (AttributeScores score in Enum.GetValues(typeof(AttributeScores)))
        {
            attributeScores[score] = 8 + new DieRoll(1, 6).Roll();
        }

        LevelUp();
    }

    public override Placeable DeepCopy()
    {
        Creature copy = new(name, teamNumber, creatureType, homePosition);
        copy.name = name;
        copy.attributeScores = new Dictionary<AttributeScores, int>(attributeScores);
        copy.claimed = claimed; // from parent
        copy.damageTaken = damageTaken; // from parent
        copy.level = level;
        copy.feats = new List<Feat>(feats);
        copy.abilities = new List<TypeAbility>(abilities);
        copy.ticksLastUsed = new Dictionary<TypeAbility, int>(ticksLastUsed);
        copy.behavior = null;
        copy.currentActivity = null;
        copy.stagedActivity = null;
        copy.newActivityComputation = null;
        copy.cooldownTicksRemaining = cooldownTicksRemaining;
        copy.skillIncreases = new Dictionary<string, float>(skillIncreases);
        copy.experience = experience;
        copy.homePosition = homePosition;
        return copy;
    }

    public string GetName()
    {
        return name;
    }

    public override string GetSpritePath()
    {
        string folder = creatureType.GetTeamFolder();
        string name = creatureType.GetSpriteName();
        return System.IO.Path.Combine(folder, name);
    }

    public string GetActivityString()
    {
        if (currentActivity != null)
        {
            return ingVerb + " " + currentActivity.TargetDescriptiveString();
        }
        else
        {
            string noPreposition = ingVerb.Split(' ')[0];
            if (noPreposition == "Fleeing")
            {
                return "Cowering";
            }
            return noPreposition;
        }
    }

    public CreatureSelf GetSelfView(Map map)
    {
        return CreatureSelf.GetSelfView(this, map);
    }

    public int GetLevel()
    {
        return level;
    }

    public int GetExperience()
    {
        return experience;
    }

    /// <summary>
    /// Uses the first ability that is ready.
    /// </summary>
    /// <param name="map"></param>
    public void UseAnyAbility(Map map)
    {
        foreach (TypeAbility ability in abilities)
        {
            if (AbilityIsUp(ability, map))
            {
                UseAbility(ability, map);
                return;
            }
        }
    }

    public bool AbilityIsUp(TypeAbility ability, Map map)
    {
        bool recharched = TicksSinceLastUse(ability, map) >= ability.GetRechargeTicks();
        List<string> weaponSkills = ability.GetWeaponSkills();
        bool weaponRequirementsMet = weaponSkills.Count == 0;
        foreach (string weaponSkill in weaponSkills)
        {
            (DieRoll damage, int range) = WeaponBaseDamangeAndRange(weaponSkill, map);
            if (damage != null)
            {
                weaponRequirementsMet = true;
                break;
            }
        }
        return recharched && weaponRequirementsMet;
    }

    public void UseAbility(TypeAbility ability, Map map)
    {
        if (!AbilityIsUp(ability, map))
        {
            throw new System.Exception("Ability not useable. Check AbilityIsUp first.");
        }
        ticksLastUsed[ability] = map.CurrentTick();
        cooldownTicksRemaining = ability.GetCooldown();
        
        // Perform the ability
        Simulation.AbilityEffect(ability, this, map);
    }

    public int TicksSinceLastUse(TypeAbility ability, Map map)
    {
        if (!ticksLastUsed.ContainsKey(ability))
        {
            return int.MaxValue;
        }
        return map.CurrentTick() - ticksLastUsed[ability];
    }

    public int ExperienceForNextLevel()
    {
        return level * level;
    }

    public void GainExperience(int encounterLevel, string skillUsed)
    {
        if (teamNumber <= 0)
        {
            return; // only player creatures gain experience
        }
        experience += (encounterLevel - 9) * (encounterLevel - 9);
        if (!skillIncreases.ContainsKey(skillUsed))
        {
            skillIncreases[skillUsed] = 0;
        }
        skillIncreases[skillUsed] += Mathf.Max(0, Mathf.Log(Mathf.Max(encounterLevel - 8, 1)) / (skillIncreases[skillUsed] + 1));
        // Debug.Log(GetName() + " " + skillUsed + ": " + skillIncreases[skillUsed]);
    }

    public void ApplySkillIncrease(string skillName, int amount)
    {
        if (!skillIncreases.ContainsKey(skillName))
        {
            skillIncreases[skillName] = 0;
        }
        skillIncreases[skillName] += amount;
    }

    public int DifficultyEstimate()
    {
        return 10 + level;
    }

    public void LevelUp()
    {
        level++;
        List<TypeAbility> abilities = creatureType.GetAbilities();
        List<int> abilityLevels = creatureType.GetAbilityLevels();
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilityLevels[i] == level)
            {
                this.abilities.Add(abilities[i]);
            }
        }
    }

    public int SkillModifier(string skillName, Map map)
    {
        if (skillName == null)
        {
            throw new System.Exception("Skill name cannot be null");
        }
        int modifier = skillIncreases.ContainsKey(skillName) ? (int)skillIncreases[skillName] : 0;
        
        // check for bonuses from feats
        foreach (Feat feat in feats)
        {
            if (feat.GetSkillBonusName().Equals(skillName))
            {
                modifier += feat.GetSkillBonus();
            }
        }
        
        // check for bonuses from items
        foreach (Placeable placeable in map.HeldPlaceablesOf(this))
        {
            if (placeable is Item item)
            {
                List<string> skillBonuses = item.itemType.GetSkillBonusNames(); 
                List<int> skillBonusAmounts = item.itemType.GetSkillBonusAmounts();
                for (int i = 0; i < skillBonuses.Count; i++)
                {
                    if (skillBonuses[i].Equals(skillName))
                    {
                        modifier += skillBonusAmounts[i];
                    }
                }
            }
        }
        
        // check for bonuses or penalties from conditions
        foreach (Condition condition in conditions)
        {
            modifier += condition.GetSkillModifier(skillName);
        }

        // add attribute modifier
        if (!EntityManager.skillsToAttributeScores.ContainsKey(skillName))
        {
            throw new System.Exception("Key attribute score not found for skill: " + skillName);
        }
        string attributeName = EntityManager.skillsToAttributeScores[skillName];
        AttributeScores score = EntityManager.ParseAttributeScore(attributeName);
        modifier += GetAttributeModifier(score);

        return modifier;
    }

    public List<string> ListSkills()
    {
        List<string> skills = new(skillIncreases.Keys);
        return skills;
    }

    /// <summary>
    /// Returns -1 if the creature does not have a crafting ability for the skill.
    /// </summary>
    /// <param name="skillName"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public int CraftingSkillModifier(string skillName, Map map)
    {
        if (skillName == "none")
        {
            return -1;
        }
        int modifier = SkillModifier(skillName, map);
        // check if the creature has a crafting ability and whether it boosts the skill
        bool hasCraftingAbility = false;
        foreach (TypeAbility ability in abilities)
        {
            if (ability.GetCraftingSkill() == skillName)
            {
                modifier += ability.GetCraftingSkillBonus();
                hasCraftingAbility = true;
            }
        }
        if (!hasCraftingAbility)
        {
            return -1;
        }
        return modifier;
    }

    /// <summary>
    /// Returns -1 if the creature does not have a planting ability for the skill.
    /// </summary>
    /// <param name="skillName"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public int PlantingSkillModifier(string skillName, Map map)
    {
        if (skillName == "none")
        {
            return -1;
        }
        int modifier = SkillModifier(skillName, map);
        // check if the creature has a planting ability and whether it boosts the skill
        bool hasPlantingAbility = false;
        foreach (TypeAbility ability in abilities)
        {
            if (ability.GetPlantingSkill() == skillName)
            {
                modifier += ability.GetPlantingSkillBonus();
                hasPlantingAbility = true;
            }
        }
        if (!hasPlantingAbility)
        {
            return -1;
        }
        return modifier;
    }

    public static int Roll20()
    {
        return UnityEngine.Random.Range(1, 21);
    }

    public bool RollForSuccess(int difficulty, string skillName, Map map)
    {
        int roll = Roll20();
        int modifier = SkillModifier(skillName, map);
        return roll + modifier >= difficulty || roll == 20;
    }

    public bool RollForPlantingSuccess(int difficulty, string skillName, Map map)
    {
        int roll = Roll20();
        int modifier = PlantingSkillModifier(skillName, map);
        return roll + modifier >= difficulty || roll == 20;
    }

    public int RollForMultiplier(int difficulty, string skillName, Map map)
    {
        int roll = Roll20();
        int modifier = SkillModifier(skillName, map);
        int multiplier = 0;
        if (roll + modifier >= difficulty)
        {
            multiplier += 1;
        }
        if (roll + modifier >= difficulty + 10)
        {
            multiplier += 1;
        }
        if (roll == 20)
        {
            multiplier += 1;
        }
        return multiplier;
    }

    public void HarvestProp(Prop prop, Map map)
    {
        // use the best ability to harvest the prop
        (int cooldown, int bestAmount) = HarvestingCooldownAndAmount(prop, map);

        // creatures should not be instructed to harvest props they cannot harvest
        // (activity impossibility should be checked before this)
        if (bestAmount == 0)
        {
            throw new System.Exception("Creature " + name + " cannot harvest prop " + prop.propType.GetName());
        }

        // set cooldown
        cooldownTicksRemaining = cooldown;

        // roll for success or failure
        bool success = RollForSuccess(prop.propType.GetHarvestingDifficulty(), prop.propType.GetHarvestingSkill(), map);
        
        if (success)
        {
            // apply the harvest and if this was the last hit, gain experience
            prop.TakeHarvest(bestAmount);
            int encounterLevel = prop.propType.GetHarvestingDifficulty();
            if (prop.IsDestroyed())
            {
                GainExperience(encounterLevel, prop.propType.GetHarvestingSkill());
            }
        }
    }

    public void PlantProp(PropType propType, Vector2Int targetPosition, Map map)
    {
        // use the best ability to plant the prop
        int bestCooldown = 0;
        foreach (TypeAbility ability in abilities)
        {
            if (ability.GetPlantingSkill() == propType.GetPlantingSkill())
            {
                int cooldown = ability.GetCooldown();
                if (bestCooldown == 0 || cooldown < bestCooldown)
                {
                    bestCooldown = cooldown;
                }
            }
        }
        // set cooldown
        cooldownTicksRemaining = bestCooldown;

        // roll for success or failure
        bool success = RollForPlantingSuccess(propType.GetPlantingDifficulty(), propType.GetPlantingSkill(), map);

        if (success)
        {
            // plant and gain experience
            Prop prop = new (propType);
            map.Add(prop, targetPosition);
            GainExperience(propType.GetPlantingDifficulty(), propType.GetPlantingSkill());
        }
    }

    public void MeleeStrike(Creature target, DieRoll damage, string weaponSkill, Map map)
    {
        int toHit = SkillModifier(weaponSkill, map);
        int toHitResult = Roll20() + toHit;
        if (!target.Defend(toHitResult, map))
        {
            int damageAmount = damage.Roll();
            target.TakeDamage(damageAmount);
            GainExperience(target.DifficultyEstimate(), weaponSkill);
        }
    }

    public void RangedStrike(Creature target, DieRoll damage, string weaponSkill, Map map)
    {
        int toHit = SkillModifier(weaponSkill, map);
        int toHitResult = Roll20() + toHit;
        if (!target.Dodge(toHitResult, map))
        {
            int damageAmount = damage.Roll();
            target.TakeDamage(damageAmount);
            GainExperience(target.DifficultyEstimate(), weaponSkill);
        }
    }

    public void NonweaponStrike(Creature target, DieRoll damage, string skillName, Map map)
    {
        int toHit = SkillModifier(skillName, map);
        int toHitResult = Roll20() + toHit;
        if (!target.Resist(toHitResult, map))
        {
            int damageAmount = damage.Roll();
            target.TakeDamage(damageAmount);
            GainExperience(target.DifficultyEstimate(), skillName);
        }
    }

    public bool Dodge(int toHitResult, Map map)
    {
        return BlockWithSkill("dodge", toHitResult, map);
    }

    public bool Defend(int toHitResult, Map map)
    {
        return BlockWithSkill("defense", toHitResult, map);
    }

    public bool Resist(int toHitResult, Map map)
    {
        return BlockWithSkill("resistance", toHitResult, map);
    }

    private bool BlockWithSkill(string skill, int toHitResult, Map map)
    {
        int blockSkill = SkillModifier(skill, map);
        bool blocked = toHitResult <= 10 + blockSkill;
        if (blocked)
        {
            GainExperience(toHitResult, skill);
        }
        return blocked;
    }

    public void RepairBuilding(Building building, Map map)
    {
        (int cooldown, int bestAmount) = RepairCooldownAndAmount(map);

        // set cooldown
        cooldownTicksRemaining = cooldown;

        // roll for success or failure
        int encounterLevel = 10;
        bool success = RollForSuccess(encounterLevel, "repair", map);

        if (success)
        {
            // apply the repair and gain experience
            building.TakeDamage(-bestAmount);
            GainExperience(encounterLevel, "repair");
        }
    }

    public (int, int) HarvestingCooldownAndAmount(Prop prop, Map map)
    {
        string neededSkill = prop.propType.GetHarvestingSkill();
        int bestCooldown = 0;
        int bestAmount = 0;
        foreach (TypeAbility ability in abilities)
        {
            if (ability.GetHarvestingSkill() == neededSkill)
            {
                int cooldown = ability.GetCooldown();
                if (!prop.propType.GetRequiresImplement())
                {
                    int harvestAmount = ability.GetHarvestingAmount();
                    if (bestCooldown == 0 || harvestAmount / cooldown > bestAmount / bestCooldown)
                    {
                        bestAmount = harvestAmount;
                        bestCooldown = cooldown;
                    }
                }
                else
                {
                    foreach (Placeable placeable in map.HeldPlaceablesOf(this))
                    {
                        if (placeable is Item item && item.itemType.GetHarvestingSkills().Contains(neededSkill))
                        {
                            int harvestAmount = item.itemType.GetHarvestingAmount();
                            if (bestCooldown == 0 || harvestAmount / cooldown > bestAmount / bestCooldown)
                            {
                                bestAmount = harvestAmount;
                                bestCooldown = cooldown;
                            }
                        }
                    }
                }
            }
        }
        return (bestAmount, bestAmount);
    }

    public (int, int) RepairCooldownAndAmount(Map map)
    {
        int bestCooldown = 0;
        int bestAmount = 0;
        foreach (TypeAbility ability in abilities)
        {
            List<string> implementSkills = ability.GetRepairImplementSkills();
            int cooldown = ability.GetCooldown();
            foreach (string implementSkill in implementSkills)
            {
                foreach (Placeable placeable in map.HeldPlaceablesOf(this))
                {
                    if (placeable is Item item && item.itemType.GetWeaponSkill() == implementSkill)
                    {
                        int repairAmount = item.itemType.GetRepairAmount();
                        if (bestCooldown == 0 || repairAmount / cooldown > bestAmount/bestCooldown)
                        {
                            bestAmount = repairAmount;
                            bestCooldown = cooldown;
                        }
                    }
                }
            }
        }
        return (bestCooldown, bestAmount);
    }

    /// <summary>
    /// Returns null damage and 0 range if no weapon is held with the given skill.
    /// </summary>
    /// <param name="weaponSkill"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public (DieRoll, int) WeaponBaseDamangeAndRange(string weaponSkill, Map map)
    {
        DieRoll damage = null;
        int range = 0;

        List<Placeable> heldItems = map.HeldPlaceablesOf(this);

        foreach (Placeable placeable in heldItems)
        {
            if (placeable is Item item)
            {
                if (item.itemType.GetWeaponSkill() == weaponSkill)
                {
                    DieRoll thisDamage = item.itemType.GetWeaponDamage();
                    int thisRange = item.itemType.GetWeaponRange();
                    if (damage == null || thisDamage.ExpectedValue() > damage.ExpectedValue())
                    {
                        damage = thisDamage;
                        range = thisRange;
                    }
                }
            }
        }

        return (damage, range);
    }

    public List<string> GetPreferredWeaponSkills()
    {
        List<string> preferredSkills = new();
        foreach (TypeAbility ability in abilities)
        {
            preferredSkills.AddRange(ability.GetWeaponSkills());
        }
        return preferredSkills;
    }

    public TypeAbility BestHarvestingAbility(string skill)
    {
        TypeAbility bestAbility = null;
        int bestCooldown = 0;
        foreach (TypeAbility ability in abilities)
        {
            if (ability.GetHarvestingSkill() == skill)
            {
                if (bestCooldown == 0 || ability.GetCooldown() < bestCooldown)
                {
                    bestAbility = ability;
                    bestCooldown = ability.GetCooldown();
                }
            }
        }
        return bestAbility;
    }

    public CreatureType GetCreatureType()
    {
        return creatureType;
    }

    public CreatureBehavior GetCreatureBehavior()
    {
        return behavior;
    }

    public override void ObserveAndFeel(Map map)
    {
        if (experience >= ExperienceForNextLevel())
        {
            LevelUp();
            experience = 0;
        }

        List<Condition> removeConditions = new();
        foreach (Condition condition in conditions)
        {
            condition.Tick(this, map);
            if (condition.GetStacks() <= 0)
            {
                removeConditions.Add(condition);
            }
        }
        foreach (Condition condition in removeConditions)
        {
            conditions.Remove(condition);
        }
    }

    public override void ThinkAndPlan(Map map)
    {
        if (currentActivity == null && stagedActivity != null) // first check if a staged activity is ready
        {
            TryPromoteStagedActivity(map);
        }
        // if not, we might need to launch a new activity computation
        else if (currentActivity == null && (newActivityComputation == null || !newActivityComputation.IsAlive)) 
        {
            LaunchNewActivityComputation(map);
        }
        // otherwise, continue with the current activity
    }

    public override void Act(Map map)
    {
        if (cooldownTicksRemaining > 0)
        {
            // Nothing can be done while on cooldown
            float speedModifier = 0f;
            foreach (Condition condition in conditions)
            {
                speedModifier += condition.GetSpeedModifier();
            }

            cooldownTicksRemaining -= Mathf.Max(0.1f, 1f + speedModifier);
            return;
        }
        // If off cooldown and interrupt hasn't been performed too recently, check for interrupts
        if (map.CurrentTick() - lastInterruptTick > 1000)
        {
            bool interrupt = behavior.CheckInterrupts(map.GetView(), GetSelfView(map));
            if (interrupt)
            {
                AbandonCurrentActivity(map);
                lastInterruptTick = map.CurrentTick();
            }
        }
        // If an action was performed during the interrupt check, we may be on cooldown again
        if (cooldownTicksRemaining > 0)
        {
            return;
        }

        // Continue on to current activity
        if (currentActivity == null)
        {
            return;
        }
        else if (currentActivity.IsCompletedOrImpossible(map, this))
        {
            if (teamNumber > 0)
            {
                Debug.Log("Activity finished " + currentActivity);
            }
            // Something else completed the activity this frame
            // or there is no activity assigned
            AbandonCurrentActivity(map);
            return;
        }
        else if (currentActivity.DistanceTo(this, map) > currentActivity.ProximityRequirement(this, map))
        {
            MoveInDirection(DirectionToNextActivity(map), map);
            return;
        }
        else // Perform the activity
        {
            currentActivity.Perform(this, map);
            if (currentActivity.IsCompletedOrImpossible(map, this))
            {
                Debug.Log("Activity finished " + currentActivity);
                AbandonCurrentActivity(map); // Otherwise, there would be a "stunned" frame
            }
            return;
        }
    }

    private void TryPromoteStagedActivity(Map map)
    {
        currentActivity = stagedActivity;
        ingVerb = stagedIngVerb;
        // when we have a new activity, we need to mark the target placeables as claimed
        // new activities always are assigned here
        // Debug.Log("Activity promoted: " + currentActivity + " " + currentActivity.GetLocation(map));
        if (currentActivity.SuccessfulBackConversion(map) && currentActivity.TryClaimPlaceables(this, map))
        {
            stagedActivity = null;
        }
        else
        {
            try
            {
                Debug.Log("Activity not promoted: " + currentActivity + " " + currentActivity.GetLocation(this, map));
            }
            catch (System.Exception e)
            {
                Debug.Log("Activity not promoted: " + e.Message);
            }
            currentActivity = null;
            stagedActivity = null;
        }
    }

    private void LaunchNewActivityComputation(Map map)
    {
        if (newActivityComputation != null && newActivityComputation.IsAlive)
        {
            newActivityComputation.Abort();
        }
        (Map mapCopy, Dictionary<Placeable, Placeable> backDictionary, Creature newMe) = map.DeepCopy(this);
        newActivityComputation = new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true; 
            Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Lowest;
            (Activity bestActivity, string newIngVerb) = behavior.NextActivity(mapCopy, newMe);
            if (bestActivity != null)
            {
                bestActivity.MarkForBackConversion(backDictionary);
                this.stagedActivity = bestActivity;
                this.stagedIngVerb = newIngVerb;
            }
        });
        newActivityComputation.Start();
    }

    /// <summary>
    /// Ticks per square
    /// </summary>
    /// <returns></returns>
    public int MoveSpeed()
    {
        int moveSpeedModifier = 0;
        foreach (Condition condition in conditions)
        {
            moveSpeedModifier += condition.GetMoveSpeedModifier();
        }
        return Mathf.Max(1, 10 + moveSpeedModifier);
    }

    public void MoveInDirection(Vector2Int direction, Map map)
    {
        Vector2Int step = new(Math.Sign(direction.x), Math.Sign(direction.y));
        Vector2Int newPosition = map.PositionOf(this) + step;
        map.MovePlaceable(this, newPosition);
        cooldownTicksRemaining = MoveSpeed();
    }

    public void MoveTowards(Vector2Int target, Map map)
    {
        Vector2Int currentPosition = map.PositionOf(this);
        Vector2Int direction = target - currentPosition;
        MoveInDirection(direction, map);
    }

    public Vector2Int DirectionToNextActivity(Map map)
    {
        Vector2Int location = currentActivity.GetLocation(this, map);
        Vector2Int currentPosition = map.PositionOf(this);
        return location - currentPosition;
    }

    public void AbandonCurrentActivity(Map map)
    {
        if (currentActivity != null)
        {
            currentActivity.UnclaimPlaceables(this, map);
            currentActivity = null;
            stagedActivity = null;
        }
    }

    public override int GetMaxHealth()
    {
        int maxHealthModifier = 0;
        foreach (Condition condition in conditions)
        {
            maxHealthModifier += condition.GetMaxHealthModifier();
        }
        return Mathf.Max(1, creatureType.GetStartingHealth() + level * creatureType.GetHealthPerLevel() + maxHealthModifier);
    }

    public override float HealthFraction()
    {
        return (float)(GetMaxHealth() - damageTaken) / GetMaxHealth();
    }

    public void PickUp(Item item, Map map)
    {
        map.PickUp(this, item);
    }

    public void TransferItem(Item item, Placeable target, Map map)
    {
        if (!map.HeldPlaceablesOf(this).Contains(item))
        {
            throw new System.Exception("Creature does not hold item");
        }
        map.Transfer(item, target);
    }

    public List<TypeAbility> ListAbilities()
    {
        return new (abilities);
    }

    public bool HasCurrentActivity()
    {
        return currentActivity != null;
    }

    public void AddCooldown(int amount)
    {
        cooldownTicksRemaining += amount;
    }

    public Building GetHomeBuilding(Map map)
    {
        if (homePosition == Map.NULL_POSITION)
        {
            return null;
        }
        List<Placeable> placeables = map.PlaceablesAt(homePosition);
        foreach (Placeable placeable in placeables)
        {
            if (placeable is Building building)
            {
                return building;
            }
        }
        throw new System.Exception("Home building not found");
    }

    public void HomeBuildingDestroyed()
    {
        homePosition = Map.NULL_POSITION;
    }

    public bool HasRepairAbility()
    {
        foreach (TypeAbility ability in abilities)
        {
            if (ability.GetRepairImplementSkills().Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void AddToOutfit(Item item, Map map)
    {
        map.AddToOutfit(this, item);
    }

    public bool OutfitContains(Item item, Map map)
    {
        return map.IsInOutfit(this, item);
    }

    public int EquipmentLevel(string equipmentCategory, Map map)
    {
        int level = 0;
        foreach (Item item in map.OutfitOf(this))
        {
            if (item.itemType.GetEquipmentCategory() == equipmentCategory)
            {
                level = Mathf.Max(level, item.itemType.GetEquipmentLevel());
            }
        }
        return level;
    }

    public int GetAttributeScore(AttributeScores score)
    {
        int modifier = 0;
        foreach (Condition condition in conditions)
        {
            modifier += condition.GetAttributeModifier(score);
        }
        return GetUnmodifiedAttributeScore(score) + modifier;
    }

    private int GetUnmodifiedAttributeScore(AttributeScores score)
    {
        int baseScore = attributeScores[score];
        return score switch
        {
            AttributeScores.STRENGTH => baseScore + creatureType.GetStrengthBonus(),
            AttributeScores.DEXTERITY => baseScore + creatureType.GetDexterityBonus(),
            AttributeScores.CONSTITUTION => baseScore + creatureType.GetConstitutionBonus(),
            AttributeScores.INTELLIGENCE => baseScore + creatureType.GetIntelligenceBonus(),
            AttributeScores.WISDOM => baseScore + creatureType.GetWisdomBonus(),
            AttributeScores.CHARISMA => baseScore + creatureType.GetCharismaBonus(),
            _ => throw new System.Exception("Attribute score not found"),
        };
    }

    public int GetAttributeModifier(AttributeScores score)
    {
        return ((GetAttributeScore(score) / 2) - 5);
    }

    public override bool IsDestroyed()
    {
        foreach (AttributeScores score in Enum.GetValues(typeof(AttributeScores)))
        {
            if (GetAttributeScore(score) <= 0)
            {
                return true;
            }
        }
        return base.IsDestroyed();
    }

    public int GetConditionProtectionClass(Map map, ConditionType conditionType)
    {
        int result = 10 + GetLevel();
        foreach (Condition condition in conditions)
        {
            result += condition.GetConditionProtection();
        }
        foreach (Placeable heldPlaceable in map.HeldPlaceablesOf(this))
        {
            if (heldPlaceable is Item item)
            {
                result += item.itemType.GetConditionProtection();
            }
        }
        result += GetAttributeModifier(conditionType.GetProtectionAttribute());
        return result;
    }

    public void InflictConditionOn(Creature target, string skillName, ConditionType conditionType, int baseStacks, int maxStacks, Map map)
    {
        if (conditionType.GetIsHarmful())
        {
            int difficulty = target.GetConditionProtectionClass(map, conditionType);
            int toHit = SkillModifier(skillName, map);
            int toHitResult = Roll20() + toHit;
            if (toHitResult >= difficulty)
            {
                target.AddCondition(conditionType, baseStacks, maxStacks);
                GainExperience(target.DifficultyEstimate(), skillName);
            }
        }
        else
        {
            target.AddCondition(conditionType, baseStacks, maxStacks);
            GainExperience(this.DifficultyEstimate()-1, skillName);
        }
    }

    public void AddCondition(ConditionType conditionType, float stacks, float maxStacks)
    {
        foreach (Condition condition in conditions)
        {
            if (condition.conditionType == conditionType)
            {
                condition.AddStacks(stacks, maxStacks);
                return;
            }
        }
        conditions.Add(new Condition(conditionType, stacks));
    }

    public List<Condition> ListConditions()
    {
        return new (conditions);
    }
}

public class CreatureView : DestructableView
{
    public CreatureView(Creature creature, Map map) : base(creature, map)
    {
        
    }

    public int GetLevel()
    {
        return (placeable as Creature).GetLevel();
    }

    public int GetExperience()
    {
        return (placeable as Creature).GetExperience();
    }

    public int TicksSinceLastUse(TypeAbility ability)
    {
        return (placeable as Creature).TicksSinceLastUse(ability, map);
    }

    public bool AbilityIsUp(TypeAbility ability)
    {
        return (placeable as Creature).AbilityIsUp(ability, map);
    }

    public int ExperienceForNextLevel()
    {
        return (placeable as Creature).ExperienceForNextLevel();
    }

    public int EncounterLevel()
    {
        return (placeable as Creature).DifficultyEstimate();
    }

    public int SkillModifier(string skillName)
    {
        return (placeable as Creature).SkillModifier(skillName, map);
    }

    public (int, int) HarvestingCooldownAndAmount(Prop prop)
    {
        return (placeable as Creature).HarvestingCooldownAndAmount(prop, map);
    }

    public (DieRoll, int) WeaponDamangeAndRange(string weaponSkill)
    {
        return (placeable as Creature).WeaponBaseDamangeAndRange(weaponSkill, map);
    }

    public CreatureType GetCreatureType()
    {
        return (placeable as Creature).GetCreatureType();
    }

    public int MoveSpeed()
    {
        return (placeable as Creature).MoveSpeed();
    }

    public Vector2Int DirectionToCurrentActivity()
    {
        return (placeable as Creature).DirectionToNextActivity(map);
    }

    public int GetMaxHealth()
    {
        return (placeable as Creature).GetMaxHealth();
    }

    public List<TypeAbility> ListAbilities()
    {
        return (placeable as Creature).ListAbilities();
    
    }

    public int GetTeamNumber()
    {
        return (placeable as Creature).teamNumber;
    }

    public HuntActivity HuntActivity()
    {
        return new HuntActivity(placeable as Creature);
    }

    public PlaceableView GetHomeBuilding()
    {
        return PlaceableView.GetPlaceableView((placeable as Creature).GetHomeBuilding(map), map);
    }
}

public class CreatureSelf 
{
    public static Dictionary<Creature, CreatureSelf> selfViews = new();

    public static CreatureSelf GetSelfView(Creature creature, Map map)
    {
        if (!selfViews.ContainsKey(creature))
        {
            selfViews[creature] = new CreatureSelf(creature, map);
        }
        return selfViews[creature];
    }

    private CreatureSelf(Creature creature, Map map)
    {
        self = creature;
        this.map = map;
    }

    private readonly Creature self;
    private readonly Map map;

    public void MoveInDirection(Vector2Int direction)
    {
        self.MoveInDirection(direction, map);
    }

    public void MoveTowards(Vector2Int target)
    {
        self.MoveTowards(target, map);
    }

    public int GetLevel()
    {
        return self.GetLevel();
    }

    public int GetExperience()
    {
        return self.GetExperience();
    }

    public void UseAnyAbility()
    {
        self.UseAnyAbility(map);
    }

    public void UseAbility(TypeAbility ability)
    {
        self.UseAbility(ability, map);
    }

    public int TicksSinceLastUse(TypeAbility ability)
    {
        return self.TicksSinceLastUse(ability, map);
    }

    public bool AbilityIsUp(TypeAbility ability)
    {
        return self.AbilityIsUp(ability, map);
    }

    public int ExperienceForNextLevel()
    {
        return self.ExperienceForNextLevel();
    }

    public int EncounterLevel()
    {
        return self.DifficultyEstimate();
    }

    public int SkillModifier(string skillName)
    {
        return self.SkillModifier(skillName, map);
    }

    public void HarvestProp(Prop prop)
    {
        self.HarvestProp(prop, map);
    }

    public (int, int) HarvestingCooldownAndAmount(Prop prop)
    {
        return self.HarvestingCooldownAndAmount(prop, map);
    }

    public (DieRoll, int) WeaponDamangeAndRange(string weaponSkill)
    {
        return self.WeaponBaseDamangeAndRange(weaponSkill, map);
    }

    public CreatureType GetCreatureType()
    {
        return self.GetCreatureType();
    }

    public int MoveSpeed()
    {
        return self.MoveSpeed();
    }

    public Vector2Int DirectionToCurrentActivity()
    {
        return self.DirectionToNextActivity(map);
    }

    public void AbandonCurrentActivity()
    {
        self.AbandonCurrentActivity(map);
    }

    public int GetMaxHealth()
    {
        return self.GetMaxHealth();
    }

    public float HealthFraction()
    {
        return self.HealthFraction();
    }

    public void PickUp(Item item)
    {
        self.PickUp(item, map);
    }

    public void TransferItem(Item item, Placeable target)
    {
        self.TransferItem(item, target, map);
    }

    public List<TypeAbility> ListAbilities()
    {
        return self.ListAbilities();
    }

    public bool HasCurrentActivity()
    {
        return self.HasCurrentActivity();
    }

    public int GetTeamNumber()
    {
        return self.teamNumber;
    }

    public CreatureView View()
    {
        return (CreatureView)PlaceableView.GetPlaceableView(self, map);
    }

    public Building GetHomeBuilding()
    {
        return self.GetHomeBuilding(map);
    }
}

public class Building : Destructable
{
    public BuildingType buildingType;
    public int teamNumber = 0; // negative numbers denote CPU teams, positive denote human player teams
    
    private Dictionary<string, List<Creature>> creaturesByType = new();
    private Dictionary<ItemType, int> requestedItemAmounts = new();
    private int spawnTicksRemaining = 0;
    private Creature spawningCreature; // Null if not spawning

    private Dictionary<Vector2Int, int> bonusPropHarvestTicks = new();

    public Building(BuildingType buildingType, int teamNumber) : base(buildingType.GetSize())
    {
        this.buildingType = buildingType;
        this.teamNumber = teamNumber;
    }

    public override string GetSpritePath()
    {
        string folder = buildingType.GetTeamFolder();
        string name = buildingType.GetSpriteName();
        return System.IO.Path.Combine(folder, name);
    }

    public override Placeable DeepCopy()
    {
        Building copy = new (buildingType, teamNumber);
        copy.claimed = claimed; // from parent
        copy.damageTaken = damageTaken; // from parent
        copy.creaturesByType = null;
        copy.requestedItemAmounts = new Dictionary<ItemType, int>(requestedItemAmounts);
        copy.spawnTicksRemaining = spawnTicksRemaining;
        copy.spawningCreature = null;
        return copy;
    }

    public override int GetMaxHealth()
    {
        return buildingType.GetMaxHealth();
    }

    public override float HealthFraction()
    {
        return (float)(GetMaxHealth() - damageTaken) / GetMaxHealth();
    }

    public void StartSpawnCreature(CreatureType type, Map map)
    {
        if (spawningCreature != null)
        {
            return; // Already spawning
        }
        if (!buildingType.GetSupportedCreatureTypes().Contains(type))
        {
            throw new System.Exception("StartSpawnCreature creature type not supported");
        }
        if (CountCurrentCreaturesSupported(type) >= GetMaxCreaturesSupported(type))
        {
            return; // Max creatures already supported
        }
        if (!creaturesByType.ContainsKey(type.GetName()))
        {
            creaturesByType[type.GetName()] = new();
        }
        spawningCreature = Creature.NewCreatureOfType(type, teamNumber, map.PositionOf(this));
        creaturesByType[type.GetName()].Add(spawningCreature);
        spawnTicksRemaining = GetSpawnTime(type);
    }

    public List<CreatureType> GetCreatureTypesAvailable()
    {
        return buildingType.GetSupportedCreatureTypes();
    }

    public int CountCurrentCreaturesSupported(CreatureType type)
    {
        if (!buildingType.GetSupportedCreatureTypes().Contains(type))
        {
            throw new System.Exception("Creature type not supported");
        }
        if (!creaturesByType.ContainsKey(type.GetName()))
        {
            return 0;
        }
        return creaturesByType[type.GetName()].Count;
    }

    public int GetMaxCreaturesSupported(CreatureType type)
    {
        int index = IndexOfCreatureType(type);
        return buildingType.GetSupportedCreatureCounts()[index];
    }

    public int GetSpawnTime(CreatureType type)
    {
        int index = IndexOfCreatureType(type);
        return buildingType.GetSupportedCreatureSpawnTimes()[index];
    }

    public List<ItemType> GetRequestableItemTypes()
    {
        return buildingType.GetRequestableItemTypes();
    }

    public List<int> GetRequestedItemAmounts()
    {
        List<int> requestedItemAmounts = new();
        foreach (ItemType itemType in GetRequestableItemTypes())
        {
            requestedItemAmounts.Add(GetRequestedItemAmount(itemType));
        }
        return requestedItemAmounts;
    }

    public int GetRequestedItemAmount(ItemType itemType)
    {
        if (!requestedItemAmounts.ContainsKey(itemType))
        {
            return 0;
        }
        return requestedItemAmounts[itemType];
    }

    /// <summary>
    /// Returns the amount of items needed to fulfill the request, or 0 if the item type is not requested.
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public int GetMissingItemAmount(ItemType itemType, Map map)
    {
        if (!GetRequestableItemTypes().Contains(itemType))
        {
            return 0;
        }
        return Mathf.Max(0, GetRequestedItemAmount(itemType) - GetItemCount(itemType, map));
    }

    public List<int> GetRequestableItemAmounts()
    {
        return buildingType.GetRequestableItemCounts();
    }

    public int GetRequestableItemAmount(ItemType itemType)
    {
        if (!GetRequestableItemTypes().Contains(itemType))
        {
            throw new System.Exception("Item type not requestable");
        }
        int index = GetRequestableItemTypes().IndexOf(itemType);
        if (GetRequestableItemAmounts().Count <= index)
        {
            throw new System.Exception("Missing requestable item amount");
        }
        return GetRequestableItemAmounts()[index];
    }

    public void ChangeRequestedItemAmount(ItemType itemType, int amount)
    {
        amount = Mathf.Max(0, amount);
        amount = Mathf.Min(amount, GetRequestableItemAmount(itemType));
        requestedItemAmounts[itemType] = amount;
    }

    public void SpendStoredItems(ItemType itemType, int amountChange, Map map)
    {
        int currentAmount = GetItemCount(itemType, map);
        if (currentAmount < amountChange)
        {
            throw new System.Exception("Not enough stored items");
        }
        List<Placeable> items = map.HeldPlaceablesOf(this).FindAll(item => (item as Item).itemType == itemType);
        for (int i = 0; i < amountChange; i++)
        {
            map.Remove(items[i]);
        }
    }

    private int IndexOfCreatureType(CreatureType type)
    {
        List<CreatureType> supportedTypes = GetCreatureTypesAvailable();
        int index = supportedTypes.IndexOf(type);
        if (index == -1)
        {
            throw new System.Exception("Creature type not supported");
        }
        return index;
    }

    public override void ObserveAndFeel(Map map)
    {
        spawnTicksRemaining--;
        if (spawnTicksRemaining == 0)
        {
            // Place the creature
            map.Add(spawningCreature, map.PositionOf(this) - new Vector2Int(1, 1));

            // Add starting equipment
            foreach (ItemType itemType in spawningCreature.GetCreatureType().GetStartingEquipment())
            {
                map.AddHeld(spawningCreature, new Item(itemType));
            }

            spawningCreature = null;
        }

        // Check for dead creatures
        foreach (List<Creature> creatures in creaturesByType.Values)
        {
            List<Creature> deadCreatures = new();
            foreach (Creature creature in creatures)
            {
                if (creature.IsDestroyed())
                {
                    deadCreatures.Add(creature);
                }
            }
            foreach (Creature deadCreature in deadCreatures)
            {
                creatures.Remove(deadCreature);
            }
        }
    }

    public override void ThinkAndPlan(Map map)
    {
        // if this is a CPU team, we may need to spawn a creature
        if (teamNumber < 0)
        {
            if (!IsSpawning())
            {
                List<CreatureType> availableTypes = GetCreatureTypesAvailable();
                foreach (CreatureType type in availableTypes)
                {
                    if (CountCurrentCreaturesSupported(type) < GetMaxCreaturesSupported(type))
                    {
                        StartSpawnCreature(type, map);
                        break;
                    }
                }
            }
        }

        // check for empty bonus prop positions
        List<PropType> bonusProps = buildingType.GetSupportedBonusProps();
        List<int> bonusPropXs = buildingType.GetSupportedBonusPropXs();
        List<int> bonusPropYs = buildingType.GetSupportedBonusPropYs();
        List<int> respawnTimes = buildingType.GetSupportedBonusPropRespawnTicks();
        for (int i = 0; i < bonusProps.Count; i++)
        {
            Vector2Int position = map.PositionOf(this) + new Vector2Int(bonusPropXs[i], bonusPropYs[i]);
            if (map.PlaceablesAt(position).Count == 0)
            {
                if (!bonusPropHarvestTicks.ContainsKey(position))
                {
                    bonusPropHarvestTicks[position] = respawnTimes[i];
                }
                else
                {
                    bonusPropHarvestTicks[position]--;
                    if (bonusPropHarvestTicks[position] == 0)
                    {
                        map.Add(new Prop(bonusProps[i]), position);
                        bonusPropHarvestTicks[position] = respawnTimes[i];
                    }
                    break;
                }
            }
        }
    }

    public bool IsSpawning()
    {
        return spawningCreature != null;
    }

    public float CreatureSpawnCompletion()
    {
        int spawnTime = GetSpawnTime(spawningCreature.GetCreatureType());
        float result = (float)(spawnTime - spawnTicksRemaining) / spawnTime;
        return Mathf.Clamp01(result);
    }

    public int GetItemCount(ItemType itemType, Map map)
    {
        int result = map.HeldPlaceablesOf(this).FindAll(item => (item as Item).itemType == itemType).Count;
        return result;
    }

    public override void OnDestroyed(Map map)
    {
        // set creatures free
        foreach (List<Creature> creatures in creaturesByType.Values)
        {
            foreach (Creature creature in creatures)
            {
                creature.HomeBuildingDestroyed();
            }
        }
    }

    public bool BuildBuildingInputMaterialsPresent(BuildingType buildingType, Map map)
    {
        List<ItemType> requiredItems = buildingType.GetConstructionItemTypes();
        List<int> requiredCounts = buildingType.GetConstructionItemAmounts();
        for (int i = 0; i < requiredItems.Count; i++)
        {
            if (GetItemCount(requiredItems[i], map) < requiredCounts[i])
            {
                return false;
            }
        }
        return true;
    }
}

public class BuildingView : DestructableView
{
    public BuildingView(Building building, Map map) : base(building, map)
    {
        
    }

    public List<CreatureType> GetCreatureTypesAvailable()
    {
        return (placeable as Building).GetCreatureTypesAvailable();
    }

    public int CountCurrentCreaturesSupported(CreatureType type)
    {
        return (placeable as Building).CountCurrentCreaturesSupported(type);
    }

    public int GetMaxCreaturesSupported(CreatureType type)
    {
        return (placeable as Building).GetMaxCreaturesSupported(type);
    }

    public int GetSpawnTime(CreatureType type)
    {
        return (placeable as Building).GetSpawnTime(type);
    }

    public List<ItemType> GetRequestableItemTypes()
    {
        return (placeable as Building).GetRequestableItemTypes();
    }

    public List<int> GetRequestedItemAmounts()
    {
        return (placeable as Building).GetRequestedItemAmounts();
    }

    public BuildingType GetBuildingType()
    {
        return (placeable as Building).buildingType;
    }
}

public class Prop : Destructable
{
    public PropType propType;

    private int harvestedAmount = 0;
    private int tickCreated = -1;

    public Prop(PropType propType) : base(propType.GetSize())
    {
        this.propType = propType;
    }

    public override string GetSpritePath()
    {
        string folder = propType.GetTeamFolder();
        string name = propType.GetSpriteName();
        return System.IO.Path.Combine(folder, name);
    }

    public override Placeable DeepCopy()
    {
        Prop copy = new (propType);
        copy.claimed = claimed; // from parent
        copy.damageTaken = damageTaken; // from parent
        copy.propType = propType;
        copy.harvestedAmount = harvestedAmount;
        return copy;
    }

    public override int GetMaxHealth()
    {
        return propType.GetMaxHealth();
    }

    public override float HealthFraction()
    {
        return (float)(GetMaxHealth() - damageTaken) / GetMaxHealth();
    }

    public float HarvestedFraction()
    {
        return (float)harvestedAmount / propType.GetHarvestingRequired();
    }

    public override bool IsDestroyed()
    {
        return damageTaken >= 100 || HarvestedFraction() >= 1;
    }

    public override void OnAdd(Map map)
    {
        
    }

    public override void OnDestroyed(Map map)
    {
        if (HarvestedFraction() >= 1)
        {
            // Drop items
            List<ItemType> droppedItems = propType.GetProducedItems();
            
            List<ItemType> droppedItemsMultipliedByProbability = new();
            List<int> probabilities = propType.GetProducedItemsProbabilities();
            for (int i = 0; i < droppedItems.Count; i++)
            {
                for (int j = 0; j < probabilities[i]; j++)
                {
                    droppedItemsMultipliedByProbability.Add(droppedItems[i]);
                }
            }

            ItemType droppedItem = droppedItemsMultipliedByProbability[UnityEngine.Random.Range(0, droppedItemsMultipliedByProbability.Count)];
            map.Add(new Item(droppedItem), map.PositionOf(this));

            // Replace with a new prop if applicable
            PropType growsInto = propType.GetGrowsInto();
            if (growsInto != null)
            {
                map.Add(new Prop(growsInto), map.PositionOf(this));
            }
        }
    }

    public float ChanceOfItemDrop(ItemType itemType)
    {
        List<ItemType> droppedItems = propType.GetProducedItems();
        List<int> probabilities = propType.GetProducedItemsProbabilities();
        int index = droppedItems.IndexOf(itemType);
        if (index == -1)
        {
            return 0;
        }
        return (float)probabilities[index] / (float)DroppedItemsProbabilitiesSum();
    }

    private int DroppedItemsProbabilitiesSum()
    {
        List<int> probabilities = propType.GetProducedItemsProbabilities();
        int sum = 0;
        foreach (int probability in probabilities)
        {
            sum += probability;
        }
        return sum;
    }

    public void TakeHarvest(int amount)
    {
        harvestedAmount += amount;
    }

    public override void ObserveAndFeel(Map map)
    {
        if (tickCreated == -1)
        {
            tickCreated = map.CurrentTick();
        }
        if (propType.GetGrowthTicks() > 0 && map.CurrentTick() - tickCreated >= propType.GetGrowthTicks())
        {
            // Grow
            PropType growsInto = propType.GetGrowsInto();
            Vector2Int position = map.PositionOf(this);
            map.Remove(this);
            map.Add(new Prop(growsInto), position);
        }
    }
}

public class PropView : DestructableView
{
    public PropView(Prop prop, Map map) : base(prop, map)
    {
        
    }

    public float HarvestedFraction()
    {
        return (placeable as Prop).HarvestedFraction();
    }

    public float ChanceOfItemDrop(ItemType itemType)
    {
        return (placeable as Prop).ChanceOfItemDrop(itemType);
    }

    public PropType GetPropType()
    {
        return (placeable as Prop).propType;
    }
}

public class Item : Placeable
{
    public ItemType itemType;
    private bool consumed = false;

    public Item(ItemType itemType) : base(itemType.GetSize())
    {
        this.itemType = itemType;
    }

    public override string GetSpritePath()
    {
        return itemType.GetSpritePath();
    }

    public override Placeable DeepCopy()
    {
        Item copy = new(itemType);
        copy.claimed = claimed; // from parent
        copy.itemType = itemType;
        copy.consumed = consumed;
        return copy;
    }

    public void Consume()
    {
        consumed = true;
    }

    public bool IsConsumed()
    {
        return consumed;
    }

    public float Value()
    {
        return 10;
    }
}

public class ItemView : PlaceableView
{
    public ItemView(Item item, Map map) : base(item, map)
    {
        
    }

    public bool IsConsumed()
    {
        return (placeable as Item).IsConsumed();
    }

    public float Value()
    {
        return (placeable as Item).Value();
    }
}
