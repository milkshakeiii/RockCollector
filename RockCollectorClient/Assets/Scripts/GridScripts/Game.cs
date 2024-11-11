using UnityEngine;

public class Game : MonoBehaviour
{
    public DisplayGrid displayGrid;

    private GameScreen currentScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentScreen = new MainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            displayGrid.Clear();
            currentScreen.UpKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            displayGrid.Clear();
            currentScreen.DownKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            displayGrid.Clear();
            currentScreen.LeftKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            displayGrid.Clear();
            currentScreen.RightKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            displayGrid.Clear();
            currentScreen.AKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            displayGrid.Clear();
            currentScreen.BKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            displayGrid.Clear();
            currentScreen.XKey(displayGrid);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            displayGrid.Clear();
            currentScreen.YKey(displayGrid);
        }
    }
}

public abstract class GameScreen
{
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
    public override void UpKey(DisplayGrid displayGrid)
    {
        Debug.Log("Main Menu Up");
    }

    public override void DownKey(DisplayGrid displayGrid)
    {
        Debug.Log("Main Menu Down");
    }

    public override void LeftKey(DisplayGrid displayGrid)
    {
        Debug.Log("Main Menu Left");
    }

    public override void RightKey(DisplayGrid displayGrid)
    {
        Debug.Log("Main Menu Right");
    }

    public override void AKey(DisplayGrid displayGrid)
    {
        Debug.Log("Main Menu A");
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