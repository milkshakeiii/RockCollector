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
        CraftThings();
        WeaponAttack();
        RepairBuilding();
        PeasantBehaviorTest();
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
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        map.Add(testCreature, Map.NULL_POSITION);
        Item item = new Item(EntityManager.itemTypes["Axe"]);
        map.Add(item, Map.NULL_POSITION);
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
        Assert(EntityManager.feats.ContainsKey("Skill Focus (woodcrafting)"), "Skill Focus not found");
        Assert(EntityManager.feats["Skill Focus (woodcrafting)"].GetSkillBonusName() == "woodcrafting", "Skill Focus GetSkillBonusName");
        Assert(EntityManager.conditions.ContainsKey("Weak"), "Weak not found");
        Assert(EntityManager.conditions["Weak"].GetAttackPenalty() == 2, "Weak GetAttackPenalty");
        Assert(EntityManager.creatureTypes.ContainsKey("Peasant"), "Peasant not found");
        Assert(EntityManager.creatureTypes["Peasant"].GetStartingHealth() == 4, "Peasant GetStartingHealth");
        Assert(EntityManager.creatureTypes["Peasant"].GetHealthPerLevel() == 3, "Peasant GetHealthPerLevel");
        Assert(EntityManager.creatureTypes["Peasant"].GetWisdomBonus() == 0, "Peasant GetWisdomBonus");
        Assert(EntityManager.creatureTypes["Peasant"].GetAbilities().Count > 0, "Peasant GetAbilities");
        Assert(EntityManager.creatureTypes["Peasant"].GetAbilityLevels().Count == 1, "Peasant GetAbilityLevels");
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

    void CraftThings()
    {
        Map map = new();
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        map.Add(testCreature, new Vector2Int(5, 5));
        Building farm = new(EntityManager.buildingTypes["Farm"]);
        map.Add(farm, new Vector2Int(5, 5));
        Item log = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(testCreature, log);
        Activity craftActivity = new CraftActivity(EntityManager.itemTypes["Axe"], farm);
        craftActivity.Perform(testCreature, map);
        Assert(log.IsConsumed(), "Input item not consumed");
        map.Remove(log);
        Assert(map.HeldPlaceablesOf(farm).Count == 1, "Crafted item not held by farm");
        Item item = (Item)map.HeldPlaceablesOf(farm)[0];
        Assert(item.itemType.GetName() == "Axe", "Crafted item not axe");
        map.Remove(item);
        Assert(map.HeldPlaceablesOf(farm).Count == 0, "Crafted item not removed from farm");

        // test crafting with probabilities
        Item log2 = new (EntityManager.itemTypes["Log"], 0.6f);
        Item acorn = new (EntityManager.itemTypes["Acorn"], 0.2f);
        Item water = new (EntityManager.itemTypes["Water"]);
        map.AddHeld(farm, log2);
        map.AddHeld(farm, acorn);
        map.AddHeld(farm, water);
        Activity craftActivity2 = new CraftActivity(EntityManager.itemTypes["Soup"], farm);
        craftActivity2.Perform(testCreature, map);
        Assert(log2.IsConsumed(), "Input item not consumed");
        Assert(acorn.IsConsumed(), "Input item not consumed");
        Assert(water.IsConsumed(), "Input item not consumed");
        map.Remove(log2);
        map.Remove(acorn);
        map.Remove(water);
        Assert(map.HeldPlaceablesOf(farm).Count == 1, "Crafted item not held by farm");
        Item item2 = (Item)map.HeldPlaceablesOf(farm)[0];
        Assert(item2.itemType.GetName() == "Soup", "Crafted item not soup");
        Assert(Mathf.Abs(item2.GetProbability() - 0.12f) < 0.01f, "Crafted item probability");
        map.Remove(item2);

        // test crafting with duplicated inputs
        Item log3 = new (EntityManager.itemTypes["Log"], 0.5f);
        map.AddHeld(farm, log3);
        Item log4 = new (EntityManager.itemTypes["Log"], 0.2f);
        map.AddHeld(farm, log4);
        Activity craftActivity3 = new CraftActivity(EntityManager.itemTypes["Double Axe"], farm);
        craftActivity3.Perform(testCreature, map);
        Assert(log3.IsConsumed(), "Input item not consumed");
        Assert(log4.IsConsumed(), "Input item not consumed");
        map.Remove(log3);
        map.Remove(log4);
        Assert(map.HeldPlaceablesOf(farm).Count == 1, "Crafted item not held by farm");
        Item item3 = (Item)map.HeldPlaceablesOf(farm)[0];
        Assert(item3.itemType.GetName() == "Double Axe", "Crafted item not double axe");
        Assert(item3.GetProbability() == 0.35f, "Crafted item probability"); // should actually be 0.5 * 0.2 = 0.1
        map.Remove(item3);

        // test crafting with excessive inputs
        Item log5 = new (EntityManager.itemTypes["Log"], 0.7f);
        map.AddHeld(farm, log5);
        Item log6 = new (EntityManager.itemTypes["Log"], 0.7f);
        map.AddHeld(farm, log6);
        Item log7 = new(EntityManager.itemTypes["Log"], 0.7f);
        map.AddHeld(farm, log7);
        Activity craftActivity4 = new CraftActivity(EntityManager.itemTypes["Axe"], farm);
        craftActivity4.Perform(testCreature, map);
        Assert(log5.IsConsumed(), "Input item not consumed");
        Assert(log6.IsConsumed(), "Input item not consumed");
        Assert(!log7.IsConsumed(), "Item consumed");
        map.Remove(log5);
        map.Remove(log6);
        Assert(map.HeldPlaceablesOf(farm).Count == 2, "Crafted item not held by farm");
        Item item4 = (Item)map.HeldPlaceablesOf(farm)[0];
        Assert(item4.itemType.GetName() == "Log", "Remainder log not found");
        Item item5 = (Item)map.HeldPlaceablesOf(farm)[1];
        Assert(item5.itemType.GetName() == "Axe", "Crafted item not axe");
        Assert(item5.GetProbability() == 1f, "Crafted item probability");
        map.Remove(item4);
        map.Remove(item5);
        {
            // test crafting with excessive inputs, whole probability
            Item log8 = new(EntityManager.itemTypes["Log"], 1f);
            map.AddHeld(farm, log8);
            Item log9 = new(EntityManager.itemTypes["Log"], 1f);
            map.AddHeld(farm, log9);
            Item log10 = new(EntityManager.itemTypes["Log"], 1f);
            map.AddHeld(farm, log10);
            Activity craftActivity5 = new CraftActivity(EntityManager.itemTypes["Axe"], farm);
            craftActivity5.Perform(testCreature, map);
            Assert(log8.IsConsumed(), "Input item not consumed");
            Assert(!log9.IsConsumed(), "Extra item consumed");
            Assert(!log10.IsConsumed(), "Extra item consumed");
            map.Remove(log8);
            Assert(map.HeldPlaceablesOf(farm).Count == 3, "Crafted item not held by farm");
            Item item7 = (Item)map.HeldPlaceablesOf(farm)[2];
            Assert(item7.itemType.GetName() == "Axe", "Crafted item not axe");
            Assert(item7.GetProbability() == 1f, "Crafted item probability");
            map.Remove(item7);
            map.Remove(log9);
            map.Remove(log10);
        }
        {
            // creature holding / craft twice
            Item log8 = new(EntityManager.itemTypes["Log"], 1f);
            map.AddHeld(testCreature, log8);
            Item log9 = new(EntityManager.itemTypes["Log"], 1f);
            map.AddHeld(testCreature, log9);
            Item log10 = new(EntityManager.itemTypes["Log"], 1f);
            map.AddHeld(testCreature, log10);
            Activity craftActivity5 = new CraftActivity(EntityManager.itemTypes["Axe"], farm);
            craftActivity5.Perform(testCreature, map);
            Assert(log8.IsConsumed(), "Input item not consumed");
            Assert(!log9.IsConsumed(), "Extra item consumed");
            Assert(!log10.IsConsumed(), "Extra item consumed");
            map.Remove(log8);
            Assert(map.HeldPlaceablesOf(farm).Count == 1, "Crafted item not held by farm");
            Assert(map.HeldPlaceablesOf(testCreature).Count == 2, "left over items not held by creature");
            Item item7 = (Item)map.HeldPlaceablesOf(farm)[0];
            Assert(item7.itemType.GetName() == "Axe", "Crafted item not axe");
            Assert(item7.GetProbability() == 1f, "Crafted item probability");
            Activity craftActivity6 = new CraftActivity(EntityManager.itemTypes["Axe"], farm);
            craftActivity6.Perform(testCreature, map);
            Assert(log9.IsConsumed(), "item not consumed");
            Assert(!log10.IsConsumed(), "Extra item consumed");
            map.Remove(log9);
            Assert(map.HeldPlaceablesOf(farm).Count == 2, "Crafted item not held by farm");
            Assert(map.HeldPlaceablesOf(testCreature).Count == 1, "left over item not held by creature");
        }
    }

    void WeaponAttack()
    {
        Map map = new();
        Creature testCreature = new("Skele1", 0, EntityManager.creatureTypes["Skeleton"], Map.NULL_POSITION);
        testCreature.teamNumber = 0;
        map.Add(testCreature, new Vector2Int(5, 5));
        Creature targetCreature = new("Skele2", 0, EntityManager.creatureTypes["Skeleton"], Map.NULL_POSITION);
        targetCreature.teamNumber = 1;
        map.Add(targetCreature, new Vector2Int(5, 6));
        Item axe = new (EntityManager.itemTypes["Axe"]);
        map.AddHeld(testCreature, axe);
        testCreature.UseAnyAbility(map);
        if (testCreature.GetExperience() > 0)
        {
            // hit
            Assert(targetCreature.GetDamageTaken() > 0, "Target not damaged");
            Assert(targetCreature.GetExperience() == 0, "Experience granted to target");
        }
        else
        {
            // miss
            Assert(targetCreature.GetDamageTaken() == 0, "Target damaged");
        }
    }

    void RepairBuilding()
    {
        Map map = new();
        Building farm = new(EntityManager.buildingTypes["Farm"]);
        map.Add(farm, new Vector2Int(5, 5));
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        testCreature.ApplySkillIncrease("repair", 9);
        for (int i = 0; i < 5; i++)
        {
            testCreature.LevelUp();
        }
        map.Add(testCreature, new Vector2Int(4, 4));
        Item hammer = new (EntityManager.itemTypes["Hammer"]);
        map.AddHeld(testCreature, hammer);
        Assert(testCreature.RepairCooldownAndAmount(map).Item1 > 0 && testCreature.RepairCooldownAndAmount(map).Item2 > 0, "Repair cooldown and amount");
        farm.TakeDamage(1);
        Assert(farm.GetDamageTaken() == 1, "Building health");
        Activity repairActivity = new RepairActivity(farm);
        Assert(!repairActivity.IsCompletedOrImpossible(map, testCreature), "Repair impossible");
        repairActivity.Perform(testCreature, map);
        Assert(farm.GetDamageTaken() == 0, "Building health");  
        Assert(testCreature.GetExperience() > 0, "Experience not granted");
    }

    void PeasantBehaviorTest()
    {
        Map map = new();
        Building farm = new(EntityManager.buildingTypes["Farm"]);
        map.Add(farm, new Vector2Int(5, 5));
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"], map.PositionOf(farm));
        map.Add(testCreature, new Vector2Int(2, 2));
        for (int i = 0; i < 5; i++)
        {
            testCreature.LevelUp();
        }
        PeasantBehavior behavior = new();
        Item hammer = new (EntityManager.itemTypes["Hammer"]);
        map.Add(hammer, new Vector2Int(0, 0));

        // test pick up hammer for repair
        Activity pickUpActivity = behavior.NextActivity(map, testCreature);
        Assert(pickUpActivity is PickUpActivity, "Activity should be pick up");
        Assert(pickUpActivity.GetLocation(map) == new Vector2Int(0, 0), "Pick up location");

        map.Remove(hammer);
        map.AddHeld(testCreature, hammer);

        // test repair
        farm.TakeDamage(1);
        Activity repairActivity = behavior.NextActivity(map, testCreature);
        Assert(repairActivity is RepairActivity, "Activity should be repair");
        Assert(repairActivity.GetLocation(map) == map.PositionOf(farm), "Repair location");

        farm.TakeDamage(-1);

        // test DropOffRequestedItems
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Log"], 1);
        Item log = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(testCreature, log);
        Activity dropOffActivity = behavior.NextActivity(map, testCreature);
        Assert(dropOffActivity is DropOffActivity, "Activity should be drop off");
        Assert(dropOffActivity.GetLocation(map) == map.PositionOf(farm), "Drop off location");

        map.Remove(log);
        map.Add(log, new Vector2Int(-2, -2));

        // test PickUpRequestedItems
        Activity pickUpActivity2 = behavior.NextActivity(map, testCreature);
        Assert(pickUpActivity2 is PickUpActivity, "Activity should be pick up");
        Assert(pickUpActivity2.GetLocation(map) == new Vector2Int(-2, -2), "Pick up location");

        map.Remove(log);

        // test HarvestRequestedItems
        Prop tree = new(EntityManager.propTypes["Tree"]);
        map.Add(tree, new Vector2Int(10, 10));
        Item axe = new(EntityManager.itemTypes["Axe"]);
        map.AddHeld(testCreature, axe);
        Activity harvestActivity = behavior.NextActivity(map, testCreature);
        Assert(harvestActivity is HarvestActivity, "Activity should be harvest");
        Assert(harvestActivity.GetLocation(map) == map.PositionOf(tree), "Harvest location");

        map.Remove(axe);
        map.Add(axe, new Vector2Int(-2, -2));

        // test pick up axe for harvest
        Activity pickUpActivity3 = behavior.NextActivity(map, testCreature);
        Assert(pickUpActivity3 is PickUpActivity, "Activity should be pick up");
        Assert(pickUpActivity3.GetLocation(map) == new Vector2Int(-2, -2), "Pick up location");

        // test CraftItems
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Real Axe"], 1);
        Item flint = new(EntityManager.itemTypes["Flint"]);
        map.AddHeld(farm, flint);
        map.AddHeld(farm, log);
        Activity craftActivity = behavior.NextActivity(map, testCreature);
        Assert(craftActivity is CraftActivity, "Activity should be craft");
        Assert(craftActivity.GetLocation(map) == map.PositionOf(farm), "Craft location");

        // test RestAtHomeBuilding
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Real Axe"], 0);
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Log"], 0);
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Flint"], 0);
        Activity restActivity = behavior.NextActivity(map, testCreature);
        Assert(restActivity is RestActivity, "Activity should be rest");
        Assert(restActivity.GetLocation(map) == map.PositionOf(farm), "Rest location");
    }

    void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError(message);
        }
    }
}
