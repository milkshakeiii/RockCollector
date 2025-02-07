using UnityEngine;

public class Scenario
{
    public ScenarioInfo scenarioInfo;

    public static Scenario ReadScenario(string scenarioName)
    {
        TextAsset scenarioFile = Resources.Load<TextAsset>("Scenarios/" + scenarioName);
        string[] lines = scenarioFile.text.Split('\n');
        foreach (string line in lines)
        {

        }

        return new Scenario();
    }
}
