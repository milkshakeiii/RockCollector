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
    private List<PurchasedModulePanel> purchasedModules = new();

    private Dictionary<string, Equipment> modulesAvailable = new();
    private Dictionary<string, float> modulePrices = new();

    private int moduleCount = 0;
    private float spentValue = 0f;

    public float LastRunValue()
    {
        // access the value of the last run from player prefs
        float storedValue = PlayerPrefs.GetFloat("LastRunValue", 0f);
        return Mathf.Max(storedValue*10, 100f);
    }

    public void UpdateValueRemainingText()
    {
        float spentValue = SpentValue();

        float lastRunValue = LastRunValue();
        valueRemainingText.text = $"Value Remaining: {lastRunValue - spentValue}";
    }

    private float SpentValue()
    {
        return spentValue;
    }

    private void AddAvailableModule(string name, Equipment module)
    {
        module.name = name;
        modulesAvailable[name] = module;
    }

    public void PostEquipment()
    {
        List<Equipment> modules = new();

        Ballast ballast = new();
        AddAvailableModule("Ballast", ballast);
        ballast.description = "Drop ballast to decrease the weight of the submarine. Water tank ballasts can be refilled to increase weight again.";
        ballast.ballastMass = 3000f;
        ballast.ballastDropTime = 1f;
        ballast.ballastRefills = 3;
        ballast.ballastRefillTime = 2f;
        modules.Add(ballast);

        DepthController depthController = new();
        AddAvailableModule("Depth Controller", depthController);
        depthController.description = "Control the depth of the submarine by adjusting the mass of the submarine.";
        depthController.depthControlMass = 300f;
        depthController.depthControlTime = 1f;
        depthController.continuousPower = 0.1f;
        modules.Add(depthController);

        Engine engine = new();
        AddAvailableModule("Engine", engine);
        engine.description = "Propel the submarine forward with the engine.";
        engine.thrust = 8000f;
        engine.continuousPower = 0.1f;
        modules.Add(engine);

        HarpoonGun harpoonGun = new();
        AddAvailableModule("Harpoon Gun", harpoonGun);
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
        AddAvailableModule("Harpoon Gun 2", harpoonGun2);
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
        AddAvailableModule("Scoop", scoop);
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

        // get list of modules and their remaing durabilities from player prefs
        string[] moduleNames = PlayerPrefs.GetString("LastRunModules", "").Split(',');
        string[] modulePrices = PlayerPrefs.GetString("LastRunDurabilities", "").Split(',');

        // add the modules to purchased modules with their remaining durabilities
        for (int i = 0; i < moduleNames.Length; i++)
        {
            if (moduleNames[i] == "")
            {
                continue;
            }
            int durability = int.Parse(modulePrices[i]);
            Equipment newModule = BuyModule(modulesAvailable[moduleNames[i]], false, durability);
        }
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

    public Equipment BuyModule(Equipment module, bool pay, int durability)
    {
        if (pay)
        {
            // check if the player has enough value to buy the module
            if (SpentValue() + modulePrices[module.name] > LastRunValue())
            {
                return null;
            }
            this.spentValue += modulePrices[module.name];
        }

        Equipment addedModule = module.Copy();
        addedModule.remainingDurability = durability;
        AddReturnModuleButton(addedModule);

        // update the value remaining text
        UpdateValueRemainingText();

        return addedModule;
    }

    public void AddReturnModuleButton(Equipment module)
    {
        // create the return module button
        GameObject returnModuleObject = Instantiate(returnModulePrefab, returnModuleParent.transform);
        returnModuleObject.GetComponent<PurchasedModulePanel>().Initialize(module, this);
        int thisReturnModuleIndex = purchasedModules.Count;
        float x = thisReturnModuleIndex % returnModulesPerRow * returnHorizontalSpacing;
        float y = -thisReturnModuleIndex / returnModulesPerRow * returnVerticalSpacing;
        // shift the anchors of the return module object by the x and y values
        returnModuleObject.GetComponent<RectTransform>().anchorMin += new Vector2(x, y);
        returnModuleObject.GetComponent<RectTransform>().anchorMax += new Vector2(x, y);
        // zero out the return module object rect transform offsets
        returnModuleObject.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        returnModuleObject.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        purchasedModules.Add(returnModuleObject.GetComponent<PurchasedModulePanel>());
    }

    public void ReturnModule(PurchasedModulePanel button)
    {
        Equipment returnedModule = button.module;

        // get a list of all the equipment except the one being returned
        List<Equipment> equipment = new();
        foreach (PurchasedModulePanel returnModuleButton in purchasedModules)
        {
            if (returnModuleButton != button)
            {
                equipment.Add(returnModuleButton.module);
            }
        }

        // destroy all the return module buttons
        foreach (PurchasedModulePanel returnModuleButton in purchasedModules)
        {
            Destroy(returnModuleButton.gameObject);
        }

        // create new return module buttons for all the equipment except the one being returned
        purchasedModules.Clear();
        foreach (Equipment module in equipment)
        {
            AddReturnModuleButton(module);
        }

        // remove the value of the module from the spent value
        spentValue -= modulePrices[returnedModule.name] * ((float)returnedModule.remainingDurability / (float)returnedModule.maxDurability);

        // update the value remaining text
        UpdateValueRemainingText();
    }

    public void AddDurability(Equipment module)
    {
        // return if the module is at max durability
        if (module.remainingDurability >= module.maxDurability)
        {
            return;
        }
        float cost = modulePrices[module.name] * (1f / (float)module.maxDurability);
        // check if the player has enough value to buy the durability
        if (SpentValue() + cost > LastRunValue())
        {
            return;
        }
        module.remainingDurability++;
        spentValue += cost;
        UpdateValueRemainingText();
    }

    public void Dive()
    {
        submarine.SetSubmarineType(submarineType);
        List<Equipment> modules = new();
        foreach (PurchasedModulePanel button in purchasedModules)
        {
            modules.Add(button.module);
        }
        submarine.AddEquipment(modules);
        gameObject.SetActive(false);
        submarine.gameObject.SetActive(true);
        OnSetupComplete?.Invoke();
    }
}
