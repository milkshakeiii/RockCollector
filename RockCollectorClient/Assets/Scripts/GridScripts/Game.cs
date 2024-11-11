using UnityEngine;

public class Game : MonoBehaviour
{
    public DisplayGrid displayGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public abstract class GameScreen
{
    public abstract void UpKey();
    public abstract void DownKey();
    public abstract void LeftKey();
    public abstract void RightKey();
    public abstract void AKey();
    public abstract void BKey();
    public abstract void XKey();
    public abstract void YKey();
}

// MainMenu
public class MainMenu : GameScreen
{

}

// Overworld
public class OverworldScreen : GameScreen
{

}

// DivePreparation
public class DivePreparationScreen : GameScreen
{

}

// Submarine
public class SubmarineScreen : GameScreen
{

}

// Battle
public class BattleScreen : GameScreen
{

}

// DiveResults
public class DiveResultsScreen : GameScreen
{

}