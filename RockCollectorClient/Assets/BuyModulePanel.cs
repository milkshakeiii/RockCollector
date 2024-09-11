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

        SetupScreen.OnPurchaseCallback += ReportPurchaseCallbackHandler;
    }

    public void Clicked()
    {
        setupScreen.BuyModule(module, true, module.maxDurability);
    }

    private void ReportPurchaseCallbackHandler(Newtonsoft.Json.Linq.JObject response)
    {
        Debug.Log(response);
    }
}
