using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class EndOfRunScreen : MonoBehaviour
{
    public Submarine submarine;
    public Environment environment;

    public GameObject fishPanelPrefab;
    public GameObject fishPanelParent;
    public float fishPanelHorizontalSpacing = 0.2f;
    public float fishPanelVerticalSpacing = 0.2f;
    public int fishPanelsPerRow = 5;

    private void OnEnable()
    {
        float biggestCatch = 0;
        // create a fish panel for each fish in the submarine
        List<Timefish> allCatches = submarine.AllCatches();
        for (int i = 0; i < allCatches.Count; i++)
        {
            Timefish fish = allCatches[i];
            biggestCatch = Mathf.Max(biggestCatch, fish.TradeValue());

            GameObject fishPanel = Instantiate(fishPanelPrefab, fishPanelParent.transform);
            // increment fish panel anchor min and max to space them out
            float x = i % fishPanelsPerRow * fishPanelHorizontalSpacing;
            float y = i / fishPanelsPerRow * fishPanelVerticalSpacing;
            fishPanel.GetComponent<RectTransform>().anchorMin += new Vector2(x, y);
            fishPanel.GetComponent<RectTransform>().anchorMax += new Vector2(x, y);
            // set offset min and max to 0
            fishPanel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            fishPanel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            
            fishPanel.GetComponent<FishPanel>().Initialize(fish);
        }

        // calculate the total value of the fish
        float totalValue = 0;
        foreach (Timefish fish in allCatches)
        {
            totalValue += fish.TradeValue();
        }

        // set the run result in player prefs
        PlayerPrefs.SetFloat("LastRunValue", totalValue);

        // report the score to the server
        string seedString = environment.GetLastSeed().ToString();
        string cohort = "pluto";
        List<string> scoreGroups = new() { seedString, cohort, "universe" };
        Dictionary<string, float> scores = new() { { "value", totalValue }, { "biggest_catch", biggestCatch } };
        WebRequests.GetInstance().ReportScore(scoreGroups, scores, (response) =>
        {
            Debug.Log("Score reported: " + response);
        });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextRun()
    {
        // reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
