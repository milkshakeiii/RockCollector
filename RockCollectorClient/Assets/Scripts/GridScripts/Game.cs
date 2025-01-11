using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Game : MonoBehaviour
{
    public const string DIVE_LOCATION = "dive_location";
    public const string CREDITS = "credits";
    public const string EQUIPMENT = "equipment";
    public const string LAST_DIVE_OVERWORLD_X = "last_dive_x";
    public const string LAST_DIVE_OVERWORLD_Y = "last_dive_y";

    public static Game Instance;

    public DisplayGrid displayGrid;

    private GameScreen currentScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // test data
        PersistentData.Instance.StoreStringList(EQUIPMENT, new List<string> { "capsule_submarine", "scoop" });
        PersistentData.Instance.StoreInt(LAST_DIVE_OVERWORLD_X, 15);
        PersistentData.Instance.StoreInt(LAST_DIVE_OVERWORLD_Y, 15);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        SetCurrentScreen(new MainMenu());
    }

    public void SetCurrentScreen(GameScreen screen)
    {
        currentScreen = screen;
        currentScreen.Start(displayGrid);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentScreen.UpKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentScreen.DownKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            currentScreen.LeftKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            currentScreen.RightKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            currentScreen.AKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            currentScreen.BKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            currentScreen.XKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            currentScreen.YKey(displayGrid);
        }
    }
}

public abstract class GameScreen
{
    public abstract void Start(DisplayGrid displayGrid);
    public abstract void UpKey(DisplayGrid displayGrid);
    public abstract void DownKey(DisplayGrid displayGrid);
    public abstract void LeftKey(DisplayGrid displayGrid);
    public abstract void RightKey(DisplayGrid displayGrid);
    public abstract void AKey(DisplayGrid displayGrid);
    public abstract void BKey(DisplayGrid displayGrid);
    public abstract void XKey(DisplayGrid displayGrid);
    public abstract void YKey(DisplayGrid displayGrid);
}

// MainMenu
public class MainMenu : GameScreen
{
    int selection = 0;

    public override void Start(DisplayGrid displayGrid)
    {
        DrawButtons(displayGrid);
    }

    public override void UpKey(DisplayGrid displayGrid)
    {
        ChangeSelection(displayGrid, -1);
    }

    public override void DownKey(DisplayGrid displayGrid)
    {
        ChangeSelection(displayGrid, 1);
    }

    public override void LeftKey(DisplayGrid displayGrid)
    {
        ChangeSelection(displayGrid, -1);
    }

    public override void RightKey(DisplayGrid displayGrid)
    {
        ChangeSelection(displayGrid, 1);
    }

    private void ChangeSelection(DisplayGrid displayGrid, int amount)
    {
        selection += amount;
        DrawButtons(displayGrid);
    }

    private void DrawButtons(DisplayGrid displayGrid)
    {
        string ButtonSprite(int index)
        {
            if (selection%5 == index)
            {
                return "Art/UI/selected_button";
            }
            else
            {
                return "Art/UI/button";
            }
        }

        displayGrid.Clear();
        displayGrid.DisplaySprite(ButtonSprite(0), 8, 119, 13, 3, 0);
        displayGrid.DisplayText("Credits", 10, 120);
        displayGrid.DisplaySprite(ButtonSprite(1), 8, 99, 13, 3, 0);
        displayGrid.DisplayText("New Game", 10, 100);
        displayGrid.DisplaySprite(ButtonSprite(2), 8, 79, 13, 3, 0);
        displayGrid.DisplayText("Continue", 10, 80);
        displayGrid.DisplaySprite(ButtonSprite(3), 8, 59, 13, 3, 0);
        displayGrid.DisplayText("Options", 10, 60);
        displayGrid.DisplaySprite(ButtonSprite(4), 8, 39, 13, 3, 0);
        displayGrid.DisplayText("Exit", 10, 40);
    }

    public override void AKey(DisplayGrid displayGrid)
    {
        if (selection%5 == 0)
        {
            Debug.Log("Credits");
        }
        else if (selection%5 == 1)
        {
            Debug.Log("New Game");
            Game.Instance.SetCurrentScreen(new OverworldScreen());
        }
        else if (selection%5 == 2)
        {
            Debug.Log("Continue");
        }
        else if (selection%5 == 3)
        {
            Debug.Log("Options");
        }
        else if (selection%5 == 4)
        {
            Debug.Log("Exit");
        }
    }

    public override void BKey(DisplayGrid displayGrid)
    {
        Debug.Log("Main Menu B");
    }

    public override void XKey(DisplayGrid displayGrid)
    {
        Debug.Log("Main Menu X");
    }

    public override void YKey(DisplayGrid displayGrid)
    {
        Debug.Log("Main Menu Y");
    }
}

// Overworld
public class OverworldScreen : GameScreen
{
    private int selected_x = 15;
    private int selected_y = 15;

    public override void Start(DisplayGrid displayGrid)
    {
        displayGrid.Clear();
        DrawEverything(displayGrid);
    }

    private void DrawEverything(DisplayGrid displayGrid)
    {
        DrawOverworld(displayGrid);
        DrawSidebars(displayGrid);
        DrawPositionArrow(displayGrid);
    }

    private void DrawOverworld(DisplayGrid displayGrid)
    {
        float[,] PerlinGrid(int seed)
        {
            // Create a 30x30 array of Perlin noise values
            float[,] grid = new float[30, 30];
            for (int x = 0; x < 30; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    grid[x, y] = Mathf.PerlinNoise(seed + x * 0.1f, seed + y * 0.1f);
                }
            }
            return grid;
        }

        // create one grid each for coral, rock, ice, and cave biomes
        float[,] coralGrid = PerlinGrid(111);
        float[,] rockGrid = PerlinGrid(222);
        float[,] iceGrid = PerlinGrid(333);
        float[,] caveGrid = PerlinGrid(444);

        string iceSprite = "Art/Terrain/Ice/FILLER08";
        string rockSprite = "Art/Terrain/Rock/Base_Rock_Tile_Filler_05";
        string coralSprite = "Art/Terrain/Reef/Coral_Tile_05";
        string caveSprite = "Art/Terrain/Cave/CAVE_05";
        string blankSprite = "Art/UI/selected_button";

        // for each cell in the grid, display the sprite of the biome with the highest value
        // if no biome has a value above 0.4, display a blank sprite
        for (int x = 0; x < 30; x++)
        {
            for (int y = 0; y < 30; y++)
            {
                float coralValue = coralGrid[x, y];
                float rockValue = rockGrid[x, y];
                float iceValue = iceGrid[x, y];
                float caveValue = caveGrid[x, y];

                if (coralValue > rockValue && coralValue > iceValue && coralValue > caveValue && coralValue > 0.4f)
                {
                    displayGrid.DisplaySprite(coralSprite, DisplayGrid.WIDTH / 2 - 15 * 4 + (x * 4), DisplayGrid.HEIGHT / 2 - 15 * 4 + (y * 4), 4, 4, 0);
                }
                else if (rockValue > coralValue && rockValue > iceValue && rockValue > caveValue && rockValue > 0.4f)
                {
                    displayGrid.DisplaySprite(rockSprite, DisplayGrid.WIDTH / 2 - 15 * 4 + (x * 4), DisplayGrid.HEIGHT / 2 - 15 * 4 + (y * 4), 4, 4, 0);
                }
                else if (iceValue > coralValue && iceValue > rockValue && iceValue > caveValue && iceValue > 0.4f)
                {
                    displayGrid.DisplaySprite(iceSprite, DisplayGrid.WIDTH / 2 - 15 * 4 + (x * 4), DisplayGrid.HEIGHT / 2 - 15 * 4 + (y * 4), 4, 4, 0);
                }
                else if (caveValue > coralValue && caveValue > rockValue && caveValue > iceValue && caveValue > 0.4f)
                {
                    displayGrid.DisplaySprite(caveSprite, DisplayGrid.WIDTH / 2 - 15 * 4 + (x * 4), DisplayGrid.HEIGHT / 2 - 15 * 4 + (y * 4), 4, 4, 0);
                }
                else
                {
                    displayGrid.DisplaySprite(blankSprite, DisplayGrid.WIDTH / 2 - 15 * 4 + (x * 4), DisplayGrid.HEIGHT / 2 - 15 * 4 + (y * 4), 4, 4, 0);
                }
            }
        }
    }

    private void DrawSidebars(DisplayGrid displayGrid)
    {
        // draw two sidebars, taking up a fifth of the screen on the left and right
        // the left sidebar is the shared submarine sidebar
        SharedScreenElements.DrawSubmarineSidebar(displayGrid);
    }

    private void DrawPositionArrow(DisplayGrid displayGrid)
    {
        // draw an arrow at the position of the last dive
        int x = PersistentData.Instance.GetInt(Game.LAST_DIVE_OVERWORLD_X);
        int y = PersistentData.Instance.GetInt(Game.LAST_DIVE_OVERWORLD_Y);

        displayGrid.DisplaySprite("Art/UI/arrow", DisplayGrid.WIDTH / 2 - 15 * 4 + (x * 4), DisplayGrid.HEIGHT / 2 - 15 * 4 + (y * 4) + 2, 4, 4, 3, overlapLayer: 1);

        // draw an arrow at the selected position
        displayGrid.DisplaySprite("Art/UI/arrow", DisplayGrid.WIDTH / 2 - 15 * 4 + (selected_x * 4), DisplayGrid.HEIGHT / 2 - 15 * 4 + (selected_y * 4) + 2, 4, 4, 3, overlapLayer: 1);
    }

    public override void UpKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Up");
        displayGrid.Clear();

        if (selected_y < 29)
        {
            selected_y++;
        }

        DrawEverything(displayGrid);
    }

    public override void DownKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Down");
        displayGrid.Clear();

        if (selected_y > 0)
        {
            selected_y--;
        }

        DrawEverything(displayGrid);
    }

    public override void LeftKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Left");
        displayGrid.Clear();

        if (selected_x > 0)
        {
            selected_x--;
        }

        DrawEverything(displayGrid);
    }

    public override void RightKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Right");
        displayGrid.Clear();

        if (selected_x < 29)
        {
            selected_x++;
        }

        DrawEverything(displayGrid);
    }

    public override void AKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld A");
        // SetCurrentScreen to MainWorldScreen
        Game.Instance.SetCurrentScreen(new MainWorldSCreen());
    }

    public override void BKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld B");
    }

    public override void XKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld X");
    }

    public override void YKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Y");
    }
}

// MainWorldSCreen
public class MainWorldSCreen : GameScreen
{
    public override void Start(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine Start");
    }
    public override void UpKey(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine Up");
    }

    public override void DownKey(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine Down");
    }

    public override void LeftKey(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine Left");
    }

    public override void RightKey(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine Right");
    }

    public override void AKey(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine A");
    }

    public override void BKey(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine B");
    }

    public override void XKey(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine X");
    }

    public override void YKey(DisplayGrid displayGrid)
    {
        Debug.Log("Submarine Y");
    }
}

public static class SharedScreenElements
{
    public static void DrawSubmarineSidebar(DisplayGrid displayGrid)
    {
        // draw background
        displayGrid.DisplaySprite("Art/UI/plain_white", 0, 0, DisplayGrid.WIDTH/5, DisplayGrid.HEIGHT, 0);

        // draw submarine sprite

        // draw info text
        Dictionary<string, string> infoRows = new();
        infoRows.Add("Dive location:", PersistentData.Instance.GetString(Game.DIVE_LOCATION));
        infoRows.Add("Credits:", PersistentData.Instance.GetString(Game.CREDITS));

        int infoRowCount = 0;
        foreach (KeyValuePair<string, string> infoRow in infoRows)
        {
            displayGrid.DisplayText(infoRow.Key, 2, (90 - 10 * infoRowCount));
            displayGrid.DisplayText(infoRow.Value, (DisplayGrid.WIDTH / 5 - infoRow.Value.Length - 2), (90 - 10*infoRowCount));

            infoRowCount++;
        }

        // get equipment
        List<string> equipmentNames = PersistentData.Instance.GetStringList(Game.EQUIPMENT);
        List<Equipment> allEquipment = new();
        foreach (string equipmentName in equipmentNames)
        {
            allEquipment.Add(Equipment.EquipmentFromName(equipmentName));
        }

        // draw equipment sprites
        for (int i = 0; i < allEquipment.Count; i++)
        {
            Equipment equipmentPiece = allEquipment[i];
            // arrange the equipment in rows of 3, starting at x=2, y=60
            int x = 2 + (i % 3) * 18;
            int y = 60 - (i / 3) * 18;
            displayGrid.DisplaySprite(equipmentPiece.SpritePath(), x, y, 15, 15, 0, overlapLayer: 1);
        }
    }
}