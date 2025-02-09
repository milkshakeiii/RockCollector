using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System;
using static UnityEngine.GraphicsBuffer;

public class MapDisplayer : MonoBehaviour
{
    public TMPro.TMP_Text fpsText;
    // array of 15 floats to average for fps calculation
    private float[] fpsBuffer = new float[15];

    public DisplayGrid displayGrid;
    public int cellsPerSquare;
    public uint widthInSquares;
    public uint heightInSquares;

    public float secondsPerTurn = 0.1f;
    public float lastTurnTime = 0;

    private Gamestate gamestate = null;
    private List<MapCommand> inputCommands = new();
    private BuildBuildingButton activePlaceBuildingButton = null;
    private Placeable mouseDownPlaceable = null;

    private struct Selection
    {
        public Vector2Int position;
        public Placeable placeable;
        public Entity entity; // takes priority over placeable to be displayed in the info panel
        public int placeableIndex;
    }

    private Selection leftMouseSelection;
    private Selection rightMouseSelection;

    private Dictionary<RectInt, Button> buttonRectsToButtons = new ();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DisplayGrid.MouseUp += OnMouseUpEvent;
        DisplayGrid.MouseDown += OnMouseDownEvent;

        Simulation.OnWeaponStrike += OnWeaponStrikeEvent;
        Simulation.OnNonweaponStrike += OnNonweaponStrikeEvent;
        Simulation.OnConditionApplied += OnConditionAppliedEvent;
    }

    private void OnWeaponStrikeEvent(Creature actor, Vector2Int target, Item weapon, Map map)
    {
        if (weapon == null)
        {
            return;
        }
        displayGrid.AnimateTo(
            weapon.itemType.GetSpritePath(),
            map.PositionOf(actor).x * cellsPerSquare,
            map.PositionOf(actor).y * cellsPerSquare,
            target.x * cellsPerSquare,
            target.y * cellsPerSquare,
            cellsPerSquare,
            cellsPerSquare,
            0.5f);
    }

    private void OnNonweaponStrikeEvent(Creature actor, Vector2Int target, TypeAbility ability, Map map)
    {
        int radius = ability.GetEffectRadius();
        displayGrid.Fade(
            ability.GetEffectSpritePath(),
            (target.x - radius) * cellsPerSquare,
            (target.y - radius) * cellsPerSquare,
            (radius*2 + 1) * cellsPerSquare,
            (radius*2 + 1) * cellsPerSquare,
            0.5f);
    }

    private void OnConditionAppliedEvent(Creature actor, Creature target, ConditionType condition, Map map)
    {
        Debug.Log("Condition applied: " + condition.GetSpritePath());
        Vector2Int targetPosition = map.PositionOf(target);
        int size = target.SquaresMinimumOne();
        displayGrid.Fade(
            condition.GetSpritePath(),
            (targetPosition.x) * cellsPerSquare,
            (targetPosition.y) * cellsPerSquare,
            size * cellsPerSquare,
            size * cellsPerSquare,
            0.5f);
    }
    
    public Map GetMap()
    {
        if (gamestate == null)
        {
            return null;
        }
        return gamestate.map;
    }

    public void SetGamestate(Gamestate gamestate)
    {
        this.gamestate = gamestate;
    }

    private Vector2Int GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
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
        if (GetMap() == null)
        {
            return;
        }

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
                callback.OnClick(this, mouseButton);
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
            Placeable mouseUpBuilding = GetMap().PlaceablesAt(gamePosition).Find(placeable => placeable is Building);
            if (mouseUpBuilding != null && previousMouseDownPlaceable is Building mouseDownBuilding && mouseDownBuilding != mouseUpBuilding)
            {
                Building sourceBuilding = (Building)previousMouseDownPlaceable;
                Building targetBuilding = (Building)mouseUpBuilding;
                inputCommands.Add(new MakeTransportRoute(GetMap().PositionOf(sourceBuilding), GetMap().PositionOf(targetBuilding)));
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

        List<Placeable> placeables = GetMap().PlaceablesAt(gamePosition);
        // if there is no placeable at the position, clear the selection
        if (placeables.Count == 0)
        {
            selection.placeable = null;
            selection.entity = null;
        }
        else if (gamePosition == selection.position)
        {
            // if the position is the same, cycle through the placeables
            selection.placeableIndex++;
            selection.placeableIndex %= placeables.Count;
            selection.placeable = placeables[selection.placeableIndex];
            selection.entity = null;
        }
        else
        {
            // if the position is different, select the first placeable
            selection.position = gamePosition;
            selection.placeableIndex = 0;
            selection.placeable = placeables[0];
            selection.entity = null;
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
        if (GetMap() == null)
        {
            return;
        }

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
        List<Placeable> placeables = GetMap().PlaceablesAt(gamePosition);
        if (placeables.Count > 0)
        {
            mouseDownPlaceable = placeables[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        // calculate fps
        float fps = 1.0f / Time.deltaTime;
        int fpsBufferIndex = (int)Time.frameCount % fpsBuffer.Length;
        fpsBuffer[fpsBufferIndex] = fps;
        float sum = 0;
        foreach (float f in fpsBuffer)
        {
            sum += f;
        }
        fpsText.text = "FPS: " + ((int)(sum / fpsBuffer.Length) + 1);
        if (gamestate != null)
        {
            fpsText.text += " kTick: " + gamestate.map.CurrentTick() / 1000;
        }

        // advance the gamestate
        if (gamestate == null)
        {
            return;
        }
        //if (Time.time - lastTurnTime > secondsPerTurn)
        //{
            lastTurnTime = Time.time;
            Simulation.AdvanceTick(gamestate, inputCommands);
            inputCommands.Clear();
            DisplayMap(GetMap());
        //}
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
            displayGrid.DisplaySprite(placeable.GetSpritePath(),
                position.x * cellsPerSquare,
                position.y * cellsPerSquare,
                cellsPerSquare * placeable.SquaresMinimumOne(),
                cellsPerSquare * placeable.SquaresMinimumOne(),
                0);
            DisplayBars(placeable, position);
            // DisplayLabels(placeable, position);
        }

        buttonRectsToButtons.Clear();

        if (leftMouseSelection.placeable != null && !map.PlaceableExists(leftMouseSelection.placeable))
        {
            if (map.PlaceablesAt(leftMouseSelection.position).Count > 0)
            {
                leftMouseSelection.placeable = map.PlaceablesAt(leftMouseSelection.position)[0];
            }
            else
            {
                leftMouseSelection.placeable = null;
            }
        }
        if (rightMouseSelection.placeable != null && !map.PlaceableExists(rightMouseSelection.placeable))
        {
            if (map.PlaceablesAt(rightMouseSelection.position).Count > 0)
            {
                rightMouseSelection.placeable = map.PlaceablesAt(rightMouseSelection.position)[0];
            }
            else
            {
                rightMouseSelection.placeable = null;
            }
        }

        DisplayInfoPanel(leftMouseSelection.placeable, leftMouseSelection.entity, new Vector2(-DisplayGrid.WIDTH/2, -DisplayGrid.HEIGHT/2f));
        DisplayInfoPanel(rightMouseSelection.placeable, rightMouseSelection.entity, new Vector2(3*DisplayGrid.WIDTH/8,-DisplayGrid.HEIGHT/2f));

        DisplayBuildingShadow();
    }

    void DisplayBars(Placeable placeable, Vector2Int position)
    {
        if (placeable is Destructable destructable && placeable is not Prop)
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
        if (placeable is Prop prop && prop.propType.GetHarvestingRequired() != 0)
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

    void DisplayInfoPanel(Placeable placeable, Entity entity, Vector2 root)
    {
        if (placeable == null && entity == null)
        {
            return;
        }
        displayGrid.DisplaySprite("Art/UI/button", // the panel background
            root.x,
            root.y,
            DisplayGrid.WIDTH/8,
            DisplayGrid.HEIGHT,
            0,
            5,
            true);
        if (entity is ItemType itemType)
        {
            DrawItemTypeInfoPanel(itemType, root, GetMap(), displayGrid);
        }
        else if (placeable is Building building)
        {
            DrawBuildingInfoPanel(building, root, GetMap(), displayGrid);
        }
        else if (placeable is Creature creature)
        {
            DrawCreatureInfoPanel(creature, root, GetMap(), displayGrid);
        }
        else if (placeable is Prop prop)
        {
            DrawPropInfoPanel(prop, root, GetMap(), displayGrid);
        }
        else if (placeable is Item item)
        {
            DrawItemTypeInfoPanel(item.itemType, root, GetMap(), displayGrid);
        }
    }

    private void DrawBuildingInfoPanel(Building building, Vector2 rootPosition, Map map, DisplayGrid displayGrid)
    {

        // display the building name
        int height = (int)rootPosition.y + DisplayGrid.HEIGHT - 4;
        displayGrid.DisplayText(
            building.buildingType.GetName(),
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);

        // display the building portrait
        height -= 3 + 24;
        displayGrid.DisplaySprite(
            "Art/UI/plain_white",
            (int)rootPosition.x + 3,
            height,
            24,
            24,
            0,
            6,
            true);
        
        // requestable items
        List<ItemType> requestableItemTypes = building.GetRequestableItemTypes();
        for (int i = 0; i < requestableItemTypes.Count; i++)
        {
            ItemType itemType = requestableItemTypes[i];
            int currentlyRequested = building.GetRequestedItemAmount(itemType);

            // display the + button to increase the requested amount
            height -= 7;
            Button increaseRequestButton = new ChangeItemRequestsButton("+", itemType.GetName(), currentlyRequested + 1, map.PositionOf(building));
            RectInt rectInt = new((int)rootPosition.x + 1, height, 3, 6);
            increaseRequestButton.Draw(displayGrid, rectInt, map);
            buttonRectsToButtons[rectInt] = increaseRequestButton;

            // display the - button to decrease the requested amount
            Button decreaseRequestButton = new ChangeItemRequestsButton("-", itemType.GetName(), currentlyRequested - 1, map.PositionOf(building));
            rectInt = new((int)rootPosition.x + DisplayGrid.WIDTH / 8 - 4, height, 3, 6);
            decreaseRequestButton.Draw(displayGrid, rectInt, map);
            buttonRectsToButtons[rectInt] = decreaseRequestButton;

            // display the item name and (amount present/amount requested)
            displayGrid.DisplayText(itemType.GetName(),
                (int)rootPosition.x + DisplayGrid.WIDTH / 16 - 2,
                height + 4,
                Color.black,
                true);

            displayGrid.DisplayText("(" + building.GetItemCount(itemType, map) + "/" + currentlyRequested + ")",
                (int)rootPosition.x + DisplayGrid.WIDTH / 16 - 2,
                height + 1,
                Color.black,
                true);

            // display the item icon button
            Button itemIconButton = new ItemIconButton(itemType);
            rectInt = new((int)rootPosition.x + 5, height, 6, 6);
            itemIconButton.Draw(displayGrid, rectInt, map);
            buttonRectsToButtons[rectInt] = itemIconButton;
        }
        
        // display a divider line
        height -= 4;
        displayGrid.DisplaySprite(
            "Art/UI/plain_black",
            (int)rootPosition.x + 1,
            height,
            DisplayGrid.WIDTH / 8 - 2,
            1,
            0,
            1,
            true);

        // build building buttons
        List<BuildingType> buildableBuildingTypes = building.buildingType.GetBuildableBuildingTypes();
        for (int i = 0; i < buildableBuildingTypes.Count; i++)
        {
            BuildingType buildingType = buildableBuildingTypes[i];
            Button buildBuildingButton = new BuildBuildingButton("Build " + buildingType.GetName(), building.teamNumber, buildingType.GetName(), map.PositionOf(building));
            height -= 7;
            RectInt rectInt = new((int)rootPosition.x + 1, height, DisplayGrid.WIDTH / 8 - 2, 6);
            buildBuildingButton.Draw(displayGrid, rectInt, map, activePlaceBuildingButton != null);
            buttonRectsToButtons[rectInt] = buildBuildingButton;
        }

        // display a divider line
        height -= 4;
        displayGrid.DisplaySprite(
            "Art/UI/plain_black",
            (int)rootPosition.x + 1,
            height,
            DisplayGrid.WIDTH / 8 - 2,
            1,
            0,
            1,
            true);

        // spawn creature buttons
        List<CreatureType> creatureTypes = building.GetCreatureTypesAvailable();
        for (int i = 0; i < creatureTypes.Count; i++)
        {
            CreatureType creatureType = creatureTypes[i];
            Button spawnCreatureButton = new SpawnCreatureButton("Spawn " + creatureType.GetName(), creatureType.GetName(), map.PositionOf(building));
            height -= 7;
            RectInt rectInt = new((int)rootPosition.x + 1, height, DisplayGrid.WIDTH / 8 - 2, 6);
            spawnCreatureButton.Draw(displayGrid, rectInt, map);
            buttonRectsToButtons[rectInt] = spawnCreatureButton;
        }
    }

    private void DrawCreatureInfoPanel(Creature creature, Vector2 rootPosition, Map map, DisplayGrid displayGrid)
    {
        // display the creature name
        int height = (int)rootPosition.y + DisplayGrid.HEIGHT - 4;
        displayGrid.DisplayText(
            "<" + creature.GetName() + ">",
            (int)rootPosition.x + 3,
            height,
            Color.black,
            true);

        height -= 3;
        displayGrid.DisplayText(
            "Level " + creature.GetLevel() + " " + creature.GetCreatureType().GetName(),
            (int)rootPosition.x + 3,
            height,
            Color.black,
            true);

        // display the creature's portrait
        height -= 2 + 24;
        displayGrid.DisplaySprite(
            "Art/UI/plain_white",
            (int)rootPosition.x + 3,
            height,
            24,
            24,
            0,
            6,
            true);

        // display the creature's activity string
        height -= 4;
        displayGrid.DisplayText(
            "<" + creature.GetActivityString() + ">",
            (int)rootPosition.x + 1,
            height,
            Color.black,
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
        height -= 4;
        displayGrid.DisplayText(
            "Strength: " + str + " (" + strMod + ")",
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);
        height -= 4;
        displayGrid.DisplayText(
            "Dexterity: " + dex + " (" + dexMod + ")",
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);
        height -= 4;
        displayGrid.DisplayText(
            "Constitution: " + con + " (" + conMod + ")",
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);
        height -= 4;
        displayGrid.DisplayText(
            "Intelligence: " + intel + " (" + intMod + ")",
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);
        height -= 4;
        displayGrid.DisplayText(
            "Wisdom: " + wis + " (" + wisMod + ")",
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);
        height -= 4;
        displayGrid.DisplayText(
            "Charisma: " + cha + " (" + chaMod + ")",
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);

        // display a divider line
        height -= 4;
        displayGrid.DisplaySprite(
            "Art/UI/plain_black",
            (int)rootPosition.x + 1,
            height,
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
            height -= 4;
            displayGrid.DisplayText(
                skillName + " (" + creature.SkillModifier(skillName, map) + ")",
                (int)rootPosition.x + 1,
                height,
                Color.black,
                true);
        }

        // display a divider line
        height -= 4;
        displayGrid.DisplaySprite(
            "Art/UI/plain_black",
            (int)rootPosition.x + 1,
            height,
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
            height -= 4;
            displayGrid.DisplayText(
                inventoryStrings[i],
                (int)rootPosition.x + 1,
                height,
                Color.black,
                true);
        }

        // display a divider line
        height -= 4;
        displayGrid.DisplaySprite(
            "Art/UI/plain_black",
            (int)rootPosition.x + 1,
            height,
            DisplayGrid.WIDTH / 8 - 2,
            1,
            0,
            1,
            true);

        // display the creature's conditions
        List<Condition> conditions = creature.ListConditions();
        List<string> conditionStrings = new List<string>();
        foreach (Condition condition in conditions)
        {
            string newPart = condition.conditionType.GetName() + " (" + condition.GetStacks() + "), ";
            if (conditionStrings.Count == 0 || conditionStrings[conditionStrings.Count - 1].Length + newPart.Length > 30)
            {
                conditionStrings.Add(newPart);
            }
            else
            {
                conditionStrings[conditionStrings.Count - 1] += newPart;
            }
        }
        for (int i = 0; i < conditionStrings.Count; i++)
        {
            height -= 4;
            displayGrid.DisplayText(
                conditionStrings[i],
                (int)rootPosition.x + 1,
                height,
                Color.black,
                true);
        }
    }

    private void DrawPropInfoPanel(Prop prop, Vector2 rootPosition, Map map, DisplayGrid displayGrid)
    {
        // display the prop name
        int height = (int)rootPosition.y + DisplayGrid.HEIGHT - 4;
        displayGrid.DisplayText(
            prop.propType.GetName(),
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);

        // display the prop portrait
        height -= 3 + 24;
        displayGrid.DisplaySprite(
            "Art/UI/plain_white",
            (int)rootPosition.x + 3,
            height,
            24,
            24,
            0,
            6,
            true);

        // display item drop chances
        foreach (ItemType itemType in prop.propType.GetProducedItems()) {
            height -= 4;
            displayGrid.DisplayText(
                itemType.GetName() + "(" + ((int)(prop.ChanceOfItemDrop(itemType) * 100)) + "%)",
                (int)rootPosition.x + 1,
                height,
                Color.black,
                true);
        }
    }

    private void DrawItemTypeInfoPanel(ItemType itemType, Vector2 rootPosition, Map map, DisplayGrid displayGrid)
    {
        // display the item name
        int height = (int)rootPosition.y + DisplayGrid.HEIGHT - 4;
        displayGrid.DisplayText(
            itemType.GetName(),
            (int)rootPosition.x + 1,
            height,
            Color.black,
            true);

        // display the item portrait
        height -= 3 + 24;
        displayGrid.DisplaySprite(
            itemType.GetSpritePath(),
            (int)rootPosition.x + 3,
            height,
            24,
            24,
            0,
            6,
            true);

        // display the power rating and required crafting skill
        int equipmentLevel = itemType.GetEquipmentLevel();
        if (equipmentLevel > 0)
        {
            height -= 4;
            displayGrid.DisplayText(
                "Power rating: " + equipmentLevel + " " + itemType.GetEquipmentCategory(),
                (int)rootPosition.x + 1,
                height,
                Color.black,
                true);
        }

        string craftingSkill = itemType.GetCraftingSkill();
        if (craftingSkill != "none")
        {
            height -= 4;
            displayGrid.DisplayText(
                "Crafting skill: ",
                (int)rootPosition.x + 1,
                height,
                Color.black,
                true);
            height -= 3;
            displayGrid.DisplayText(
                itemType.GetCraftingSkill() + " " + itemType.GetCraftingLevel().ToString("+0;-#"),
                (int)rootPosition.x + 4,
                height,
                Color.black,
                true);
        }

        List<ItemType> inputItemTypes = itemType.GetCraftingInputs();
        if (inputItemTypes.Count > 0)
        {
            height -= 4;
            displayGrid.DisplayText(
                "Inputs: ",
                (int)rootPosition.x + 1,
                height,
                Color.black,
                true);

            // display the input items
            int itemButtonsPerRow = 4;
            for (int i = 0; i < inputItemTypes.Count; i++)
            {
                if (i % itemButtonsPerRow == 0)
                {
                    height -= 7;
                }
                ItemType inputItemType = inputItemTypes[i];
                Button itemIconButton = new ItemIconButton(inputItemType);
                RectInt rectInt = new((int)rootPosition.x + 1 + 7 * (i % itemButtonsPerRow), height, 6, 6);
                itemIconButton.Draw(displayGrid, rectInt, map);
                buttonRectsToButtons[rectInt] = itemIconButton;
            }
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
        Vector2Int mousePosition = GetMouseWorldPosition();
        BuildingType buildingType = activePlaceBuildingButton.GetBuildingType();
        displayGrid.DisplaySprite("Art/UI/button",
            mousePosition.x * cellsPerSquare,
            mousePosition.y * cellsPerSquare,
            cellsPerSquare * buildingType.GetSize(),
            cellsPerSquare * buildingType.GetSize(),
            0,
            0,
            false);
    }

    private void AllRoutesFeedback(Building building)
    {
        // visual feedback for all routes to and from the building
        foreach (Placeable placeable in GetMap().UnheldPlaceables())
        {
            if (placeable is Building otherBuilding)
            {
                if (GetMap().TransportRouteExists(building, otherBuilding))
                {
                    RouteFeedback(building, otherBuilding);
                }
                if (GetMap().TransportRouteExists(otherBuilding, building))
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
            (GetMap().PositionOf(sourceBuilding).x + sourceBuilding.buildingType.GetSize() / 2f) * cellsPerSquare - 2,
            (GetMap().PositionOf(sourceBuilding).y + sourceBuilding.buildingType.GetSize() / 2f) * cellsPerSquare - 2,
            (GetMap().PositionOf(targetBuilding).x + targetBuilding.buildingType.GetSize() / 2f) * cellsPerSquare - 2,
            (GetMap().PositionOf(targetBuilding).y + targetBuilding.buildingType.GetSize() / 2f) * cellsPerSquare - 2,
            4, 4, 1f);
    }

    public void DisplayEntity(int buttonNumber, Entity entity)
    {
        if (buttonNumber == 0)
        {
            leftMouseSelection.entity = entity;
        }
        else if (buttonNumber == 1)
        {
            rightMouseSelection.entity = entity;
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

    public virtual void Draw(DisplayGrid displayGrid, RectInt rectInt, Map map, bool placingBuilding = false)
    {
        displayGrid.DisplaySprite("Art/UI/plain_white",
            rectInt.x,
            rectInt.y,
            rectInt.width,
            rectInt.height,
            0,
            overlapLayer: 6,
            true);
        displayGrid.DisplayText(text, rectInt.x+1, rectInt.y+ rectInt.height / 2, Color.black, true);
    }

    public abstract void OnClick(MapDisplayer mapDisplayer, int buttonNumber);
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
                overlapLayer: 7,
                true);
        }
        displayGrid.DisplaySprite("Art/UI/plain_white",
            rectInt.x,
            rectInt.y,
            rectInt.width,
            rectInt.height,
            0,
            overlapLayer: 6,
            true);
        displayGrid.DisplayText(text, rectInt.x + 1, rectInt.y + rectInt.height / 2, Color.black, true);
    }

    public override void OnClick(MapDisplayer mapDisplayer, int buttonNumber)
    {
        if (buttonNumber != 0)
        {
            return;
        }

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

    public override void OnClick(MapDisplayer mapDisplayer, int buttonNumber)
    {
        if (buttonNumber != 0)
        {
            return;
        }

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
            overlapLayer: 6,
            true);
        displayGrid.DisplayText(text, rectInt.x + 1, rectInt.y + rectInt.height / 2, Color.black, true);
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

    public override void OnClick(MapDisplayer mapDisplayer, int buttonNumber)
    {
        if (buttonNumber != 0)
        {
            return;
        }

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

    public bool TryMakeCommand(MapDisplayer mapDisplayer, Vector2Int builtBuildingPosition)
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

public class ItemIconButton : Button
{
    private ItemType itemType;

    public ItemIconButton(ItemType itemType) : base(itemType.GetName())
    {
        this.itemType = itemType;
    }

    public override void Draw(DisplayGrid displayGrid, RectInt rectInt, Map map, bool placingBuilding = false)
    {
        displayGrid.DisplaySprite(
            itemType.GetSpritePath(),
            rectInt.x,
            rectInt.y,
            rectInt.width,
            rectInt.height,
            0,
            overlapLayer: 6,
            true);
    }

    public override void OnClick(MapDisplayer mapDisplayer, int buttonNumber) 
    {
        mapDisplayer.DisplayEntity(buttonNumber, itemType);
    }
}