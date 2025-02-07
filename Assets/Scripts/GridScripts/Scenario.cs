using NUnit.Framework;
using System.Collections.Generic;
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
        Map map = new ();

        // starting lairs
        List<string> lairNames = scenarioInfo.GetLairNames();
        List<int> lairXs = scenarioInfo.GetLairXs();
        List<int> lairYs = scenarioInfo.GetLairYs();
        for (int i = 0; i < lairNames.Count; i++)
        {
            Vector2Int lairPosition = new (lairXs[i], lairYs[i]);
            BuildingType lairType = EntityManager.buildingTypes[lairNames[i]];
            Building newLair = new (lairType, -1);
            map.Add(newLair, lairPosition);
        }

        // starting player buildings
        int team1StartX = scenarioInfo.GetTeam1StartX();
        int team1StartY = scenarioInfo.GetTeam1StartY();
        foreach (BuildingType buildingType in EntityManager.buildingTypes.Values)
        {
            if (buildingType.GetIsStartingBuilding() && buildingType.GetTeamOrScenarioName() == playerTeamName)
            {
                Building newBuilding = new (buildingType, 0);
                map.Add(newBuilding, new Vector2Int(team1StartX, team1StartY));
                break;
            }
        }

        return new Gamestate(map, this);
    }

    public static Scenario ReadScenario(string scenarioName, string newPlayerTeamName)
    {
        EntityManager.ReadScenario(scenarioName);
        ScenarioInfo scenarioInfo = EntityManager.scenarioInfos[scenarioName];
        return new Scenario(scenarioInfo, newPlayerTeamName);
    }
}
