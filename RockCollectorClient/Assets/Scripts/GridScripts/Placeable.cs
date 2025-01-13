using NUnit.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public virtual bool IsDestroyed()
    {
        return HealthFraction() <= 0;
    }

    public virtual void OnDestroyed(Map map)
    {
        
    }

    public abstract float HealthFraction();
}

public class Creature : Destructable
{
    private string name;
    public int teamNumber = 0;

    private int level = 0;
    private int experience = 0; // reset to 0 on level up
    private List<Feat> feats = new();
    private List<TypeAbility> abilities = new();

    private CreatureType creatureType;

    private Goal pursuingGoal;
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
        copy.pursuingGoal = pursuingGoal; // This is OK because goals are stateless
        copy.currentActivity = null;
        copy.newActivityComputation = null;
        copy.cooldownTicksRemaining = cooldownTicksRemaining;
        return copy;
    }

    public string GetName()
    {
        return name;
    }

    public int GetLevel()
    {
        return level;
    }

    public int ExperienceForNextLevel()
    {
        return level * level;
    }

    public void GainExperience(int encounterLevel)
    {
        experience += (encounterLevel - 9) * (encounterLevel - 9);
    }

    public int EncounterLevel()
    {
        return level;
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

    public int d20Roll()
    {
        return UnityEngine.Random.Range(1, 21);
    }

    public bool RollForSuccess(int difficulty, string skillName)
    {
        int roll = d20Roll();
        int modifier = SkillModifier(skillName);
        return roll + modifier >= difficulty || roll == 20;
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
                GainExperience(encounterLevel);
            }
        }
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

    private TypeAbility BestHarvestingAbility(string skill)
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

    public Goal GetGoal()
    {
        return pursuingGoal;
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
        else if (ShouldGetNewGoal(map)) // if not, check for circumstances to get a new goal
        {
            pursuingGoal = GetNewGoal(map);
            LaunchNewActivityComputation(map); // we will also need an activity
        }
        // if not, we might need to launch a new activity computation
        else if (currentActivity == null && (newActivityComputation == null || !newActivityComputation.IsAlive)) 
        {
            LaunchNewActivityComputation(map);
        }
        // otherwise, continue with the current activity
    }

    private void TryPromoteStagedActivity(Map map)
    {
        currentActivity = stagedActivity;
        if (currentActivity.SuccessfulBackConversion(map) && !currentActivity.IsSourcePlaceableClaimed())
        {
            stagedActivity = null;
            // when we have a new activity, we need to mark the target placeable as claimed
            // new activities always are assigned here
            Debug.Log("Activity promoted: " + currentActivity + " " + currentActivity.GetLocation(map));
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
            Activity bestActivity = Search.GoalSearch(mapCopy, newMe);
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

    public override void Act(Map map)
    {
        if (cooldownTicksRemaining > 0)
        {
            cooldownTicksRemaining--;
            return;
        }
        else if (currentActivity == null)
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
        else if (currentActivity.DistanceTo(this, map) > currentActivity.ProximityRequirement(this))
        {
            Vector2Int difference = DirectionToNextActivity(map);
            Vector2Int direction = new (Math.Sign(difference.x), Math.Sign(difference.y));
            Vector2Int newPosition = map.PositionOf(this) + direction;
            map.MovePlaceable(this, newPosition);
            cooldownTicksRemaining = MoveSpeed();
            return;
        }
        else // Perform the activity
        {
            currentActivity.Perform(this, map);
            if (currentActivity.IsCompletedOrImpossible(map, this))
            {
                Debug.Log("Activity completed");
                AbandonCurrentActivity(); // Otherwise, there would be a "stunned" frame
            }
            return;
        }
    }

    private Vector2Int DirectionToNextActivity(Map map)
    {
        Vector2Int location = currentActivity.GetLocation(map);
        Vector2Int currentPosition = map.PositionOf(this);
        return location - currentPosition;
    }

    private void AbandonCurrentActivity()
    {
        if (currentActivity != null)
        {
            currentActivity.MarkSourcePlaceableUnclaimed();
            currentActivity = null;
            stagedActivity = null;
        }
    }

    private bool ShouldGetNewGoal(Map map)
    {
        return pursuingGoal == null || pursuingGoal.IsAchieved();
    }

    private Goal GetNewGoal(Map map)
    {
        Goal newGoal = creatureType.GetGoals()[0];
        newGoal.Initialize(map.CurrentTick());
        return newGoal;
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
        map.MovePlaceable(this, activity.GetLocation(map) - (Vector2Int.one * activity.ProximityRequirement(this)));
        return estimatedTicks;
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
        if (GetCurrentCreaturesSupported(type) >= GetMaxCreaturesSupported(type))
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

    public int GetCurrentCreaturesSupported(CreatureType type)
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

    public List<ItemType> GetRequestedItemTypes()
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
