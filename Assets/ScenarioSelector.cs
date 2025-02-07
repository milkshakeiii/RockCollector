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

    private string scenarioName;
    private List<GameObject> buttons = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        titleText.text = "Select a scenario";
        List<string> scenarioNames = EntityManager.ListScenarios();
        for (int i = 0; i < scenarioNames.Count; i++)
        {
            string thisScenarioName = scenarioNames[i];
            GameObject button = Instantiate(buttonPrefab, transform);
            button.GetComponentInChildren<TMP_Text>().text = thisScenarioName;
            button.GetComponent<RectTransform>().anchoredPosition += new Vector2((i % buttonsPerRow) * spacing, -Mathf.Floor(i / buttonsPerRow) * spacing);
            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => SelectScenario(thisScenarioName));
            buttons.Add(button);
        }

        Simulation.OnGameLose += (gamestate) => titleText.text = "Game over";
        Simulation.OnGameWin += (gamestate) => titleText.text = "You win!";
    }

    public void SelectScenario(string newScenarioName)
    {
        scenarioName = newScenarioName;
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();
        titleText.text = "Select your team";
        SpawnTeamButtons();
    }

    private void SpawnTeamButtons()
    {
        List<string> teamNames = EntityManager.ListTeams();
        for (int i = 0; i < teamNames.Count; i++)
        {
            string thisTeamName = teamNames[i];
            GameObject button = Instantiate(buttonPrefab, transform);
            button.GetComponentInChildren<TMP_Text>().text = thisTeamName;
            button.GetComponent<RectTransform>().anchoredPosition += new Vector2((i % buttonsPerRow) * spacing, -Mathf.Floor(i / buttonsPerRow) * spacing);
            button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => StartGame(thisTeamName));
            buttons.Add(button);
        }
    }

    public void StartGame(string playerTeam)
    {
        // clear buttons
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
        buttons.Clear();
        titleText.text = "";

        // start game
        Debug.Log("Starting game with scenario " + scenarioName + " and team " + playerTeam);
        Scenario scenario = Scenario.ReadScenario(scenarioName, playerTeam);
        FindAnyObjectByType<MapDisplayer>().SetGamestate(scenario.StartingGamestate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
