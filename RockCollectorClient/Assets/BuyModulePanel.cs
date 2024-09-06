using TMPro;
using UnityEngine;

public class BuyModulePanel : MonoBehaviour
{
    public TMP_Text moduleNameText;
    public TMP_Text moduleDescriptionText;

    private Equipment module;
    private SetupScreen setupScreen;

    public void Initialize(Equipment module, SetupScreen setupScreen)
    {
        this.module = module;
        this.setupScreen = setupScreen;

        moduleNameText.text = module.name;
        moduleDescriptionText.text = module.description;
    }

    public void Clicked()
    {
        setupScreen.BuyModule(module, true, module.maxDurability);
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
