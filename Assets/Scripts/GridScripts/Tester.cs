using UnityEngine;
using System.Collections.Generic;

public class Tester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadEntities();
        AddAndRemovePlaceablesOfVariousSizes();
        DistancesBetweenPlaceablesOfVariousSizes();
        HoldAndDropItems();
        CraftThings();
        WeaponAttack();
        RepairBuilding();
        PeasantBehaviorTest();
        ConditionsTest();
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
        PropType propType = EntityManager.propTypes["Tree"];
        Prop referencedPlaceable = new (propType);
        map.Add(new Prop(propType), new Vector2Int(0, 0));
        map.Add(new Prop(propType), new Vector2Int(5, 5));
        map.Add(referencedPlaceable, new Vector2Int(10, 10));
        Building shed = new(EntityManager.buildingTypes["Shed"], 1);
        map.Add(shed, new Vector2Int(15, 15));

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

        // moving a creature doesn't remove the held placeables or outfit
        Creature testCreature = new("George", 0, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        map.Add(testCreature, new Vector2Int(5, 5));
        Item item = new (EntityManager.itemTypes["Axe"]);
        map.AddHeld(testCreature, item);
        testCreature.AddToOutfit(item, map);
        map.MovePlaceable(testCreature, new Vector2Int(10, 10));
        Assert(map.PositionOf(testCreature) == new Vector2Int(10, 10), "Creature not moved correctly");
        Assert(map.PositionOf(item) == new Vector2Int(10, 10), "Item not moved correctly");
        Assert(map.HolderOf(item) == testCreature, "Item not in held placeables");
        Assert(testCreature.OutfitContains(item, map), "Item not in outfit");
    }

    void DistancesBetweenPlaceablesOfVariousSizes()
    {
        Map map = new();
        PropType propType = EntityManager.propTypes["Tree"];
        Prop placeable1 = new (propType);
        Prop placeable2 = new (propType);
        BuildingType shedType = EntityManager.buildingTypes["Shed"];
        Building placeable3 = new (shedType, 1);
        Prop placeable4 = new (propType);
        BuildingType graveyardType = EntityManager.buildingTypes["Graveyard"];
        Building placeable5 = new (graveyardType, 1);
        Prop placeable6 = new (propType);
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
        EntityManager.ReadTeam("Testanians");
        EntityManager.ReadScenario("Testario");
        Assert(EntityManager.feats.ContainsKey("Skill Focus (woodcrafting)"), "Skill Focus not found");
        Assert(EntityManager.feats["Skill Focus (woodcrafting)"].GetSkillBonusName() == "woodcrafting", "Skill Focus GetSkillBonusName");
        Assert(EntityManager.conditions.ContainsKey("Weak"), "Weak not found");
        Assert(EntityManager.conditions["Weak"].GetModifiedAttributeScores()[0] == AttributeScores.STRENGTH, "Weak GetDuration");
        Assert(EntityManager.conditions["Weak"].GetAttributeModifierAmounts()[0] == -2, "Weak GetAttackPenalty");
        Assert(EntityManager.creatureTypes.ContainsKey("Peasant"), "Peasant not found");
        Assert(EntityManager.creatureTypes["Peasant"].GetStartingHealth() == 4, "Peasant GetStartingHealth");
        Assert(EntityManager.creatureTypes["Peasant"].GetHealthPerLevel() == 3, "Peasant GetHealthPerLevel");
        Assert(EntityManager.creatureTypes["Peasant"].GetWisdomBonus() == 0, "Peasant GetWisdomBonus");
        Assert(EntityManager.creatureTypes["Peasant"].GetAbilities().Count > 0, "Peasant GetAbilities");
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

        // Peasant can craft
        Creature testCreature = new("George", 1, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        map.Add(testCreature, new Vector2Int(5, 5));
        Building farm = new(EntityManager.buildingTypes["Farm"], 1);
        map.Add(farm, new Vector2Int(5, 5));
        Item log = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(testCreature, log);
        Item flint = new(EntityManager.itemTypes["Flint"]);
        map.AddHeld(testCreature, flint);
        Activity craftActivity = new CraftActivity(EntityManager.itemTypes["Axe"], farm);
        craftActivity.Perform(testCreature, map);
        Assert(log.IsConsumed(), "Input item not consumed");
        Assert(flint.IsConsumed(), "Input item not consumed");
        map.Remove(log);
        map.Remove(flint);
        Assert(map.HeldPlaceablesOf(farm).Count == 1, "Crafted item not held by farm");
        Item item = (Item)map.HeldPlaceablesOf(farm)[0];
        Assert(item.itemType.GetName() == "Axe", "Crafted item not axe");
        map.Remove(item);
        Assert(map.HeldPlaceablesOf(farm).Count == 0, "Crafted item not removed from farm");

        // test crafting with duplicated inputs
        Building barracks = new(EntityManager.buildingTypes["Barracks"], 1);
        map.Add(barracks, new Vector2Int(5, 5));
        Creature woodCrafter = new("George", 1, EntityManager.creatureTypes["Woodcrafter"], Map.NULL_POSITION);
        barracks.ChangeRequestedItemAmount(EntityManager.itemTypes["Wooden Shield"], 1);
        Item log3 = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(barracks, log3);
        Activity craftActivity3 = new CraftActivity(EntityManager.itemTypes["Wooden Shield"], barracks);
        Assert(craftActivity3.IsCompletedOrImpossible(map, woodCrafter), "Crafting should be impossible");
        Item log4 = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(barracks, log4);
        craftActivity3.Perform(woodCrafter, map);
        Assert(log3.IsConsumed(), "Input item not consumed");
        Assert(log4.IsConsumed(), "Input item not consumed");
        map.Remove(log3);
        map.Remove(log4);
        Assert(map.HeldPlaceablesOf(barracks).Count == 1, "Crafted item not held by farm");
        Item item3 = (Item)map.HeldPlaceablesOf(barracks)[0];
        Assert(item3.itemType.GetName() == "Wooden Shield", "Crafted item not Shield");
        map.Remove(item3);
        barracks.ChangeRequestedItemAmount(EntityManager.itemTypes["Wooden Shield"], 0);

        // test crafting with excessive inputs
        Item log5 = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(farm, log5);
        Item log6 = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(farm, log6);
        Item log7 = new(EntityManager.itemTypes["Log"]);
        map.AddHeld(farm, log7);
        Activity craftActivity4 = new CraftActivity(EntityManager.itemTypes["Wooden Sword"], farm);
        craftActivity4.Perform(testCreature, map);
        Assert(log5.IsConsumed(), "Input item not consumed");
        Assert(!log6.IsConsumed(), "Item consumed");
        Assert(!log7.IsConsumed(), "Item consumed");
        map.Remove(log5);
        Assert(map.HeldPlaceablesOf(farm).Count == 3, "Crafted item not held by farm");
        Item item4 = (Item)map.HeldPlaceablesOf(farm)[0];
        Assert(item4.itemType.GetName() == "Log", "Remainder log not found");
        Item item5 = (Item)map.HeldPlaceablesOf(farm)[1];
        Assert(item5.itemType.GetName() == "Log", "Second remainder log not found");
        Item item6 = (Item)map.HeldPlaceablesOf(farm)[2];
        Assert(item6.itemType.GetName() == "Wooden Sword", "Crafted item not sword");
        map.Remove(item4);
        map.Remove(item5);
        map.Remove(item6);

        {
            // test crafting with excessive inputs
            Item log8 = new(EntityManager.itemTypes["Log"]);
            map.AddHeld(farm, log8);
            Item log9 = new(EntityManager.itemTypes["Log"]);
            map.AddHeld(farm, log9);
            Item log10 = new(EntityManager.itemTypes["Log"]);
            map.AddHeld(farm, log10);
            Activity craftActivity5 = new CraftActivity(EntityManager.itemTypes["Wooden Sword"], farm);
            craftActivity5.Perform(testCreature, map);
            Assert(log8.IsConsumed(), "Input item not consumed");
            Assert(!log9.IsConsumed(), "Extra item consumed");
            Assert(!log10.IsConsumed(), "Extra item consumed");
            map.Remove(log8);
            Assert(map.HeldPlaceablesOf(farm).Count == 3, "Crafted item not held by farm");
            Item item7 = (Item)map.HeldPlaceablesOf(farm)[2];
            Assert(item7.itemType.GetName() == "Wooden Sword", "Crafted item not sword");
            map.Remove(item7);
            map.Remove(log9);
            map.Remove(log10);
        }

        {
            // creature holding / craft twice
            Item log8 = new(EntityManager.itemTypes["Log"]);
            map.AddHeld(testCreature, log8);
            Item log9 = new(EntityManager.itemTypes["Log"]);
            map.AddHeld(testCreature, log9);
            Item log10 = new(EntityManager.itemTypes["Log"]);
            map.AddHeld(testCreature, log10);
            Activity craftActivity5 = new CraftActivity(EntityManager.itemTypes["Wooden Sword"], farm);
            craftActivity5.Perform(testCreature, map);
            Assert(log8.IsConsumed(), "Input item not consumed");
            Assert(!log9.IsConsumed(), "Extra item consumed");
            Assert(!log10.IsConsumed(), "Extra item consumed");
            map.Remove(log8);
            Assert(map.HeldPlaceablesOf(farm).Count == 1, "Crafted item not held by farm");
            Assert(map.HeldPlaceablesOf(testCreature).Count == 2, "left over items not held by creature");
            Item item7 = (Item)map.HeldPlaceablesOf(farm)[0];
            Assert(item7.itemType.GetName() == "Wooden Sword", "Crafted item not axe");
            Activity craftActivity6 = new CraftActivity(EntityManager.itemTypes["Wooden Sword"], farm);
            craftActivity6.Perform(testCreature, map);
            Assert(log9.IsConsumed(), "item not consumed");
            Assert(!log10.IsConsumed(), "Extra item consumed");
            map.Remove(log9);
            Assert(map.HeldPlaceablesOf(farm).Count == 2, "Crafted item not held by farm");
            Assert(map.HeldPlaceablesOf(testCreature).Count == 1, "left over item not held by creature");
        }
        {
            // Skeleton can't craft
            Creature skeleton = new("Skele1", 0, EntityManager.creatureTypes["Skeleton"], Map.NULL_POSITION);
            map.Add(skeleton, new Vector2Int(5, 5));
            Item log8 = new(EntityManager.itemTypes["Log"]);
            map.AddHeld(skeleton, log8);
            Item flint2 = new(EntityManager.itemTypes["Flint"]);
            map.AddHeld(skeleton, flint2);
            Activity craftActivity7 = new CraftActivity(EntityManager.itemTypes["Axe"], farm);
            Assert(craftActivity7.IsCompletedOrImpossible(map, skeleton), "Crafting should be impossible");
            map.Remove(log8);
            map.Remove(flint2);
        }
    }

    void WeaponAttack()
    {
        Map map = new();
        Creature testCreature = new("Skele1", 0, EntityManager.creatureTypes["Skeleton"], Map.NULL_POSITION);
        testCreature.teamNumber = 1;
        map.Add(testCreature, new Vector2Int(5, 5));
        Creature targetCreature = new("Skele2", 0, EntityManager.creatureTypes["Skeleton"], Map.NULL_POSITION);
        targetCreature.teamNumber = 2;
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
        Building farm = new(EntityManager.buildingTypes["Farm"], 1);
        map.Add(farm, new Vector2Int(5, 5));
        Creature testCreature = new("George", 1, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
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
        Building farm = new(EntityManager.buildingTypes["Farm"], 1);
        map.Add(farm, new Vector2Int(5, 5));
        Creature testCreature = new("George", 1, EntityManager.creatureTypes["Peasant"], map.PositionOf(farm));
        map.Add(testCreature, new Vector2Int(2, 2));
        for (int i = 0; i < 5; i++)
        {
            testCreature.LevelUp();
        }
        CreatureBehavior behavior = CreatureBehavior.FromName("Peasant Behavior");
        Item hammer = new (EntityManager.itemTypes["Hammer"]);
        map.Add(hammer, new Vector2Int(0, 0));

        // test pick up hammer for repair
        Activity pickUpActivity = behavior.NextActivity(map, testCreature).Item1;
        Assert(pickUpActivity is PickUpActivity, "Activity should be pick up");
        Assert(pickUpActivity.GetLocation(testCreature, map) == new Vector2Int(0, 0), "Pick up location");

        map.Remove(hammer);
        map.AddHeld(testCreature, hammer);

        // test repair
        farm.TakeDamage(1);
        Activity repairActivity = behavior.NextActivity(map, testCreature).Item1;
        Assert(repairActivity is RepairActivity, "Activity should be repair");
        Assert(repairActivity.GetLocation(testCreature, map) == map.PositionOf(farm), "Repair location");

        farm.TakeDamage(-1);

        // test DropOffRequestedItems
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Log"], 1);
        Item log = new (EntityManager.itemTypes["Log"]);
        map.AddHeld(testCreature, log);
        Activity dropOffActivity = behavior.NextActivity(map, testCreature).Item1;
        Assert(dropOffActivity is DeliverActivity, "Activity should be deliver");
        Assert(dropOffActivity.GetLocation(testCreature, map) == map.PositionOf(farm), "Drop off location");

        map.Remove(log);
        map.Add(log, new Vector2Int(-2, -2));

        // test PickUpRequestedItems
        Activity pickUpActivity2 = behavior.NextActivity(map, testCreature).Item1;
        Assert(pickUpActivity2 is DeliverActivity, "Activity should be deliver");
        Assert(pickUpActivity2.GetLocation(testCreature, map) == new Vector2Int(-2, -2), "Pick up location");

        map.Remove(log);

        // test HarvestRequestedItems
        Prop tree = new(EntityManager.propTypes["Tree"]);
        map.Add(tree, new Vector2Int(10, 10));
        Item axe = new(EntityManager.itemTypes["Axe"]);
        map.AddHeld(testCreature, axe);
        Activity harvestActivity = behavior.NextActivity(map, testCreature).Item1;
        Assert(harvestActivity is HarvestActivity, "Activity should be harvest");
        Assert(harvestActivity.GetLocation(testCreature, map) == map.PositionOf(tree), "Harvest location");

        map.Remove(axe);
        map.Add(axe, new Vector2Int(-2, -2));

        // test pick up axe for harvest
        Activity pickUpActivity3 = behavior.NextActivity(map, testCreature).Item1;
        Assert(pickUpActivity3 is PickUpActivity, "Activity should be pick up");
        Assert(pickUpActivity3.GetLocation(testCreature, map) == new Vector2Int(-2, -2), "Pick up location");

        map.Remove(axe);

        // test CraftItems
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Axe"], 1);
        Item flint = new(EntityManager.itemTypes["Flint"]);
        map.AddHeld(farm, flint);
        Item log2 = new(EntityManager.itemTypes["Log"]);
        map.AddHeld(farm, log2);
        Activity craftActivity = behavior.NextActivity(map, testCreature).Item1;
        Assert(craftActivity is CraftActivity, "Activity should be craft");
        Assert(craftActivity.GetLocation(testCreature, map) == map.PositionOf(farm), "Craft location");

        // test RestAtHomeBuilding
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Axe"], 0);
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Log"], 0);
        farm.ChangeRequestedItemAmount(EntityManager.itemTypes["Flint"], 0);
        Activity restActivity = behavior.NextActivity(map, testCreature).Item1;
        Assert(restActivity is RestActivity, "Activity should be rest");
        Assert(restActivity.GetLocation(testCreature, map) == map.PositionOf(farm), "Rest location");
    }

    void ConditionsTest()
    {
        Map map = new();
        
        Creature testInflictor = new("George", 1, EntityManager.creatureTypes["Wizard"], Map.NULL_POSITION);
        map.Add(testInflictor, new Vector2Int(5, 5));

        Creature testTarget = new("George", -1, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        map.Add(testTarget, new Vector2Int(5, 6));
        
        testInflictor.LevelUp();
        testInflictor.LevelUp();
        Item staff = new (EntityManager.itemTypes["Wooden Staff"]);
        map.AddHeld(testTarget, staff);
        Assert(testTarget.GetConditionProtectionClass(map, EntityManager.conditions["Weak"]) == 11 + 2 + testTarget.GetAttributeModifier(AttributeScores.STRENGTH), "Condition protection class");

        int startingExperience = testInflictor.GetExperience();
        testInflictor.UseAbility(EntityManager.typeAbilities["Weakness"], map);
        if (testInflictor.GetExperience() > startingExperience)
        {
            Assert(testTarget.ListConditions().Count == 1, "Condition not applied");
            Assert(testTarget.ListConditions()[0].conditionType.GetName() == "Weak", "Condition not applied");
            Assert(testTarget.ListConditions()[0].GetStacks() == 2, "Condition stacks");
        }
        else
        {
            Assert(testTarget.ListConditions().Count == 0, "Condition applied");
        }

        int startingDodge = testInflictor.SkillModifier("dodge", map);
        int peasantConditionsCount = testTarget.ListConditions().Count;
        testInflictor.UseAbility(EntityManager.typeAbilities["Magic Shield"], map);
        Assert(testInflictor.SkillModifier("dodge", map) == startingDodge + 2, "Condition skill bonus not applied");
        Assert(testTarget.ListConditions().Count == peasantConditionsCount, "Condition applied to peasant");
    }

    void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError(message);
        }
    }
}
