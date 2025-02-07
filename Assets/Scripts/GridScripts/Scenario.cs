using UnityEngine;

public class Scenario
{
    public ScenarioInfo scenarioInfo;
    private string playerTeamName;

    public Scenario(ScenarioInfo scenarioInfo, string playerTeamName)
    {
        this.scenarioInfo = scenarioInfo;
        this.playerTeamName = playerTeamName;
    }

    public string GetTile1SpritePath()
    {
        return scenarioInfo.GetTile1SpritePath();
    }

    public string GetFile2Path()
    {
        return scenarioInfo.GetTile2SpritePath();
    }

    public Gamestate StartingGamestate()
    {
        return new Gamestate(new(), this);
    }

    public static Scenario ReadScenario(string scenarioName, string newPlayerTeamNAme)
    {
        EntityManager.ReadScenario(scenarioName);
        ScenarioInfo scenarioInfo = EntityManager.scenarioInfos[scenarioName];
        return new Scenario(scenarioInfo, newPlayerTeamNAme);
    }
}
