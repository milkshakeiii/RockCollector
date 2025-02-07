using NUnit.Framework;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioSelector : MonoBehaviour
{
    public GameObject buttonPrefab;
    public TMP_Text titleText;
    public float spacing = 150f;
    public int buttonsPerRow = 4;

    private Scenario scenario;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        titleText.text = "Select a scenario";
        List<string> scenarioNames = EntityManager.ListScenarios();
        for (int i = 0; i < scenarioNames.Count; i++)
        {
            GameObject button = Instantiate(buttonPrefab, transform);
            button.GetComponentInChildren<TMP_Text>().text = scenarioNames[i];
            button.GetComponent<RectTransform>().anchoredPosition += new Vector2((i % buttonsPerRow) * spacing, -Mathf.Floor(i / buttonsPerRow) * spacing);
            // button.GetComponent<Button>().onClick.AddListener(() => EntityManager.LoadScenario(scenarioNames[i]));
        }
    }

    public void LoadScenario(string scenarioName)
    {
        scenario = Scenario.ReadScenario(scenarioName);
    }

    public void StartGame(string playerTeam)
    {
        scenario.ReadStartingTeam(playerTeam);
        FindAnyObjectByType<MapDisplayer>().SetGamestate(scenario.StartingGamestate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
