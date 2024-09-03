using System.Collections;
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
    public GameObject moduleButtonParent;

    private SubmarineType submarineType = new(); // selected submarine type
    private List<Equipment> coreModules = new(); // purchased core modules
    private List<Equipment> internalModules = new(); // purchased internal modules
    private List<Equipment> hullMountedModules = new(); // purchased hull-mounted modules

    private Dictionary<string, float> modulePrices = new();

    private int moduleCount = 0;

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

    public void PostEquipment()
    {
        List<Equipment> modules = new();

        Ballast ballast = new();
        ballast.name = "Ballast";
        ballast.description = "Drop ballast to decrease the weight of the submarine. Water tank ballasts can be refilled to increase weight again.";
        ballast.ballastMass = 3000f;
        ballast.ballastDropTime = 1f;
        ballast.ballastRefills = 3;
        ballast.ballastRefillTime = 2f;
        modules.Add(ballast);

        DepthController depthController = new();
        depthController.name = "Depth Controller";
        depthController.description = "Control the depth of the submarine by adjusting the mass of the submarine.";
        depthController.depthControlMass = 300f;
        depthController.depthControlTime = 1f;
        depthController.continuousPower = 0.1f;
        modules.Add(depthController);

        Engine engine = new();
        engine.name = "Engine";
        engine.description = "Propel the submarine forward with the engine.";
        engine.thrust = 8000f;
        engine.continuousPower = 0.1f;
        modules.Add(engine);

        HarpoonGun harpoonGun = new();
        harpoonGun.name = "Harpoon Gun";
        harpoonGun.description = "Fire harpoons to tether fish.";
        harpoonGun.size = 1f;
        harpoonGun.velocity = 10f;
        harpoonGun.activationPower = 2f;
        harpoonGun.continuousPower = 1f;
        harpoonGun.range = 5f;
        harpoonGun.maxHarpoons = 2;
        harpoonGun.reelSpeed = 0.2f;
        harpoonGun.pullStrength = 1500f;
        harpoonGun.ropeElasticity = 0.5f;
        modules.Add(harpoonGun);

        HarpoonGun harpoonGun2 = new();
        harpoonGun2.name = "Harpoon Gun 2";
        harpoonGun2.description = "Fire harpoons to tether fish.";
        harpoonGun2.size = 2f;
        harpoonGun2.velocity = 15f;
        harpoonGun2.activationPower = 2f;
        harpoonGun2.continuousPower = 1f;
        harpoonGun2.range = 7f;
        harpoonGun2.maxHarpoons = 3;
        harpoonGun2.reelSpeed = 0.4f;
        harpoonGun2.pullStrength = 3000f;
        harpoonGun2.ropeElasticity = 0.5f;
        modules.Add(harpoonGun2);

        Scoop scoop = new();
        scoop.name = "Scoop";
        scoop.description = "Scoop up fish with the scoop.";
        scoop.scoopDiameter = 1f;
        scoop.scoopTime = 1f;
        scoop.activationPower = 1f;
        modules.Add(scoop);

        StartCoroutine(SpawnModulesWithPrices(modules));
    }

    // fetch the module prices from the server
    private IEnumerator SpawnModulesWithPrices(List<Equipment> modules)
    {
        // simulate a delay
        yield return new WaitForSeconds(1f);

        foreach (Equipment module in modules)
        {
            modulePrices[module.name] = Random.Range(100f, 1000f);
        }

        foreach (Equipment module in modules)
        {
            AddModule(module);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateValueRemainingText(0.0f);

        PostEquipment();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddModule(Equipment module)
    {
        float x = moduleCount % modulesPerRow * horizonalSpacing;
        float y = -moduleCount / modulesPerRow * verticalSpacing;
        GameObject moduleObject = Instantiate(modulePrefab, moduleButtonParent.transform);
        // shift the anchors of the module object by the x and y values
        moduleObject.GetComponent<RectTransform>().anchorMin += new Vector2(x, y);
        moduleObject.GetComponent<RectTransform>().anchorMax += new Vector2(x, y);
        // zero out the module object rect transform offsets
        moduleObject.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        moduleObject.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        moduleObject.GetComponent<BuyModulePanel>().Initialize(module);

        moduleCount++;
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
