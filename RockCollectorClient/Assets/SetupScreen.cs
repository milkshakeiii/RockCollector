using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetupScreen : MonoBehaviour
{
    public delegate void SetupComplete();
    public static event SetupComplete OnSetupComplete;

    public float horizonalSpacing = 1.0f;
    public float verticalSpacing = 1.0f;
    public int modulesPerRow = 6;
    public GameObject modulePrefab;
    public TMP_Text valueRemainingText;
    public Submarine submarine;

    private SubmarineType submarineType = new(); // selected submarine type
    private List<Equipment> coreModules = new(); // purchased core modules
    private List<Equipment> internalModules = new(); // purchased internal modules
    private List<Equipment> hullMountedModules = new(); // purchased hull-mounted modules

    public float LastRunValue()
    {
        // access the value of the last run from player prefs
        float storedValue = PlayerPrefs.GetFloat("LastRunValue", 0f);
        return Mathf.Max(storedValue, 100f);
    }

    public void UpdateValueRemainingText(float spentValue)
    {
        float lastRunValue = LastRunValue();
        valueRemainingText.text = $"Value Remaining: {lastRunValue - spentValue}";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateValueRemainingText(0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddModule(int index)
    {

    }

    public void Dive()
    {
        submarine.SetSubmarineType(submarineType);
        gameObject.SetActive(false);
        submarine.gameObject.SetActive(true);
        if (OnSetupComplete != null)
        {
            OnSetupComplete();
        }
    }
}
