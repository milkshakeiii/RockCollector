using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MapDisplayer : MonoBehaviour
{
    public DisplayGrid displayGrid;
    public uint cellsPerSquare;
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
        DisplayMap(map);
    }

    // Update is called once per framex
    void Update()
    {
        
    }

    void DisplayMap(Map map)
    {
        for (uint x = 0; x < widthInSquares; x++)
        {
            for (uint y = 0; y < heightInSquares; y++)
            {
                List<Placeable> placeablesHere = map.PlaceablesAt(new Vector2Int((int)x, (int)y));
                foreach (Placeable placeable in placeablesHere)
                {
                    displayGrid.DisplaySprite("Art/UI/button",
                        x * cellsPerSquare,
                        y * cellsPerSquare,
                        cellsPerSquare * placeable.SquaresMinimumOne(),
                        cellsPerSquare * placeable.SquaresMinimumOne(),
                        0);
                }
            }
        }
    }
}
