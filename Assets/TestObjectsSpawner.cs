using System.Collections;
using UnityEngine;

public class TestObjectsSpawner : MonoBehaviour
{
    public MapDisplayer mapDisplayer;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        Map map = new();
        //Creature testCreature = new ("Scarecrow", 1, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        //map.Add(testCreature, new Vector2Int(10, -5));
        //Creature testCreature2 = new("Scarecrow", 1, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        //map.Add(testCreature2, new Vector2Int(16, -5));

        Item axe2 = new(EntityManager.itemTypes["Axe"]);
        map.Add(axe2, new Vector2Int(7, 7));
        Item woodenSword = new(EntityManager.itemTypes["Wooden Sword"]);
        map.Add(woodenSword, new Vector2Int(2, 2));
        Item woodenShield = new(EntityManager.itemTypes["Wooden Shield"]);
        map.Add(woodenShield, new Vector2Int(1, 2));

        Building testBuilding = new(EntityManager.buildingTypes["Farm"], 1);
        testBuilding.TakeDamage(50);
        map.Add(testBuilding, new Vector2Int(-10, -10));

        Building barracks = new(EntityManager.buildingTypes["Barracks"], 1);
        map.Add(barracks, new Vector2Int(0, -10));

        Building barracks2 = new(EntityManager.buildingTypes["Guild"], 1);
        map.Add(barracks2, new Vector2Int(0, -15));

        Building lair = new(EntityManager.buildingTypes["Graveyard"], -1);
        map.Add(lair, new Vector2Int(10, -10));

        Building lair2 = new(EntityManager.buildingTypes["Graveyard"], -1);
        map.Add(lair2, new Vector2Int(10, -15));

        Building lair3 = new(EntityManager.buildingTypes["Graveyard"], -1);
        map.Add(lair3, new Vector2Int(15, -15));

        Building lair4 = new(EntityManager.buildingTypes["Graveyard"], -1);
        map.Add(lair4, new Vector2Int(10, -20));

        for (int i = 1; i <= 3; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                Prop testProp = new(EntityManager.propTypes["Tree"]);
                map.Add(testProp, new Vector2Int(i * 3, j * 3));
            }
        }
        for (int i = 1; i <= 3; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                Prop testProp = new(EntityManager.propTypes["Rock"]);
                map.Add(testProp, new Vector2Int(i * 3 - 15, j * 3));
            }
        }

        Prop bush = new(EntityManager.propTypes["Bush"]);
        map.Add(bush, new Vector2Int(-20, 0));

        Prop bush2 = new(EntityManager.propTypes["Bush"]);
        map.Add(bush2, new Vector2Int(-22, 0));

        Item log = new(EntityManager.itemTypes["Log"]);
        map.Add(log, new Vector2Int(1, 3));

        mapDisplayer.SetMap(map);
    }
}
