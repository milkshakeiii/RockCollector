using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MapDisplayer : MonoBehaviour
{
    public DisplayGrid displayGrid;
    public int cellsPerSquare;
    public uint widthInSquares;
    public uint heightInSquares;

    public float secondsPerTurn = 0.1f;
    public float lastTurnTime = 0;

    private Map map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        map = new ();
        Creature testCreature = new ("George", 0, EntityManager.creatureTypes["Peasant"]);
        map.Add(testCreature, new Vector2Int(0, 0));
        Building testBuilding = new (EntityManager.buildingTypes["Farm"]);
        testBuilding.TakeDamage(50);
        map.Add(testBuilding, new Vector2Int(-10, -10));
        for (int i = 1; i <= 3; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                Prop testProp = new (EntityManager.propTypes["Tree"]);
                map.Add(testProp, new Vector2Int(i*3, j*3));
            }
        }
        DisplayMap(map);
    }

    // Update is called once per framex
    void Update()
    {
        if (map == null)
        {
            return;
        }
        if (Time.time - lastTurnTime > secondsPerTurn)
        {
            lastTurnTime = Time.time;
            map.AdvanceTick();
            DisplayMap(map);
        }
    }

    void DisplayMap(Map map)
    {
        displayGrid.Clear();
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            Vector2Int position = map.PositionOf(placeable);
            displayGrid.DisplaySprite("Art/UI/button",
                position.x * cellsPerSquare,
                position.y * cellsPerSquare,
                cellsPerSquare * placeable.SquaresMinimumOne(),
                cellsPerSquare * placeable.SquaresMinimumOne(),
                0);
            DisplayBars(placeable, position);
        }
    }

    void DisplayBars(Placeable placeable, Vector2Int position)
    {
        if (placeable is Destructable destructable)
        {
            displayGrid.DisplaySprite("Art/UI/button",
                position.x * cellsPerSquare,
                position.y * cellsPerSquare - 1,
                cellsPerSquare * destructable.SquaresMinimumOne(),
                1,
                0);
            displayGrid.DisplaySprite("Art/UI/plain_white",
                position.x * cellsPerSquare,
                position.y * cellsPerSquare - 1,
                (int)(cellsPerSquare * destructable.SquaresMinimumOne() * destructable.HealthFraction()),
                1,
                0,
                overlapLayer: 1);
        }
        if (placeable is Prop prop)
        {
            displayGrid.DisplaySprite("Art/UI/button",
                position.x * cellsPerSquare,
                position.y * cellsPerSquare - 2,
                cellsPerSquare * prop.SquaresMinimumOne(),
                2,
                0);
            displayGrid.DisplaySprite("Art/UI/plain_white",
                position.x * cellsPerSquare,
                position.y * cellsPerSquare - 2,
                (int)(cellsPerSquare * prop.SquaresMinimumOne() * prop.HarvestedFraction()),
                2,
                0,
                overlapLayer: 1);
        }
    }
}
