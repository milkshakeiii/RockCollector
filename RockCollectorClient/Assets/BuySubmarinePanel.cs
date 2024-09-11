using TMPro;
using UnityEngine;

public class BuySubmarinePanel : MonoBehaviour
{
    public TMP_Text submarineNameText;
    public TMP_Text submarineDescriptionText;
    public TMP_Text priceText;

    private SubmarineType submarineType;
    private SetupScreen setupScreen;

    public void Initialize(SubmarineType submarineType, float price, SetupScreen setupScreen)
    {
        this.submarineType = submarineType;
        this.setupScreen = setupScreen;

        submarineNameText.text = submarineType.name;
        submarineDescriptionText.text = submarineType.description;
        // display price with 2 decimal places
        priceText.text = price.ToString("F2");

        SetupScreen.OnPurchaseCallback += ReportPurchaseCallbackHandler;
    }

    private void ReportPurchaseCallbackHandler(string updatedItem, float newPrice)
    {
        if (updatedItem == submarineType.name)
        {
            priceText.text = newPrice.ToString("F2");
        }
    }

    public void Clicked()
    {
        setupScreen.BuySubmarine(submarineType, true, submarineType.maxDurability);
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
