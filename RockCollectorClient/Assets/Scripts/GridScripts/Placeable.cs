using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Placeable 
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

public class Creature : Destructable
{
    private string name;
    public int teamNumber = 0;

    private int level = 0;
    private int experience = 0; // reset to 0 on level up
    private Dictionary<string, float> skillIncreases = new();
    private List<Feat> feats = new();
    private List<TypeAbility> abilities = new();
    private Dictionary<TypeAbility, int> ticksLastUsed = new();

    private CreatureType creatureType;

    private CreatureBehavior behavior;
    private Activity currentActivity;
    private Activity stagedActivity;
    private Thread newActivityComputation;

    private int cooldownTicksRemaining = 0;

    public static Creature NewCreatureOfType(CreatureType type)
    {
        return new Creature("Random Name", 0, type);
    }

    public Creature(string name, int teamNumber, CreatureType creatureType) : base(creatureType.GetSizeCategory())
    {
        this.name = name;
        this.teamNumber = teamNumber;
        this.creatureType = creatureType;

        this.behavior = CreatureBehavior.FromName(creatureType.GetBehavior());
        LevelUp();
    }

    public override Placeable DeepCopy()
    {
        Creature copy = new(name, teamNumber, creatureType);
        copy.claimed = claimed; // from parent
        copy.damageTaken = damageTaken; // from parent
        copy.level = level;
        copy.feats = new List<Feat>(feats);
        copy.abilities = new List<TypeAbility>(abilities);
        copy.behavior = null;
        copy.currentActivity = null;
        copy.newActivityComputation = null;
        copy.cooldownTicksRemaining = cooldownTicksRemaining;
        return copy;
    }

    public string GetName()
    {
        return name;
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
            (DieRoll damage, int range) = WeaponDamangeAndRange(weaponSkill, map);
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
        experience += (encounterLevel - 9) * (encounterLevel - 9);
        if (!skillIncreases.ContainsKey(skillUsed))
        {
            skillIncreases[skillUsed] = 0;
        }
        skillIncreases[skillUsed] += Mathf.Max(0, Mathf.Log(Mathf.Max(encounterLevel - 8, 1)) / (skillIncreases[skillUsed] + 1));
        // Debug.Log(GetName() + " " + skillUsed + ": " + skillIncreases[skillUsed]);
    }

    public int EncounterLevel()
    {
        return 10 + level;
    }

    public void LevelUp()
    {
        level++;
        if (level == 1 || level % 3 == 0)
        {
            // Add a feat
        }
        if (level == 1 || level % 5 == 0)
        {
            // Add an ability
            TypeAbility ability = creatureType.GetAbilities()[0];
            abilities.Add(ability); 
        }
    }

    public int SkillModifier(string skillName)
    {
        if (skillName == null)
        {
            throw new System.Exception("Skill name cannot be null");
        }
        int modifier = GetLevel();
        foreach (Feat feat in feats)
        {
            if (feat.GetSkillBonusName().Equals(skillName))
            {
                modifier += feat.GetSkillBonus();
            }
        }
        return modifier;
    }

    public static int Roll20()
    {
        return UnityEngine.Random.Range(1, 21);
    }

    public bool RollForSuccess(int difficulty, string skillName)
    {
        int roll = Roll20();
        int modifier = SkillModifier(skillName);
        return roll + modifier >= difficulty || roll == 20;
    }

    public int RollForMultiplier(int difficulty, string skillName)
    {
        int roll = Roll20();
        int modifier = SkillModifier(skillName);
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
        (int cooldown, int bestAmount) = HarvestingCooldownAndAmount(prop);

        // creatures should not be instructed to harvest props they cannot harvest
        // (activity impossibility should be checked before this)
        if (bestAmount == 0)
        {
            throw new System.Exception("Creature " + name + " cannot harvest prop " + prop.propType.GetName());
        }

        // set cooldown
        cooldownTicksRemaining = cooldown;

        // roll for success or failure
        bool success = RollForSuccess(prop.propType.GetHarvestingDifficulty(), prop.propType.GetHarvestingSkill());
        
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

    public void Strike(Creature target, DieRoll damage, int toHit, string weaponSkill, Map map)
    {
        int toHitResult = Roll20() + toHit;
        if (!target.Defend(toHitResult))
        {
            int damageAmount = damage.Roll();
            target.TakeDamage(damageAmount);
            GainExperience(target.EncounterLevel(), weaponSkill);
        }
    }

    public bool Defend(int toHitResult)
    {
        int defenseSkill = SkillModifier("defense");
        bool defended = toHitResult <= 10 + defenseSkill;
        if (defended)
        {
            GainExperience(toHitResult, "defense");
        }
        return defended;
    }

    public (int, int) HarvestingCooldownAndAmount(Prop prop)
    {
        string neededSkill = prop.propType.GetHarvestingSkill();
        TypeAbility bestAbility = BestHarvestingAbility(neededSkill);
        if (bestAbility == null)
        {
            return (0, 0);
        }
        int bestAmount = bestAbility.GetHarvestingAmount();
        int cooldown = bestAbility.GetCooldown();
        return (cooldown, bestAmount);
    }

    /// <summary>
    /// Returns null damage and 0 range if no weapon is held with the given skill.
    /// </summary>
    /// <param name="weaponSkill"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public (DieRoll, int) WeaponDamangeAndRange(string weaponSkill, Map map)
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
        int bestAmount = 0;
        foreach (TypeAbility ability in abilities)
        {
            if (ability.GetHarvestingSkill() == skill)
            {
                if (ability.GetHarvestingAmount() > bestAmount)
                {
                    bestAbility = ability;
                    bestAmount = ability.GetHarvestingAmount();
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
            cooldownTicksRemaining--;
            return;
        }
        // If off cooldown, check for short-term interrupts to perform instead of the current activity
        behavior.CheckInterrupts(map.GetView(), GetSelfView(map));
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
            Debug.Log("Activity impossible");
            // Something else completed the activity this frame
            // or there is no activity assigned
            AbandonCurrentActivity();
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
                // Debug.Log("Activity completed");
                AbandonCurrentActivity(); // Otherwise, there would be a "stunned" frame
            }
            return;
        }
    }

    private void TryPromoteStagedActivity(Map map)
    {
        currentActivity = stagedActivity;
        if (currentActivity.SuccessfulBackConversion(map) && !currentActivity.IsSourcePlaceableClaimed())
        {
            stagedActivity = null;
            // when we have a new activity, we need to mark the target placeable as claimed
            // new activities always are assigned here
            // Debug.Log("Activity promoted: " + currentActivity + " " + currentActivity.GetLocation(map));
            currentActivity.MarkSourcePlaceableClaimed();
        }
        else
        {
            try
            {
                Debug.Log("Activity not promoted: " + currentActivity + " " + currentActivity.GetLocation(map));
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
            Activity bestActivity = behavior.NextActivity(mapCopy, newMe);
            if (bestActivity != null)
            {
                bestActivity.MarkForBackConversion(backDictionary);
                this.stagedActivity = bestActivity;
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
        return 10;
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
        Vector2Int location = currentActivity.GetLocation(map);
        Vector2Int currentPosition = map.PositionOf(this);
        return location - currentPosition;
    }

    public void AbandonCurrentActivity()
    {
        if (currentActivity != null)
        {
            currentActivity.MarkSourcePlaceableUnclaimed();
            currentActivity = null;
            stagedActivity = null;
        }
    }

    public int GetMaxHealth()
    {
        return creatureType.GetStartingHealth() + level * creatureType.GetHealthPerLevel();
    }

    public override float HealthFraction()
    {
        return (float)(GetMaxHealth() - damageTaken) / GetMaxHealth();
    }

    public int MoveAndEstimate(Activity activity, Map map)
    {
        int distance = activity.DistanceTo(this, map);
        int ticksPerSquare = MoveSpeed();
        int estimatedTicks = distance * ticksPerSquare;
        map.MovePlaceable(this, activity.GetLocation(map) - (Vector2Int.one * activity.ProximityRequirement(this, map)));
        return estimatedTicks;
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
        return (placeable as Creature).EncounterLevel();
    }

    public int SkillModifier(string skillName)
    {
        return (placeable as Creature).SkillModifier(skillName);
    }

    public (int, int) HarvestingCooldownAndAmount(Prop prop)
    {
        return (placeable as Creature).HarvestingCooldownAndAmount(prop);
    }

    public (DieRoll, int) WeaponDamangeAndRange(string weaponSkill)
    {
        return (placeable as Creature).WeaponDamangeAndRange(weaponSkill, map);
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
        return self.EncounterLevel();
    }

    public int SkillModifier(string skillName)
    {
        return self.SkillModifier(skillName);
    }

    public void HarvestProp(Prop prop)
    {
        self.HarvestProp(prop, map);
    }

    public (int, int) HarvestingCooldownAndAmount(Prop prop)
    {
        return self.HarvestingCooldownAndAmount(prop);
    }

    public (DieRoll, int) WeaponDamangeAndRange(string weaponSkill)
    {
        return self.WeaponDamangeAndRange(weaponSkill, map);
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
        self.AbandonCurrentActivity();
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
}

public class Building : Destructable
{
    public BuildingType buildingType;
    
    private Dictionary<string, List<Creature>> creaturesByType = new();
    private int spawnTicksRemaining = 0;
    private Creature spawningCreature; // Null if not spawning

    public Building(BuildingType buildingType) : base(buildingType.GetSize())
    {
        this.buildingType = buildingType;
    }

    public override Placeable DeepCopy()
    {
        Building copy = new (buildingType);
        copy.claimed = claimed; // from parent
        copy.damageTaken = damageTaken; // from parent
        copy.creaturesByType = null;
        copy.spawnTicksRemaining = spawnTicksRemaining;
        copy.spawningCreature = null;
        return copy;
    }

    public override float HealthFraction()
    {
        return (float)(100 - damageTaken) / 100;
    }

    public void StartSpawnCreature(CreatureType type)
    {
        if (spawningCreature != null)
        {
            return; // Already spawning
        }
        if (!creaturesByType.ContainsKey(type.GetName()))
        {
            throw new System.Exception("Creature type not supported");
        }
        if (CountCurrentCreaturesSupported(type) >= GetMaxCreaturesSupported(type))
        {
            return; // Max creatures already supported
        }

        spawningCreature = Creature.NewCreatureOfType(type);
        creaturesByType[type.GetName()].Add(spawningCreature);
        spawnTicksRemaining = GetSpawnTime(type);
    }

    public List<CreatureType> GetCreatureTypesAvailable()
    {
        return buildingType.GetSupportedCreatureTypes();
    }

    public int CountCurrentCreaturesSupported(CreatureType type)
    {
        if (!creaturesByType.ContainsKey(type.GetName()))
        {
            throw new System.Exception("Creature type not supported");
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
        return buildingType.GetRequestableItemCounts();
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
            spawningCreature = null;
            // Place the creature
            map.Add(spawningCreature, map.PositionOf(this) - new Vector2Int(1, 1));
        }
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

    public Prop(PropType propType) : base(propType.GetSize())
    {
        this.propType = propType;
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

    public override float HealthFraction()
    {
        return (float)(100 - damageTaken) / 100;
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

    public void MakeImaginaryDrops(Map map)
    {
        List<ItemType> droppedItems = propType.GetProducedItems();
        List<int> probabilities = propType.GetProducedItemsProbabilities();
        int probabilitySum = DroppedItemsProbabilitiesSum();
        for (int i = 0; i < droppedItems.Count; i++)
        {
            ItemType droppedItem = droppedItems[i];
            int probability = probabilities[i];
            Item item = new(droppedItem, (float)probability / (float)probabilitySum);
            map.Add(item, map.PositionOf(this));
        }
    }

    public void TakeHarvest(int amount)
    {
        harvestedAmount += amount;
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
    private float probability;

    public Item(ItemType itemType, float probability = 1) : base(itemType.GetSize())
    {
        this.itemType = itemType;
        this.probability = probability;
    }

    public override Placeable DeepCopy()
    {
        Item copy = new(itemType);
        copy.claimed = claimed; // from parent
        copy.itemType = itemType;
        copy.consumed = consumed;
        copy.probability = probability;
        return copy;
    }

    public float GetProbability()
    {
        return probability;
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
        return 10 * GetProbability();
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
