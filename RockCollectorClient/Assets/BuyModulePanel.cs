using TMPro;
using UnityEngine;

public class BuyModulePanel : MonoBehaviour
{
    public TMP_Text moduleNameText;
    public TMP_Text moduleDescriptionText;

    private Equipment module;

    public void Initialize(Equipment module)
    {
        this.module = module;

        moduleNameText.text = module.name;
        moduleDescriptionText.text = module.description;
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
