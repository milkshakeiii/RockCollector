using UnityEngine;

public class ScenarioReader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Map ReadScenario(string scenarioName)
    {
        Map map = new();

        TextAsset scenario = Resources.Load<TextAsset>("Scenarios/" + scenarioName);
        string[] lines = scenario.text.Split('\n');
        foreach (string line in lines)
        {

        }

        return map;
    }
}
