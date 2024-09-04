using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetupScreen : MonoBehaviour
{
    public delegate void SetupComplete();
    public static event SetupComplete OnSetupComplete;

    public float buyHorizontalSpacing = 1.0f;
    public float buyVerticalSpacing = 1.0f;
    public int buyModulesPerRow = 6;
    public float returnHorizontalSpacing = 1.0f;
    public float returnVerticalSpacing = 1.0f;
    public int returnModulesPerRow = 6;

    public TMP_Text valueRemainingText;
    public Submarine submarine;

    public GameObject buyModuleParent;
    public GameObject buyModulePrefab;

    public GameObject returnModuleParent;
    public GameObject returnModulePrefab;

    private SubmarineType submarineType = new(); // selected submarine type
    private List<ReturnModuleButton> purchasedModules = new();

    private Dictionary<string, float> modulePrices = new();

    private int moduleCount = 0;

    public float LastRunValue()
    {
        // access the value of the last run from player prefs
        float storedValue = PlayerPrefs.GetFloat("LastRunValue", 0f);
        return Mathf.Max(storedValue, 100f);
    }

    public void UpdateValueRemainingText()
    {
        float spentValue = SpentValue();

        float lastRunValue = LastRunValue();
        valueRemainingText.text = $"Value Remaining: {lastRunValue - spentValue}";
    }

    private float SpentValue()
    {
        float spentValue = 0.0f;
        foreach (ReturnModuleButton button in purchasedModules)
        {
            Equipment module = button.module;
            spentValue += modulePrices[module.name];
        }
        return spentValue;
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
            modulePrices[module.name] = Random.Range(1f, 10f);
        }

        foreach (Equipment module in modules)
        {
            AddModule(module);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateValueRemainingText();

        PostEquipment();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddModule(Equipment module)
    {
        float x = moduleCount % buyModulesPerRow * buyHorizontalSpacing;
        float y = -moduleCount / buyModulesPerRow * buyVerticalSpacing;
        GameObject moduleObject = Instantiate(buyModulePrefab, buyModuleParent.transform);
        // shift the anchors of the module object by the x and y values
        moduleObject.GetComponent<RectTransform>().anchorMin += new Vector2(x, y);
        moduleObject.GetComponent<RectTransform>().anchorMax += new Vector2(x, y);
        // zero out the module object rect transform offsets
        moduleObject.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        moduleObject.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        moduleObject.GetComponent<BuyModulePanel>().Initialize(module, this);

        moduleCount++;
    }

    public void BuyModule(Equipment module)
    {
        // check if the player has enough value to buy the module
        float spentValue = SpentValue();

        if (spentValue + modulePrices[module.name] > LastRunValue())
        {
            return;
        }

        AddReturnModuleButton(module.Copy());

        // update the value remaining text
        UpdateValueRemainingText();
    }

    public void AddReturnModuleButton(Equipment module)
    {
        // create the return module button
        GameObject returnModuleObject = Instantiate(returnModulePrefab, returnModuleParent.transform);
        returnModuleObject.GetComponent<ReturnModuleButton>().Initialize(module, this);
        int thisReturnModuleIndex = purchasedModules.Count;
        float x = thisReturnModuleIndex % returnModulesPerRow * returnHorizontalSpacing;
        float y = -thisReturnModuleIndex / returnModulesPerRow * returnVerticalSpacing;
        // shift the anchors of the return module object by the x and y values
        returnModuleObject.GetComponent<RectTransform>().anchorMin += new Vector2(x, y);
        returnModuleObject.GetComponent<RectTransform>().anchorMax += new Vector2(x, y);
        // zero out the return module object rect transform offsets
        returnModuleObject.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        returnModuleObject.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        purchasedModules.Add(returnModuleObject.GetComponent<ReturnModuleButton>());
    }

    public void ReturnModule(ReturnModuleButton button)
    {
        Equipment returnedModule = button.module;

        // get a list of all the equipment except the one being returned
        List<Equipment> equipment = new();
        foreach (ReturnModuleButton returnModuleButton in purchasedModules)
        {
            if (returnModuleButton != button)
            {
                equipment.Add(returnModuleButton.module);
            }
        }

        // destroy all the return module buttons
        foreach (ReturnModuleButton returnModuleButton in purchasedModules)
        {
            Destroy(returnModuleButton.gameObject);
        }

        // create new return module buttons for all the equipment except the one being returned
        purchasedModules.Clear();
        foreach (Equipment module in equipment)
        {
            AddReturnModuleButton(module);
        }

        // update the value remaining text
        UpdateValueRemainingText();
    }

    public void Dive()
    {
        submarine.SetSubmarineType(submarineType);
        List<Equipment> modules = new();
        foreach (ReturnModuleButton button in purchasedModules)
        {
            modules.Add(button.module);
        }
        submarine.AddEquipment(modules);
        gameObject.SetActive(false);
        submarine.gameObject.SetActive(true);
        OnSetupComplete?.Invoke();
    }
}
