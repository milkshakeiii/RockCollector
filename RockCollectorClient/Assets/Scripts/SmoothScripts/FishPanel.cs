using TMPro;
using UnityEngine;

public class FishPanel : MonoBehaviour
{
    public UnityEngine.UI.Image fishImage;
    public TMP_Text fishText;

    public void Initialize(Timefish fish)
    {
        fishImage.sprite = fish.GetComponent<SpriteRenderer>().sprite;
        fishText.text = fish.species.name;
        fishText.text += "\n" + fish.Mass() + " kg - value " + fish.TradeValue();
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
