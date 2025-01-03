using UnityEngine;
using System.Collections.Generic;

public class Tester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        AddAndRemovePlaceablesOfVariousSizes();
        LoadEntities();
        SetInitialGoal();
        AddActivity();
        Debug.Log("Tests finished");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AddAndRemovePlaceablesOfVariousSizes()
    {
        Map map = new();
        
        // add placeables of various sizes
        Placeable referencedPlaceable = new Placeable(1);
        map.Add(new Placeable(-5), new Vector2Int(0, 0));
        map.Add(new Placeable(0), new Vector2Int(5, 5));
        map.Add(referencedPlaceable, new Vector2Int(10, 10));
        map.Add(new Placeable(2), new Vector2Int(15, 15));

        // check that the placeables were added correctly
        Assert(map.PlaceablesAt(new Vector2Int(10, 10)).Contains(referencedPlaceable), "Referenced placeable not found at position");
        Assert(map.PositionOf(referencedPlaceable) == new Vector2Int(10, 10), "Referenced placeable not found at correct position");
        Assert(map.PlaceablesAt(new Vector2Int(0, 0)).Count == 1, "Placeable of size -5 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(0, 0))[0].SquaresMinimumOne() == 1, "Placeable of size 1 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(5, 5)).Count == 1, "Placeable of size 0 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(15, 15)).Count == 1, "Placeable of size 2 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(16, 15)).Count == 1, "Placeable of size 2 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(15, 16)).Count == 1, "Placeable of size 2 not found at position");
        Assert(map.PlaceablesAt(new Vector2Int(16, 16)).Count == 1, "Placeable of size 2 not found at position");

        // remove placeables of various sizes
        map.Remove(map.PlaceablesAt(new Vector2Int(0, 0))[0]);
        map.Remove(map.PlaceablesAt(new Vector2Int(5, 5))[0]);
        map.Remove(referencedPlaceable);
        map.Remove(map.PlaceablesAt(new Vector2Int(15, 15))[0]);

        // check that the placeables were removed correctly
        Assert(map.CountAllPlaceables() == 0, "Placeables not removed correctly");
        Assert(map.CountOccupiedSquares() == 0, "Placeables not removed correctly");
    }

    void LoadEntities()
    {
        EntityManager.ReadAllEntities();
        Assert(EntityManager.feats.ContainsKey("Skill Focus"), "Skill Focus not found");
        Assert(EntityManager.feats["Skill Focus"].GetSkillBonusName() == "woodcutter", "Skill Focus GetSkillBonusName");
        Assert(EntityManager.conditions.ContainsKey("Weak"), "Weak not found");
        Assert(EntityManager.conditions["Weak"].GetAttackPenalty() == 2, "Weak GetAttackPenalty");
        Assert(EntityManager.creatureTypes.ContainsKey("Peasant"), "Peasant not found");
        Assert(EntityManager.creatureTypes["Peasant"].GetStartingHealth() == 4, "Peasant GetStartingHealth");
        Assert(EntityManager.creatureTypes["Peasant"].GetHealthPerLevel() == 3, "Peasant GetHealthPerLevel");
        Assert(EntityManager.creatureTypes["Peasant"].GetWisdomBonus() == 0, "Peasant GetWisdomBonus");
        Assert(EntityManager.creatureTypes["Peasant"].GetAbilities().Count == 1, "Peasant GetAbilities");
        Assert(EntityManager.creatureTypes["Peasant"].GetAbilityLevels().Count == 1, "Peasant GetAbilityLevels");
        Assert(EntityManager.creatureTypes["Peasant"].GetGoals().Count == 1, "Peasant GetGoals");
        Assert(EntityManager.itemTypes.Count == 2, "Items not found");
        Assert(EntityManager.itemTypes.ContainsKey("Axe"), "Axe not found");
        Assert(EntityManager.propTypes.ContainsKey("Tree"), "Tree not found");
        Assert(EntityManager.typeAbilities.ContainsKey("Woodcutting"), "Woodcutting not found");
        Assert(EntityManager.buildingTypes.ContainsKey("Farm"), "Farm not found");
        Assert(EntityManager.buildingTypes["Farm"].GetSupportedCreatureTypes()[0].GetName() == "Peasant", "Farm GetSupportedCreatureTypes");
        Assert(EntityManager.buildingTypes["Farm"].GetSupportedCreatureSpawnTimes()[0] == 500, "Farm GetSupportedCreatureSpawnTimes");
    }

    void SetInitialGoal()
    {
        Map map = new();
        Creature testCreature = new("George", EntityManager.creatureTypes["Peasant"]);
        Prop prop = new(EntityManager.propTypes["Tree"]);
        map.Add(testCreature, new Vector2Int(0, 0));
        map.Add(prop, new Vector2Int(5, 5));
        testCreature.ThinkAndPlan(map);
        Assert(testCreature.GetGoal() != null, "Initial goal not set");
        Assert(testCreature.GetCreatureType().GetGoals()[0].GetType() == testCreature.GetGoal().GetType(), "Initial goal not set correctly");
    }

    void AddActivity()
    {
        Map map = new();
        Prop prop = new(EntityManager.propTypes["Tree"]);
        map.Add(prop, new Vector2Int(0, 0));
        Assert(map.GetActivities().Count == 1, "Activities empty");
        Activity activity = null;
        foreach (Activity a in map.GetActivities())
        {
            activity = a;
        }
        if (activity == null)
        {
            Debug.LogError("Activity not found");
            return;
        }
        Assert(activity.GetLocation(map) == new Vector2Int(0, 0), "Activity location");
        Assert(activity.droppedItems.Count == 1, "Activity dropped items");
        Assert(activity.droppedItems[0].GetName() == "Log", "Activity dropped item");
    }

    void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError(message);
        }
    }
}
