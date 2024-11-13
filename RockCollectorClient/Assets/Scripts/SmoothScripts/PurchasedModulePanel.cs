using TMPro;
using UnityEngine;

public class PurchasedModulePanel : MonoBehaviour
{
    public TMP_Text durabilityText;
    public TMP_Text moduleNameText;

    public Equipments module;

    private SetupScreen setupScreen;

    public void Initialize(Equipments module, SetupScreen setupScreen)
    {
        this.module = module;
        this.setupScreen = setupScreen;
        moduleNameText.text = module.name;
        durabilityText.text = module.remainingDurability.ToString() + "/" + module.maxDurability.ToString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddDurability()
    {
        setupScreen.AddDurability(module);
        durabilityText.text = module.remainingDurability.ToString() + "/" + module.maxDurability.ToString();
    }

    public void Clicked()
    {
        setupScreen.ReturnModule(this);
    }
}
