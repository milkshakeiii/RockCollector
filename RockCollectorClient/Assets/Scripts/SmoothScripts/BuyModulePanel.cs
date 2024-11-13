using TMPro;
using UnityEngine;

public class BuyModulePanel : MonoBehaviour
{
    public TMP_Text moduleNameText;
    public TMP_Text moduleDescriptionText;
    public TMP_Text priceText;

    private Equipments module;
    private SetupScreen setupScreen;

    public void Initialize(Equipments module, float price, SetupScreen setupScreen)
    {
        this.module = module;
        this.setupScreen = setupScreen;

        moduleNameText.text = module.name;
        moduleDescriptionText.text = module.description;
        // display price with 2 decimal places
        priceText.text = price.ToString("F2");

        SetupScreen.OnPurchaseCallback += ReportPurchaseCallbackHandler;
    }

    public void Clicked()
    {
        setupScreen.BuyModule(module, true, module.maxDurability);
    }

    private void ReportPurchaseCallbackHandler(string updatedItem, float newPrice)
    {
        if (updatedItem == module.name)
        {
            priceText.text = newPrice.ToString("F2");
        }
    }
}
