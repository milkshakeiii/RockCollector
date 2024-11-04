using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HighScoreEntryPanel : MonoBehaviour
{
    public TMP_Text leftText;
    public TMP_Text rightText;

    public void Initialize(KeyValuePair<string, Newtonsoft.Json.Linq.JToken> keyValuePair)
    {
        leftText.text = keyValuePair.Key;
        rightText.text = keyValuePair.Value.ToString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
