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
    private BuildBuildingButton activePlaceBuildingButton = null;
    private Placeable mouseDownPlaceable = null;

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
        DisplayGrid.MouseUp += OnMouseUpEvent;
        DisplayGrid.MouseDown += OnMouseDownEvent;

        yield return new WaitForSeconds(1);
        map = new ();
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

        Building testBuilding = new (EntityManager.buildingTypes["Farm"], 1);
        testBuilding.TakeDamage(50);
        map.Add(testBuilding, new Vector2Int(-10, -10));

        Building barracks = new (EntityManager.buildingTypes["Barracks"], 1);
        map.Add(barracks, new Vector2Int(0, -10));

        Building barracks2 = new(EntityManager.buildingTypes["Guild"], 1);
        map.Add(barracks2, new Vector2Int(0, -15));

        Building lair = new (EntityManager.buildingTypes["Graveyard"], -1);
        map.Add(lair, new Vector2Int(10, -10));

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
    
    public Map GetMap()
    {
        return map;
    }

    private Vector2Int GetMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = DisplayGrid.HEIGHT / 2f;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return WorldPositionToGamePosition(worldPosition);
    }

    private Vector2Int WorldPositionToGamePosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / (float)cellsPerSquare);
        int y = Mathf.FloorToInt(worldPosition.y / (float)cellsPerSquare);
        return new Vector2Int(x, y);
    }

    void OnMouseUpEvent(Vector3 worldPosition, Vector2Int screenPosition, int mouseButton)
    {
        // always clear the mouseDownPlaceable
        Placeable previousMouseDownPlaceable = mouseDownPlaceable;
        mouseDownPlaceable = null;

        // check if this is a button press
        Debug.Log(screenPosition);
        foreach (RectInt rectInt in buttonRectsToButtons.Keys)
        {
            if (rectInt.Contains(screenPosition))
            {
                Debug.Log("Button clicked " + buttonRectsToButtons[rectInt].text);
                Button callback = buttonRectsToButtons[rectInt];
                callback.OnClick(this);
                return;
            }
        }

        // if the leftMouseSelection is not null and this click is in the left 1/8 of the screen, do nothing
        if (leftMouseSelection.placeable != null && screenPosition.x < -(DisplayGrid.WIDTH/2) + DisplayGrid.WIDTH/8)
        {
            return;
        }

        // if the rightMouseSelection is not null and this click is in the right 1/8 of the screen, do nothing
        if (rightMouseSelection.placeable != null && screenPosition.x > (DisplayGrid.WIDTH/2) - DisplayGrid.WIDTH/8)
        {
            return;
        }

        // otherwise check for building placement
        Vector2Int gamePosition = WorldPositionToGamePosition(worldPosition);
        if (activePlaceBuildingButton != null)
        {
            if (activePlaceBuildingButton.TryMakeCommand(this, gamePosition))
            {
                activePlaceBuildingButton = null;
            }
            return;
        }

        // otherwise check for dragging style input
        if (mouseButton == 0 && previousMouseDownPlaceable != null)
        {
            // if the mouse is over a building and we were dragging from a building, make a transport route
            Placeable mouseUpBuilding = map.PlaceablesAt(gamePosition).Find(placeable => placeable is Building);
            if (mouseUpBuilding != null && previousMouseDownPlaceable is Building mouseDownBuilding && mouseDownBuilding != mouseUpBuilding)
            {
                Building sourceBuilding = (Building)previousMouseDownPlaceable;
                Building targetBuilding = (Building)mouseUpBuilding;
                inputCommands.Add(new MakeTransportRoute(map.PositionOf(sourceBuilding), map.PositionOf(targetBuilding)));
                // visual feedback for the route
                RouteFeedback(sourceBuilding, targetBuilding);
                return;
            }
        }

        // otherwise check for clicked placeables
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

        if (selection.placeable is Building building)
        {
            AllRoutesFeedback(building);
        }
    }

    void OnMouseDownEvent(Vector3 worldPosition, Vector2Int screenPosition, int mouseButton)
    {
        // if the leftMouseSelection is not null and this click is in the left 1/8 of the screen, do nothing
        if (leftMouseSelection.placeable != null && screenPosition.x < -DisplayGrid.WIDTH / 8)
        {
            return;
        }

        // if the rightMouseSelection is not null and this click is in the right 1/8 of the screen, do nothing
        if (rightMouseSelection.placeable != null && screenPosition.x > 7 * DisplayGrid.WIDTH / 8)
        {
            return;
        }

        // don't set mouseDownPlaceable if we are placing a building
        if (activePlaceBuildingButton != null)
        {
            return;
        }

        // otherwise, set the mouseDownPlaceable to the clicked placeable if there is one
        Vector2Int gamePosition = WorldPositionToGamePosition(worldPosition);
        List<Placeable> placeables = map.PlaceablesAt(gamePosition);
        if (placeables.Count > 0)
        {
            mouseDownPlaceable = placeables[0];
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

        DisplayBuildingShadow();
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
        if (placeable is Creature creature)
        {
            DrawCreatureInfoPanel(creature, root, map, displayGrid);
        }
    }

    private void DrawBuildingInfoPanel(Building building, Vector2 rootPosition, Map map, DisplayGrid displayGrid)
    {
        // spawn creature buttons
        List<CreatureType> creatureTypes = building.GetCreatureTypesAvailable();
        for (int i = 0; i < creatureTypes.Count; i++)
        {
            CreatureType creatureType = creatureTypes[i];
            Button spawnCreatureButton = new SpawnCreatureButton("Spawn " + creatureType.GetName(), creatureType.GetName(), map.PositionOf(building));
            RectInt rectInt = new ((int)rootPosition.x + 1, (int)rootPosition.y + 1 + 7*i, DisplayGrid.WIDTH / 8 - 2, 6);
            spawnCreatureButton.Draw(displayGrid, rectInt, map);
            buttonRectsToButtons[rectInt] = spawnCreatureButton;
        }

        // requestable items
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

        // build building buttons
        List<BuildingType> buildableBuildingTypes = building.buildingType.GetBuildableBuildingTypes();
        for (int i = 0; i < buildableBuildingTypes.Count; i++)
        {
            BuildingType buildingType = buildableBuildingTypes[i];
            Button buildBuildingButton = new BuildBuildingButton("Build " + buildingType.GetName(), building.teamNumber, buildingType.GetName(), map.PositionOf(building));
            RectInt rectInt = new((int)rootPosition.x + 1, (int)rootPosition.y + DisplayGrid.HEIGHT - 36 - 7 * (i + 1 + requestableItemTypes.Count), DisplayGrid.WIDTH / 8 - 2, 6);
            buildBuildingButton.Draw(displayGrid, rectInt, map, activePlaceBuildingButton != null);
            buttonRectsToButtons[rectInt] = buildBuildingButton;
        }
    }

    private void DrawCreatureInfoPanel(Creature creature, Vector2 rootPosition, Map map, DisplayGrid displayGrid)
    {
        // display the creature name
        displayGrid.DisplayText(
            creature.GetName() + " - Level " + creature.GetLevel(),
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 4,
            Color.black);

        // display the creature's portrait
        displayGrid.DisplaySprite(
            "Art/UI/plain_white",
            (int)rootPosition.x + 3,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24,
            24,
            24,
            0,
            1,
            true);

        // display the creature's attribute scores
        int str, dex, con, intel, wis, cha;
        str = creature.GetAttributeScore(AttributeScores.STRENGTH);
        dex = creature.GetAttributeScore(AttributeScores.DEXTERITY);
        con = creature.GetAttributeScore(AttributeScores.CONSTITUTION);
        intel = creature.GetAttributeScore(AttributeScores.INTELLIGENCE);
        wis = creature.GetAttributeScore(AttributeScores.WISDOM);
        cha = creature.GetAttributeScore(AttributeScores.CHARISMA);
        string strMod, dexMod, conMod, intMod, wisMod, chaMod;
        strMod = creature.GetAttributeModifier(AttributeScores.STRENGTH).ToString("+0;-#");
        dexMod = creature.GetAttributeModifier(AttributeScores.DEXTERITY).ToString("+0;-#");
        conMod = creature.GetAttributeModifier(AttributeScores.CONSTITUTION).ToString("+0;-#");
        intMod = creature.GetAttributeModifier(AttributeScores.INTELLIGENCE).ToString("+0;-#");
        wisMod = creature.GetAttributeModifier(AttributeScores.WISDOM).ToString("+0;-#");
        chaMod = creature.GetAttributeModifier(AttributeScores.CHARISMA).ToString("+0;-#");
        displayGrid.DisplayText(
            "Strength: " + str + " (" + strMod + ")",
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4,
            Color.black);
        displayGrid.DisplayText(
            "Dexterity: " + dex + " (" + dexMod + ")",
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * 2,
            Color.black);
        displayGrid.DisplayText(
            "Constitution: " + con + " (" + conMod + ")",
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * 3,
            Color.black);
        displayGrid.DisplayText(
            "Intelligence: " + intel + " (" + intMod + ")",
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * 4,
            Color.black);
        displayGrid.DisplayText(
            "Wisdom: " + wis + " (" + wisMod + ")",
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * 5,
            Color.black);
        displayGrid.DisplayText(
            "Charisma: " + cha + " (" + chaMod + ")",
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * 6,
            Color.black);

        // display a divider line
        displayGrid.DisplaySprite(
            "Art/UI/plain_black",
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * 7,
            DisplayGrid.WIDTH / 8 - 2,
            1,
            0,
            1,
            true);

        // display the creature's skills
        List<string> skillNames = creature.ListSkills();
        for (int i = 0; i < skillNames.Count; i++)
        {
            string skillName = skillNames[i];
            displayGrid.DisplayText(
                skillName + " (" + creature.SkillModifier(skillName, map) + ")",
                (int)rootPosition.x + 1,
                (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * (8 + i),
                Color.black);
        }

        // display a divider line
        displayGrid.DisplaySprite(
            "Art/UI/plain_black",
            (int)rootPosition.x + 1,
            (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * (8 + skillNames.Count),
            DisplayGrid.WIDTH / 8 - 2,
            1,
            0,
            1,
            true);

        // display the creature's inventory
        List<Placeable> items = map.HeldPlaceablesOf(creature);
        List<string> inventoryStrings = new List<string>();
        foreach (Placeable placeable in items)
        {
            if (placeable is Item item)
            {
                string newPart = item.itemType.GetName() + ", ";
                if (creature.OutfitContains(item, map))
                {
                    newPart = "<" + newPart + ">";
                }
                if (inventoryStrings.Count == 0 || inventoryStrings[inventoryStrings.Count - 1].Length + newPart.Length > 30)
                {
                    inventoryStrings.Add(newPart);
                }
                else
                {
                    inventoryStrings[inventoryStrings.Count - 1] += newPart;
                }
            }
        }
        for (int i = 0; i < inventoryStrings.Count; i++)
        {
            displayGrid.DisplayText(
                inventoryStrings[i],
                (int)rootPosition.x + 1,
                (int)rootPosition.y + DisplayGrid.HEIGHT - 7 - 24 - 4 * (9 + skillNames.Count + i),
                Color.black);
        }
    }

    public void SetPlaceBuildingButton(BuildBuildingButton button)
    {
        activePlaceBuildingButton = button;
    }

    void DisplayBuildingShadow()
    {
        if (activePlaceBuildingButton == null)
        {
            return;
        }
        Vector2Int mousePosition = GetMousePosition();
        BuildingType buildingType = activePlaceBuildingButton.GetBuildingType();
        displayGrid.DisplaySprite("Art/UI/button",
            mousePosition.x * cellsPerSquare,
            mousePosition.y * cellsPerSquare,
            cellsPerSquare * buildingType.GetSize(),
            cellsPerSquare * buildingType.GetSize(),
            0,
            0,
            true);
    }

    private void AllRoutesFeedback(Building building)
    {
        // visual feedback for all routes to and from the building
        foreach (Placeable placeable in map.UnheldPlaceables())
        {
            if (placeable is Building otherBuilding)
            {
                if (map.TransportRouteExists(building, otherBuilding))
                {
                    RouteFeedback(building, otherBuilding);
                }
                if (map.TransportRouteExists(otherBuilding, building))
                {
                    RouteFeedback(otherBuilding, building);
                }
            }
        }
    }

    private void RouteFeedback(Building sourceBuilding, Building targetBuilding)
    {
        displayGrid.AnimateTo(
            "Art/UI/selected_button",
            (map.PositionOf(sourceBuilding).x + sourceBuilding.buildingType.GetSize() / 2f) * cellsPerSquare - 2,
            (map.PositionOf(sourceBuilding).y + sourceBuilding.buildingType.GetSize() / 2f) * cellsPerSquare - 2,
            (map.PositionOf(targetBuilding).x + targetBuilding.buildingType.GetSize() / 2f) * cellsPerSquare - 2,
            (map.PositionOf(targetBuilding).y + targetBuilding.buildingType.GetSize() / 2f) * cellsPerSquare - 2,
            4, 4, 1f);
    }
}

public abstract class Button
{
    public string text;
    
    public Button(string text)
    {
        this.text = text;
    }

    public virtual void Draw(DisplayGrid displayGrid, RectInt rectInt, Map map, bool placingBuilding = false)
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

    public override void Draw(DisplayGrid displayGrid, RectInt rectInt, Map map, bool placingBuilding = false)
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

public class BuildBuildingButton : Button
{
    private int teamNumber;
    private string buildingTypeName;
    private Vector2Int sourceBuildingPosition;

    public BuildBuildingButton(string text, int teamNumber, string buildingTypeName, Vector2Int sourceBuildingPosition) : base(text)
    {
        this.teamNumber = teamNumber;
        this.buildingTypeName = buildingTypeName;
        this.sourceBuildingPosition = sourceBuildingPosition;
    }

    public BuildingType GetBuildingType()
    {
        return EntityManager.buildingTypes[buildingTypeName];
    }

    public override void Draw(DisplayGrid displayGrid, RectInt rectInt, Map map, bool placingBuilding = false)
    {
        string spriteName = "Art/UI/plain_white";
        if (placingBuilding)
        {
            spriteName = "Art/UI/selected_button";
        }
        Building sourceBuilding = GetSourceBuilding(map);
        if (!sourceBuilding.BuildBuildingInputMaterialsPresent(EntityManager.buildingTypes[buildingTypeName], map))
        {
            spriteName = "Art/UI/selected_button";
        }
        displayGrid.DisplaySprite(spriteName,
            rectInt.x,
            rectInt.y,
            rectInt.width,
            rectInt.height,
            0,
            overlapLayer: 1);
        displayGrid.DisplayText(text, rectInt.x + 1, rectInt.y + rectInt.height / 2, Color.black);
    }

    private Building GetSourceBuilding(Map map)
    {
        List<Placeable> placeables = map.PlaceablesAt(sourceBuildingPosition);
        foreach (Placeable placeable in placeables)
        {
            if (placeable is Building building && building.teamNumber == teamNumber)
            {
                return building;
            }
        }
        return null;
    }

    public override void OnClick(MapDisplayer mapDisplayer)
    {
        Map map = mapDisplayer.GetMap();
        Building sourceBuilding = GetSourceBuilding(map);
        if (sourceBuilding == null)
        {
            return;
        }
        if (sourceBuilding.BuildBuildingInputMaterialsPresent(EntityManager.buildingTypes[buildingTypeName], map))
        {
            mapDisplayer.SetPlaceBuildingButton(this);
        }
    }

    public bool TryMakeCommand( MapDisplayer mapDisplayer, Vector2Int builtBuildingPosition)
    {
        BuildBuilding buildBuilding = new(teamNumber, buildingTypeName, sourceBuildingPosition, builtBuildingPosition);
        if (buildBuilding.CheckStillValid(mapDisplayer.GetMap()))
        {
            mapDisplayer.AddInputCommand(buildBuilding);
            return true;
        }
        return false;
    }
}