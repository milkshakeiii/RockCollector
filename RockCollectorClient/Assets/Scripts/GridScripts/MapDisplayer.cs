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

    private struct Selection
    {
        public Vector2Int position;
        public Placeable placeable;
        public int placeableIndex;
    }

    private Selection leftMouseSelection;
    private Selection rightMouseSelection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        DisplayGrid.MouseUp += OnSelect;

        yield return new WaitForSeconds(1);
        map = new ();
        Creature testCreature = new ("George", 1, EntityManager.creatureTypes["Peasant"]);
        map.Add(testCreature, new Vector2Int(0, 0));
        Creature testCreature2 = new("George2", 1, EntityManager.creatureTypes["Peasant"]);
        map.Add(testCreature2, new Vector2Int(10, 0));

        Creature testCreature3 = new("Skele", -1, EntityManager.creatureTypes["Skeleton"]);
        map.Add(testCreature3, new Vector2Int(-5, 0));
        Item axe = new (EntityManager.itemTypes["Axe"]);
        map.AddHeld(testCreature3, axe);

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
        for (int i = 1; i <= 3; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                Prop testProp = new(EntityManager.propTypes["Rock"]);
                map.Add(testProp, new Vector2Int(i * 3 - 15, j * 3));
            }
        }
        DisplayMap(map);
    }

    void OnSelect(Vector3 worldPos, int mouseButton)
    {
        int x = Mathf.FloorToInt(worldPos.x / (float)cellsPerSquare);
        int y = Mathf.FloorToInt(worldPos.y / (float)cellsPerSquare);
        Vector2Int gamePosition = new (x, y);

        Selection selection;
        if (mouseButton == 0)
        {
            selection = leftMouseSelection;
        } 
        else
        {
            selection = rightMouseSelection;
        }

        List<Placeable> placeables = map.PlaceablesAt(gamePosition);
        // if there is no placeable at the position, clear the selection
        if (placeables.Count == 0)
        {
            selection.placeable = null;
        }
        else if (gamePosition == selection.position)
        {
            // if the position is the same, cycle through the placeables
            selection.placeableIndex++;
            selection.placeableIndex %= placeables.Count;
            selection.placeable = placeables[selection.placeableIndex];
        }
        else
        {
            // if the position is different, select the first placeable
            selection.position = gamePosition;
            selection.placeableIndex = 0;
            selection.placeable = placeables[0];
        }

        if (mouseButton == 0)
        {
            leftMouseSelection = selection;
        }
        else
        {
            rightMouseSelection = selection;
        }
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
            DisplayLabels(placeable, position);
        }

        DisplayInfoPanel(leftMouseSelection.placeable, new Vector2(-DisplayGrid.WIDTH/2, -DisplayGrid.HEIGHT/2f));
        DisplayInfoPanel(rightMouseSelection.placeable, new Vector2(3*DisplayGrid.WIDTH/8,-DisplayGrid.HEIGHT/2f));
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

    void DisplayLabels(Placeable placeable, Vector2Int position)
    {
        if (placeable is Creature creature)
        {
            displayGrid.DisplayText(creature.GetName() + " (" + creature.GetLevel() + ")",
                position.x * cellsPerSquare,
                position.y * cellsPerSquare + creature.SquaresMinimumOne() * cellsPerSquare + 1);
        }
       
    }

    void DisplayInfoPanel(Placeable placeable, Vector2 root)
    {
        if (placeable == null)
        {
            return;
        }
        displayGrid.DisplaySprite("Art/UI/button",
            root.x,
            root.y,
            DisplayGrid.WIDTH/8,
            DisplayGrid.HEIGHT,
            0,
            0,
            true);
    }
}
