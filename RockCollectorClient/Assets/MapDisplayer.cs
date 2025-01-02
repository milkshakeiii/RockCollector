using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MapDisplayer : MonoBehaviour
{
    public DisplayGrid displayGrid;
    public int cellsPerSquare;
    public uint widthInSquares;
    public uint heightInSquares;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Map map = new Map();
        Creature testCreature = new ("George", EntityManager.creatureTypes["Peasant"]);
        map.Add(testCreature, new Vector2Int(0, 0));
        Building testBuilding = new (EntityManager.buildingTypes["Farm"]);
        map.Add(testBuilding, new Vector2Int(5, 5));
        Prop testProp = new (EntityManager.propTypes["Tree"]);
        map.Add(testProp, new Vector2Int(-5, -5));
        DisplayMap(map);
    }

    // Update is called once per framex
    void Update()
    {
        
    }

    void DisplayMap(Map map)
    {
        foreach (Placeable placeable in map.AllPlaceables())
        {
            Vector2Int position = map.PositionOf(placeable);
            displayGrid.DisplaySprite("Art/UI/button",
                position.x * cellsPerSquare,
                position.y * cellsPerSquare,
                cellsPerSquare * placeable.SquaresMinimumOne(),
                cellsPerSquare * placeable.SquaresMinimumOne(),
                0);
        }
    }
}
