using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class MapDisplayer : MonoBehaviour
{
    public DisplayGrid displayGrid;
    public int cellsPerSquare;
    public uint widthInSquares;
    public uint heightInSquares;

    public float secondsPerTurn = 0.1f;
    public float lastTurnTime = 0;

    private Map map;
    private List<MapCommand> inputCommands = new();

    private struct Selection
    {
        public Vector2Int position;
        public Placeable placeable;
        public int placeableIndex;
    }

    private Selection leftMouseSelection;
    private Selection rightMouseSelection;

    private Dictionary<RectInt, Button> buttonRectsToButtons = new ();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        DisplayGrid.MouseUp += OnSelect;

        yield return new WaitForSeconds(1);
        map = new ();
        Creature testCreature = new ("Scarecrow", 1, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        map.Add(testCreature, new Vector2Int(10, -5));
        Creature testCreature2 = new("Scarecrow", 1, EntityManager.creatureTypes["Peasant"], Map.NULL_POSITION);
        map.Add(testCreature2, new Vector2Int(16, -5));

        Item axe2 = new(EntityManager.itemTypes["Axe"]);
        map.Add(axe2, new Vector2Int(7, 7));

        Building testBuilding = new (EntityManager.buildingTypes["Farm"], 1);
        testBuilding.TakeDamage(50);
        map.Add(testBuilding, new Vector2Int(-10, -10));

        Building lair = new (EntityManager.buildingTypes["Graveyard"], -1);
        map.Add(lair, new Vector2Int(10, -10));
        Item axe = new(EntityManager.itemTypes["Axe"]);
        map.Add(axe, new Vector2Int(9, -10));

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

    void OnSelect(Vector3 worldPosition, Vector2Int screenPosition, int mouseButton)
    {
        // check if this is a button press
        Debug.Log(screenPosition);
        foreach (RectInt rectInt in buttonRectsToButtons.Keys)
        {
            Debug.Log("Checking button rect " + rectInt);
            if (rectInt.Contains(screenPosition))
            {
                Debug.Log("Button clicked");
                Button callback = buttonRectsToButtons[rectInt];
                callback.OnClick(this);
                return;
            }
        }

        // otherwise check for clicked placeables
        int x = Mathf.FloorToInt(worldPosition.x / (float)cellsPerSquare);
        int y = Mathf.FloorToInt(worldPosition.y / (float)cellsPerSquare);
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
            map.AdvanceTick(inputCommands);
            inputCommands.Clear();
            DisplayMap(map);
        }
    }

    public void AddInputCommand(MapCommand command)
    {
        inputCommands.Add(command);
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

        buttonRectsToButtons.Clear();
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
        displayGrid.DisplaySprite("Art/UI/button", // the panel background
            root.x,
            root.y,
            DisplayGrid.WIDTH/8,
            DisplayGrid.HEIGHT,
            0,
            0,
            true);
        if (placeable is Building building)
        {
            DrawBuildingInfoPanel(building, root, map, displayGrid);
        }
    }

    private void DrawBuildingInfoPanel(Building building, Vector2 rootPosition, Map map, DisplayGrid displayGrid)
    {
        List<CreatureType> creatureTypes = building.GetCreatureTypesAvailable();
        for (int i = 0; i < creatureTypes.Count; i++)
        {
            CreatureType creatureType = creatureTypes[i];
            Button spawnCreatureButton = new SpawnCreatureButton("Spawn " + creatureType.GetName(), creatureType.GetName(), map.PositionOf(building));
            RectInt rectInt = new ((int)rootPosition.x + 1, (int)rootPosition.y + 1 + 7*i, DisplayGrid.WIDTH / 8 - 2, 6);
            spawnCreatureButton.Draw(displayGrid, rectInt, map);
            buttonRectsToButtons[rectInt] = spawnCreatureButton;
        }

        List<ItemType> requestableItemTypes = building.GetRequestableItemTypes();
        for (int i = 0; i < requestableItemTypes.Count; i++)
        {
            ItemType itemType = requestableItemTypes[i];
            int currentlyRequested = building.GetRequestedItemAmount(itemType);
            
            // display the + button to increase the requested amount
            Button increaseRequestButton = new ChangeItemRequestsButton("+", itemType.GetName(), currentlyRequested + 1, map.PositionOf(building));
            RectInt rectInt = new((int)rootPosition.x + 1, (int)rootPosition.y + DisplayGrid.HEIGHT - 1 - 7 * (i + 1), 6, 6);
            increaseRequestButton.Draw(displayGrid, rectInt, map);
            buttonRectsToButtons[rectInt] = increaseRequestButton;

            // display the - button to decrease the requested amount
            Button decreaseRequestButton = new ChangeItemRequestsButton("-", itemType.GetName(), currentlyRequested - 1, map.PositionOf(building));
            rectInt = new((int)rootPosition.x + DisplayGrid.WIDTH / 8 - 7, (int)rootPosition.y + DisplayGrid.HEIGHT - 1 - 7 * (i + 1), 6, 6);
            decreaseRequestButton.Draw(displayGrid, rectInt, map);
            buttonRectsToButtons[rectInt] = decreaseRequestButton;

            // display the item name and (amount present/amount requested)
            displayGrid.DisplayText(itemType.GetName() + " (" + building.GetItemCount(itemType, map) + "/" + currentlyRequested + ")",
                (int)rootPosition.x + 8,
                (int)rootPosition.y + DisplayGrid.HEIGHT - 1 - 7 * (i + 1),
                Color.black);
        }
    }
}

public abstract class Button
{
    public string text;
    
    public Button(string text)
    {
        this.text = text;
    }

    public virtual void Draw(DisplayGrid displayGrid, RectInt rectInt, Map map)
    {
        displayGrid.DisplaySprite("Art/UI/plain_white",
            rectInt.x,
            rectInt.y,
            rectInt.width,
            rectInt.height,
            0,
            overlapLayer: 1);
        displayGrid.DisplayText(text, rectInt.x+1, rectInt.y+ rectInt.height / 2, Color.black);
    }

    public abstract void OnClick(MapDisplayer mapDisplayer);
}

public class SpawnCreatureButton : Button
{
    private string creatureTypeName;
    private Vector2Int buildingPosition;

    public SpawnCreatureButton(string text, string creatureTypeName, Vector2Int buildingPosition) : base(text)
    {
        this.creatureTypeName = creatureTypeName;
        this.buildingPosition = buildingPosition;
    }

    public override void Draw(DisplayGrid displayGrid, RectInt rectInt, Map map)
    {
        // get the building at the position
        List<Placeable> placeables = map.PlaceablesAt(buildingPosition);
        Building building = null;
        foreach (Placeable placeable in placeables)
        {
            if (placeable is Building)
            {
                building = (Building)placeable;
                break;
            }
        }
        if (building == null)
        {
            throw new System.Exception("No building at position");
        }

        if (building.IsSpawning()) {
            // get the portion completion of the current creature spawn
            float completion = building.CreatureSpawnCompletion();
            // display the completion bar
            displayGrid.DisplaySprite("Art/UI/selected_button",
                rectInt.x,
                rectInt.y,
                Mathf.RoundToInt(rectInt.width * completion),
                rectInt.height,
                0,
                overlapLayer: 2);
        }
        displayGrid.DisplaySprite("Art/UI/plain_white",
            rectInt.x,
            rectInt.y,
            rectInt.width,
            rectInt.height,
            0,
            overlapLayer: 1);
        displayGrid.DisplayText(text, rectInt.x + 1, rectInt.y + rectInt.height / 2, Color.black);
    }

    public override void OnClick(MapDisplayer mapDisplayer)
    {
        mapDisplayer.AddInputCommand(new SpawnCreature(creatureTypeName, buildingPosition));
    }
}

public class ChangeItemRequestsButton : Button
{
    private string itemName;
    private int amount;
    private Vector2Int buildingPosition;

    public ChangeItemRequestsButton(string text, string itemName, int amount, Vector2Int buildingPosition) : base(text)
    {
        this.itemName = itemName;
        this.amount = amount;
        this.buildingPosition = buildingPosition;
    }

    public override void OnClick(MapDisplayer mapDisplayer)
    {
        mapDisplayer.AddInputCommand(new ChangeRequestedItemAmount(itemName, amount, buildingPosition));
    }
}