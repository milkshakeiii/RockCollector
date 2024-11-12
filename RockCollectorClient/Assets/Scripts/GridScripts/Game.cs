using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance;

    public DisplayGrid displayGrid;

    private GameScreen currentScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
    public override void Start(DisplayGrid displayGrid)
    {
        displayGrid.Clear();
        Debug.Log("Overworld Start");
    }

    public override void UpKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Up");
    }

    public override void DownKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Down");
    }

    public override void LeftKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Left");
    }

    public override void RightKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld Right");
    }

    public override void AKey(DisplayGrid displayGrid)
    {
        Debug.Log("Overworld A");
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

// DivePreparation
public class DivePreparationScreen : GameScreen
{
    public override void Start(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation Start");
    }

    public override void UpKey(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation Up");
    }

    public override void DownKey(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation Down");
    }

    public override void LeftKey(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation Left");
    }

    public override void RightKey(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation Right");
    }

    public override void AKey(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation A");
    }

    public override void BKey(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation B");
    }

    public override void XKey(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation X");
    }

    public override void YKey(DisplayGrid displayGrid)
    {
        Debug.Log("DivePreparation Y");
    }
}

// Submarine
public class SubmarineScreen : GameScreen
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

// Battle
public class BattleScreen : GameScreen
{
    public override void Start(DisplayGrid displayGrid)
    {
        Debug.Log("Battle Start");
    }

    public override void UpKey(DisplayGrid displayGrid)
    {
        Debug.Log("Battle Up");
    }

    public override void DownKey(DisplayGrid displayGrid)
    {
        Debug.Log("Battle Down");
    }

    public override void LeftKey(DisplayGrid displayGrid)
    {
        Debug.Log("Battle Left");
    }

    public override void RightKey(DisplayGrid displayGrid)
    {
        Debug.Log("Battle Right");
    }

    public override void AKey(DisplayGrid displayGrid)
    {
        Debug.Log("Battle A");
    }

    public override void BKey(DisplayGrid displayGrid)
    {
        Debug.Log("Battle B");
    }

    public override void XKey(DisplayGrid displayGrid)
    {
        Debug.Log("Battle X");
    }

    public override void YKey(DisplayGrid displayGrid)
    {
        Debug.Log("Battle Y");
    }
}

// DiveResults
public class DiveResultsScreen : GameScreen
{
    public override void Start(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults Start");
    }

    public override void UpKey(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults Up");
    }

    public override void DownKey(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults Down");
    }

    public override void LeftKey(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults Left");
    }

    public override void RightKey(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults Right");
    }

    public override void AKey(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults A");
    }

    public override void BKey(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults B");
    }

    public override void XKey(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults X");
    }

    public override void YKey(DisplayGrid displayGrid)
    {
        Debug.Log("DiveResults Y");
    }
}