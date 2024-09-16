using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class HighScoreScreen : MonoBehaviour
{
    public TMPro.TMP_Dropdown cohortDropdown;
    public TMPro.TMP_Dropdown scoreTypeDropdown;
    public TMPro.TMP_InputField startPositionInput;
    public TMPro.TMP_InputField diveLocationFilter;
    public Toggle diveLocationFilterToggle;

    public GameObject highScoreRowParent;
    public GameObject highScoreRowPrefab;
    public float rowOffset = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakeRequest(bool aroundUser)
    {
        string scoreType = scoreTypeDropdown.options[scoreTypeDropdown.value].text.ToLower();
        string cohort = cohortDropdown.options[cohortDropdown.value].text.ToLower();
        string diveLocation = diveLocationFilter.text;
        List<string> scoreGroups = new () {cohort, "universe"};
        if (diveLocation != "" && diveLocationFilterToggle.isOn)
        {
            scoreGroups.Add(diveLocation);
        }

        int start = 0;
        if (startPositionInput.text != "")
        {
            start = int.Parse(startPositionInput.text);
        }
        if (aroundUser)
        {
            start = -1;
        }

        WebRequests.GetInstance().GetScores(scoreGroups, scoreType, 10, GetScoresCallback, start);
    }

    void GetScoresCallback(Newtonsoft.Json.Linq.JObject result)
    {
        // instantiate one highScoreRowPrefab for each key in result
        int row = 0;
        foreach (var resultRow in result)
        {
            GameObject highScoreRow = Instantiate(highScoreRowPrefab, highScoreRowParent.transform);
            highScoreRow.GetComponent<HighScoreEntryPanel>().Initialize(resultRow);
            highScoreRow.GetComponent<RectTransform>().anchorMin += new Vector2(0, -row * rowOffset);
            highScoreRow.GetComponent<RectTransform>().anchorMax += new Vector2(0, -row * rowOffset);
            // set bounds to zero to move highScoreRow to its anchors
            highScoreRow.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            highScoreRow.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            row++;
        }
    }
}
