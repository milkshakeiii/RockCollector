using TMPro;
using UnityEngine;

public class BuySubmarinePanel : MonoBehaviour
{
    public TMP_Text submarineNameText;
    public TMP_Text submarineDescriptionText;

    private SubmarineType submarineType;
    private SetupScreen setupScreen;

    public void Initialize(SubmarineType submarineType, SetupScreen setupScreen)
    {
        this.submarineType = submarineType;
        this.setupScreen = setupScreen;

        submarineNameText.text = submarineType.name;
        submarineDescriptionText.text = submarineType.description;
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
