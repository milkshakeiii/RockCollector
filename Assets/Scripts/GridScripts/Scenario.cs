using UnityEngine;

public class Scenario
{
    public ScenarioInfo scenarioInfo;

    public Scenario(ScenarioInfo scenarioInfo)
    {
        this.scenarioInfo = scenarioInfo;
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

    public static Scenario ReadScenario(string scenarioName)
    {
        EntityManager.ReadScenario(scenarioName);
        ScenarioInfo scenarioInfo = EntityManager.scenarioInfos[scenarioName];
        return new Scenario(scenarioInfo);
    }
}
