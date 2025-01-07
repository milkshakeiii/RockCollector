using UnityEngine;
using System.Collections.Generic;

public class Tester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        AddAndRemovePlaceablesOfVariousSizes();
        DistancesBetweenPlaceablesOfVariousSizes();
        LoadEntities();
        HoldAndDropItems();
        SetInitialGoal();
        AddActivity();
        CraftThings();
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

    void DistancesBetweenPlaceablesOfVariousSizes()
    {
        Map map = new();
        Placeable placeable1 = new (1);
        Placeable placeable2 = new (0);
        Placeable placeable3 = new (2);
        Placeable placeable4 = new (-2);
        Placeable placeable5 = new (3);
        Placeable placeable6 = new (1);
        map.Add(placeable1, new Vector2Int(10, 10));
        map.Add(placeable2, new Vector2Int(5, 5));
        map.Add(placeable3, new Vector2Int(10, 10));
        map.Add(placeable4, new Vector2Int(12, 12));
        map.Add(placeable5, new Vector2Int(10, 15));
        map.Add(placeable6, new Vector2Int(11, 18));
        Assert(map.DistanceBetween(placeable1, placeable2) == 5, "Distance between placeables 1 and 2");
        Assert(map.DistanceBetween(placeable1, placeable3) == 0, "Distance between placeables 1 and 3");
        Assert(map.DistanceBetween(placeable2, placeable3) == 5, "Distance between placeables 2 and 3");
        Assert(map.DistanceBetween(placeable3, placeable4) == 1, "Distance between placeables 1 and 4");
        Assert(map.DistanceBetween(placeable3, placeable5) == 4, "Distance between placeables 3 and 5");
        Assert(map.DistanceBetween(placeable4, placeable5) == 3, "Distance between placeables 4 and 5");
        Assert(map.DistanceBetween(placeable5, placeable6) == 1, "Distance between placeables 5 and 6");
        
        Assert(map.DistanceTo(new Vector2Int(10, 10), placeable1) == 0, "Distance to placeable 1");
        Assert(map.DistanceTo(new Vector2Int(15, 15), placeable3) == 4, "Distance to placeable 2");
    }

    void HoldAndDropItems()
    {
        Map map = new();
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"]);
        map.Add(testCreature, new Vector2Int(0, 0));
        Item item = new Item(EntityManager.itemTypes["Axe"]);
        map.Add(item, new Vector2Int(0, 0));
        Assert(map.UnheldPlaceables().Count == 2, "Items not added correctly");
        Assert(map.HeldPlaceables().Count == 0, "Item not picked up correctly");
        map.PickUp(testCreature, item);
        Assert(map.UnheldPlaceables().Count == 1, "Item not picked up correctly");
        Assert(map.HeldPlaceables().Count == 1, "Item not picked up correctly");
        map.MovePlaceable(testCreature, new Vector2Int(5, 5));
        Assert(map.PositionOf(testCreature) == new Vector2Int(5, 5), "Creature not moved correctly");
        Assert(map.PositionOf(item) == new Vector2Int(5, 5), "Item not moved correctly");
        Assert(map.UnheldPlaceables().Count == 1, "Item not picked up correctly");
        Assert(map.HeldPlaceables().Count == 1, "Item not picked up correctly");
        map.Remove(item);
        Assert(map.HeldPlaceables().Count == 0, "Item not removed correctly");
        Assert(map.UnheldPlaceables().Count == 1, "Item not removed correctly");
        Item secondItem = new Item(EntityManager.itemTypes["Axe"]);
        map.Add(secondItem, new Vector2Int(5, 5));
        map.PickUp(testCreature, secondItem);
        Assert(map.UnheldPlaceables().Count == 1, "Item not picked up correctly");
        Assert(map.HeldPlaceables().Count == 1, "Item not picked up correctly");
        map.Remove(testCreature);
        Assert(map.UnheldPlaceables().Count == 0, "Item not removed correctly");
        Assert(map.HeldPlaceables().Count == 0, "Item not removed correctly");
        map.AddHeld(testCreature, secondItem);
        Assert(map.HeldPlaceablesOf(testCreature).Count == 1, "Item not added to held correctly");
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
        Assert(EntityManager.itemTypes.Count > 2, "Items not found");
        Assert(EntityManager.itemTypes.ContainsKey("Axe"), "Axe not found");
        Assert(EntityManager.propTypes.ContainsKey("Tree"), "Tree not found");
        Assert(EntityManager.propTypes["Tree"].GetProducedItems().Count == 1, "Tree GetDroppedItems");
        Assert(EntityManager.propTypes["Tree"].GetProducedItemsProbabilities().Count == 1, "Tree GetProducedItemsProbabilities");
        Assert(EntityManager.typeAbilities.ContainsKey("Woodcutting"), "Woodcutting not found");
        Assert(EntityManager.buildingTypes.ContainsKey("Farm"), "Farm not found");
        Assert(EntityManager.buildingTypes["Farm"].GetSupportedCreatureTypes()[0].GetName() == "Peasant", "Farm GetSupportedCreatureTypes");
        Assert(EntityManager.buildingTypes["Farm"].GetSupportedCreatureSpawnTimes()[0] == 500, "Farm GetSupportedCreatureSpawnTimes");
    }

    void SetInitialGoal()
    {
        Map map = new();
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"]);
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
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"]);
        map.Add(testCreature, new Vector2Int(5, 5));
        Assert(map.GetActivities(testCreature).Count == 1, "Activities empty");
        Activity activity = null;
        foreach (Activity a in map.GetActivities(testCreature))
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

    void CraftThings()
    {
        Map map = new();
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"]);
        map.Add(testCreature, new Vector2Int(5, 5));
        Building farm = new(EntityManager.buildingTypes["Farm"]);
        map.Add(farm, new Vector2Int(5, 5));
        Item log = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(farm, log);
        Activity craftActivity = new CraftActivity(EntityManager.itemTypes["Axe"], farm);
        craftActivity.Perform(testCreature, map);
        Assert(log.IsConsumed(), "Input item not consumed");
        map.Remove(log);
        Assert(map.HeldPlaceablesOf(farm).Count == 1, "Crafted item not held by farm");
        Item item = (Item)map.HeldPlaceablesOf(farm)[0];
        Assert(item.itemType.GetName() == "Axe", "Crafted item not axe");
    }

    void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError(message);
        }
    }
}
